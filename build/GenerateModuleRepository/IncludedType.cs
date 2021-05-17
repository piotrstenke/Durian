internal readonly struct IncludedType
{
	public string Name { get; }
	public string Namespace { get; }

	public IncludedType(string name, string @namespace)
	{
		Name = name;
		Namespace = @namespace;
	}
}
