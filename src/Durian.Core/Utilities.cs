using System;
using System.Collections.Immutable;

namespace Durian.Info;

internal static class Utilities
{
	public static bool CompareImmutableArrays<T>(ImmutableArray<T> first, ImmutableArray<T> second) where T : IEquatable<T>
	{
		int length = first.Length;

		if (length != second.Length)
		{
			return false;
		}

		for (int i = 0; i < length; i++)
		{
			if (!first[i].Equals(second[i]))
			{
				return false;
			}
		}

		return true;
	}

	public static int GetHashCodeOfImmutableArray<T>(ImmutableArray<T> array) where T : IEquatable<T>
	{
		if (array.Length == 0)
		{
			return 0;
		}

		int code = -726504116;

		foreach (T t in array)
		{
			code = (code * -1521134295) + t.GetHashCode();
		}

		return code;
	}

	public static string GetParsableIdentityName(string infoName)
	{
		string name;

		int index = infoName!.IndexOf("durian.", StringComparison.OrdinalIgnoreCase);

		if (index == -1)
		{
			name = infoName;
		}
		else
		{
			name = infoName.Substring(index);
		}

		return name.Trim();
	}
}
