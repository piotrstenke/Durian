// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Contains extension methods for syntax filtration.
	/// </summary>
	public static class FilterExtensions
	{
		/// <summary>
		/// Returns a reference to the child <see cref="ISyntaxValidatorContext"/>.
		/// </summary>
		/// <typeparam name="T">Type of child <see cref="ISyntaxValidatorContext"/>.</typeparam>
		/// <param name="context"><see cref="SyntaxValidatorContextWrapper{T}"/> to return the child <see cref="ISyntaxValidatorContext"/> of.</param>
		public static ref readonly T GetContext<T>(this in SyntaxValidatorContextWrapper<T> context) where T : struct, ISyntaxValidatorContext
		{
			return ref context._context;
		}
	}
}
