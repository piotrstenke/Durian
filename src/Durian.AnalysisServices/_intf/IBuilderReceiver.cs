namespace Durian.Analysis
{
	/// <summary>
	/// Provides mechanism for receiving data from a builder object.
	/// </summary>
	/// <typeparam name="TBuilder">Type of received builder.</typeparam>
	public interface IBuilderReceiver<in TBuilder>
	{
		/// <summary>
		/// Receives data from the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder">Builder to receive the data from.</param>
		void Receive(TBuilder builder);
	}
}
