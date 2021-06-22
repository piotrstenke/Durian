// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Durian.Analysis.Data;
using Durian.Configuration;
using Durian.Analysis.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to generate source code using the <see cref="GenericSpecializationGenerator"/>.
	/// </summary>
	public sealed class GenSpecCompilationData : CompilationData
	{
		private readonly string _allowSpecializationAttribute = typeof(DefaultParamAttribute).ToString();
		private readonly string _genericSpecializationAttribute = typeof(GenericSpecializationAttribute).ToString();
		private readonly string _genericSpecializationConfigurationAttribute = typeof(DefaultParamConfigurationAttribute).ToString();

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Durian.AllowSpecializationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? AllowSpecializationAttribute { get; private set; }

		/// <summary>
		/// <see cref="GenSpecConfiguration"/> created from the <see cref="GenericSpecializationConfigurationAttribute"/> defined on the <see cref="CompilationData.Compilation"/>'s main assembly. -or- <see cref="GenSpecConfiguration.Default"/> if no <see cref="GenericSpecializationConfigurationAttribute"/> was found.
		/// </summary>
		public GenSpecConfiguration Configuration { get; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="GenericSpecializationConfigurationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? ConfigurationAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="GenericSpecializationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? GenericSpecializationAttribute { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(true, nameof(ConfigurationAttribute), nameof(AllowSpecializationAttribute), nameof(GenericSpecializationAttribute))]
		public override bool HasErrors
		{
			get => base.HasErrors;
			protected set => base.HasErrors = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecCompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public GenSpecCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Reset();
			Configuration = GetConfiguration(compilation, ConfigurationAttribute);
		}

		/// <summary>
		/// Creates a new instance of <see cref="GenSpecConfiguration"/> based on the <see cref="GenericSpecializationConfigurationAttribute"/> defined on the <see cref="CompilationData.Compilation"/>'s main assembly or <see cref="GenSpecConfiguration.Default"/> if no <see cref="GenericSpecializationConfigurationAttribute"/> was found.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="GenSpecConfiguration"/> for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static GenSpecConfiguration GetConfiguration(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol? configurationAttribute = compilation.GetTypeByMetadataName(typeof(GenericSpecializationConfigurationAttribute).ToString());
			return GetConfiguration(compilation, configurationAttribute);
		}

		/// <summary>
		/// Creates a new instance of <see cref="GenSpecConfiguration"/> based on data contained within the specified <paramref name="attribute"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to create the <see cref="GenSpecConfiguration"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static GenSpecConfiguration GetConfiguration(AttributeData attribute)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			string templateName = GetStringValue(attribute, nameof(GenericSpecializationConfigurationAttribute.TemplateName)) ?? GenSpecConfiguration.DefaultTemplateName;
			string interfaceName = GetStringValue(attribute, nameof(GenericSpecializationConfigurationAttribute.InterfaceName)) ?? GenSpecConfiguration.DefaultInterfaceName;
			GenSpecImport importOptions = (GenSpecImport)attribute.GetNamedArgumentValue<int>(nameof(GenericSpecializationConfigurationAttribute.ImportOptions));
			bool forceInherit = attribute.GetNamedArgumentValue<bool>(nameof(GenericSpecializationConfigurationAttribute.ForceInherit));

			return new(templateName, interfaceName)
			{
				ImportOptions = importOptions,
				ForceInherit = forceInherit
			};
		}

		/// <inheritdoc/>
		public void Reset()
		{
			AllowSpecializationAttribute = Compilation.GetTypeByMetadataName(_allowSpecializationAttribute);
			ConfigurationAttribute = Compilation.GetTypeByMetadataName(_genericSpecializationConfigurationAttribute);
			GenericSpecializationAttribute = Compilation.GetTypeByMetadataName(_genericSpecializationAttribute);

			HasErrors =
				AllowSpecializationAttribute is null ||
				ConfigurationAttribute is null ||
				GenericSpecializationAttribute is null;
		}

		/// <inheritdoc/>
		protected override void OnUpdate(CSharpCompilation oldCompilation)
		{
			Reset();
		}

		private static GenSpecConfiguration GetConfiguration(CSharpCompilation compilation, INamedTypeSymbol? configurationAttribute)
		{
			if (configurationAttribute is null)
			{
				return GenSpecConfiguration.Default;
			}

			foreach (AttributeData attr in compilation.Assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, configurationAttribute))
				{
					return GetConfiguration(attr);
				}
			}

			return GenSpecConfiguration.Default;
		}

		private static string? GetStringValue(AttributeData attr, string propertyName)
		{
			if (attr.TryGetNamedArgumentValue(propertyName, out string? value) &&
				!string.IsNullOrWhiteSpace(value) &&
				AnalysisUtilities.IsValidIdentifier(value)
			)
			{
				return value;
			}

			return null;
		}
	}
}
