// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Samples.FriendClass
{
	internal class Friend
	{
		public static string Test()
		{
			// Success!
			return Target.Name;
		}
	}

	internal class Other
	{
		//public static string Test()
		//{
		//	// Error!
		//	return Target.Name;
		//}
	}

	internal class Program
	{
		private static void Main()
		{
		}
	}

	// Only 'Friend' can access this type's internal members.
	[FriendClass(typeof(Friend))]
	internal class Target
	{
		internal static string Name => nameof(Target);
	}
}