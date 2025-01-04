using System;

namespace Durian.Analysis;

/// <summary>
/// Defines special requirements of operator declarations.
/// </summary>
[Flags]
public enum OperatorRequirements
{
	/// <summary>
	/// No special requirements.
	/// </summary>
	None = 0,

	/// <summary>
	/// The operator requires a matching operator. This applies to:
	/// <list type="bullet">
	/// <item><see cref="OverloadableOperator.Equality"/> &lt;--&gt; <see cref="OverloadableOperator.Inequality"/></item>
	/// <item><see cref="OverloadableOperator.True"/> &lt;--&gt; <see cref="OverloadableOperator.False"/></item>
	/// <item><see cref="OverloadableOperator.GreaterThan"/> &lt;--&gt; <see cref="OverloadableOperator.LessThan"/></item>
	/// <item><see cref="OverloadableOperator.GreaterThanOrEqual"/> &lt;--&gt; <see cref="OverloadableOperator.LessThanOrEqual"/></item>
	/// </list>
	/// </summary>
	MatchingOperator = 1,

	/// <summary>
	/// The operator requires the second operand to be an <see cref="int"/>. This applies to:
	/// <list type="bullet">
	/// <item><see cref="OverloadableOperator.RightShift"/></item>
	/// <item><see cref="OverloadableOperator.LeftShift"/></item>
	/// </list>
	/// </summary>
	IntOperand = 2,

	/// <summary>
	/// The operator requires <see cref="bool"/> return type. This applies to:
	/// <list type="bullet">
	/// <item><see cref="OverloadableOperator.True"/></item>
	/// <item><see cref="OverloadableOperator.False"/></item>
	/// </list>
	/// </summary>
	ReturnBoolean = 4,

	/// <summary>
	/// The operator requires the return type to match the type of the first parameter or its child type. This applies to:
	/// <list type="bullet">
	/// <item><see cref="OverloadableOperator.Increment"/></item>
	/// <item><see cref="OverloadableOperator.Decrement"/></item>
	/// </list>
	/// </summary>
	ReturnParameterType = 8
}
