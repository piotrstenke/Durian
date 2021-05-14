internal readonly struct DiagnosticData
{
	public string Title { get; }
	public string Id { get; }
	public string Module { get; }
	public bool HasLocation { get; }
	public bool Fatal { get; }

	public DiagnosticData(string title, string id, string module, bool hasLocation, bool fatal)
	{
		Title = title;
		Id = id;
		Module = module;
		HasLocation = hasLocation;
		Fatal = fatal;
	}
}
