// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.Configuration.FriendClassConfigurationAttribute</c> class.
	/// </summary>
	public sealed class FriendClassConfigurationAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "FriendClassConfigurationAttribute";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.ConfigurationNamespace;

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'AllowsChildren' property.
		/// </summary>
		public const string AllowsChildren = "AllowsChildren";

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassAttributeProvider"/> class.
		/// </summary>
		public FriendClassConfigurationAttributeProvider()
		{
		}

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
		public override string GetText()
		{
			return
@$"using System;

namespace {Namespace}
{{
	/// <summary>
	/// Configures how friend classes of the target <see cref=""Type""/> are handled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Determines whether sub-classes of the current type should be treated like friend types. Defaults to <see langword=""false""/>.
		/// </summary>
		public bool {AllowsChildren} {{ get; set; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/>.
		/// </summary>
		public {TypeName}()
		{{
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