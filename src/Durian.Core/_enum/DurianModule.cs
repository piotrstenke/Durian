namespace Durian.Info;

/// <summary>
/// Defines all modules a package can be part of.
/// </summary>
public enum DurianModule
{
	/// <summary>
	/// This package does not belong to any Durian module.
	/// </summary>
	None,

	/// <summary>
	/// Represents the <c>Durian.Core</c> module.
	/// </summary>
	Core,

	/// <summary>
	/// Represents the <c>Durian.Development</c> module.
	/// </summary>
	Development,

	/// <summary>
	/// Represents the <c>Durian.DefaultParam</c> module.
	/// </summary>
	DefaultParam,

	/// <summary>
	/// Represents the <c>Durian.FriendClass</c> module.
	/// </summary>
	FriendClass,

	/// <summary>
	/// Represents the <c>Durian.InterfaceTargets</c> module.
	/// </summary>
	InterfaceTargets,

	/// <summary>
	/// Represents the <c>Durian.CopyFrom</c> module.
	/// </summary>
	CopyFrom
}
