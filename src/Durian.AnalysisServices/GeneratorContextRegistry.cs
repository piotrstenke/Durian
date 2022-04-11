// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Reg = System.Collections.Concurrent.ConcurrentDictionary<int, Durian.Analysis.IGeneratorPassContext>;

namespace Durian.Analysis
{
	/// <summary>
	/// Provides easy access to <see cref="IGeneratorPassContext"/>s between multiple threads.
	/// </summary>
	public static class GeneratorContextRegistry
	{
		private static readonly ConcurrentDictionary<Guid, object> _generators = new();

		/// <summary>
		/// Attempts to add the specified <paramref name="context"/> associated with a generator with the specified <paramref name="generatorId"/> to the registry.
		/// </summary>
		/// <param name="generatorId">Id of generator the <paramref name="context"/> is associated with.</param>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to add to the registry.</param>
		public static bool AddContext(Guid generatorId, IGeneratorPassContext context)
		{
			return AddContext(generatorId, AnalysisUtilities.MainThreadId, context);
		}

		/// <summary>
		/// Attempts to add the specified <paramref name="context"/> associated with a generator with the specified <paramref name="generatorId"/> and a thread with the given <paramref name="threadId"/> to the registry
		/// </summary>
		/// <param name="generatorId">Id of generator the <paramref name="context"/> is associated with.</param>
		/// <param name="threadId">Id of thread the <paramref name="context"/> is associated with.</param>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to add to the registry.</param>
		public static bool AddContext(Guid generatorId, int threadId, IGeneratorPassContext context)
		{
			if (!_generators.TryGetValue(generatorId, out object? value))
			{
				if (threadId == AnalysisUtilities.MainThreadId)
				{
					return _generators.TryAdd(generatorId, context);
				}

				Reg reg = new();
				reg.TryAdd(threadId, context);

				return _generators.TryAdd(generatorId, reg);
			}

			if (value is IGeneratorPassContext c)
			{
				if (threadId == AnalysisUtilities.MainThreadId)
				{
					return false;
				}

				Reg reg = new();
				reg.TryAdd(AnalysisUtilities.MainThreadId, c);
				reg.TryAdd(threadId, context);

				return _generators.TryUpdate(generatorId, reg, c);
			}

			Reg data = (Reg)value;
			return data.TryAdd(threadId, context);
		}

		/// <summary>
		/// Determines whether any <see cref="IGeneratorPassContext"/> present in the registry is associated with a generator with the specified <paramref name="generatorId"/>.
		/// </summary>
		/// <param name="generatorId">Id of the target generator.</param>
		public static bool ContainsAnyContext(Guid generatorId)
		{
			return _generators.ContainsKey(generatorId);
		}

		/// <summary>
		/// Determines whether any <see cref="IGeneratorPassContext"/> present in the registry is associated with a thread with the specified <paramref name="threadId"/>.
		/// </summary>
		/// <param name="threadId">Id of the target thread.</param>
		public static bool ContainsAnyContext(int threadId)
		{
			bool isMainThread = threadId == AnalysisUtilities.MainThreadId;

			return _generators.Values.Any(value =>
			{
				if (value is IGeneratorPassContext)
				{
					return isMainThread;
				}

				Reg data = (Reg)value;
				return data.ContainsKey(threadId);
			});
		}

		/// <summary>
		/// Determines whether any <see cref="IGeneratorPassContext"/> present in the registry is associated with a generator with the specified <paramref name="generatorId"/> and id of the main thread.
		/// </summary>
		/// <param name="generatorId">Id of target generator.</param>
		public static bool ContainsContext(Guid generatorId)
		{
			return ContainsContext(generatorId, AnalysisUtilities.MainThreadId);
		}

		/// <summary>
		/// Determines whether any <see cref="IGeneratorPassContext"/> present in the registry is associated with a generator with the specified <paramref name="generatorId"/> and a thread with the specified <paramref name="threadId"/>.
		/// </summary>
		/// <param name="generatorId">Id of the target generator.</param>
		/// <param name="threadId">Id of the target thread.</param>
		public static bool ContainsContext(Guid generatorId, int threadId)
		{
			if (!_generators.TryGetValue(generatorId, out object? value))
			{
				return false;
			}

			if (value is IGeneratorPassContext)
			{
				return threadId == AnalysisUtilities.MainThreadId;
			}

			Reg data = (Reg)value;
			return data.ContainsKey(threadId);
		}

