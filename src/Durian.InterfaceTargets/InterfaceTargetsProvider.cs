// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.InterfaceTargets</c> enum.
	/// </summary>
	public sealed class InterfaceTargetsProvider : SourceTextProvider
	{
		/// <summary>
		/// Name of the provided type.
		/// </summary>
		public const string TypeName = "InterfaceTargets";

		/// <summary>
		/// Namespace the provided type is located in.
		/// </summary>
		public const string Namespace = DurianStrings.MainNamespace;

		/// <summary>
		/// Full name of the provided type.
		/// </summary>
		public const string FullName = Namespace + "." + TypeName;

		/// <summary>
		/// Name of the 'None' field.
		/// </summary>
		public const string None = nameof(IntfTargets.None);

		/// <summary>
		/// Name of the 'ReflectionOnly' field.
		/// </summary>
		public const string ReflectionOnly = nameof(IntfTargets.ReflectionOnly);

		/// <summary>
		/// Name of the 'Class' field.
		/// </summary>
		public const string Class = nameof(IntfTargets.Class);

		/// <summary>
		/// Name of the 'RecordClass' field.
		/// </summary>
		public const string RecordClass = nameof(IntfTargets.RecordClass);

		/// <summary>
		/// Name of the 'Interface' field.
		/// </summary>
		public const string Interface = nameof(IntfTargets.Interface);

		/// <summary>
		/// Name of the 'Struct' field.
		/// </summary>
		public const string Struct = nameof(IntfTargets.Struct);

		/// <summary>
		/// Name of the 'RecordStruct' field.
		/// </summary>
		public const string RecordStruct = nameof(IntfTargets.RecordStruct);

		/// <summary>
		/// Name of the 'All' field.
		/// </summary>
		public const string All = nameof(IntfTargets.All);

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsProvider"/> class.
		/// </summary>
		public InterfaceTargetsProvider()
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
$@"using System;

namespace {Namespace}
{{
	/// <summary>
	/// Specifies possible targets of an <see langword=""interface""/>.
	/// </summary>
	[Flags]
	public enum {TypeName}
	{{
		/// <summary>
		/// Interface cannot be implemented in code, only through reflection.
		/// </summary>
		{ReflectionOnly} = 0,

		/// <summary>
		/// Interface cannot be implemented in code. This value is the same as <see cref=""{ReflectionOnly}""/>.
		/// </summary>
		{None} = {ReflectionOnly},

		/// <summary>
		/// Interface can be implemented by normal C# classes.
		/// </summary>
		{Class} = 1,

		/// <summary>
		/// Interface can be implemented by record classes.
		/// </summary>
		{RecordClass} = 2,

		/// <summary>
		/// Interface can be a base for other interface.
		/// </summary>
		{Interface} = 4,

		/// <summary>
		/// Interface can be implemented by normal C# structs.
		/// </summary>
		{Struct} = 8,

		/// <summary>
		/// Interface can be implemented by record structs.
		/// </summary>
		{RecordStruct} = 16,

		/// <summary>
		/// Interface can be implemented by all valid member kinds.
		/// </summary>
		{All} = {Class} | {RecordClass} | {Struct} | {RecordStruct} | {Interface}
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