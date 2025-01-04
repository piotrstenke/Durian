using System;
using System.Collections.Generic;

using static Durian.Analysis.GeneratorServiceContainer;

namespace Durian.Analysis
{
	/// <inheritdoc cref="IGeneratorServiceResolver"/>
	public sealed class GeneratorServiceResolver : IGeneratorServiceResolver
	{
		private readonly Dictionary<Type, object> _services;

		internal GeneratorServiceResolver(Dictionary<Type, object> services, List<Entry> scoped)
		{
			_services = new(services);

			foreach (Entry entry in scoped)
			{
				AddScopedEntry(entry);
			}
		}

		/// <inheritdoc/>
		public T GetService<T>() where T : class
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
		public T GetService<T>(string name) where T : class
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

		private void AddScopedEntry(Entry entry)
		{
			object? value = default;

			Func<object> del = () => value ??= (entry.Delegate as Func<object>)!.Invoke();

			if (entry.Name is null)
			{
				AddService(entry.Type, _services, del);
			}
			else
			{
				AddService(entry.Type, _services, del, entry.Name);
			}
		}
	}
}
