// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Durian.Analysis
{
	/// <inheritdoc cref="IGeneratorServiceContainer"/>
	public sealed class GeneratorServiceContainer : IGeneratorServiceContainer
	{
		internal sealed class Entry
		{
			public Delegate Delegate { get; }
			public string? Name { get; }
			public Type Type { get; }

			public Entry(Type type, Delegate @delegate)
			{
				Type = type;
				Delegate = @delegate;
			}

			public Entry(Type type, Delegate @delegate, string? name) : this(type, @delegate)
			{
				Name = name;
			}
		}

		private readonly List<Entry> _scoped;
		private readonly Dictionary<Type, object> _services;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorServiceContainer"/> class.
		/// </summary>
		public GeneratorServiceContainer()
		{
			_services = new();
			_scoped = new();
		}

		/// <inheritdoc/>
		public void AddScoped<TService, TActual>() where TService : class where TActual : TService, new()
		{
			Func<TService> func = () => new TActual();

			AddService(func);
			_scoped.Add(new(typeof(TService), func));
		}

		/// <inheritdoc/>
		public void AddScoped<TService, TActual>(string name) where TService : class where TActual : TService, new()
		{
			Func<TService> func = () => new TActual();

			AddService(func, name);
			_scoped.Add(new(typeof(TService), func, name));
		}

		/// <inheritdoc/>
		public void AddService<TService>(Func<TService> serviceCreator) where TService : class
		{
			if (serviceCreator is null)
			{
				throw new ArgumentNullException(nameof(serviceCreator));
			}

			AddService(typeof(TService), _services, serviceCreator);
		}

		/// <inheritdoc/>
		public void AddService<TService>(Func<TService> serviceCreator, string name) where TService : class
		{
			if (serviceCreator is null)
			{
				throw new ArgumentNullException(nameof(serviceCreator));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Name cannot be null or empty", nameof(name));
			}

			AddService(typeof(TService), _services, serviceCreator, name);
		}

		/// <inheritdoc/>
		public void AddSingleton<TService, TActual>() where TService : class where TActual : TService, new()
		{
			TActual? actual = default;

			AddService<TService>(() => actual ??= new TActual());
		}

		/// <inheritdoc/>
		public void AddSingleton<TService, TActual>(string name) where TService : class where TActual : TService, new()
		{
			TActual? actual = default;

			AddService<TService>(() => actual ??= new TActual(), name);
		}

		/// <inheritdoc/>
		public void AddTransient<TService, TActual>() where TService : class where TActual : TService, new()
		{
			AddService<TService>(() => new TActual());
		}

		/// <inheritdoc/>
		public void AddTransient<TService, TActual>(string name) where TService : class where TActual : TService, new()
		{
			AddService<TService>(() => new TActual(), name);
		}

		/// <inheritdoc cref="IGeneratorServiceContainer.CreateResolver"/>
		public GeneratorServiceResolver CreateResolver()
		{
			return new(_services, _scoped);
		}

		IGeneratorServiceResolver IGeneratorServiceContainer.CreateResolver()
		{
			return CreateResolver();
		}

		internal static void AddService(Type type, Dictionary<Type, object> services, Delegate serviceCreator)
		{
			if (services.TryGetValue(type, out object value))
			{
				if (value is not List<Entry> entries || entries[0].Name is null)
				{
					throw new ArgumentException($"Service of type '{type}' already registered", nameof(serviceCreator));
				}

				entries.Insert(0, new Entry(type, serviceCreator));
			}
			else
			{
				services[type] = serviceCreator;
			}
		}

		internal static void AddService(Type type, Dictionary<Type, object> services, Delegate serviceCreator, string name)
		{
			List<Entry> list;

			if (services.TryGetValue(type, out object value))
			{
				if (value is List<Entry> entries)
				{
					if (entries.Exists(e => e.Name == name))
					{
						throw new ArgumentException($"Service of type '{type}' and name '{name}' already registered", nameof(name));
					}
				}
				else
				{
					entries = new()
					{
						new Entry(type, (Delegate)value)
					};

					services[type] = entries;
				}

				list = entries;
			}
			else
			{
				list = new();
				services[type] = list;
			}

			list.Add(new Entry(type, serviceCreator, name));
		}
	}
}
