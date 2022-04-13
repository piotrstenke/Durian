// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains extension methods for the <see cref="IGeneratorServiceContainer"/> interface.
	/// </summary>
	public static class ServiceExtensions
	{
		/// <summary>
		/// Adds a service of the specified type.
		/// </summary>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <param name="services"><see cref="IGeneratorServiceContainer"/> to add the service to.</param>
		/// <param name="service">Service to add.</param>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> already registered.</exception>
		public static void AddService<T>(this IGeneratorServiceContainer services, T service)
		{
			services.AddService(() => service);
		}

		/// <summary>
		/// Adds a service of the specified type and <paramref name="name"/>.
		/// </summary>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <param name="services"><see cref="IGeneratorServiceContainer"/> to add the service to.</param>
		/// <param name="service">Service to add.</param>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="T"/> and <paramref name="name"/> already registered.</exception>
		public static void AddService<T>(this IGeneratorServiceContainer services, T service, string name)
		{
			services.AddService(() => service, name);
		}

		/// <summary>
		/// Adds a singleton service of the specified type.
		/// </summary>
		/// <param name="services"><see cref="IGeneratorServiceContainer"/> to add the service to.</param>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> already registered.</exception>
		public static void AddSingleton<T>(this IGeneratorServiceContainer services) where T : new()
		{
			T service = new();
			services.AddService(() => service);
		}

		/// <summary>
		/// Adds a transient service of the specified type.
		/// </summary>
		/// <param name="services"><see cref="IGeneratorServiceContainer"/> to add the service to.</param>
		/// <typeparam name="T">Type of service to add.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="T"/> already registered.</exception>
		public static void AddTransient<T>(this IGeneratorServiceContainer services) where T : new()
		{
			services.AddService(() => new T());
		}
	}
}
