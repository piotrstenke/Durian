using Microsoft.CodeAnalysis;

namespace Durian.Generator.Logging
{
	/// <summary>
	/// Provides a method that creates a file name for a specified <see cref="ISymbol"/>.
	/// </summary>
	public interface IFileNameProvider
	{
		/// <summary>
		/// Returns a name of the log file for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the file name for.</param>
		string GetFileName(ISymbol symbol);

		/// <summary>
		/// A callback method that changes the state of the current <see cref="IFileNameProvider"/>.
		/// </summary>
		void Success();

		/// <summary>
		/// Resets the <see cref="IFileNameProvider"/> to its original state.
		/// </summary>
		void Reset();
	}
}