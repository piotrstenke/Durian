using System;

namespace Durian.Samples.DefaultParam
{
	internal partial class Parent
	{
		private static void Main()
		{
		}

		public static void Method<[DefaultParam(typeof(string))]T>(T value) where T : class
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}
		}
	}
}
