﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.CopyFromTypeAttribute</c> class.
	/// </summary>
	public sealed class CopyFromTypeAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Name of the 'Order' property.
		/// </summary>
		public const string Order = CopyFromMethodAttributeProvider.Order;

		/// <summary>
		/// Name of the 'Pattern' property.
		/// </summary>
		public const string Pattern = CopyFromMethodAttributeProvider.Pattern;

		/// <summary>
		/// Name of the 'Replacement' property.
		/// </summary>
		public const string Replacement = CopyFromMethodAttributeProvider.Replacement;

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
		/// Regex pattern that is used when copying implementation from other source.
		/// </summary>
		public string? {Pattern} {{ get; }}

		/// <summary>
		/// Value to replace matched text with.
		/// </summary>
		public string? {Replacement} {{ get; }}

		/// <summary>
		/// Source of the copied implementation.
		/// </summary>
		public string? {Source} {{ get; }}

		/// <summary>
		/// Source type of the copied implementation.
		/// </summary>
		public Type? {SourceType} {{ get; }}

		/// <summary>
		/// Order in which multiple <see cref=""{TypeName}""/>s are applied.
		/// </summary>
		public int {Order} {{ get; set; }}

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
		/// <param name=""sourceType"">Source type of the copied implementation.</param>
		/// <param name=""pattern"">Regex pattern that is used when copying implementation from other source.</param>
		/// <param name=""replacement"">Value to replace matched text with.</param>
		public {TypeName}(Type sourceType, string? pattern, string? replacement)
		{{
			{SourceType} = sourceType;
			{Pattern} = pattern;
			{Replacement} = replacement;
		}}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""source"">Source of the copied implementation.</param>
		public {TypeName}(string source)
		{{
			{Source} = source;
		}}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""source"">Source of the copied implementation.</param>
		/// <param name=""pattern"">Regex pattern that is used when copying implementation from other source.</param>
		/// <param name=""replacement"">Value to replace matched text with.</param>
		public {TypeName}(string source, string? pattern, string? replacement)
		{{
			{Source} = source;
			{Pattern} = pattern;
			{Replacement} = replacement;
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