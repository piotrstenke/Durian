namespace Durian.Samples.DefaultParam
{
	internal partial class Program
	{
		public delegate void D<T>(T value);

		private static void Main()
		{
			Logger<int>.Log(12);
		}

		public static T Method<T, U, [DefaultParam(typeof(string))]V>(T value) where T : class
		{
			return value;
		}
	}
}
