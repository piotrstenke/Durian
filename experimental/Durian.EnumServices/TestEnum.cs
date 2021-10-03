#pragma warning disable CS0162 // Unreachable code detected

namespace Durian.EnumServices
{
	[EnumServices(EnumServices.All)]
	public enum TestEnum
	{
		A,
		B,
		C
	}

	// Generated

	public static partial class EnumExtensions
	{
		public static int FastCompareTo(this TestEnum value, TestEnum other)
		{
			// Convert to underlaying type.
			int first = (int)value;
			int second = (int)other;

			return first.CompareTo(second);
		}

		public static bool FastEquals(this TestEnum value, TestEnum other)
		{
			return value == other;
		}

		public static int FastGetHashCode(this TestEnum value)
		{
			// Convert to underlaying type.
			int n = (int)value;

			return n.GetHashCode();
		}

		public static string FastToString(this TestEnum value)
		{
			switch (value)
			{
				case TestEnum.A:
					return "A";

				case TestEnum.B:
					return "B";

				case TestEnum.C:
					return "C";

				default:
					return ((int)value).ToString();
			}
		}

		public static bool IsDefined(this TestEnum value)
		{
			// If enum values are not incremented by one.
			return value == TestEnum.A || value == TestEnum.B || value == TestEnum.C;

			// If enum values are incremented by one.
			return !(value < TestEnum.A || value > TestEnum.C);
		}
	}
}

#pragma warning restore CS0162 // Unreachable code detected
