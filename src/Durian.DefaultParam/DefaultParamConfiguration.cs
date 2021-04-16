namespace Durian.DefaultParam
{
	public sealed class DefaultParamConfiguration
	{
		public bool AllowOverridingOfDefaultParamValues { get; set; }
		public bool AllowAddingDefaultParamToNewParameters { get; set; }
		public bool ApplyNewToGeneratedMembersWithEquivalentSignature { get; set; }

		public static DefaultParamConfiguration Default => new();
	}
}
