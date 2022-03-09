// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.FriendClassAttribute</c> class.
	/// </summary>
	public sealed class FriendClassAttributeProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the 'AllowFriendChildren' property.
		/// </summary>
		public const string AllowFriendChildren = "AllowFriendChildren";

		/// <summary>
		/// Name of the 'FriendType' property.
		/// </summary>
		public const string FriendType = "FriendType";

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
		public const string TypeName = "FriendClassAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassAttributeProvider"/> class.
		/// </summary>
		public FriendClassAttributeProvider()
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
	/// Specifies a <see cref=""Type""/> that can use <see langword=""internal""/> members of the current <see cref=""Type""/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public sealed class {TypeName} : Attribute
	{{
		/// <summary>
		/// Determines whether <see langword=""internal""/> members of the current <see cref=""Type""/> can be accessed by <see cref=""Type""/>s that inherit the <see cref=""{FriendType}""/>. Defaults to <see langword=""false""/>.
		/// </summary>
		public bool {AllowFriendChildren} {{ get; set; }}

		/// <summary>
		/// Friend <see cref=""Type""/> of the current <see cref=""Type""/>.
		/// </summary>
		public Type {FriendType} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{TypeName}""/> class.
		/// </summary>
		/// <param name=""type"">Friend <see cref=""Type""/> of the current <see cref=""Type""/>.</param>
		public {TypeName}(Type type)
		{{
			{FriendType} = type;
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