// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;

namespace Durian.ConstExpr
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.ConstExprSourceAttribute</c> class.
	/// </summary>
	public sealed class ConstExprSourceAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'Name' property.
		/// </summary>
		public const string Name = "Name";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Name of the 'Arguments' property.
		/// </summary>
		public const string Arguments = "Arguments";

		/// <summary>
		/// Name of the 'Source' property.
		/// </summary>
		public const string Source = "Source";

		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "ConstExprAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstExprSourceAttributeProvider"/> class.
		/// </summary>
		public ConstExprSourceAttributeProvider()
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

namespace {Namespace}
{{
	/// <summary>
	/// Methods marked with this attribute can be executed at compile time.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTarget.Struct, AllowMultiple = true, Inherited = false)]
	[Conditional(""DEBUG"")]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Arguments for the source method.
		/// </summary>
		public object[]? {Arguments} {{ get; }}

		/// <summary>
		/// Name of the generated method.
		/// </summary>
		public string? {Name} {{ get; set; }}

		/// <summary>
		/// Name of method to execute at compile time.
		/// </summary>
		public string {Source} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/> class.
		/// </summary>
		/// <param name=""source"">Name of method to execute at compile time.</param>
		/// <param name=""args"">Arguments for the source method.</param>
		public {TypeName}(string source, params object?[]? args)
		{{
			{Source} = source;
			{Arguments} = args;
		}}
	}}
}}";
		}

		/// <inheritdoc/>
		public override string GetTypeName()
		{
			return TypeName;
		}
	}
}
