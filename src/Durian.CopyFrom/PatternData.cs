// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Encapsulates data of a Regex pattern specified in a <c>Durian.PatternAttribute</c>.
	/// </summary>
	public readonly struct PatternData : IEquatable<PatternData>
	{
		/// <summary>
		/// Regex pattern to use.
		/// </summary>
		public string Pattern { get; }

		/// <summary>
		/// Value to replace the matched values with.
		/// </summary>
		public string Replacement { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PatternData"/> struct.
		/// </summary>
		/// <param name="pattern">Regex pattern to use.</param>
		/// <param name="replacement">Value to replace the matched values with.</param>
		public PatternData(string pattern, string replacement)
		{
			Pattern = pattern;
			Replacement = replacement;
		}

		/// <inheritdoc/>
		public static bool operator !=(PatternData left, PatternData right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(PatternData left, PatternData right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Deconstructs the current object.
		/// </summary>
		/// <param name="pattern">Regex pattern to use.</param>
		/// <param name="replacement">Value to replace the matched values with.</param>
		public void Deconstruct(out string pattern, out string replacement)
		{
			pattern = Pattern;
			replacement = Replacement;
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			return
				obj is PatternData data &&
				Equals(data);
		}

		/// <inheritdoc/>
		public bool Equals(PatternData other)
		{
			return
				Pattern == other.Pattern &&
				Replacement == other.Replacement;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -1155513704;
			hashCode = (hashCode * -1521134295) + Pattern.GetHashCode();
			hashCode = (hashCode * -1521134295) + Replacement.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Pattern = \"{Pattern}\", Replacement = \"{Replacement}\"";
		}
	}
}