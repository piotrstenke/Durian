// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Durian.Analysis
{
	/// <inheritdoc cref="IGeneratorServiceContainer"/>
	public sealed class GeneratorServiceContainer : IGeneratorServiceContainer
	{
		private sealed class Entry
		{
			public string? Name { get; }
			public Delegate Delegate { get; }

			public Entry(Delegate @delegate)
			{
				Delegate = @delegate;
			}

			public Entry(Delegate @delegate, string name)
			{
				Delegate = @delegate;
				Name = name;
			}
		}

		private readonly Dictionary<Type, object> _services;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorServiceContainer"/> class.
		/// </summary>
		public GeneratorServiceContainer()
		{
			_services = new();
		}

		/// <inheritdoc/>
		public void AddService<T>() where T : new()
		{
			AddService(() => new T());
		}

		/// <inheritdoc/>
		public void AddService<T>(Func<T> serviceCreator)
		{
			if (serviceCreator is null)
			{
				throw new ArgumentNullException(nameof(serviceCreator));
			}

			Type type = typeof(T);

			if (_services.TryGetValue(type, out object value))
			{
				if (value is not List<Entry> entries || entries[0].Name is null)
				{
					throw new ArgumentException($"Service of type '{type}' already registered", nameof(serviceCreator));
				}

				entries.Insert(0, new Entry(serviceCreator));
			}
			else
			{
				_services[type] = serviceCreator;
			}
		}

		/// <inheritdoc/>
		public void AddService<T>(Func<T> serviceCreator, string name)
		{
			if (serviceCreator is null)
			{
				throw new ArgumentNullException(nameof(serviceCreator));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Name cannot be null or empty", nameof(name));
			}

			Type type = typeof(T);
			List<Entry> list;

			if (_services.TryGetValue(type, out object value))
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
					entries = new();
					entries.Add(new Entry((Func<T>)value));

					_services[type] = entries;
				}

				list = entries;
			}
			else
			{
				list = new();
				_services[type] = list;
			}

			list.Add(new Entry(serviceCreator, name));
		}

		/// <inheritdoc/>
		public void AddService<T>(string name) where T : new()
		{
			AddService(() => new T(), name);
		}

		/// <inheritdoc/>
		public T GetService<T>()
		{
			Type type = typeof(T);

			if (!_services.TryGetValue(type, out object value))
			{
				throw new ArgumentException($"Service of type '{type}' could not be resolved");
			}

			Func<T> func;

			if (value is List<Entry> entries)
			{
				Entry entry = entries[0];

				if (entry.Name is not null)
				{
					throw new ArgumentException("Service without name not found");
				}

				func = (Func<T>)entry.Delegate;
			}
			else
			{
				func = (Func<T>)value;
			}

			return func();
		}

		/// <inheritdoc/>
		public T GetService<T>(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Name cannot be null or empty", nameof(name));
			}

			Type type = typeof(T);

			if (!_services.TryGetValue(type, out object value))
			{
				throw new ArgumentException($"Service of type '{type}' and name '{name}' could not be resolved", nameof(name));
			}

			Func<T> func;

			if (value is List<Entry> entries)
			{
				Entry? entry = entries.Find(e => e.Name == name);

				if (entry is null)
				{
					throw new ArgumentException($"Service of type '{type}' and name '{name}' could not be resolved", nameof(name));
				}

				func = (Func<T>)entry.Delegate;
			}
			else
			{
				func = (Func<T>)value;
			}

			return func();
		}
	}
}
