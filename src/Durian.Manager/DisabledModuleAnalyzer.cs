// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Durian.Analysis;
using Durian.Analysis.Extensions;
using Durian.Generator;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Collects information about disabled Durian modules.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[Generator(LanguageNames.CSharp)]
	public sealed class DisabledModuleAnalyzer : DiagnosticAnalyzer, ISourceGenerator
	{
		private static readonly object _lockObject = new();
		private static DurianModule[]? _disabledModules;
		private static int _previousNumOfDisabledModules;

		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DurianDiagnostics.DUR0002_ModuleOfTypeIsNotImported);

		/// <summary>
		/// Initializes a new instance of the <see cref="DisabledModuleAnalyzer"/> class.
		/// </summary>
		public DisabledModuleAnalyzer()
		{
		}

		/// <summary>
		/// Returns an array of all modules that are disabled for the current compilation.
		/// </summary>
		/// <exception cref="InvalidOperationException">Analyzer is not initialized.</exception>
		public static DurianModule[] GetDisabledModules()
		{
			lock (_lockObject)
			{
				if (_disabledModules is null)
				{
					throw Exc_NotInitialized();
				}

				DurianModule[] modules = new DurianModule[_disabledModules!.Length];
				Array.Copy(_disabledModules, modules, _disabledModules.Length);

				return modules;
			}
		}

		/// <summary>
		/// Returns an array of all modules that are disabled for the current compilation.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> that is used if the analyzer is not initialized or the disabled managers are changed.</param>
		public static DurianModule[] GetDisabledModules(Compilation compilation)
		{
			lock (_lockObject)
			{
				UpdateIfNullOrChanged(compilation);

				DurianModule[] modules = new DurianModule[_disabledModules!.Length];
				Array.Copy(_disabledModules, modules, _disabledModules.Length);

				return modules;
			}
		}

		/// <summary>
		/// Returns an array of all modules that are enabled for the current compilation.
		/// </summary>
		/// <exception cref="InvalidOperationException">Analyzer is not initialized.</exception>
		public static DurianModule[] GetEnabledModules()
		{
			DurianModule[] modules = ModuleIdentity.GetAllModulesAsEnums();
			List<DurianModule> enabled = new(modules.Length);

			lock (_lockObject)
			{
				if (_disabledModules is null)
				{
					throw Exc_NotInitialized();
				}

				foreach (DurianModule module in modules)
				{
					if (IsEnabled_Internal(module))
					{
						enabled.Add(module);
					}
				}
			}

			return enabled.ToArray();
		}

		/// <summary>
		/// Returns an array of all modules that are enabled for the current compilation.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> that is used if the analyzer is not initialized or the disabled managers are changed.</param>
		public static DurianModule[] GetEnabledModules(Compilation compilation)
		{
			DurianModule[] modules = ModuleIdentity.GetAllModulesAsEnums();
			List<DurianModule> enabled = new(modules.Length);

			lock (_lockObject)
			{
				UpdateIfNullOrChanged(compilation);

				foreach (DurianModule module in modules)
				{
					if (IsEnabled_Internal(module))
					{
						enabled.Add(module);
					}
				}
			}

			return enabled.ToArray();
		}

		/// <summary>
		/// Initializes the analyzer if needed.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> that the analyzer is initialized for.</param>
		public static void InitializeOrUpdateIfNeeded(Compilation compilation)
		{
			lock (_lockObject)
			{
				UpdateIfNullOrChanged(compilation);
			}
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the current compilation.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check for.</param>
		/// <exception cref="InvalidOperationException">Analyzer is not initialized.</exception>
		public static bool IsEnabled(DurianModule module)
		{
			lock (_lockObject)
			{
				if (_disabledModules is null)
				{
					throw Exc_NotInitialized();
				}

				return IsEnabled_Internal(module);
			}
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the current compilation.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check for.</param>
		/// <param name="compilation"><see cref="Compilation"/> that is used if the analyzer is not initialized or the disabled managers are changed.</param>
		public static bool IsEnabled(DurianModule module, Compilation compilation)
		{
			InitializeOrUpdateIfNeeded(compilation);

			lock (_lockObject)
			{
				UpdateIfNullOrChanged(compilation);

				return IsEnabled_Internal(module);
			}
		}

		/// <summary>
		/// Writes the <see cref="EnableModuleAttribute"/> for all the <see cref="DurianModule"/> that are not disabled using the <see cref="DisableModuleAttribute"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <exception cref="InvalidOperationException">Analyzer is not initialized.</exception>
		public static void WriteEnableModuleAttributes(StringBuilder builder)
		{
			DurianModule[] modules = GetEnabledModules();
			WriteEnableModuleAttribute_Internal(builder, modules);
		}

		/// <summary>
		/// Writes the <see cref="EnableModuleAttribute"/> for all the <see cref="DurianModule"/> that are not disabled using the <see cref="DisableModuleAttribute"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="compilation"><see cref="Compilation"/> that is used if the analyzer is not initialized or the disabled managers are changed.</param>
		public static void WriteEnableModuleAttributes(StringBuilder builder, Compilation compilation)
		{
			DurianModule[] modules = GetEnabledModules(compilation);
			WriteEnableModuleAttribute_Internal(builder, modules);
		}

		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterCompilationStartAction(Analyze);
		}

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			StringBuilder builder = new();
			WriteEnableModuleAttributes(builder, context.Compilation);
			context.AddSource("__EnableModules__", builder.ToString());
		}

		void ISourceGenerator.Initialize(GeneratorInitializationContext context)
		{
			// Do nothing.
		}

#pragma warning disable RS1012 // Start action has no registered actions.

		private static void Analyze(CompilationStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
		{
			DurianModule[] modules = FindDisabledModules(context.Compilation);

			lock (_lockObject)
			{
				_disabledModules = modules;
				_previousNumOfDisabledModules = _disabledModules.Length;
			}
		}

		private static InvalidOperationException Exc_NotInitialized()
		{
			return new InvalidOperationException("Analyzer is not initialized!");
		}

		private static DurianModule[] FindDisabledModules(Compilation compilation)
		{
			INamedTypeSymbol? attribute = compilation.GetTypeByMetadataName("Durian.DisableModuleAttribute");

			if (attribute is null)
			{
				return Array.Empty<DurianModule>();
			}

			List<DurianModule> disabledModules = new(4);

			foreach (AttributeData attr in compilation.Assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attribute) &&
					attr.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule module = (DurianModule)value;

					if (DurianInfo.IsValidModuleValue(module))
					{
						disabledModules.Add((DurianModule)value);
					}
				}
			}

			return disabledModules.ToArray();
		}

		private static bool IsEnabled_Internal(DurianModule module)
		{
			foreach (DurianModule m in _disabledModules!)
			{
				if (m == module)
				{
					return false;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void UpdateIfNullOrChanged(Compilation compilation)
		{
			if (_disabledModules is null || _disabledModules!.Length != _previousNumOfDisabledModules)
			{
				_disabledModules = FindDisabledModules(compilation);
				_previousNumOfDisabledModules = _disabledModules.Length;
			}
		}

		private static void WriteEnableModuleAttribute_Internal(StringBuilder builder, DurianModule[] modules)
		{
			if (modules.Length == 0)
			{
				return;
			}

			builder.AppendLine(AutoGenerated.GetHeader());

			foreach (DurianModule module in modules)
			{
				builder
					.Append("[assembly: Durian.Generator.EnableModule(Durian.Info.")
					.Append(nameof(DurianModule))
					.Append('.')
					.Append(module)
					.AppendLine(")]");
			}
		}
	}
}
