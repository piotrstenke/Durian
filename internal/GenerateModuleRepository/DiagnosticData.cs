// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

internal readonly struct DiagnosticData : IEquatable<DiagnosticData>
{
	public readonly bool Fatal { get; }

	public readonly string File { get; }

	public readonly bool HasLocation { get; }

	public readonly string Id { get; }

	public readonly string Module { get; }

	public readonly string Title { get; }

	public DiagnosticData(string title, string id, string module, bool hasLocation, bool fatal, string file)
	{
		Title = title;
		Id = id;
		Module = module;
		HasLocation = hasLocation;
		Fatal = fatal;
		File = file;
	}

	public readonly bool Equals(DiagnosticData other)
	{
		return other.Id == other.Id;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is DiagnosticData data && Equals(data);
	}

	public override readonly int GetHashCode()
	{
		return Id.GetHashCode();
	}
}