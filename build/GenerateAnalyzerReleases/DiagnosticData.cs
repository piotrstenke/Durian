internal readonly struct DiagnosticData
{
	public string Id { get; }
	public string Title { get; }
	public string Category { get; }
	public string Severity { get; }

	public DiagnosticData(string id, string title, string category, string severity)
	{
		Id = id;
		Title = title;
		Category = category;
		Severity = severity;
	}
}
