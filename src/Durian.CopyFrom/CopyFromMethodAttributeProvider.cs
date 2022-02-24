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
		public const string Order = "Order";

		/// <summary>
		/// Name of the 'Pattern' property.
		/// </summary>
		public const string Pattern = "Pattern";

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
		/// Regex pattern that is used when copying implementation from other source.
		/// </summary>
		public string? {Pattern} {{ get; }}

		/// <summary>
		/// Source of the copied implementation.
		/// </summary>
		public string? {Source} {{ get; }}

		/// <summary>
		/// Order in which multiple <see cref=""{TypeName}""/>s are applied.
		/// </summary>
		public int {Order} {{ get; set; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""source"">Source of the copied implementation.</param>
		public {TypeName}(string source) : this(source, null)
		{{
		}}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""source"">Source of the copied implementation.</param>
		/// <param name=""pattern"">Regex pattern that is used when copying implementation from other source.</param>
		public {TypeName}(string source, string? pattern)
		{{
			{Source} = source;
			{Pattern} = pattern;
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