		/// <summary>
		/// Determines whether any <see cref="IGeneratorPassContext"/> present in the registry is associated with the specified <paramref name="handle"/>.
		/// </summary>
		/// <param name="handle">Target <see cref="GeneratorThreadHandle"/>.</param>
		public static bool ContainsContext(in GeneratorThreadHandle handle)
		{
			if (!_generators.TryGetValue(handle.GeneratorId, out object? value))
			{
				return false;
			}

			if (value is IGeneratorPassContext)
			{
				return handle.IsMainThread;
			}

			Reg data = (Reg)value;
			return data.ContainsKey(handle.ThreadId) || data.ContainsKey(handle.SourceThreadId);
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext"/> associated with the specified <paramref name="handle"/>.
		/// </summary>
		/// <param name="handle"><see cref="GeneratorPassContext"/> to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <exception cref="ArgumentException">Context not found for the specified <paramref name="handle"/>.</exception>
		public static IGeneratorPassContext GetContext(in GeneratorThreadHandle handle)
		{
			if (!TryGetContext(handle, out IGeneratorPassContext? context))
			{
				throw new ArgumentException($"Context not found for handle '{handle}'", nameof(handle));
			}

			return context;
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and id of the main thread.
		/// </summary>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <exception cref="ArgumentException">Context not found for generator with the specified <paramref name="generatorId"/>.</exception>
		public static IGeneratorPassContext GetContext(Guid generatorId)
		{
			if (!TryGetContext(generatorId, out IGeneratorPassContext? context))
			{
				throw new ArgumentException($"Context not found for generator with id '{generatorId}'", nameof(generatorId));
			}

			return context;
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and <paramref name="threadId"/>.
		/// </summary>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="threadId">Id of thread to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <exception cref="ArgumentException">Context not found for generator with the specified <paramref name="generatorId"/>. -or- Context not found for thread with the specified <paramref name="threadId"/>.</exception>
		public static IGeneratorPassContext GetContext(Guid generatorId, int threadId)
		{
			if (!TryGetContext(generatorId, threadId, out IGeneratorPassContext? context))
			{
				throw new ArgumentException($"Context not found for generator with id '{generatorId}' and thread with id '{threadId}'", nameof(generatorId));
			}

			return context;
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext"/> associated with the specified <paramref name="handle"/>.
		/// </summary>
		/// <typeparam name="TContext">Type of the registered <see cref="IGeneratorPassContext"/>.</typeparam>
		/// <param name="handle"><see cref="GeneratorPassContext"/> to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <exception cref="ArgumentException">Context not found for the specified <paramref name="handle"/>.</exception>
		/// <exception cref="InvalidOperationException">Registered context is not of type <typeparamref name="TContext"/>.</exception>
		public static TContext GetContext<TContext>(in GeneratorThreadHandle handle) where TContext : class, IGeneratorPassContext
		{
			if (GetContext(handle) is not TContext context)
			{
				throw Exc_WrongContextType(typeof(TContext));
			}

			return context;
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and id of the main thread.
		/// </summary>
		/// <typeparam name="TContext">Type of the registered <see cref="IGeneratorPassContext"/>.</typeparam>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <exception cref="ArgumentException">Context not found for generator with the specified <paramref name="generatorId"/>.</exception>
		/// <exception cref="InvalidOperationException">Registered context is not of type <typeparamref name="TContext"/>.</exception>
		public static TContext GetContext<TContext>(Guid generatorId) where TContext : class, IGeneratorPassContext
		{
			if (GetContext(generatorId) is not TContext context)
			{
				throw Exc_WrongContextType(typeof(TContext));
			}

			return context;
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and <paramref name="threadId"/>.
		/// </summary>
		/// <typeparam name="TContext">Type of the registered <see cref="IGeneratorPassContext"/>.</typeparam>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="threadId">Id of thread to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <exception cref="ArgumentException">Context not found for generator with the specified <paramref name="generatorId"/>. -or- Context not found for thread with the specified <paramref name="threadId"/>.</exception>
		/// <exception cref="InvalidOperationException">Registered context is not of type <typeparamref name="TContext"/>.</exception>
		public static TContext GetContext<TContext>(Guid generatorId, int threadId) where TContext : class, IGeneratorPassContext
		{
			if (GetContext(generatorId, threadId) is not TContext context)
			{
				throw Exc_WrongContextType(typeof(TContext));
			}

			return context;
		}

		/// <summary>
		/// Removes all registered <see cref="IGeneratorPassContext"/>.
		/// </summary>
		public static void RemoveAllContexts()
		{
			_generators.Clear();
		}

		/// <summary>
		/// Removes all <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/>.
		/// </summary>
		/// <param name="generatorId">Id of generator to remove all the <see cref="IGeneratorPassContext"/>s associated with.</param>
		public static void RemoveAllContexts(Guid generatorId)
		{
			_generators.TryRemove(generatorId, out _);
		}

		/// <summary>
		/// Removes all <see cref="IGeneratorPassContext"/> associated with a thread with the specified <paramref name="threadId"/>.
		/// </summary>
		/// <param name="threadId">Id of thread to remove all the <see cref="IGeneratorPassContext"/>s associated with.</param>
		public static void RemoveAllContexts(int threadId)
		{
			bool isMainThread = threadId == AnalysisUtilities.MainThreadId;

			List<Guid> toRemove = new(_generators.Count);
			List<(Guid generatorId, Reg data, IGeneratorPassContext context)> toReplace = new(_generators.Count);

			foreach (KeyValuePair<Guid, object> generator in _generators)
			{
				if (generator.Value is IGeneratorPassContext)
				{
					if (isMainThread)
					{
						toRemove.Add(generator.Key);
					}

					continue;
				}

				Reg data = (Reg)generator.Value;

				if (data.TryRemove(threadId, out _))
				{
					if (data.IsEmpty)
					{
						toRemove.Add(generator.Key);
					}
					else if (data.Count == 1 && data.TryGetValue(AnalysisUtilities.MainThreadId, out IGeneratorPassContext? pass))
					{
						toReplace.Add((generator.Key, data, pass));
					}
				}
			}

			foreach (Guid generatorId in toRemove)
			{
				_generators.TryRemove(generatorId, out _);
			}

			foreach ((Guid generatorId, Reg data, IGeneratorPassContext context) in toReplace)
			{
				AddOrReplace(generatorId, data, context);
			}
		}

		/// <inheritdoc cref="RemoveContext(Guid, out IGeneratorPassContext?)"/>
		public static bool RemoveContext(Guid generatorId)
		{
			return RemoveContext(generatorId, out _);
		}

		/// <summary>
		/// Attempts to remove the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and id of the main thread.
		/// </summary>
		/// <param name="generatorId">Id of generator to remove the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Removed <see cref="IGeneratorPassContext"/>.</param>
		public static bool RemoveContext(Guid generatorId, [NotNullWhen(true)] out IGeneratorPassContext? context)
		{
			return RemoveContext(generatorId, AnalysisUtilities.MainThreadId, out context);
		}

		/// <inheritdoc cref="RemoveContext(Guid, int, out IGeneratorPassContext?)"/>
		public static bool RemoveContext(Guid generatorId, int threadId)
		{
			return RemoveContext(generatorId, threadId, out _);
		}

		/// <summary>
		/// Attempts to remove the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and <paramref name="threadId"/>.
		/// </summary>
		/// <param name="generatorId">Id of generator to remove the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="threadId">Id of thread to remove the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Removed <see cref="IGeneratorPassContext"/>.</param>
		public static bool RemoveContext(Guid generatorId, int threadId, [NotNullWhen(true)] out IGeneratorPassContext? context)
		{
			if (!_generators.TryGetValue(generatorId, out object? value))
			{
				context = default;
				return false;
			}

			if (value is IGeneratorPassContext)
			{
				if (_generators.TryRemove(generatorId, out value))
				{
					if (value is IGeneratorPassContext pass)
					{
						context = pass;
						return true;
					}
				}
				else
				{
					context = default;
					return false;
				}
			}

			Reg data = (Reg)value;

			if (data.TryRemove(threadId, out context))
			{
				if (data.IsEmpty)
				{
					_generators.TryRemove(generatorId, out _);
				}
				else if (data.Count == 1 && data.TryGetValue(AnalysisUtilities.MainThreadId, out IGeneratorPassContext? pass))
				{
					AddOrReplace(generatorId, data, pass);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to return the <see cref="IGeneratorPassContext"/> associated with the specified <paramref name="handle"/>.
		/// </summary>
		/// <param name="handle"><see cref="GeneratorPassContext"/> to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Returned <see cref="IGeneratorPassContext"/>.</param>
		public static bool TryGetContext(in GeneratorThreadHandle handle, [NotNullWhen(true)] out IGeneratorPassContext? context)
		{
			ReadOnlySpan<int> span = stackalloc int[3] { handle.ThreadId, handle.SourceThreadId, AnalysisUtilities.MainThreadId };
			return TryGetContext_Internal(handle.GeneratorId, span, out context);
		}

		/// <summary>
		/// Attempts to return the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and id of the main thread.
		/// </summary>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Returned <see cref="IGeneratorPassContext"/>.</param>
		public static bool TryGetContext(Guid generatorId, [NotNullWhen(true)] out IGeneratorPassContext? context)
		{
			return TryGetContext(generatorId, AnalysisUtilities.MainThreadId, out context);
		}

		/// <summary>
		/// Attempts to return the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and <paramref name="threadId"/>.
		/// </summary>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="threadId">Id of thread to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Returned <see cref="IGeneratorPassContext"/>.</param>
		public static bool TryGetContext(Guid generatorId, int threadId, [NotNullWhen(true)] out IGeneratorPassContext? context)
		{
			ReadOnlySpan<int> span = stackalloc int[1] { threadId };
			return TryGetContext_Internal(generatorId, span, out context);
		}

		/// <summary>
		/// Attempts to return the <see cref="IGeneratorPassContext"/> associated with the specified <paramref name="handle"/>.
		/// </summary>
		/// <typeparam name="TContext">Type of the registered <see cref="IGeneratorPassContext"/>.</typeparam>
		/// <param name="handle"><see cref="GeneratorPassContext"/> to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Returned <see cref="IGeneratorPassContext"/>.</param>
		public static bool TryGetContext<TContext>(in GeneratorThreadHandle handle, [NotNullWhen(true)] out TContext? context) where TContext : class, IGeneratorPassContext
		{
			ReadOnlySpan<int> span = stackalloc int[3] { handle.ThreadId, handle.SourceThreadId, AnalysisUtilities.MainThreadId };

			if (TryGetContext_Internal(handle.GeneratorId, span, out IGeneratorPassContext? c))
			{
				context = c as TContext;
				return context is not null;
			}

			context = default;
			return false;
		}

		/// <summary>
		/// Attempts to return the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and id of the main thread.
		/// </summary>
		/// <typeparam name="TContext">Type of the registered <see cref="IGeneratorPassContext"/>.</typeparam>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Returned <see cref="IGeneratorPassContext"/>.</param>
		public static bool TryGetContext<TContext>(Guid generatorId, [NotNullWhen(true)] out TContext? context) where TContext : class, IGeneratorPassContext
		{
			return TryGetContext(generatorId, AnalysisUtilities.MainThreadId, out context);
		}

		/// <summary>
		/// Attempts to return the <see cref="IGeneratorPassContext"/> associated with a generator with the specified <paramref name="generatorId"/> and <paramref name="threadId"/>.
		/// </summary>
		/// <typeparam name="TContext">Type of the registered <see cref="IGeneratorPassContext"/>.</typeparam>
		/// <param name="generatorId">Id of generator to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="threadId">Id of thread to get the <see cref="IGeneratorPassContext"/> associated with.</param>
		/// <param name="context">Returned <see cref="IGeneratorPassContext"/>.</param>
		public static bool TryGetContext<TContext>(Guid generatorId, int threadId, [NotNullWhen(true)] out TContext? context) where TContext : class, IGeneratorPassContext
		{
			ReadOnlySpan<int> span = stackalloc int[1] { threadId };

			if (TryGetContext_Internal(generatorId, span, out IGeneratorPassContext? c))
			{
				context = c as TContext;
				return context is not null;
			}

			context = default;
			return false;
		}

		private static void AddOrReplace(Guid generatorId, Reg data, IGeneratorPassContext context)
		{
			_generators.AddOrUpdate(generatorId, context, (_, value) => ReferenceEquals(value, data) ? context : value);
		}

		private static bool TryGetContext_Internal(Guid generatorId, ReadOnlySpan<int> threads, [NotNullWhen(true)] out IGeneratorPassContext? context)
		{
			if (!_generators.TryGetValue(generatorId, out object? value))
			{
				context = default;
				return false;
			}

			if (value is IGeneratorPassContext pass)
			{
				context = pass;
				return true;
			}

			Reg data = (Reg)value;

			foreach (int threadId in threads)
			{
				if (data.TryGetValue(threadId, out context))
				{
					return true;
				}
			}

			context = default;
			return false;
		}

		private static InvalidOperationException Exc_WrongContextType(Type type)
		{
			return new InvalidOperationException($"Registered context is not of type '{type}'");
		}
	}
}
