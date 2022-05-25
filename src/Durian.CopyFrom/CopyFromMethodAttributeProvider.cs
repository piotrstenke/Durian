// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.CopyFromMethodAttribute</c> class.
	/// </summary>
	public sealed class CopyFromMethodAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the 'AdditionalNodes' property.
		/// </summary>
		public const string AdditionalNodes = "AdditionalNodes";

		/// <summary>
		/// Name of the 'AddUsings' property.
		/// </summary>
		public const string AddUsings = "AddUsings";

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Name of the 'Source' property.
		/// </summary>
		public const string Source = "Source";

		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "CopyFromMethodAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodAttributeProvider"/> class.
		/// </summary>
		public CopyFromMethodAttributeProvider()
		{
		}

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
@$"using System;
using System.Diagnostics;
using Durian.Configuration;

#nullable enable

namespace {Namespace}
{{
	/// <summary>
	/// Specifies that implementation of a specific member should be copied from an external source.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	[Conditional(""DEBUG"")]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Determines which non-standard nodes from the target type to include in the generated source.
		/// </summary>
		public {CopyFromAdditionalNodesProvider.TypeName} {AdditionalNodes} {{ get; set; }}

		/// <summary>
		/// Specifies, which namespaces should be imported for the generated code.
		/// </summary>
		public string[]? {AddUsings} {{ get; set; }}

		/// <summary>
		/// Source of the copied implementation.
		/// </summary>
		public string? {Source} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""source"">Source of the copied implementation.</param>
		public {TypeName}(string source)
		{{
			{Source} = source;
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
