using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Resolves services during a generator pass.
	/// </summary>
	public interface IGeneratorServiceResolver
	{
		/// <summary>
		/// Returns a service of the specified type.
		/// </summary>
		/// <typeparam name="T">Type of service to return.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> could not be resolved. -or- Service without name not found.</exception>
		T GetService<T>() where T : class;

		/// <summary>
		/// Returns a service of the specified type and <paramref name="name"/>.
		/// </summary>
		/// <typeparam name="T">Type of service to return.</typeparam>
		/// <param name="name">Name of service to return.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be <see langword="null"/> or empty. -or-
		/// Service with <paramref name="name"/> could not be resolved. -or-
		/// Service of type <typeparamref name="T"/> could not be resolved.
		/// </exception>
		T GetService<T>(string name) where T : class;
	}
}
