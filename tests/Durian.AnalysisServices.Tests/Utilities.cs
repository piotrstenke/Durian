// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Tests
{
	internal static class Utilities
	{
		public sealed class NonPredefinedTypeCollection : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				HashSet<SpecialType> predefined = new(GetPredefinedTypes());

				foreach (SpecialType type in GetSpecialTypes())
				{
					if (!predefined.Contains(type))
					{
						yield return new object[] { type };
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public sealed class PredefinedTypeCollection : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				foreach (SpecialType type in GetPredefinedTypes())
				{
					yield return new object[] { type };
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public static string OtherAttribute =>
@"[System.AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
class OtherAttribute : System.Attribute
{
	public OtherAttribute() { }
	public OtherAttribute(string name) { }
}";

		public static string TestAttribute =>
						@"[System.AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
class TestAttribute : System.Attribute
{
	public TestAttribute() { }
	public TestAttribute(string name) { }
}";

		public static string[] GetCSharpTypeKeywords()
		{
			return new string[]
{
			"bool",
			"char",
			"sbyte",
			"byte",
			"short",
			"ushort",
			"int",
			"uint",
			"long",
			"ulong",
			"nint",
			"nuint",
			"float",
			"double",
			"decimal",
			"string",
			"object",
			"void"
};
		}

		public static string[] GetDotNetTypeWithKeywords()
		{
			return new string[]
{
			"Boolean",
			"Char",
			"SByte",
			"Byte",
			"Int16",
			"UInt16",
			"Int32",
			"UInt32",
			"Int64",
			"UInt64",
			"IntPtr",
			"UIntPtr",
			"Single",
			"Double",
			"Decimal",
			"String",
			"Object",
			"Void"
};
		}

		public static SpecialType[] GetPredefinedTypes()
		{
			return new SpecialType[]
			{
				SpecialType.System_Boolean,
				SpecialType.System_Char,
				SpecialType.System_SByte,
				SpecialType.System_Byte,
				SpecialType.System_Int16,
				SpecialType.System_Int32,
				SpecialType.System_Int64,
				SpecialType.System_UInt16,
				SpecialType.System_UInt32,
				SpecialType.System_UInt64,
				SpecialType.System_Single,
				SpecialType.System_Double,
				SpecialType.System_Decimal,
				SpecialType.System_String,
				SpecialType.System_Void,
				SpecialType.System_Object
			};
		}

		public static SpecialType[] GetSpecialTypes()
		{
			Array array = Enum.GetValues(typeof(SpecialType));
			SpecialType[] output = new SpecialType[array.Length - 2];
			Array.Copy(array, 1, output, 0, array.Length - 2);
			return output;
		}
	}
}
