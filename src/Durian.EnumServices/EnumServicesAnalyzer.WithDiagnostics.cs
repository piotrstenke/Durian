// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Durian.Analysis.Extensions;
using static Durian.Analysis.EnumServices.EnumServicesDiagnostics;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.EnumServices
{
	public partial class EnumServicesAnalyzer
	{
		/// <summary>
		/// Contains static methods that analyze enums marked with the <see cref="EnumServicesAttribute"/>.
		/// </summary>
		public static class WithDiagnostics
		{
			public static bool AnalyzeConfiguration(
				IDiagnosticReceiver diagnosticReceiver,
				IAssemblySymbol assembly,
				EnumServicesConfiguration configuration,
				AttributeSyntax syntax)
			{
				if (!IsValidServicesValue(configuration.EnumServices))
				{
					diagnosticReceiver.ReportDiagnostic(
						DUR0201_InvalidEnumValue,
						syntax.GetArgumentLocation(nameof(EnumServicesAttribute.Services)),
						assembly);
				}
			}

			public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, EnumServicesCompilationData compilation)
			{
			}
		}
	}
}
