// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains data that helps identify a generator execution pass in multi-threaded environment.
	/// </summary>
	public readonly struct GeneratorThreadHandle : IEquatable<GeneratorThreadHandle>
	{
		/// <summary>
		/// Id of target source generator.
		/// </summary>
		public Guid GeneratorId { get; }

		/// <summary>
		/// Determines whether the current <see cref="ThreadId"/> represents id of the main thread.
		/// </summary>
		public bool IsMainThread => ThreadId == AnalysisUtilities.MainThreadId;

		/// <summary>
		/// Id of parent thread.
		/// </summary>
		public int SourceThreadId { get; }

		/// <summary>
		/// Id of target thread.
		/// </summary>
		public int ThreadId { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorThreadHandle"/> structure.
		/// </summary>
		/// <param name="generatorId">Id of target source generator.</param>
		/// <param name="threadId">Id of target thread.</param>
		/// <param name="sourceThreadId">Id of parent thread.</param>
		public GeneratorThreadHandle(Guid generatorId, int threadId, int sourceThreadId)
		{
			GeneratorId = generatorId;
			ThreadId = threadId;
			SourceThreadId = sourceThreadId;
		}

		/// <inheritdoc/>
		public static bool operator !=(in GeneratorThreadHandle left, in GeneratorThreadHandle right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(in GeneratorThreadHandle left, in GeneratorThreadHandle right)
		{
			return left.Equals(right);
		}

		/// <inheritdoc cref="Deconstruct(out Guid, out int, out int, out bool)"/>
		public void Deconstruct(out Guid generatorId, out int threadId, out int sourceThreadId)
		{
			generatorId = GeneratorId;
			threadId = ThreadId;
			sourceThreadId = SourceThreadId;
		}

		/// <summary>
		/// Deconstructs the current object.
		/// </summary>
		/// <param name="generatorId">Id of target source generator.</param>
		/// <param name="threadId">Id of target thread.</param>
		/// <param name="sourceThreadId">Id of parent thread.</param>
		/// <param name="isMainThread">Determines whether the current <see cref="ThreadId"/> represents id of the main thread.</param>
		public void Deconstruct(out Guid generatorId, out int threadId, out int sourceThreadId, out bool isMainThread)
		{
			Deconstruct(out generatorId, out threadId, out sourceThreadId);
			isMainThread = IsMainThread;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is GeneratorThreadHandle other && Equals(other);
		}

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		public bool Equals(in GeneratorThreadHandle other)
		{
			return
				other.GeneratorId == GeneratorId &&
				other.ThreadId == ThreadId &&
				other.SourceThreadId == SourceThreadId;
		}

		bool IEquatable<GeneratorThreadHandle>.Equals(GeneratorThreadHandle other)
		{
			return Equals(in other);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = 288908264;
			hashCode = (hashCode * -1521134295) + GeneratorId.GetHashCode();
			hashCode = (hashCode * -1521134295) + ThreadId.GetHashCode();
			hashCode = (hashCode * -1521134295) + SourceThreadId.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{GeneratorId}, {ThreadId}-{SourceThreadId}";
		}
	}
}
