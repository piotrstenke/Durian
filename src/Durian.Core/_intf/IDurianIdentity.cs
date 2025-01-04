using System;

namespace Durian.Info;

/// <summary>
/// Provides basic properties of an identity object.
/// </summary>
public interface IDurianIdentity : ICloneable
{
	/// <summary>
	/// Name of the identity object.
	/// </summary>
	string Name { get; }

	/// <inheritdoc cref="ICloneable.Clone"/>
	new IDurianIdentity Clone();
}
