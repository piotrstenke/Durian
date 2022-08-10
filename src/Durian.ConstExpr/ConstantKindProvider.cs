// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;

namespace Durian.ConstExpr
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.ConstantKind</c> enum.
	/// </summary>
	public sealed class ConstantKindProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the 'Constant' field.
		/// </summary>
		public const string Constant = "Constant";

		/// <summary>
		/// Name of the 'Field' field.
		/// </summary>
		public const string Field = "Field";

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'Method' field.
		/// </summary>
		public const string Method = "Method";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Name of the 'Property' field.
		/// </summary>
		public const string Property = "Property";

		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "ConstantKind";

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantKindProvider"/> class.
		/// </summary>
		public ConstantKindProvider()
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
@$"namespace {Namespace}
{{
	public enum {TypeName}
	{{
		{Constant},
		{Field},
		{Property},
		{Method}
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
