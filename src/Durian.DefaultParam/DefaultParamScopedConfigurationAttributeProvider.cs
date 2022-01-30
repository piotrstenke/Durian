// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.DefaultParamScopedConfigurationAttribute</c> class.
	/// </summary>
	public sealed class DefaultParamScopedConfigurationAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "DefaultParamScopedConfigurationAttribute";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.ConfigurationNamespace;

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'ApplyNewModifierWhenPossible' property.
		/// </summary>
		public const string ApplyNewModifierWhenPossible = "ApplyNewModifierWhenPossible";

		/// <summary>
		/// Name of the 'MethodConvention' property.
		/// </summary>
		public const string MethodConvention = "MethodConvention";

		/// <summary>
		/// Name of the 'TypeConvention' property.
		/// </summary>
		public const string TypeConvention = "TypeConvention";

		/// <summary>
		/// Name of the 'MethodConvention' property.
		/// </summary>
		public const string TargetNamespace = "TargetNamespace";

		/// <inheritdoc/>
		public override string GetNamespace()
		{
			return Namespace;
		}

		/// <inheritdoc/>
		public override string GetFullName()
		{
			return FullName;
		}

		/// <inheritdoc/>
		public override string GetTypeName()
		{
			return TypeName;
		}

		/// <inheritdoc/>
		public override string GetText()
		{
			return
$@"using System;

#nullable enable

namespace {Namespace}
{{
	/// <summary>
	/// Configures how members with the <see cref=""{DefaultParamAttributeProvider.TypeName}""/> are handled by the generator. Applies to all members in the current scope.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public sealed class {TypeName} : Attribute
	{{
		// <inheritdoc cref=""{DefaultParamConfigurationAttributeProvider.TypeName}.{DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible}""/>
		public bool {ApplyNewModifierWhenPossible} {{ get; set; }} = true;

		// <inheritdoc cref=""{DefaultParamConfigurationAttributeProvider.TypeName}.{DefaultParamConfigurationAttributeProvider.MethodConvention}""/>
		public {DPMethodConventionProvider.TypeName} {MethodConvention} {{ get; set; }}

		// <inheritdoc cref=""{DefaultParamConfigurationAttributeProvider.TypeName}.{DefaultParamConfigurationAttributeProvider.TargetNamespace}""/>
		public string? {TargetNamespace} {{ get; set; }}

		/// <inheritdoc cref=""{DefaultParamConfigurationAttributeProvider.TypeName}.{DefaultParamConfigurationAttributeProvider.TypeConvention}""/>
		public {DPTypeConventionProvider.TypeName} {TypeConvention} {{ get; set; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/> class.
		/// </summary>
		public {TypeName}()
		{{
		}}
	}}
}}
";
		}
	}
}