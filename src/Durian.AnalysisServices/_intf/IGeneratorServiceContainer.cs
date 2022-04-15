// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies services that will be used during execution of the generator.
	/// </summary>
	public interface IGeneratorServiceContainer
	{
		/// <summary>
		/// Adds a service of the specified type. A new service instance is created per each generator pass.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <typeparam name="TActual">Type of service to add.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="TService"/> already registered.</exception>
		void AddScoped<TService, TActual>() where TService : class where TActual : TService, new();

		/// <summary>
		/// Adds a service of the specified type and <paramref name="name"/>. A new service instance is created per each generator pass.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <typeparam name="TActual">Type of service to add.</typeparam>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="TService"/> and <paramref name="name"/> already registered.</exception>
		void AddScoped<TService, TActual>(string name) where TService : class where TActual : TService, new();

		/// <summary>
		/// Adds a service of the specified type. A service instance is shared between each generator pass.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <typeparam name="TActual">Type of service to add.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="TService"/> already registered.</exception>
		void AddSingleton<TService, TActual>() where TService : class where TActual : TService, new();

		/// <summary>
		/// Adds a service of the specified type and <paramref name="name"/>. A service instance is shared between each generator pass.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <typeparam name="TActual">Type of service to add.</typeparam>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="TService"/> and <paramref name="name"/> already registered.</exception>
		void AddSingleton<TService, TActual>(string name) where TService : class where TActual : TService, new();

		/// <summary>
		/// Adds a service of the specified type. A new service instance is created with every call to this method.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <typeparam name="TActual">Type of service to add.</typeparam>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="TService"/> already registered.</exception>
		void AddTransient<TService, TActual>() where TService : class where TActual : TService, new();

		/// <summary>
		/// Adds a service of the specified type and <paramref name="name"/>. A new service instance is created with every call to this method.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <typeparam name="TActual">Type of service to add.</typeparam>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="TService"/> and <paramref name="name"/> already registered.</exception>
		void AddTransient<TService, TActual>(string name) where TService : class where TActual : TService, new();

		/// <summary>
		/// Adds a service created by calling the specified <paramref name="serviceCreator"/>.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <param name="serviceCreator">Function that creates a service to be used.</param>
		/// <exception cref="ArgumentNullException"><paramref name="serviceCreator"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Service of type <typeparamref name="TService"/> already registered.</exception>
		void AddService<TService>(Func<TService> serviceCreator) where TService : class;

		/// <summary>
		/// Adds a service with the given <paramref name="name"/> created by calling the specified <paramref name="serviceCreator"/>.
		/// </summary>
		/// <typeparam name="TService">Type of service to register.</typeparam>
		/// <param name="serviceCreator">Function that creates a service to be used.</param>
		/// <param name="name">Name of service to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="serviceCreator"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or empty. -or- Service of type <typeparamref name="TService"/> and <paramref name="name"/> already registered.</exception>
		void AddService<TService>(Func<TService> serviceCreator, string name) where TService : class;

		/// <summary>
		/// Creates a new <see cref="IGeneratorServiceResolver"/> with all services registered in the current container.
		/// </summary>
		IGeneratorServiceResolver CreateResolver();
	}
}
