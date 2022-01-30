// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Samples.InterfaceTargets
{
	internal class Program
	{
		private static void Main()
		{
		}
	}

	// This interface can only be implemented by classes.
	[InterfaceTargets(Durian.InterfaceTargets.Class)]
	public interface IInterface
	{
	}

	// Success!
	public class Class : IInterface
	{
	}

	// Error!
	//public struct Struct : IInterface
	//{
	//}
}