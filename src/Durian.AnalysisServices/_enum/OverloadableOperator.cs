// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all overloadable operators.
	/// </summary>
	public enum OverloadableOperator
	{
		/// <summary>
		/// Not an operator.
		/// </summary>
		None,

		/// <summary>
		/// Represents the unary plus operator: +x.
		/// </summary>
		UnaryPlus,

		/// <summary>
		/// Represents the unary minus operator: -x.
		/// </summary>
		UnaryMinus,

		/// <summary>
		/// Represents the negation operator: !x.
		/// </summary>
		Negation,

		/// <summary>
		/// Represents the bitwise complement operator: ~x.
		/// </summary>
		Complement,

		/// <summary>
		/// Represents the increment operator: ++x or x++.
		/// </summary>
		Increment,

		/// <summary>
		/// Represents the decrement operator: --x or x--.
		/// </summary>
		Decrement,

		/// <summary>
		/// Represents the <see langword="true"/> operator: if(x).
		/// </summary>
		True,

		/// <summary>
		/// Represents the <see langword="false"/> operator: if(x).
		/// </summary>
		False,

		/// <summary>
		/// Represents the addition operator: x + y.
		/// </summary>
		Addition,

		/// <summary>
		/// Represents the subtraction operator: x - y.
		/// </summary>
		Subtraction,

		/// <summary>
		/// Represents the multiplication operator: x * y.
		/// </summary>
		Multiplication,

		/// <summary>
		/// Represents the division operator: x / y.
		/// </summary>
		Division,

		/// <summary>
		/// Represents the remainder operator: x % y.
		/// </summary>
		Remainder,

		/// <summary>
		/// Represents the logical <see langword="and"/> operator: x &amp; y.
		/// </summary>
		LogicalAnd,

		/// <summary>
		/// Represents the logical <see langword="or"/> operator: x | y.
		/// </summary>
		LogicalOr,

		/// <summary>
		/// Represents the logical <see langword="xor"/> operator: x ^ y.
		/// </summary>
		LogicalXor,

		/// <summary>
		/// Represents the left shift operator: x &lt;&lt; y.
		/// </summary>
		LeftShift,

		/// <summary>
		/// Represents the right shift operator: x &gt;&gt; y.
		/// </summary>
		RightShift,

		/// <summary>
		/// Represents the equality operator: x == y.
		/// </summary>
		Equality,

		/// <summary>
		/// Represents the inequality operator: x != y.
		/// </summary>
		Inequality,

		/// <summary>
		/// Represents the less than operator: x &lt; y.
		/// </summary>
		LessThan,

		/// <summary>
		/// Represents the greater than operator: x &gt; y.
		/// </summary>
		GreaterThan,

		/// <summary>
		/// Represents the less than or equal operator: x &lt;= y.
		/// </summary>
		LestThanOrEqual,

		/// <summary>
		/// Represents the greater than or equal operator: x &gt;= y.
		/// </summary>
		GreaterThanOrEqual
	}
}
