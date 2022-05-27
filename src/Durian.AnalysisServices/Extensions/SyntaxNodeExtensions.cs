// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for <see cref="SyntaxNode"/>-derived classes.
	/// </summary>
	public static class SyntaxNodeExtensions
	{
		/// <summary>
		/// Builds a <see cref="UsingDirectiveSyntax"/> from the specified <paramref name="namespace"/> and adds it to the given <paramref name="compilationUnit"/>.
		/// </summary>
		/// <param name="compilationUnit"><see cref="CompilationUnitSyntax"/> to add the using directive to.</param>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to build the <see cref="UsingDirectiveSyntax"/> from.</param>
		public static CompilationUnitSyntax AddUsings(this CompilationUnitSyntax compilationUnit, INamespaceSymbol @namespace)
		{
			UsingDirectiveSyntax directive = @namespace.GetUsingDirective();
			string name = directive.Name.ToString();

			if (compilationUnit.Usings.Any(u => u.Name.ToString() == name))
			{
				return compilationUnit;
			}

			return compilationUnit.AddUsings(directive);
		}

		/// <summary>
		/// Builds <see cref="UsingDirectiveSyntax"/>es from the specified <paramref name="namespaces"/> and adds them to the given <paramref name="compilationUnit"/>.
		/// </summary>
		/// <param name="compilationUnit"><see cref="CompilationUnitSyntax"/> to add the using directives to.</param>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to build the <see cref="UsingDirectiveSyntax"/>es from.</param>
		public static CompilationUnitSyntax AddUsings(this CompilationUnitSyntax compilationUnit, IEnumerable<INamespaceSymbol>? namespaces)
		{
			HashSet<string> usings = new(compilationUnit.Usings.Where(u => u.Alias is null).Select(u => u.Name.ToString()));
			UsingDirectiveSyntax[] directives = namespaces
				.Where(n => n.Name != string.Empty)
				.Select(n => n.GetUsingDirective())
				.Where(n => usings.Add(n.Name.ToString()))
				.ToArray();

			if (directives.Length == 0)
			{
				return compilationUnit;
			}

			return compilationUnit.AddUsings(directives);
		}

		/// <summary>
		/// Returns attribute argument with the specified <paramref name="argumentName"/>
		/// or <see langword="null"/> if no argument with the <paramref name="argumentName"/> was found.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the argument of.</param>
		/// <param name="argumentName">Name of argument to get.</param>
		/// <param name="includeParameters">Determines whether to include arguments with colons in the search.</param>
		public static AttributeArgumentSyntax? GetArgument(this AttributeSyntax attribute, string argumentName, bool includeParameters = false)
		{
			if (attribute.ArgumentList is null)
			{
				return null;
			}

			SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList.Arguments;

			if (!arguments.Any())
			{
				return null;
			}

			Func<AttributeArgumentSyntax, bool> func;

			if (includeParameters)
			{
				func = arg =>
				{
					if (arg.NameEquals is not null)
					{
						return arg.NameEquals.Name.Identifier.ValueText == argumentName;
					}

					return arg.NameColon is not null && arg.NameColon.Name.Identifier.ValueText == argumentName;
				};
			}
			else
			{
				func = arg => arg.NameEquals is not null && arg.NameEquals.Name.Identifier.ValueText == argumentName;
			}

			return arguments.FirstOrDefault(func);
		}

		/// <summary>
		/// Returns attribute argument at the specified <paramref name="position"/> and with the given <paramref name="argumentName"/>.
		/// <para>If <paramref name="argumentName"/> is <see langword="null"/>, only <paramref name="position"/> is included in the search.</para>
		/// <para>If no appropriate argument found, returns <see langword="null"/>.</para>
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the argument of.</param>
		/// <param name="position">Position of argument to get.</param>
		/// <param name="argumentName">Name of the argument to get the location of.</param>
		public static AttributeArgumentSyntax? GetArgument(this AttributeSyntax attribute, int position, string? argumentName = null)
		{
			if (attribute.ArgumentList is null)
			{
				return null;
			}

			SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList.Arguments;

			if (!arguments.Any())
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(argumentName))
			{
				if (position >= arguments.Count)
				{
					return null;
				}

				return arguments[position];
			}

			if (position < arguments.Count)
			{
				AttributeArgumentSyntax arg = arguments[position];

				if (arg.GetName() == argumentName)
				{
					return arg;
				}
			}

			return arguments.FirstOrDefault(arg => arg.GetName() == argumentName);
		}

		/// <summary>
		/// Returns location of attribute argument with the specified <paramref name="argumentName"/>
		/// or location of the <paramref name="attribute"/> if no argument with the <paramref name="argumentName"/> was found.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the location of argument of.</param>
		/// <param name="argumentName">Name of the argument to get the location of.</param>
		/// <param name="includeParameters">Determines whether to include arguments with colons in the search.</param>
		public static Location GetArgumentLocation(this AttributeSyntax attribute, string argumentName, bool includeParameters = false)
		{
			AttributeArgumentSyntax? arg = attribute.GetArgument(argumentName, includeParameters);

			if (arg is null)
			{
				return attribute.GetLocation();
			}

			return arg.GetLocation();
		}

		/// <summary>
		/// Returns location of attribute argument at the specified <paramref name="position"/> and with the given <paramref name="argumentName"/>
		/// or location of the <paramref name="attribute"/> is no appropriate argument was found.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the location of argument of.</param>
		/// <param name="position">Position of argument to get.</param>
		/// <param name="argumentName">Name of the argument to get the location of.</param>
		public static Location GetArgumentLocation(this AttributeSyntax attribute, int position, string? argumentName = null)
		{
			AttributeArgumentSyntax? arg = attribute.GetArgument(position, argumentName);

			if (arg is null)
			{
				return attribute.GetLocation();
			}

			return arg.GetLocation();
		}

		/// <summary>
		/// Returns attribute declaration list of the specified <paramref name="node"/> or an empty list if the <paramref name="node"/> has no attributes.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> to get attribute lists of.</param>
		public static SyntaxList<AttributeListSyntax> GetAttributeLists(this SyntaxNode node)
		{
			return node switch
			{
				MemberDeclarationSyntax member => member.AttributeLists,
				StatementSyntax statement => statement.AttributeLists,
				LambdaExpressionSyntax lambda => lambda.AttributeLists,
				AccessorDeclarationSyntax accessor => accessor.AttributeLists,
				CompilationUnitSyntax unit => unit.AttributeLists,
				TypeParameterSyntax typeParameter => typeParameter.AttributeLists,
				BaseParameterSyntax parameter => parameter.AttributeLists,
				_ => SyntaxFactory.List<AttributeListSyntax>()
			};
		}

		/// <summary>
		/// Returns the block body of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the block body of.</param>
		public static BlockSyntax? GetBlock(this CSharpSyntaxNode node)
		{
			return node switch
			{
				BaseMethodDeclarationSyntax method => method.Body,
				AccessorDeclarationSyntax accessor => accessor.Body,
				AnonymousFunctionExpressionSyntax lambda => lambda.Block,
				StatementSyntax statement => statement.GetBlock(),
				CatchClauseSyntax @catch => @catch.Block,
				FinallyClauseSyntax @finally => @finally.Block,
				_ => default
			};
		}

		/// <summary>
		/// Returns the block body of the specified <paramref name="statement"/>.
		/// </summary>
		/// <param name="statement"><see cref="StatementSyntax"/> to get the block body of.</param>
		public static BlockSyntax? GetBlock(this StatementSyntax statement)
		{
			return statement switch
			{
				LocalFunctionStatementSyntax local => local.Body,
				CheckedStatementSyntax @checked => @checked.Block,
				UnsafeStatementSyntax @unsafe => @unsafe.Block,
				TryStatementSyntax @try => @try.Block,
				BlockSyntax block => block,
				_ => default
			};
		}

		/// <summary>
		/// Returns the body of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="BaseMethodDeclarationSyntax"/> to get the body of.</param>
		public static CSharpSyntaxNode? GetBody(this BaseMethodDeclarationSyntax method)
		{
			if (method.Body is not null)
			{
				return method.Body;
			}

			if (method.ExpressionBody is not null)
			{
				return method.ExpressionBody;
			}

			return null;
		}

		/// <summary>
		/// Returns the body of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="LocalFunctionStatementSyntax"/> to get the body of.</param>
		public static CSharpSyntaxNode? GetBody(this LocalFunctionStatementSyntax method)
		{
			if (method.Body is not null)
			{
				return method.Body;
			}

			if (method.ExpressionBody is not null)
			{
				return method.ExpressionBody;
			}

			return null;
		}

		/// <summary>
		/// Returns the body of the specified <paramref name="accessor"/>.
		/// </summary>
		/// <param name="accessor"><see cref="AccessorDeclarationSyntax"/> to get the body of.</param>
		public static CSharpSyntaxNode? GetBody(this AccessorDeclarationSyntax accessor)
		{
			if (accessor.Body is not null)
			{
				return accessor.Body;
			}

			if (accessor.ExpressionBody is not null)
			{
				return accessor.ExpressionBody;
			}

			return null;
		}

		/// <summary>
		/// Returns type of the body of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="BaseMethodDeclarationSyntax"/> to get the type of body of.</param>
		public static MethodStyle GetBodyType(this BaseMethodDeclarationSyntax method)
		{
			if (method.Body is not null)
			{
				return MethodStyle.Block;
			}

			if (method.ExpressionBody is not null)
			{
				return MethodStyle.Expression;
			}

			return MethodStyle.None;
		}

		/// <summary>
		/// Returns parent namespaces of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> to get the parent namespaces of.</param>
		/// <param name="order">Specifies ordering of the returned values.</param>
		public static IEnumerable<string> GetContainingNamespaces(this SyntaxNode node, ReturnOrder order = ReturnOrder.Parent)
		{
			return AnalysisUtilities.ByOrder(Yield(), order);

			IEnumerable<string> Yield()
			{
				SyntaxNode? current = node;

				while ((current = current!.Parent) is not null)
				{
					if (current is NamespaceDeclarationSyntax decl)
					{
						string[] split = decl.Name.ToString().Split('.');
						int length = split.Length;

						for (int i = length - 1; i > -1; i--)
						{
							yield return split[i];
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the expression body of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the expression body of.</param>
		public static ArrowExpressionClauseSyntax? GetExpressionBody(this CSharpSyntaxNode node)
		{
			return node switch
			{
				BaseMethodDeclarationSyntax method => method.ExpressionBody,
				PropertyDeclarationSyntax property => property.ExpressionBody,
				AccessorDeclarationSyntax accessor => accessor.ExpressionBody,
				IndexerDeclarationSyntax indexer => indexer.ExpressionBody,
				LocalFunctionStatementSyntax local => local.ExpressionBody,
				ArrowExpressionClauseSyntax arrow => arrow,
				_ => default
			};
		}

		/// <summary>
		/// Returns the keyword that is used to declare the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="BaseTypeDeclarationSyntax"/> to get the keyword of.</param>
		public static string? GetKeyword(this BaseTypeDeclarationSyntax type)
		{
			return type switch
			{
				EnumDeclarationSyntax => "enum",
				RecordDeclarationSyntax record => record.ClassOrStructKeyword == default ? "record" : $"record {record.ClassOrStructKeyword}",
				TypeDeclarationSyntax t => t.Keyword.ValueText,
				_ => default
			};
		}

		/// <summary>
		/// Returns new instance of <see cref="IMemberData"/> associated with the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to get the data of.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		public static IMemberData GetMemberData(this CSharpSyntaxNode member, ICompilationData compilation)
		{
			return member switch
			{
				ClassDeclarationSyntax => new ClassData((ClassDeclarationSyntax)member, compilation),
				StructDeclarationSyntax => new StructData((StructDeclarationSyntax)member, compilation),
				InterfaceDeclarationSyntax => new InterfaceData((InterfaceDeclarationSyntax)member, compilation),
				RecordDeclarationSyntax => new RecordData((RecordDeclarationSyntax)member, compilation),
				EnumDeclarationSyntax => new EnumData((EnumDeclarationSyntax)member, compilation),
				MethodDeclarationSyntax => new MethodData((MethodDeclarationSyntax)member, compilation),
				FieldDeclarationSyntax => new FieldData((FieldDeclarationSyntax)member, compilation),
				PropertyDeclarationSyntax => new PropertyData((PropertyDeclarationSyntax)member, compilation),
				BaseNamespaceDeclarationSyntax => new NamespaceData((BaseNamespaceDeclarationSyntax)member, compilation),
				EventDeclarationSyntax => new EventData((EventDeclarationSyntax)member, compilation),
				EventFieldDeclarationSyntax => new EventData((EventFieldDeclarationSyntax)member, compilation),
				DelegateDeclarationSyntax => new DelegateData((DelegateDeclarationSyntax)member, compilation),
				ParameterSyntax => new ParameterData((ParameterSyntax)member, compilation),
				TypeParameterSyntax => new TypeParameterData((TypeParameterSyntax)member, compilation),
				IndexerDeclarationSyntax => new IndexerData((IndexerDeclarationSyntax)member, compilation),
				ConstructorDeclarationSyntax => new ConstructorData((ConstructorDeclarationSyntax)member, compilation),
				DestructorDeclarationSyntax => new DestructorData((DestructorDeclarationSyntax)member, compilation),
				OperatorDeclarationSyntax => new OperatorData((OperatorDeclarationSyntax)member, compilation),
				ConversionOperatorDeclarationSyntax => new ConversionOperatorData((ConversionOperatorDeclarationSyntax)member, compilation),
				LocalFunctionStatementSyntax => new LocalFunctionData((LocalFunctionStatementSyntax)member, compilation),
				LocalDeclarationStatementSyntax => new LocalData((LocalDeclarationStatementSyntax)member, compilation),

				_ => new MemberData(member, compilation),
			};
		}

		/// <summary>
		/// Return list of modifiers of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> to get the modifiers of.</param>
		public static SyntaxTokenList GetModifiers(this SyntaxNode node)
		{
			return node switch
			{
				MemberDeclarationSyntax member => member.Modifiers,
				BaseParameterSyntax parameter => parameter.Modifiers,
				LocalFunctionStatementSyntax localFunction => localFunction.Modifiers,
				LocalDeclarationStatementSyntax local => local.Modifiers,
				AccessorDeclarationSyntax accessor => accessor.Modifiers,
				AnonymousFunctionExpressionSyntax lambda => lambda.Modifiers,
				_ => SyntaxFactory.TokenList()
			};
		}

		/// <summary>
		/// Returns modifiers contained withing the given collection of <see cref="TypeDeclarationSyntax"/>es.
		/// </summary>
		/// <param name="decl">Collection of <see cref="TypeDeclarationSyntax"/>es to get the modifiers from.</param>
		public static IEnumerable<SyntaxToken> GetModifiers(this IEnumerable<MemberDeclarationSyntax> decl)
		{
			List<SyntaxToken> tokens = new();

			foreach (TypeDeclarationSyntax d in decl)
			{
				if (d is null)
				{
					continue;
				}

				foreach (SyntaxToken modifier in d.Modifiers)
				{
					if (!tokens.Exists(m => m.IsKind(modifier.Kind())))
					{
						tokens.Add(modifier);
						yield return modifier;
					}
				}
			}
		}

		/// <summary>
		/// Returns the name of attribute argument represented by the specified <paramref name="syntax"/>
		/// or <see langword="null"/> if the argument has neither <see cref="NameEqualsSyntax"/> or <see cref="NameColonSyntax"/>.
		/// </summary>
		/// <param name="syntax"><see cref="AttributeArgumentSyntax"/> to get the name of.</param>
		public static string? GetName(this AttributeArgumentSyntax syntax)
		{
			if (syntax.NameEquals is not null)
			{
				return syntax.NameEquals.GetName();
			}

			if (syntax.NameColon is not null)
			{
				return syntax.NameColon.GetName();
			}

			return default;
		}

		/// <summary>
		/// Returns the name of member represented by the specified <paramref name="syntax"/>.
		/// </summary>
		/// <param name="syntax"><see cref="NameColonSyntax"/> that contains the member name.</param>
		public static string GetName(this NameEqualsSyntax syntax)
		{
			return syntax.Name.Identifier.ValueText;
		}

		/// <summary>
		/// Returns the name of member represented by the specified <paramref name="syntax"/>.
		/// </summary>
		/// <param name="syntax"><see cref="NameColonSyntax"/> that contains the member name.</param>
		public static string GetName(this NameColonSyntax syntax)
		{
			return syntax.Name.Identifier.ValueText;
		}

		/// <summary>
		/// Returns a <see cref="TypeParameterListSyntax"/> of the <paramref name="member"/> or <see langword="null"/> if the <paramref name="member"/> has no type parameters.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to get the <see cref="TypeParameterListSyntax"/> of.</param>
		public static TypeParameterListSyntax? GetTypeParameterList(this MemberDeclarationSyntax member)
		{
			return member switch
			{
				TypeDeclarationSyntax t => t.TypeParameterList,
				MethodDeclarationSyntax m => m.TypeParameterList,
				DelegateDeclarationSyntax d => d.TypeParameterList,
				_ => null
			};
		}

		/// <summary>
		/// Checks if the target <paramref name="method"/> has a body, either block or expression.
		/// </summary>
		/// <param name="method"><see cref="BaseMethodDeclarationSyntax"/> to check if has a body.</param>
		public static bool HasBody(this BaseMethodDeclarationSyntax method)
		{
			return method.GetBody() is not null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="abstract"/> token.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="abstract"/> token.</param>
		public static bool IsAbstract(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsAbstract();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="new"/> token.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="new"/> token.</param>
		public static bool IsNew(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsNew();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="virtual"/> token.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="virtual"/> token.</param>
		public static bool IsVirtual(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsVirtual();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="partial"/> token.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="parital"/> token.</param>
		public static bool IsPartial(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsPartial();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="async"/> token.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="async"/> token.</param>
		public static bool IsAsync(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsAsync();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="readonly"/> or <see langword="in"/> modifier (<see langword="ref"/> <see langword="readonly"/> does not count).
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="readonly"/> or <see langword="in"/>  modifier.</param>
		public static bool IsReadOnly(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsReadOnly();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="unsafe"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="unsafe"/> modifier.</param>
		public static bool IsUnsafe(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsUnsafe();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="static"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="static"/> modifier.</param>
		public static bool IsStatic(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsStatic();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="ref"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="ref"/> modifier.</param>
		public static bool IsRef(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsRef();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="ref"/> and <see langword="readonly"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="ref"/> and <see langword="readonly"/> modifier.</param>
		public static bool IsRefReadOnly(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsRefReadOnly();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="override"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="override"/> modifier.</param>
		public static bool IsOverride(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsOverride();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="sealed"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="sealed"/> modifier.</param>
		public static bool IsSealed(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsSealed();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="fixed"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="fixed"/> modifier.</param>
		public static bool IsFixed(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsFixed();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="volatile"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="volatile"/> modifier.</param>
		public static bool IsVolatile(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsVolatile();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="extern"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="extern"/> modifier.</param>
		public static bool IsExtern(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsExtern();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="params"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="params"/> modifier.</param>
		public static bool IsParams(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsParams();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> contains the <see langword="this"/> modifier.
		/// </summary>
		/// <param name="node">Determines whether the specified <paramref name="node"/> contains the <see langword="this"/> modifier.</param>
		public static bool IsThis(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().IsThis();
		}

		/// <summary>
		/// Returns the <see cref="Accessibility"/> of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the accessibility of.</param>
		public static Accessibility GetAccessibility(this CSharpSyntaxNode node)
		{
			return node.GetModifiers().GetAccessibility();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
		/// </summary>
		/// <param name="node"><see cref="PropertyDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
		/// <param name="accessor">Kind of accessor to check for.</param>
		public static bool HasAccessor(this PropertyDeclarationSyntax node, PropertyAccessor accessor)
		{
			if(node.ExpressionBody is not null)
			{
				return accessor == PropertyAccessor.Get;
			}

			return node.AccessorList?.HasAccessor(accessor.GetAccessor()) ?? false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
		/// </summary>
		/// <param name="node"><see cref="IndexerDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
		/// <param name="accessor">Kind of accessor to check for.</param>
		public static bool HasAccessor(this IndexerDeclarationSyntax node, PropertyAccessor accessor)
		{
			if(node.ExpressionBody is not null)
			{
				return accessor == PropertyAccessor.Get;
			}

			return node.AccessorList?.HasAccessor(accessor.GetAccessor()) ?? false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
		/// </summary>
		/// <param name="node"><see cref="EventDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
		/// <param name="accessor">Kind of accessor to check for.</param>
		public static bool HasAccessor(this EventDeclarationSyntax node, EventAccessor accessor)
		{
			return node.AccessorList?.HasAccessor(accessor.GetAccessor()) ?? false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
		/// </summary>
		/// <param name="node"><see cref="BasePropertyDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
		/// <param name="accessor">Kind of accessor to check for.</param>
		public static bool HasAccessor(this BasePropertyDeclarationSyntax node, Accessor accessor)
		{
			return node.AccessorList?.HasAccessor(accessor) ?? false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
		/// </summary>
		/// <param name="node"><see cref="AccessorListSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
		/// <param name="accessor">Kind of accessor to check for.</param>
		public static bool HasAccessor(this AccessorListSyntax node, Accessor accessor)
		{
			return node.Accessors.Any(acc => acc.GetAccessorKind() == accessor);
		}

		/// <summary>
		/// Returns the kind of <see cref="Accessor"/> the specified <paramref name="node"/> represents.
		/// </summary>
		/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the <see cref="Accessor"/> kind represented by.</param>
		public static Accessor GetAccessorKind(this AccessorDeclarationSyntax node)
		{
			return node.Keyword.GetAccessor();
		}

		/// <summary>
		/// Returns the kind of <see cref="PropertyAccessor"/> the specified <paramref name="node"/> represents.
		/// </summary>
		/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the <see cref="Accessor"/> kind represented by.</param>
		public static PropertyAccessor GetPropertyAccessorKind(this AccessorDeclarationSyntax node)
		{
			return node.GetAccessorKind().GetPropertyAccessor();
		}

		/// <summary>
		/// Returns the kind of <see cref="EventAccessor"/> the specified <paramref name="node"/> represents.
		/// </summary>
		/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the <see cref="Accessor"/> kind represented by.</param>
		public static EventAccessor GetEventAccessorKind(this AccessorDeclarationSyntax node)
		{
			return node.GetAccessorKind().GetEventAccessor();
		}

		/// <summary>
		/// Returns the <see cref="AttributeTarget"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="AttributeTargetSpecifierSyntax"/> to get the <see cref="AttributeTarget"/> associated with.</param>
		public static AttributeTarget GetAttributeTarget(this AttributeTargetSpecifierSyntax node)
		{
			return node.Identifier.GetAttributeTarget();
		}

		/// <summary>
		/// Returns the <see cref="AttributeTarget"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="AttributeListSyntax"/> to get the <see cref="AttributeTarget"/> associated with.</param>
		public static AttributeTarget GetAttributeTarget(this AttributeListSyntax node)
		{
			return node.Target is AttributeTargetSpecifierSyntax target ? target.GetAttributeTarget() : default;
		}

		/// <summary>
		/// Returns the <see cref="AttributeTarget"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="AttributeSyntax"/> to get the <see cref="AttributeTarget"/> associated with.</param>
		public static AttributeTarget GetAttributeTarget(this AttributeSyntax node)
		{
			return node.Parent is AttributeListSyntax attrList ? attrList.GetAttributeTarget() : default;
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeParameterConstraintSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
		public static GenericConstraint GetConstraint(this TypeParameterConstraintSyntax node)
		{
			switch (node)
			{
				case ClassOrStructConstraintSyntax classOrStruct:

					if(classOrStruct.IsKind(SyntaxKind.ClassConstraint))
					{
						return GenericConstraint.Class;
					}

					if(classOrStruct.IsKind(SyntaxKind.StructConstraint))
					{
						return GenericConstraint.Struct;
					}

					return default;

				case TypeConstraintSyntax type:

					if(type.Type.IsNotNull)
					{
						return GenericConstraint.NotNull;
					}

					if(type.Type.IsUnmanaged)
					{
						return GenericConstraint.Unmanaged;
					}

					return GenericConstraint.Type;

				case ConstructorConstraintSyntax:
					return GenericConstraint.New;

				case DefaultConstraintSyntax:
					return GenericConstraint.Default;

				default:
					return default;
			}
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeParameterConstraintClauseSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
		public static GenericConstraint GetConstraints(this TypeParameterConstraintClauseSyntax node)
		{
			GenericConstraint constraint = default;

			foreach (TypeParameterConstraintSyntax syntax in node.Constraints)
			{
				constraint |= syntax.GetConstraint();
			}

			return constraint;
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeParameterSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
		public static GenericConstraint GetConstraints(this TypeParameterSyntax node)
		{
			MemberDeclarationSyntax? member = node.Parent?.Parent as MemberDeclarationSyntax;

			return member switch
			{
				TypeDeclarationSyntax type => GetConstraints(type.ConstraintClauses),
				MethodDeclarationSyntax method => GetConstraints(method.ConstraintClauses),
				DelegateDeclarationSyntax @delegate => GetConstraints(@delegate.ConstraintClauses),
				_ => default
			};

			GenericConstraint GetConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> clauses)
			{
				return clauses.FirstOrDefault(c => c.Name.Identifier.Value == node.Identifier.Value).GetConstraints();
			}
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeDeclarationSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
		public static GenericConstraint[] GetConstraints(this TypeDeclarationSyntax node)
		{
			return node.ConstraintClauses.Select(c => c.GetConstraints()).ToArray();
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="DelegateDeclarationSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
		public static GenericConstraint[] GetConstraints(this DelegateDeclarationSyntax node)
		{
			return node.ConstraintClauses.Select(c => c.GetConstraints()).ToArray();
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="MethodDeclarationSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
		public static GenericConstraint[] GetConstraints(this MethodDeclarationSyntax node)
		{
			return node.ConstraintClauses.Select(c => c.GetConstraints()).ToArray();
		}

		/// <summary>
		/// Returns the kind of <see cref="ConstructorInitializer"/> the specified <paramref name="node"/> represents.
		/// </summary>
		/// <param name="node"><see cref="ConstructorInitializerSyntax"/> to get the <see cref="ConstructorInitializer"/> kind represented by.</param>
		public static ConstructorInitializer GetConstructorInitializer(this ConstructorInitializerSyntax node)
		{
			return ((SyntaxKind)node.RawKind).GetConstructorInitializer();
		}

		/// <summary>
		/// Returns the <see cref="ConstructorInitializer"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="ConstructorDeclarationSyntax"/> to get the <see cref="ConstructorInitializer"/> associated with.</param>
		public static ConstructorInitializer GetConstructorInitializer(this ConstructorDeclarationSyntax node)
		{
			return node.Initializer?.GetConstructorInitializer() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="DecimalLiteralSuffix"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="DecimalLiteralSuffix"/> applied to.</param>
		public static DecimalLiteralSuffix GetDecimalSuffix(this LiteralExpressionSyntax node)
		{
			return node.Token.GetDecimalSuffix();
		}

		/// <summary>
		/// Returns the <see cref="NumericLiteralPrefix"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="NumericLiteralPrefix"/> applied to.</param>
		public static NumericLiteralPrefix GetNumericPrefix(this LiteralExpressionSyntax node)
		{
			return node.Token.GetNumericPrefix();
		}

		/// <summary>
		/// Returns the <see cref="NumericLiteralSuffix"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="NumericLiteralSuffix"/> applied to.</param>
		public static NumericLiteralSuffix GetNumerixSuffix(this LiteralExpressionSyntax node)
		{
			return node.Token.GetNumericSuffix();
		}

		/// <summary>
		/// Returns the <see cref="DecimalValueType"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="DecimalValueType"/> represented by.</param>
		public static DecimalValueType GetDecimalType(this LiteralExpressionSyntax node)
		{
			return node.Token.GetDecimalType();
		}

		/// <summary>
		/// Returns the <see cref="DecimalValueType"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="PredefinedTypeSyntax"/> to get the <see cref="DecimalValueType"/> represented by.</param>
		public static DecimalValueType GetDecimalType(this PredefinedTypeSyntax node)
		{
			return node.Keyword.GetDecimalType();
		}

		/// <summary>
		/// Returns the <see cref="DecimalValueType"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="DecimalValueType"/> represented by.</param>
		public static DecimalValueType GetDecimalType(this TypeSyntax node)
		{
			return (node as PredefinedTypeSyntax)?.GetDecimalType() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="DecimalLiteralSuffix"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="DecimalLiteralSuffix"/> applied to.</param>
		public static ExponentialStyle GetExponentialStyle(this LiteralExpressionSyntax node)
		{
			return node.Token.GetExponentialStyle();
		}

		/// <summary>
		/// Returns the <see cref="IntegerValueType"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="IntegerValueType"/> represented by.</param>
		public static IntegerValueType GetIntegerType(this LiteralExpressionSyntax node)
		{
			return node.Token.GetIntegerType();
		}

		/// <summary>
		/// Returns the <see cref="IntegerValueType"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="PredefinedTypeSyntax"/> to get the <see cref="IntegerValueType"/> represented by.</param>
		public static IntegerValueType GetIntegerType(this PredefinedTypeSyntax node)
		{
			return node.Keyword.GetIntegerType();
		}

		/// <summary>
		/// Returns the <see cref="IntegerValueType"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="IntegerValueType"/> represented by.</param>
		public static IntegerValueType GetIntegerType(this TypeSyntax node)
		{
			return (node as PredefinedTypeSyntax)?.GetIntegerType() ?? default;
		}

		/// <summary>
		/// Returns the literal value of type <typeparamref name="T"/> the <paramref name="node"/> represents.
		/// </summary>
		/// <typeparam name="T">Type of literal value this <paramref name="node"/> represents.</typeparam>
		/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the literal value of.</param>
		public static T GetLiteralValue<T>(this LiteralExpressionSyntax node) where T : unmanaged
		{
			return node.Token.GetLiteralValue<T>();
		}

		/// <summary>
		/// Returns the literal value of type <typeparamref name="T"/> the <paramref name="node"/> represents.
		/// </summary>
		/// <typeparam name="T">Type of literal value this <paramref name="node"/> represents.</typeparam>
		/// <param name="node"><see cref="ExpressionSyntax"/> to get the literal value of.</param>
		public static T GetLiteralValue<T>(this ExpressionSyntax node) where T : unmanaged
		{
			return (node as LiteralExpressionSyntax)?.Token.GetLiteralValue<T>() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="NamespaceStyle"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="BaseNamespaceDeclarationSyntax"/> to get the <see cref="NamespaceStyle"/> applied to.</param>
		public static NamespaceStyle GetNamespaceStyle(this BaseNamespaceDeclarationSyntax node)
		{
			if(node is FileScopedNamespaceDeclarationSyntax)
			{
				return NamespaceStyle.File;
			}

			if(node.Parent is NamespaceDeclarationSyntax)
			{
				return NamespaceStyle.Nested;
			}

			return NamespaceStyle.Default;
		}

		public static OverloadableOperator GetOperator(this OperatorDeclarationSyntax node)
		{
		}

		public static OverloadableOperator GetOperator(this BinaryExpressionSyntax node)
		{

		}

		public static OverloadableOperator GetOperator(this PostfixUnaryExpressionSyntax node)
		{

		}

		public static OverloadableOperator GetOperator(this PrefixUnaryExpressionSyntax node)
		{

		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="PropertyDeclarationSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this PropertyDeclarationSyntax node)
		{
			return node.Type.GetRefKind();
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="IndexerDeclarationSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this IndexerDeclarationSyntax node)
		{
			return node.Type.GetRefKind();
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="MethodDeclarationSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this MethodDeclarationSyntax node)
		{
			return node.ReturnType.GetRefKind();
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="DelegateDeclarationSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this DelegateDeclarationSyntax node)
		{
			return node.ReturnType.GetRefKind();
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="ParenthesizedLambdaExpressionSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this ParenthesizedLambdaExpressionSyntax node)
		{
			return node.ReturnType?.GetRefKind() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="AnonymousFunctionExpressionSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this AnonymousFunctionExpressionSyntax node)
		{
			return (node as ParenthesizedLambdaExpressionSyntax)?.GetRefKind() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this LocalFunctionStatementSyntax node)
		{
			return node.ReturnType.GetRefKind();
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this TypeSyntax node)
		{
			return (node as RefTypeSyntax)?.GetRefKind() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="RefTypeSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this RefTypeSyntax node)
		{
			if(node.ReadOnlyKeyword != default)
			{
				return RefKind.RefReadOnly;
			}

			return RefKind.Ref;
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="PropertyDeclarationSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
		public static RefKind GetRefKind(this BaseParameterSyntax node)
		{
			return node.Type?.GetRefKind() ?? default;
		}

		/// <summary>
		/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="PredefinedTypeSyntax"/> to get the <see cref="TypeKeyword"/> represented by.</param>
		public static TypeKeyword GetTypeKeyword(this PredefinedTypeSyntax node)
		{
			return node.Keyword.GetTypeKeyword();
		}

		/// <summary>
		/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="TypeKeyword"/> represented by.</param>
		public static TypeKeyword GetTypeKeyword(this TypeSyntax node)
		{
			return (node as PredefinedTypeSyntax)?.GetTypeKeyword() ?? default;
		}
	}
}
