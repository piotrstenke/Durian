using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Removes all syntax nodes except for structured trivia.
	/// </summary>
	public class StructuredTriviaPreserver : CSharpSyntaxRewriter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StructuredTriviaPreserver"/> class.
		/// </summary>
		public StructuredTriviaPreserver()
		{
		}

		/// <inheritdoc/>
		public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
		{
			if (trivia.HasStructure)
			{
				return trivia;
			}

			return default;
		}
	}
}
