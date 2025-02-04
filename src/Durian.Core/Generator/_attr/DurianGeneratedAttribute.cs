﻿using System;

namespace Durian.Generator;

/// <summary>
/// Marks the target member as generated by a Durian source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class DurianGeneratedAttribute : Attribute
{
	/// <summary>
	/// Member this code was generated from.
	/// </summary>
	public string? Source { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianGeneratedAttribute"/> class.
	/// </summary>
	public DurianGeneratedAttribute()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianGeneratedAttribute"/> class.
	/// </summary>
	/// <param name="source">Member this code was generated from.</param>
	public DurianGeneratedAttribute(string? source)
	{
		Source = source;
	}
}
