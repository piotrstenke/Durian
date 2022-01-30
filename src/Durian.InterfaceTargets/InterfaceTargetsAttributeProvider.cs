// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.InterfaceTargetsAttribute</c> class.
	/// </summary>
	public sealed class InterfaceTargetsAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "InterfaceTargetsAttribute";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'Targets' property.
		/// </summary>
		public const string Targets = "Targets";

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
		public override string GetText()
		{
			return
$@"using System;

namespace {Namespace}
{{
	/// <summary>
	/// Specifies that an <see langword=""interface""/> can be implemented only by members of certain kind.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Specifies member kinds this interface is valid on.
		/// </summary>
		public {InterfaceTargetsProvider.TypeName} {Targets} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/> class.
		/// </summary>
		/// <param name=""targets"">Specifies member kinds this interface is valid on.</param>
		public {TypeName}({InterfaceTargetsProvider.TypeName} targets)
		{{
			{Targets} = targets;
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