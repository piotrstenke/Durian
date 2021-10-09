// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CS0162 // Unreachable code detected

using System;
using System.Collections.Generic;
using System.Text;

namespace Durian.EnumServices
{
	[Flags]
	[EnumServices(EnumServices.All)]
	public enum FlagsEnum
	{
		None = 0,
		A = 1,
		B = 2,
		C = 4
	}

	// Generated

	public partial class EnumExtensions
	{
		public static int FastCompareTo(this FlagsEnum value, FlagsEnum other)
		{
			// Convert to underlaying type.
			int first = (int)value;
			int second = (int)other;

			return first.CompareTo(second);
		}

		public static bool FastEquals(this FlagsEnum value, FlagsEnum other)
		{
			return value == other;
		}

		public static int FastGetHashCode(this FlagsEnum value)
		{
			// Convert to underlaying type.
			int n = (int)value;

			return n.GetHashCode();
		}

		public static bool FastHasFlag(this FlagsEnum value, FlagsEnum flag)
		{
			return (value & flag) == flag;
		}

		public static string FastToString(this FlagsEnum value)
		{
			// If has defined value 0.
			if (value == FlagsEnum.None)
			{
				return "None";
			}

			List<string> flags = new(3);

			if (value.HasFlag(FlagsEnum.A))
			{
				flags.Add("A");
			}

			if (value.HasFlag(FlagsEnum.B))
			{
				flags.Add("B");
			}

			if (value.HasFlag(FlagsEnum.C))
			{
				flags.Add("C");
			}

			int length = flags.Count;

			if (length == 0)
			{
				return ((int)value).ToString();
			}

			if (length == 1)
			{
				return flags[0];
			}

			StringBuilder builder = new(length);
			builder.Append(flags[0]);

			for (int i = 1; i < length; i++)
			{
				builder
					.Append(',')
					.Append(' ')
					.Append(flags[i]);
			}

			return builder.ToString();
		}

		public static FlagsEnum[] GetFlags(this FlagsEnum value)
		{
			List<FlagsEnum> flags = new(3);

			if (value.HasFlag(FlagsEnum.A))
			{
				flags.Add(FlagsEnum.A);
			}

			if (value.HasFlag(FlagsEnum.B))
			{
				flags.Add(FlagsEnum.B);
			}

			if (value.HasFlag(FlagsEnum.C))
			{
				flags.Add(FlagsEnum.C);
			}

			return flags.ToArray();
		}

		public static bool IsDefined(this FlagsEnum value)
		{
			// If enum values are not power of 2 or some powers of 2 are missing.
			return value == FlagsEnum.None || value == FlagsEnum.A || value == FlagsEnum.B || value == FlagsEnum.C;

			// If enum values are next powers of 2.

			FlagsEnum maxValue = FlagsEnum.C;

			// If 0 is defined.
			return value == 0 || (((value & (value - 1)) == 0) && value < maxValue + 1);

			// If 0 is not defined
			return value != 0 && ((value & (value - 1)) == 0) && value < maxValue + 1;
		}
	}
}

#pragma warning restore CS0162 // Unreachable code detected
