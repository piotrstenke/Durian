// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.CopyFromIncludeNode</c> enum.
	/// </summary>
	public sealed class CopyFromAdditionalNodesProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the 'All' field.
		/// </summary>
		public const string All = "All";

		/// <summary>
		/// Name of the 'Constraints' field.
		/// </summary>
		public const string Attributes = "Attributes";

		/// <summary>
		/// Name of the 'BaseInterfaces' field.
		/// </summary>
		public const string BaseInterfaces = "BaseInterfaces";

		/// <summary>
		/// Name of the 'BaseType' field.
		/// </summary>
		public const string BaseType = "BaseType";

		/// <summary>
		/// Name of the 'Constraints' field.
		/// </summary>
		public const string Constraints = "Constraints";

		/// <summary>
		/// Name of the 'Default' field.
		/// </summary>
		public const string Default = "Default";

		/// <summary>
		/// Name of the 'Documentation' field.
		/// </summary>
		public const string Documentation = "Documentation";

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Name of the 'None' field.
		/// </summary>
		public const string None = "None";

		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "CopyFromAddionalNodes";

		/// <summary>
		/// Name of the 'Usings' field.
		/// </summary>
		public const string Usings = "Usings";

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
$@"using System;

namespace {Namespace}
{{
	/// <summary>
	/// Specifies which non-standard nodes should also be copied.
	/// </summary>
	[Flags]
	public enum {TypeName}
	{{
		/// <summary>
		/// No non-standard nodes are copied.
		/// </summary>
		{None} = 0,

		/// <summary>
		/// Specifies that all attribute lists of the target member should also be copied.
		/// </summary>
		{Attributes} = 1,

		/// <summary>
		/// Specifies that all generic constraints of the target member should also be copied.
		/// </summary>
		{Constraints} = 2,

		/// <summary>
		/// Specifies that the base type list of the target member should also be copied.
		/// </summary>
		{BaseType} = 4,

		/// <summary>
		/// Specifies that the base interface list of the target member should also be copied.
		/// </summary>
		{BaseInterfaces} = 8,

		/// <summary>
		/// Specifies that the documentation of the target member should also be copied.
		/// </summary>
		{Documentation} = 16,

		/// <summary>
		/// Specifies that all using directives in the file where target member is located should also be copied.
		/// </summary>
		{Usings} = 32,

		/// <summary>
		/// Specifies that all available non-standard nodes of the target member should also be copied.
		/// </summary>
		{All} = {Attributes} | {BaseType} | {BaseInterfaces} | {Constraints} | {Documentation} | {Usings},

		/// <summary>
		/// Specifies that the default configuration should be used.
		/// </summary>
		{Default} = {Usings}
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
