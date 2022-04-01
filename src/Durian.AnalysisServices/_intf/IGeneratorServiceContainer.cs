// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Container for services used during the current generator pass.
	/// </summary>
	public interface IGeneratorServiceContainer
	{
		/// <summary>
		/// Adds a service of the specified type.
		/// </summary>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> already registered.</exception>
		void AddService<T>() where T : new();

		/// <summary>
		/// Adds a service of the specified type and <paramref name="name"/>.
		/// </summary>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="T"/> and <paramref name="name"/> already registered.</exception>
		void AddService<T>(string name) where T : new();

		/// <summary>
		/// Adds a service created by calling the specified <paramref name="serviceCreator"/>.
		/// </summary>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <param name="serviceCreator">Function that creates a service to be used.</param>
		/// <exception cref="ArgumentNullException"><paramref name="serviceCreator"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> already registered.</exception>
		void AddService<T>(Func<T> serviceCreator);

		/// <summary>
		/// Adds a service with the given <paramref name="name"/> created by calling the specified <paramref name="serviceCreator"/>.
		/// </summary>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <param name="serviceCreator">Function that creates a service to be used.</param>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="serviceCreator"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="T"/> and <paramref name="name"/> already registered.</exception>
		void AddService<T>(Func<T> serviceCreator, string name);

		/// <summary>
		/// Returns a service of the specified type.
		/// </summary>
		/// <typeparam name="T">Type of service to return.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> could not be resolved. -or- Service without name not found.</exception>
		T GetService<T>();

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
		T GetService<T>(string name);
	}
}
