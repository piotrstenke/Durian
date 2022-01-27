// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// Specifies that the marked <c>static readonly</c> or <c>const</c> <see cref="string"/> field will be automatically added to the provided <see cref="CSharpCompilation"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class AddToCompilationAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AddToCompilationAttribute"/> class.
		/// </summary>
		public AddToCompilationAttribute()
		{
		}

		/// <summary>
		/// Creates new <see cref="CSharpSyntaxTree"/>s from the marked <c>static readonly</c> or <c>const</c> <see cref="string"/> fields in the specified <paramref name="sourceType"/>
		/// and returns a new <see cref="CSharpCompilation"/> that contains them.
		/// </summary>
		/// <param name="sourceType"><see cref="Type"/> to collect the <see cref="string"/> fields from.</param>
		public static CSharpCompilation Collect(Type? sourceType)
		{
			return Collect(sourceType, RoslynUtilities.CreateCompilationWithAssemblies(sources: null, null));
		}

		/// <summary>
		/// Creates new <see cref="CSharpSyntaxTree"/>s from the marked <c>static readonly</c> or <c>const</c> <see cref="string"/> fields in the specified <paramref name="sourceType"/>
		/// and returns a new <see cref="CSharpCompilation"/> that contains them.
		/// </summary>
		/// <param name="sourceType"><see cref="Type"/> to collect the <see cref="string"/> fields from.</param>
		/// <param name="compilation">Original <see cref="CSharpCompilation"/> to be used as a base for the newly created <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation Collect(Type? sourceType, CSharpCompilation? compilation)
		{
			CSharpCompilation c;

			if (compilation is null)
			{
				c = RoslynUtilities.CreateCompilationWithAssemblies(sources: null, null);
			}
			else
			{
				c = compilation;
			}

			if (sourceType is null)
			{
				return c;
			}

			Type attributeType = typeof(AddToCompilationAttribute);
			Type stringType = typeof(string);

			return c.AddSyntaxTrees(sourceType.GetFields(
				BindingFlags.Static |
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy)
				.Where(f => (f.IsInitOnly || f.IsLiteral) && f.FieldType == stringType && IsDefined(f, attributeType))
				.Select(f => CSharpSyntaxTree.ParseText((string)f.GetValue(null)!))
				.ToArray());
		}
	}
}