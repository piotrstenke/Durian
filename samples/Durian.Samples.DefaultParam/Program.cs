namespace Durian.Samples.DefaultParam
{
	internal partial class Program
	{
		public delegate void D<T>(T value);

		private static void Main()
		{
			Logger<int>.Log(12);
		}

		public static T Method<[DefaultParam(typeof(string))]T>(T value)
		{
			return value;
		}
	}
}
