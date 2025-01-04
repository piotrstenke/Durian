using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	internal interface IVariableDeclaratorProperties
	{
		int? Index { get; set; }
		VariableDeclaratorSyntax? Variable { get; set; }
		ISymbol? Symbol { get; set; }

		void FillWithDefaultData();
	}
}
