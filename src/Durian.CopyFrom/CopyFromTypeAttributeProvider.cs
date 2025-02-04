﻿namespace Durian.Analysis.CopyFrom;

/// <summary>
/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.CopyFromTypeAttribute</c> class.
/// </summary>
public sealed class CopyFromTypeAttributeProvider : SourceTextProvider
{
	/// <summary>
	/// Name of the 'AdditionalNodes' property.
	/// </summary>
	public const string AdditionalNodes = CopyFromMethodAttributeProvider.AdditionalNodes;

	/// <summary>
	/// Name of the 'AddUsings' property.
	/// </summary>
	public const string AddUsings = CopyFromMethodAttributeProvider.AddUsings;

	/// <summary>
	/// Full name of the provided type.
	/// </summary>
	public const string FullName = Namespace + "." + TypeName;

	/// <summary>
	/// Name of the 'HandleSpecialMembers' property.
	/// </summary>
	public const string HandleSpecialMembers = "HandleSpecialMembers";

	/// <summary>
	/// Namespace the provided type is located in.
	/// </summary>
	public const string Namespace = DurianStrings.MainNamespace;

	/// <summary>
	/// Name of the 'Order' property.
	/// </summary>
	public const string Order = "Order";

	/// <summary>
	/// Name of the 'PartialPart' property.
	/// </summary>
	public const string PartialPart = "PartialPart";

	/// <summary>
	/// Name of the 'Source' property.
	/// </summary>
	public const string Source = CopyFromMethodAttributeProvider.Source;

	/// <summary>
	/// Name of the 'SourceType' property.
	/// </summary>
	public const string SourceType = "SourceType";

	/// <summary>
	/// Name of the provided type.
	/// </summary>
	public const string TypeName = "CopyFromTypeAttribute";

	/// <summary>
	/// Initializes a new instance of the <see cref="CopyFromTypeAttributeProvider"/> class.
	/// </summary>
	public CopyFromTypeAttributeProvider()
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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
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
		/// Determines whether to automatically replace name of the target type in constructor, destructor and operator declarations. Defaults to <see langword=""true""/>.
		/// </summary>
		public bool {HandleSpecialMembers} {{ get; set; }} = true;

		/// <summary>
		/// Order in which multiple <see cref=""{TypeName}""/>s are applied.
		/// </summary>
		public int {Order} {{ get; set; }}

		/// <summary>
		/// Partial part of the source type to copy the implementation from.
		/// </summary>
		public string? {PartialPart} {{ get; set; }}

		/// <summary>
		/// Source of the copied implementation.
		/// </summary>
		public string? {Source} {{ get; }}

		/// <summary>
		/// Source type of the copied implementation.
		/// </summary>
		public Type? {SourceType} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""sourceType"">Source type of the copied implementation.</param>
		public {TypeName}(Type sourceType)
		{{
			{SourceType} = sourceType;
		}}

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
