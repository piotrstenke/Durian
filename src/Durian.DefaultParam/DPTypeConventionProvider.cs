// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.Configuration.DPTypeConvention</c> enum.
	/// </summary>
	public sealed class DPTypeConventionProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "DPTypeConvention";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.ConfigurationNamespace;

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'Default' field.
		/// </summary>
		public const string Default = nameof(TypeConvention.Default);

		/// <summary>
		/// Name of the 'Inherit' field.
		/// </summary>
		public const string Inherit = nameof(TypeConvention.Inherit);

		/// <summary>
		/// Name of the 'Copy' field.
		/// </summary>
		public const string Copy = nameof(TypeConvention.Copy);

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
		public override string GetTypeName()
		{
			return TypeName;
		}

		/// <inheritdoc/>
		public override string GetText()
		{
			return
$@"namespace {Namespace}
{{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> type is generated.
	/// </summary>
	public enum {TypeName}
	{{
		/// <summary>
		/// Uses default convention, which is <see cref=""{Inherit}""/>.
		/// </summary>
		{Default} = {Inherit},

		/// <summary>
		/// Inherits from the type.
		/// </summary>
		{Inherit} = 0,

		/// <summary>
		/// Copies contents of the type.
		/// </summary>
		{Copy} = 1
	}}
}}";
		}
	}
}