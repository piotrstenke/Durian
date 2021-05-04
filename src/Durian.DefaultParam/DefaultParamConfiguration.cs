using System.Diagnostics;

namespace Durian.DefaultParam
{
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public sealed record DefaultParamConfiguration
	{
		public bool AllowOverridingOfDefaultParamValues { get; set; }
		public bool AllowAddingDefaultParamToNewParameters { get; set; }
		public bool ApplyNewToGeneratedMembersWithEquivalentSignature { get; set; }
		public bool CallInsteadOfCopying { get; set; }

		public static DefaultParamConfiguration Default => new();

		private string GetDebuggerDisplay()
		{
			return $"{{{base.ToString()}}}";
		}
	}
}
