using System.Collections.Generic;
using System.Linq;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian
{
	public sealed partial class CodeBuilder
	{
		private void WriteUsings_Internal(IEnumerable<string> namespaces)
		{
			if (CurrentIndent == 0)
			{
				foreach (string u in namespaces)
				{
					TextBuilder.Append("using ").Append(u).AppendLine(";");
				}
			}
			else
			{
				foreach (string u in namespaces)
				{
					Indent();
					TextBuilder.Append("using ").Append(u).AppendLine(";");
				}
			}
		}

		private void BeginNamespaceDeclaration_Internal(IEnumerable<INamespaceSymbol> namespaces)
		{
			BeginNamespaceDeclaration_Internal(namespaces.Select(n => n.Name));
		}

		private void BeginNamespaceDeclaration_Internal(IEnumerable<string> namespaces)
		{
			Indent();
			TextBuilder.Append("namespace ").AppendLine(AnalysisUtilities.JoinNamespaces(namespaces));
			Indent();
			TextBuilder.AppendLine("{");
			CurrentIndent++;
		}

		private void BeginTypeDeclaration_Internal(ITypeData type)
		{
			Indent();

			foreach (SyntaxToken modifier in type.Modifiers)
			{
				TextBuilder.Append(modifier.ValueText);
				TextBuilder.Append(' ');
			}

			TextBuilder.Append(type.Declaration.Keyword);
			TextBuilder.Append(' ');

			TextBuilder.AppendLine(type.Symbol.Name);
			Indent();
			CurrentIndent++;
			TextBuilder.AppendLine("{");
		}

		private void BeginTypeDeclaration_Internal(TypeDeclarationSyntax type, bool includeTrivia)
		{
			TextBuilder.Append(GetDeclarationText(SyntaxFactory.TypeDeclaration(type.Kind(), type.Identifier), includeTrivia));
		}

		private void BeginMethodDeclaration_Internal(MethodDeclarationSyntax method, bool blockOrExpression, bool includeTrivia)
		{
			TextBuilder.Append(GetDeclarationText(SyntaxFactory.MethodDeclaration(method.ReturnType, method.Identifier), includeTrivia));

			if (blockOrExpression)
			{
				TextBuilder.AppendLine();
				Indent();
				CurrentIndent++;
				TextBuilder.AppendLine("{");
			}
			else
			{
				TextBuilder.Append(" => ");
			}
		}

		private void WriteParentDeclarations_Internal(IEnumerable<ITypeData> types)
		{
			foreach (ITypeData parent in types)
			{
				BeginTypeDeclaration_Internal(parent);
			}
		}

		private void WriteDeclarationLead_Internal(IMemberData member, IEnumerable<string> usings, string? generatorName, string? version)
		{
			WriteHeader(generatorName, version);
			TextBuilder.AppendLine();
			string[] namespaces = usings.ToArray();

			if (namespaces.Length > 0)
			{
				WriteUsings_Internal(usings);
				TextBuilder.AppendLine();
			}

			if (member.Symbol.ContainingNamespace is not null && !member.Symbol.ContainingNamespace.IsGlobalNamespace)
			{
				BeginNamespaceDeclaration_Internal(member.GetContainingNamespaces());
			}

			WriteParentDeclarations_Internal(member.GetContainingTypes());
		}

		private static string GetDeclarationText(MemberDeclarationSyntax declaration, bool includeTrivia)
		{
			if (includeTrivia)
			{
				MemberDeclarationSyntax decl = declaration
					.WithLeadingTrivia(declaration.GetLeadingTrivia())
					.WithTrailingTrivia(declaration.GetTrailingTrivia());

				return decl.ToFullString();
			}

			return declaration.ToString();
		}
	}
}