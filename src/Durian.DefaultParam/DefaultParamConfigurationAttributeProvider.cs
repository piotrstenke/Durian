// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.DefaultParamConfigurationAttribute</c> class.
	/// </summary>
	public sealed class DefaultParamConfigurationAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the 'ApplyNewModifierWhenPossible' property.
		/// </summary>
		public const string ApplyNewModifierWhenPossible = "ApplyNewModifierWhenPossible";

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'MethodConvention' property.
		/// </summary>
		public const string MethodConvention = "MethodConvention";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.ConfigurationNamespace;

		/// <summary>
		/// Name of the 'MethodConvention' property.
		/// </summary>
		public const string TargetNamespace = "TargetNamespace";

		/// <summary>
		/// Name of the 'TypeConvention' property.
		/// </summary>
		public const string TypeConvention = "TypeConvention";

		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "DefaultParamConfigurationAttribute";

		/// <inheritdoc/>
		public override string GetFullName()
		{
			return FullName;
		}

		/// <inheritdoc/>
		public override string GetNamespace()
		{
			return Namespace;
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
	/// Configures how members with the <see cref=""{DefaultParamAttributeProvider.TypeName}""/> are handled by the generator. Applies only to this member.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Determines whether to apply the <see langword=""new""/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword=""true""/>.
		/// </summary>
		public bool {ApplyNewModifierWhenPossible} {{ get; set; }} = true;

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a method. The default value is <see cref=""{DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call}""/>.
		/// </summary>
		public {DPMethodConventionProvider.TypeName} {MethodConvention} {{ get; set; }}

		/// <summary>
		/// Specifies the namespace where the target member should be generated in.
		/// </summary>
		/// <remarks>Set this property to <c>global</c> to use the global namespace or to <see langword=""null""/> to use namespace of the original member.</remarks>
		public string? {TargetNamespace} {{ get; set; }}

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a type. The default value is <see cref=""{DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit}""/>.
		/// </summary>
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

		/// <inheritdoc/>
		public override string GetTypeName()
		{
			return TypeName;
		}
	}
}