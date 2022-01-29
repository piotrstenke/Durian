// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	public sealed partial class LoggingConfiguration
	{
		private static readonly Dictionary<Assembly, LoggingConfiguration> _assemblyConfigurations = new();

		/// <summary>
		/// Creates a new instance of the <see cref="LoggingConfiguration"/> class with its values set to default.
		/// </summary>
		public static LoggingConfiguration Default => new()
		{
			EnableLogging = false,
			EnableDiagnostics = false,
			EnableExceptions = false,
			LogDirectory = DefaultLogDirectory,
			SupportedLogs = GeneratorLogs.None,
			SupportsDiagnostics = false,
		};

		/// <summary>
		/// Default directory where the generator log files are placed, which is '<c>&lt;documents&gt;/Durian/logs</c>'.
		/// </summary>
		public static string DefaultLogDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Durian/logs";

		/// <summary>
		/// Determines whether generator logging is globally enabled.
		/// </summary>
		/// <remarks>
		/// Logging becomes globally disabled when an <see cref="DisableLoggingGloballyAttribute"/>
		/// is detected on any assembly in the current <see cref="AppDomain"/>.
		/// </remarks>
		public static bool IsEnabled { get; } = CheckLoggingIsEnabled();

		/// <summary>
		/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the <see cref="LoggingConfiguration"/> for.</param>
		/// <exception cref="ArgumentException">
		/// <see cref="DefaultLoggingConfigurationAttribute.LogDirectory"/> cannot be empty or whitespace only. -or-
		/// <see cref="DefaultLoggingConfigurationAttribute.LogDirectory"/> must be specified if <see cref="DefaultLoggingConfigurationAttribute.RelativeToDefault"/> is set to <see langword="false"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static LoggingConfiguration CreateConfigurationForAssembly(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			DefaultLoggingConfigurationAttribute? attr = assembly.GetCustomAttribute<DefaultLoggingConfigurationAttribute>();

			if (attr is null)
			{
				return Default;
			}
			else
			{
				return new LoggingConfiguration()
				{
					LogDirectory = attr.GetAndValidateFullLogDirectory(),
					SupportedLogs = attr.SupportedLogs,
					EnableLogging = IsEnabled && !Attribute.IsDefined(assembly, typeof(DisableLoggingAttribute)),
					SupportsDiagnostics = attr.SupportsDiagnostics,
					EnableDiagnostics = attr.SupportsDiagnostics,
					EnableExceptions = attr.EnableExceptions
				};
			}
		}

		/// <summary>
		/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</typeparam>
		/// <exception cref="ArgumentException">
		/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
		/// <see cref="DefaultLoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
		/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
		/// </exception>
		public static LoggingConfiguration CreateConfigurationForGenerator<T>() where T : ISourceGenerator
		{
			return CreateConfigurationForGenerator_Internal(typeof(T));
		}

		/// <summary>
		/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
		/// </summary>
		/// <param name="type"><see cref="Type"/> of <see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface. -or-
		/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
		/// <see cref="DefaultLoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
		/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static LoggingConfiguration CreateConfigurationForGenerator(Type type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			CheckTypeIsISourceGenerator(type);
			return CreateConfigurationForGenerator_Internal(type);
		}

		/// <summary>
		/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
		/// <exception cref="ArgumentException">
		/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
		/// <see cref="DefaultLoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
		/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static LoggingConfiguration CreateConfigurationForGenerator(ISourceGenerator generator)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return CreateConfigurationForGenerator_Internal(generator.GetType());
		}

		/// <summary>
		/// Returns a reference to the <see cref="LoggingConfiguration"/> for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the <see cref="LoggingConfiguration"/> for.</param>
		/// <exception cref="ArgumentException"><see cref="DefaultLoggingConfigurationAttribute.LogDirectory"/> cannot be empty or whitespace only.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static LoggingConfiguration GetConfigurationForAssembly(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			lock (_assemblyConfigurations)
			{
				if (_assemblyConfigurations.TryGetValue(assembly, out LoggingConfiguration config))
				{
					return config;
				}
				else
				{
					config = CreateConfigurationForAssembly(assembly);
					_assemblyConfigurations.Add(assembly, config);
					return config;
				}
			}
		}

		/// <summary>
		/// Checks if the specified <paramref name="type"/> has the <see cref="DisableLoggingAttribute"/> applied, either directly or by inheritance.
		/// </summary>
		/// <param name="type"><see cref="Type"/> to perform the check for.</param>
		/// <exception cref="ArgumentException"><paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool HasDisableAttribute(Type type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			CheckTypeIsISourceGenerator(type);
			return HasDisableAttribute_Internal(type);
		}

		/// <summary>
		/// Checks if the specified <paramref name="assembly"/> has the <see cref="DisableLoggingAttribute"/> applied.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool HasDisableAttribute(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return !IsEnabledForAssembly_Internal(assembly);
		}

		/// <summary>
		/// Checks if generator logging is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <remarks>Always returns <see langword="false"/> is <see cref="IsEnabled"/> is <see langword="false"/>.</remarks>
		/// <param name="assembly"><see cref="Assembly"/> to perform the check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool IsEnabledForAssembly(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return IsEnabled && assembly.GetCustomAttribute(typeof(DisableLoggingAttribute)) is null;
		}

		/// <summary>
		/// Checks if generator logging is enabled for the specified <paramref name="generator"/>.
		/// </summary>
		/// <remarks>Always returns <see langword="false"/> is <see cref="IsEnabled"/> is <see langword="false"/>.</remarks>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static bool IsEnabledForGenerator(ISourceGenerator generator)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			if (!IsEnabled)
			{
				return false;
			}

			Type type = generator.GetType();

			if (!IsEnabledForAssembly_Internal(type.Assembly))
			{
				return false;
			}

			return IsEnabledForGenerator_Internal(type, true);
		}

		/// <summary>
		/// Checks if generator logging is enabled for the specified generator <paramref name="type"/>.
		/// </summary>
		/// <remarks>Always returns <see langword="false"/> is <see cref="IsEnabled"/> is <see langword="false"/>.</remarks>
		/// <param name="type"><see cref="Type"/> of <see cref="ISourceGenerator"/> to perform the check for.</param>
		/// <exception cref="ArgumentException"><paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsEnabledForGenerator(Type type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			CheckTypeIsISourceGenerator(type);

			if (!IsEnabled)
			{
				return false;
			}

			if (!IsEnabledForAssembly_Internal(type.Assembly))
			{
				return false;
			}

			return IsEnabledForGenerator_Internal(type, true);
		}

		/// <summary>
		/// Checks if generator logging is enabled for the specified generator type.
		/// </summary>
		/// <remarks>Always returns <see langword="false"/> is <see cref="IsEnabled"/> is <see langword="false"/>.</remarks>
		/// <typeparam name="T">Type of <see cref="ISourceGenerator"/> to perform the check for.</typeparam>
		public static bool IsEnabledForGenerator<T>() where T : ISourceGenerator
		{
			if (!IsEnabled)
			{
				return false;
			}

			Type type = typeof(T);

			if (!IsEnabledForAssembly_Internal(type.Assembly))
			{
				return false;
			}

			return IsEnabledForGenerator_Internal(type, true);
		}

		internal static ReportDiagnosticTarget GetReportDiagnosticTarget(bool enableLogging, bool enableReport)
		{
			ReportDiagnosticTarget target = ReportDiagnosticTarget.None;

			if (enableLogging)
			{
				target += (int)ReportDiagnosticTarget.Log;
			}

			if (enableReport)
			{
				target += (int)ReportDiagnosticTarget.Report;
			}

			return target;
		}

		private static bool CheckLoggingIsEnabled()
		{
			return !AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetCustomAttribute(typeof(DisableLoggingGloballyAttribute)) is not null);
		}

		private static void CheckTypeIsISourceGenerator(Type type)
		{
			if (!typeof(ISourceGenerator).IsAssignableFrom(type))
			{
				throw new ArgumentException($"Specified type does not implement the {nameof(ISourceGenerator)} interface!");
			}
		}

		private static LoggingConfiguration CreateConfigurationForGenerator_Internal(Type type)
		{
			LoggingConfigurationAttribute? attr = type.GetCustomAttribute<LoggingConfigurationAttribute>(true);

			if (attr is null)
			{
				LoggingConfiguration config = GetConfigurationForAssembly(type.Assembly);

				if (config.EnableLogging)
				{
					config._enableLogging = IsEnabledForGenerator_Internal(type, false);
				}

				return config;
			}
			else
			{
				return new LoggingConfiguration()
				{
					LogDirectory = attr.GetAndValidateFullLogDirectory(GetConfigurationForAssembly(type.Assembly)),
					SupportedLogs = attr.SupportedLogs,
					EnableLogging = IsEnabled && !HasDisableAttribute_Internal(type),
					SupportsDiagnostics = attr.SupportsDiagnostics,
					EnableDiagnostics = attr.SupportsDiagnostics,
					EnableExceptions = attr.EnableExceptions
				};
			}
		}

		private static bool HasDisableAttribute_Internal(Type type)
		{
			DisableLoggingAttribute? attr = type.GetCustomAttribute<DisableLoggingAttribute>();

			if (attr is null)
			{
				attr = type.BaseType.GetCustomAttribute<DisableLoggingAttribute>(true);

				if (attr is null || !attr.Inherit)
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsEnabledForAssembly_Internal(Assembly assembly)
		{
			return assembly.GetCustomAttribute(typeof(DisableLoggingAttribute)) is null;
		}

		private static bool IsEnabledForGenerator_Internal(Type type, bool checkForConfigurationAttribute)
		{
			if (HasDisableAttribute_Internal(type))
			{
				return false;
			}

			if (checkForConfigurationAttribute)
			{
				return type.GetCustomAttribute(typeof(LoggingConfigurationAttribute), true) is not null;
			}

			return false;
		}
	}
}