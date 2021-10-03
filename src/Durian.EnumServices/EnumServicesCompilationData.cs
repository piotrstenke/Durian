// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System;
using Durian.Configuration;
using Durian.Analysis.Extensions;

namespace Durian.Analysis.EnumServices
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to generate source code using the <see cref="EnumServicesGenerator"/>.
	/// </summary>
	public class EnumServicesCompilationData : CompilationData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Durian.EnumServices"/>.
		/// </summary>
		public INamedTypeSymbol? EnumServices { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Durian.EnumServicesAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? EnumServicesAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Durian.GeneratedTypeAccess"/>.
		/// </summary>
		public INamedTypeSymbol? GeneratedTypeAccess { get; private set; }

		/// <summary>
		/// <see cref="EnumServicesConfiguration"/> created from the <see cref="Durian.EnumServicesAttribute"/>
		/// defined on the <see cref="CompilationData.Compilation"/>'s main assembly. -or-
		/// <see cref="EnumServicesConfiguration.Default"/> if no appropriate <see cref="Durian.EnumServicesAttribute"/> was found.
		/// </summary>
		public EnumServicesConfiguration GlobalConfiguration { get; }

		/// <inheritdoc/>
		[MemberNotNullWhen(true, nameof(EnumServices), nameof(EnumServicesAttribute), nameof(GeneratedTypeAccess))]
		public override bool HasErrors
		{
			get => base.HasErrors;
			protected set => base.HasErrors = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesCompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public EnumServicesCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Reset();
			GlobalConfiguration = GetConfiguration(compilation, EnumServicesAttribute);
		}

		/// <summary>
		/// Creates a new instance of <see cref="EnumServicesConfiguration"/> based on the
		/// <see cref="Durian.EnumServicesAttribute"/> defined on the
		/// <paramref name="compilation"/>'s main assembly or <see cref="EnumServicesConfiguration.Default"/>
		/// if no appropriate <see cref="Durian.EnumServicesAttribute"/> was found.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="EnumServicesConfiguration"/> for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static EnumServicesConfiguration GetConfiguration(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol? attribute = compilation.GetTypeByMetadataName(typeof(EnumServicesAttribute).ToString());
			return GetConfiguration(compilation, attribute);
		}

		/// <inheritdoc cref="CompilationDataWithSymbols.Reset"/>
		public void Reset()
		{
			EnumServices = Compilation.GetTypeByMetadataName(typeof(Durian.EnumServices).ToString());
			EnumServicesAttribute = Compilation.GetTypeByMetadataName(typeof(EnumServicesAttribute).ToString());
			GeneratedTypeAccess = Compilation.GetTypeByMetadataName(typeof(GeneratedTypeAccess).ToString());

			HasErrors =
				EnumServices is null ||
				EnumServicesAttribute is null ||
				GeneratedTypeAccess is null;
		}

		/// <inheritdoc/>s
		protected override void OnUpdate(CSharpCompilation oldCompilation)
		{
			Reset();
		}

		private static EnumServicesConfiguration GetConfiguration(CSharpCompilation compilation, INamedTypeSymbol? attributeSymbol)
		{
			if (attributeSymbol is null)
			{
				return EnumServicesConfiguration.Default;
			}

			foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
				{
					bool allowCustomization = attribute.GetNamedArgumentValue<bool>(
						nameof(Durian.EnumServicesAttribute.AllowCustomization));

					GeneratedTypeAccess accessibility = (GeneratedTypeAccess)attribute.GetNamedArgumentValue<int>(
						nameof(Durian.EnumServicesAttribute.Accessibility));

					Durian.EnumServices services = (Durian.EnumServices)attribute.GetNamedArgumentValue<int>(
						nameof(Durian.EnumServicesAttribute.Services));

					string? prefix = attribute.GetNamedArgumentValue<string>(
						nameof(Durian.EnumServicesAttribute.Prefix));

					string @namespace = attribute.GetNamedArgumentValue<string>(
						nameof(Durian.EnumServicesAttribute.Namespace)) ?? "EnumExtensions";

					string? className = attribute.GetNamedArgumentValue<string>(
						nameof(Durian.EnumServicesAttribute.ClassName));

					return new()
					{
						AllowCustomization = allowCustomization,
						Accessibility = accessibility,
						EnumServices = services,
						Prefix = prefix,
						Namespace = @namespace,
						ClassName = className
					};
				}
			}

			return EnumServicesConfiguration.Default;
		}
	}
}
