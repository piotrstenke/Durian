using System;

internal readonly struct DiagnosticData : IEquatable<DiagnosticData>
{
	public string Title { get; }
	public string Id { get; }
	public string Module { get; }
	public string File { get; }
	public bool HasLocation { get; }
	public bool Fatal { get; }

	public DiagnosticData(string title, string id, string module, bool hasLocation, bool fatal, string file)
	{
		Title = title;
		Id = id;
		Module = module;
		HasLocation = hasLocation;
		Fatal = fatal;
		File = file;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public bool Equals(DiagnosticData other)
	{
		return other.Id == other.Id;
	}

	public override bool Equals(object? obj)
	{
		return obj is DiagnosticData data && Equals(data);
	}
}
