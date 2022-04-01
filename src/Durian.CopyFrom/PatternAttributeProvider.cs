// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.PatternAttribute</c> class.
	/// </summary>
	public sealed class PatternAttributeProvider : SourceTextProvider
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
		/// Name of the 'Replacement' property.
		/// </summary>
		public const string Replacement = "Replacement";

		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "PatternAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="PatternAttributeProvider"/> class.
		/// </summary>
		public PatternAttributeProvider()
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
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
	[Conditional(""DEBUG"")]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Order in which multiple <see cref=""{TypeName}""/>s are applied.
		/// </summary>
		public int {Order} {{ get; set; }}

		/// <summary>
		/// Regex pattern that is used when copying implementation from other source.
		/// </summary>
		public string? {Pattern} {{ get; }}

		/// <summary>
		/// Value to replace matched text with.
		/// </summary>
		public string? {Replacement} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		/// <param name=""pattern"">Regex pattern that is used when copying implementation from other source.</param>
		/// <param name=""replacement"">Value to replace matched text with.</param>
		public {TypeName}(string pattern, string replacement)
		{{
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
