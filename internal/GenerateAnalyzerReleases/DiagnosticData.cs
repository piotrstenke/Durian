// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

internal readonly struct DiagnosticData
{
	public readonly string Category { get; }

	public readonly string Id { get; }

	public readonly string Severity { get; }

	public readonly string Title { get; }

	public DiagnosticData(string id, string title, string category, string severity)
	{
		Id = id;
		Title = title;
		Category = category;
		Severity = severity;
	}
}
