// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;

namespace Durian.ConstExpr
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.ConstExprAttribute</c> class.
	/// </summary>
	public sealed class ConstExprAttributeProvider : SourceTextProvider
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
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "ConstExprAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstExprAttributeProvider"/> class.
		/// </summary>
		public ConstExprAttributeProvider()
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

namespace {Namespace}
{{
	/// <summary>
	/// Methods marked with this attribute can be executed at compile time.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/> class.
		/// </summary>
		public {TypeName}()
		{{
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
