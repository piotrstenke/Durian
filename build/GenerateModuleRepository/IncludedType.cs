internal readonly struct IncludedType
{
	public string Kind { get; }
	public string Name { get; }
	public string Namespace { get; }

	public IncludedType(string name, string @namespace, string kind)
	{
		Kind = kind;
		Name = name;
		Namespace = @namespace;
	}
}
