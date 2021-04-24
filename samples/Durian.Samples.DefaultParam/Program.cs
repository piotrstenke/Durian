namespace Durian.Samples.DefaultParam
{
	internal partial class Program
	{
		public delegate void D<T>(T value);

		private static void Main()
		{

		}

		public static void Method<[Durian.DefaultParam(typeof(int))]U>(U value)
		{

		}
	}
}
