using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Durian.Analysis.Data.FromSource;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis;

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
		string name = directive.NamespaceOrType.ToString();

		if (compilationUnit.Usings.Any(u => u.NamespaceOrType.ToString() == name))
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
		HashSet<string> usings = new(compilationUnit.Usings.Where(u => u.Alias is null).Select(u => u.NamespaceOrType.ToString()));
		UsingDirectiveSyntax[] directives = namespaces
			.Where(n => n.Name != string.Empty)
			.Select(n => n.GetUsingDirective())
			.Where(n => usings.Add(n.NamespaceOrType.ToString()))
			.ToArray();

		if (directives.Length == 0)
		{
			return compilationUnit;
		}

		return compilationUnit.AddUsings(directives);
	}

	/// <summary>
	/// Determines whether the specified <see cref="SyntaxNode"/> can have accessibility modifiers applied.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine can have accessibility modifiers applied.</param>
	public static bool CanApplyAccessibility(this SyntaxNode node)
	{
		return node switch
		{
			ConstructorDeclarationSyntax ctor => !ctor.IsStatic(),

			BaseMethodDeclarationSyntax or
			BaseFieldDeclarationSyntax or
			BasePropertyDeclarationSyntax or
			DelegateDeclarationSyntax or
			BaseTypeDeclarationSyntax
				=> true,

			AccessorDeclarationSyntax accessor => accessor.IsPropertyAccessor(),

			_ => default
		};
	}

	/// <summary>
	/// Gets the first node of type <typeparamref name="TNode"/> that matches the <paramref name="predicate"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of ancestor node to return.</typeparam>
	/// <param name="node"><see cref="SyntaxNode"/> to get the ancestor of.</param>
	/// <param name="predicate">Function that filters the ancestor nodes.</param>
	/// <param name="ascendOutOfTrivia">Determine whether to leave from structured trivia.</param>
	public static TNode? FirstAncestor<TNode>(this SyntaxNode node, Func<TNode, bool>? predicate = default, bool ascendOutOfTrivia = true) where TNode : SyntaxNode
	{
		return node.Parent?.FirstAncestorOrSelf(predicate, ascendOutOfTrivia);
	}

	/// <summary>
	/// Gets the first node of type <typeparamref name="TNode"/> that matches the <paramref name="predicate"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of ancestor node to return.</typeparam>
	/// <typeparam name="TArg">Type of argument passed for each call to the <paramref name="predicate"/>.</typeparam>
	/// <param name="node"><see cref="SyntaxNode"/> to get the ancestor of.</param>
	/// <param name="predicate">Function that filters the ancestor nodes.</param>
	/// <param name="argument">Argument to pass for each <paramref name="predicate"/> call.</param>
	/// <param name="ascendOutOfTrivia">Determine whether to leave from structured trivia.</param>
	public static TNode? FirstAncestor<TNode, TArg>(this SyntaxNode node, Func<TNode, TArg, bool> predicate, TArg argument, bool ascendOutOfTrivia = true) where TNode : SyntaxNode
	{
		return node.Parent?.FirstAncestorOrSelf(predicate, argument, ascendOutOfTrivia);
	}

	/// <summary>
	/// Returns the <see cref="Accessibility"/> of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the accessibility of.</param>
	public static Accessibility GetAccessibility(this SyntaxNode node)
	{
		return node.GetModifiers().GetAccessibility();
	}

	/// <summary>
	/// Returns the kind of <see cref="AccessorKind"/> the specified <paramref name="node"/> represents.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the <see cref="AccessorKind"/> kind represented by.</param>
	public static AccessorKind GetAccessorKind(this AccessorDeclarationSyntax node)
	{
		return node.Keyword.GetAccessor();
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
	/// Returns the <see cref="SyntaxNode"/> that is accessed by using the specified <paramref name="target"/> in the context of the given <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the attribute target node of.</param>
	/// <param name="target">Kind of attribute target.</param>
	/// <remarks>
	/// <b>Note:</b> In some cases, the <see cref="AttributeTarget"/> refers to symbols that are compiler-generated, thus have no associated <see cref="SyntaxNode"/>s. In such situations, <see langword="null"/> is returned. This includes:
	/// <list type="bullet">
	/// <item><see cref="AttributeTarget.Assembly"/>.</item>
	/// <item><see cref="AttributeTarget.Module"/>.</item>
	/// <item><see cref="AttributeTarget.Return"/> for <see langword="set"/>, <see langword="init"/>, <see langword="add"/> and <see langword="remove"/> accessors.</item>
	/// <item><see cref="AttributeTarget.Param"/> for <see langword="set"/>, <see langword="init"/>, <see langword="add"/> and <see langword="remove"/> accessors.</item>
	/// <item><see cref="AttributeTarget.Field"/> for properties and events.</item>
	/// <item><see cref="AttributeTarget.Method"/> for events.</item>
	/// </list>
	/// </remarks>
	public static SyntaxNode? GetAttributeTarget(this SyntaxNode node, AttributeTarget target)
	{
		switch (target)
		{
			case AttributeTarget.Return:
				return node.GetReturnType();

			case AttributeTarget.Field:
				return node as FieldDeclarationSyntax;

			case AttributeTarget.Method:
				return node is
					BaseMethodDeclarationSyntax or
					LocalFunctionStatementSyntax or
					ParenthesizedLambdaExpressionSyntax or
					AccessorDeclarationSyntax
					? node : default;

			case AttributeTarget.Type:
				return node is
					BaseTypeDeclarationSyntax or
					DelegateDeclarationSyntax
					? node : default;

			case AttributeTarget.TypeVar:
				return node as TypeParameterSyntax;

			case AttributeTarget.Event:

				if (node is EventDeclarationSyntax or EventFieldDeclarationSyntax)
				{
					return node;
				}

				return default;

			case AttributeTarget.Param:
				return node as ParameterSyntax;

			case AttributeTarget.Property:
				return node as BasePropertyDeclarationSyntax;

			default:
				return default;
		}
	}

	/// <summary>
	/// Returns the <see cref="SyntaxNode"/> that is accessed by using the specified <paramref name="target"/> in the context of the given <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the attribute target node of.</param>
	/// <param name="target">Kind of attribute target.</param>
	public static SyntaxNode? GetAttributeTarget(this SyntaxNode node, AttributeTargetKind target)
	{
		return target switch
		{
			AttributeTargetKind.This => node is
				ParameterSyntax or
				TypeParameterSyntax or
				BaseFieldDeclarationSyntax or
				BasePropertyDeclarationSyntax or
				BaseTypeDeclarationSyntax or
				BaseMethodDeclarationSyntax or
				LocalFunctionStatementSyntax or
				ParenthesizedLambdaExpressionSyntax or
				AccessorDeclarationSyntax
				? node : default,

			AttributeTargetKind.Value => node is not BasePropertyDeclarationSyntax ? node.GetReturnType() : default,

			// All members resolved using AttributeTargetKind.Handler are compiler-generated.
			// AttributeTargetKind.Handler =>

			_ => default
		};
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
	/// Returns the <see cref="AttributeTargetKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AttributeSyntax"/> to get the <see cref="AttributeTargetKind"/> associated with.</param>
	public static AttributeTargetKind GetAttributeTargetKind(this AttributeTargetSpecifierSyntax node)
	{
		if (node.Parent?.Parent is not SyntaxNode decl)
		{
			return default;
		}

		AttributeTarget target = node.GetAttributeTarget();

		if (decl.GetAttributeTarget(target) is SyntaxNode targetNode)
		{
			return targetNode == decl ? AttributeTargetKind.This : AttributeTargetKind.Value;
		}

		return default;
	}

	/// <summary>
	/// Returns the <see cref="AttributeTargetKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AttributeSyntax"/> to get the <see cref="AttributeTargetKind"/> associated with.</param>
	public static AttributeTargetKind GetAttributeTargetKind(this AttributeListSyntax node)
	{
		return node.Target?.GetAttributeTargetKind() ?? default;
	}

	/// <summary>
	/// Returns the <see cref="AttributeTargetKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AttributeSyntax"/> to get the <see cref="AttributeTargetKind"/> associated with.</param>
	public static AttributeTargetKind GetAttributeTargetKind(this AttributeSyntax node)
	{
		return (node.Parent as AttributeListSyntax)?.GetAttributeTargetKind() ?? default;
	}

	/// <summary>
	/// Returns the <see cref="AutoPropertyKind"/> of the specified <see cref="BasePropertyDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="BasePropertyDeclarationSyntax"/> to get the <see cref="AutoPropertyKind"/> of.</param>
	public static AutoPropertyKind GetAutoPropertyKind(this BasePropertyDeclarationSyntax node)
	{
		return (node as PropertyDeclarationSyntax)?.GetAutoPropertyKind() ?? default;
	}

	/// <summary>
	/// Returns the <see cref="AutoPropertyKind"/> of the specified <see cref="PropertyDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="PropertyDeclarationSyntax"/> to get the <see cref="AutoPropertyKind"/> of.</param>
	public static AutoPropertyKind GetAutoPropertyKind(this PropertyDeclarationSyntax node)
	{
		return node.AccessorList?.GetAutoPropertyKind() ?? default;
	}

	/// <summary>
	/// Returns the <see cref="AutoPropertyKind"/> of the specified <see cref="AccessorListSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="AccessorListSyntax"/> to get the <see cref="AutoPropertyKind"/> of.</param>
	public static AutoPropertyKind GetAutoPropertyKind(this AccessorListSyntax node)
	{
		if (!node.Accessors.Any() || node.Accessors.Count > 2)
		{
			return default;
		}

		PropertyAccessorKind first = node.Accessors[0].GetPropertyAccessorKind();

		if (node.Accessors.Count == 1)
		{
			return first.GetAutoPropertyKind();
		}

		PropertyAccessorKind second = node.Accessors[1].GetPropertyAccessorKind();

		return second switch
		{
			PropertyAccessorKind.Set => AutoPropertyKind.GetSet,
			PropertyAccessorKind.Init => AutoPropertyKind.GetInit,
			_ => default
		};
	}

	/// <summary>
	/// Returns the block body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the block body of.</param>
	public static BlockSyntax? GetBlock(this SyntaxNode node)
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
	/// Returns the block body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="StatementSyntax"/> to get the block body of.</param>
	public static BlockSyntax? GetBlock(this StatementSyntax node)
	{
		return node switch
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
	/// Returns the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to get the body of.</param>
	public static SyntaxNode? GetBody(this BaseMethodDeclarationSyntax node)
	{
		return node.Body ?? (SyntaxNode?)node.ExpressionBody;
	}

	/// <summary>
	/// Returns the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to get the body of.</param>
	public static SyntaxNode? GetBody(this LocalFunctionStatementSyntax node)
	{
		return node.Body ?? (SyntaxNode?)node.ExpressionBody;
	}

	/// <summary>
	/// Returns the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the body of.</param>
	public static SyntaxNode? GetBody(this AccessorDeclarationSyntax node)
	{
		return node.Body ?? (SyntaxNode?)node.ExpressionBody;
	}

	/// <summary>
	/// Returns the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AnonymousFunctionExpressionSyntax"/> to get the body of.</param>
	public static SyntaxNode? GetBody(this AnonymousFunctionExpressionSyntax node)
	{
		return node.Body ?? (SyntaxNode?)node.ExpressionBody;
	}

	/// <summary>
	/// Returns type of the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to get the type of body of.</param>
	public static MethodStyle GetBodyType(this BaseMethodDeclarationSyntax node)
	{
		if (node.Body is not null)
		{
			return MethodStyle.Block;
		}

		if (node.ExpressionBody is not null)
		{
			return MethodStyle.Expression;
		}

		return default;
	}

	/// <summary>
	/// Returns type of the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the type of body of.</param>
	public static MethodStyle GetBodyType(this AccessorDeclarationSyntax node)
	{
		if (node.Body is not null)
		{
			return MethodStyle.Block;
		}

		if (node.ExpressionBody is not null)
		{
			return MethodStyle.Expression;
		}

		return default;
	}

	/// <summary>
	/// Returns type of the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to get the type of body of.</param>
	public static MethodStyle GetBodyType(this LocalFunctionStatementSyntax node)
	{
		if (node.Body is not null)
		{
			return MethodStyle.Block;
		}

		if (node.ExpressionBody is not null)
		{
			return MethodStyle.Expression;
		}

		return default;
	}

	/// <summary>
	/// Returns type of the body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="AnonymousFunctionExpressionSyntax"/> to get the type of body of.</param>
	public static LambdaStyle GetBodyType(this AnonymousFunctionExpressionSyntax node)
	{
		if (node is AnonymousMethodExpressionSyntax)
		{
			return LambdaStyle.Method;
		}

		if (node.Body is not null)
		{
			return LambdaStyle.Block;
		}

		if (node.ExpressionBody is not null)
		{
			return LambdaStyle.Expression;
		}

		return default;
	}

	/// <summary>
	/// Returns the <see cref="TypeParameterConstraintClauseSyntax"/> associated with the specified <see cref="TypeParameterSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterSyntax"/> to get the <see cref="TypeParameterConstraintClauseSyntax"/> associated with.</param>
	public static TypeParameterConstraintClauseSyntax? GetConstraintClause(this TypeParameterSyntax node)
	{
		return node.Parent?.Parent switch
		{
			TypeDeclarationSyntax type => GetNode(type.ConstraintClauses),
			MethodDeclarationSyntax method => GetNode(method.ConstraintClauses),
			DelegateDeclarationSyntax @delegate => GetNode(@delegate.ConstraintClauses),
			LocalFunctionStatementSyntax local => GetNode(local.ConstraintClauses),
			_ => default
		};

		TypeParameterConstraintClauseSyntax? GetNode(SyntaxList<TypeParameterConstraintClauseSyntax> clauses)
		{
			return clauses.FirstOrDefault(c => c.Name.Identifier.Value == node.Identifier.Value);
		}
	}

	/// <summary>
	/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterConstraintSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static GenericConstraint GetConstraints(this TypeParameterConstraintSyntax node, bool includeImplicit = false)
	{
		switch (node)
		{
			case ClassOrStructConstraintSyntax classOrStruct:

				if (classOrStruct.IsClass())
				{
					return GenericConstraint.Class;
				}

				if (classOrStruct.IsStruct())
				{
					if (includeImplicit)
					{
						return GenericConstraint.Struct | GenericConstraint.New;
					}

					return GenericConstraint.Struct;
				}

				return default;

			case TypeConstraintSyntax type:

				if (type.Type.IsNotNull)
				{
					return GenericConstraint.NotNull;
				}

				if (type.Type.IsUnmanaged)
				{
					if (includeImplicit)
					{
						return GenericConstraint.Unmanaged | GenericConstraint.Struct | GenericConstraint.New;
					}

					return GenericConstraint.Unmanaged;
				}

				if (includeImplicit)
				{
					return GenericConstraint.Type | GenericConstraint.Class;
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
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static GenericConstraint GetConstraints(this TypeParameterConstraintClauseSyntax node, bool includeImplicit = false)
	{
		GenericConstraint constraint = default;

		foreach (TypeParameterConstraintSyntax syntax in node.Constraints)
		{
			constraint |= syntax.GetConstraints(includeImplicit);
		}

		return constraint;
	}

	/// <summary>
	/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static GenericConstraint GetConstraints(this TypeParameterSyntax node, bool includeImplicit = false)
	{
		return node.GetConstraintClause()?.GetConstraints(includeImplicit) ?? default;
	}

	/// <summary>
	/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeDeclarationSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static GenericConstraint[] GetConstraints(this TypeDeclarationSyntax node, bool includeImplicit = false)
	{
		return node.ConstraintClauses.Select(c => c.GetConstraints(includeImplicit)).ToArray();
	}

	/// <summary>
	/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="DelegateDeclarationSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static GenericConstraint[] GetConstraints(this DelegateDeclarationSyntax node, bool includeImplicit = false)
	{
		return node.ConstraintClauses.Select(c => c.GetConstraints(includeImplicit)).ToArray();
	}

	/// <summary>
	/// Returns the <see cref="GenericConstraint"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="MethodDeclarationSyntax"/> to get the <see cref="GenericConstraint"/> associated with.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static GenericConstraint[] GetConstraints(this MethodDeclarationSyntax node, bool includeImplicit = false)
	{
		return node.ConstraintClauses.Select(c => c.GetConstraints(includeImplicit)).ToArray();
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
	/// Returns the kind of the specified <see cref="ConstructorDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="ConstructorDeclarationSyntax"/> to get the kind of.</param>
	public static SpecialConstructor GetConstructorKind(this ConstructorDeclarationSyntax node)
	{
		if (node.ParameterList.IsParameterless())
		{
			if (node.IsStatic())
			{
				return SpecialConstructor.Static;
			}

			return SpecialConstructor.Parameterless;
		}

		if (node.ParameterList.Parameters.Count != 1)
		{
			return default;
		}

		ParameterSyntax parameter = node.ParameterList.Parameters[0];

		if (parameter.Type is IdentifierNameSyntax name && name.Identifier.IsEquivalentTo(node.Identifier))
		{
			return SpecialConstructor.Copy;
		}

		return default;
	}

	/// <summary>
	/// Returns the <see cref="BaseNamespaceDeclarationSyntax"/> that contains the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the containing <see cref="BaseNamespaceDeclarationSyntax"/> of.</param>
	public static BaseNamespaceDeclarationSyntax? GetContainingNamespace(this SyntaxNode node)
	{
		return node.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
	}

	/// <summary>
	/// Returns names of namespaces that contain the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the containing namespaces of.</param>
	/// <param name="order">Specifies ordering of the returned values.</param>
	public static IReturnOrderEnumerable<string> GetContainingNamespaces(this SyntaxNode node, ReturnOrder order = ReturnOrder.ParentToChild)
	{
		return Yield().OrderBy(order, ReturnOrder.ParentToChild);

		IEnumerable<string> Yield()
		{
			SyntaxNode? current = node;

			while ((current = current!.Parent) is not null)
			{
				if (current is BaseNamespaceDeclarationSyntax decl)
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
	/// Returns the <see cref="BaseTypeDeclarationSyntax"/> that contains the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the containing <see cref="BaseTypeDeclarationSyntax"/> of.</param>
	public static BaseTypeDeclarationSyntax? GetContainingType(this SyntaxNode node)
	{
		return node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();
	}

	/// <summary>
	/// Returns <see cref="BaseTypeDeclarationSyntax"/>es that contain the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the containing <see cref="BaseTypeDeclarationSyntax"/>es of.</param>
	/// <param name="order">Specifies ordering of the returned values.</param>
	public static IReturnOrderEnumerable<BaseTypeDeclarationSyntax> GetContainingTypes(this SyntaxNode node, ReturnOrder order = ReturnOrder.ParentToChild)
	{
		return Yield().OrderBy(order, ReturnOrder.ParentToChild);

		IEnumerable<BaseTypeDeclarationSyntax> Yield()
		{
			SyntaxNode? current = node;

			while ((current = current!.Parent) is not null)
			{
				if (current is BaseTypeDeclarationSyntax type)
				{
					yield return type;
				}
			}
		}
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
	/// Returns a keyword used to declare the specified <see cref="MemberDeclarationSyntax"/> (e.g. <see langword="class"/>, <see langword="event"/>).
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to get the keyword of.</param>
	public static string? GetDeclaredKeyword(this MemberDeclarationSyntax node)
	{
		return node switch
		{
			BaseTypeDeclarationSyntax type => type.GetDeclaredKeyword(),
			EventDeclarationSyntax or EventFieldDeclarationSyntax => "event",
			OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax => "operator",
			DelegateDeclarationSyntax => "delegate",
			_ => default
		};
	}

	/// <summary>
	/// Returns a keyword used to declare the specified <see cref="BaseTypeDeclarationSyntax"/>(e.g. <see langword="class"/>, <see langword="enum"/>).
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to get the keyword of.</param>
	public static string? GetDeclaredKeyword(this BaseTypeDeclarationSyntax node)
	{
		return node switch
		{
			ClassDeclarationSyntax => "class",
			StructDeclarationSyntax => "struct",
			EnumDeclarationSyntax => "enum",
			InterfaceDeclarationSyntax => "interface",
			RecordDeclarationSyntax record => (SyntaxKind)record.ClassOrStructKeyword.RawKind switch
			{
				SyntaxKind.ClassKeyword => "record class",
				SyntaxKind.StructKeyword => "record struct",
				_ => "record"
			},
			_ => default
		};
	}

	/// <summary>
	/// Returns the default <see cref="Accessibility"/> that is applied to the <paramref name="node"/> during semantic analysis, that is:
	/// <list type="bullet">
	/// <item>For top-level types: <see cref="Accessibility.Internal"/>.</item>
	/// <item>For interface members other than partial methods: <see cref="Accessibility.Public"/>.</item>
	/// <item>For property/event accessors: accessibility of the parent property/event.</item>
	/// <item>For all other members: <see cref="Accessibility.Private"/>.</item>
	/// </list>
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the default accessibility of.</param>
	/// <param name="includeAssociated">Determines whether accessibility of an associated member of the <paramref name="node"/> (e.g. parent property of an accessor) should be treated as default.</param>
	public static Accessibility GetDefaultAccessibility(this SyntaxNode node, bool includeAssociated = true)
	{
		if (node.IsTopLevelOrInNamespace())
		{
			return node.IsTypeDeclaration() ? Accessibility.Internal : default;
		}

		if (node.GetContainingType() is InterfaceDeclarationSyntax)
		{
			if (node is MethodDeclarationSyntax method && method.IsPartial())
			{
				return Accessibility.Private;
			}

			return Accessibility.Public;
		}

		if (includeAssociated && node is AccessorDeclarationSyntax accessor)
		{
			return accessor.GetProperty()?.GetAccessibility() ?? default;
		}

		if (node.CanApplyAccessibility())
		{
			return Accessibility.Private;
		}

		return default;
	}

	/// <summary>
	/// Returns the effective <see cref="Accessibility"/> of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the effective <see cref="Accessibility"/> of.</param>
	public static Accessibility GetEffectiveAccessibility(this SyntaxNode node)
	{
		SyntaxNode? n = node;
		Accessibility lowest = Accessibility.Public;

		while (n is not null)
		{
			Accessibility current = n.GetAccessibility();

			if (current == Accessibility.Private)
			{
				return current;
			}

			if (current != Accessibility.NotApplicable && current < lowest)
			{
				lowest = current;
			}

			n = n.Parent;
		}

		return lowest;
	}

	/// <summary>
	/// Returns element <see cref="TypeSyntax"/> of the specified <paramref name="node"/>. Node kinds with element types are:
	/// <list type="bullet">
	/// <item><see cref="ArrayTypeSyntax"/></item>
	/// <item><see cref="NullableTypeSyntax"/></item>
	/// <item><see cref="PointerTypeSyntax"/></item>
	/// </list>
	/// </summary>
	/// <param name="node"><see cref="TypeSyntax"/> to get the element type of.</param>
	public static TypeSyntax? GetElementType(this TypeSyntax node)
	{
		return node switch
		{
			ArrayTypeSyntax array => array.ElementType,
			PointerTypeSyntax pointer => pointer.ElementType,
			NullableTypeSyntax nullable => nullable.ElementType,
			_ => default
		};
	}

	/// <summary>
	/// Returns the kind of <see cref="EventAccessorKind"/> the specified <paramref name="node"/> represents.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the <see cref="AccessorKind"/> kind represented by.</param>
	public static EventAccessorKind GetEventAccessorKind(this AccessorDeclarationSyntax node)
	{
		return node.GetAccessorKind().GetEventAccessorKind();
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
	/// Returns the expression body of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the expression body of.</param>
	public static ArrowExpressionClauseSyntax? GetExpressionBody(this SyntaxNode node)
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
	/// Returns the <see cref="GoToKind"/> of the specified <see cref="GotoStatementSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="GotoStatementSyntax"/> to get the <see cref="GoToKind"/> of.</param>
	public static GoToKind GetGoToKind(this GotoStatementSyntax node)
	{
		return ((SyntaxKind)node.RawKind).GetGoToKind();
	}

	/// <summary>
	/// Returns a collection of all inner types of the specified <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to get the inner types of.</param>
	/// <param name="includeSelf">Determines whether to include the <paramref name="node"/> in the returned collection.</param>
	public static IEnumerable<BaseTypeDeclarationSyntax> GetInnerTypes(this BaseTypeDeclarationSyntax node, bool includeSelf = false)
	{
		return (node as TypeDeclarationSyntax)?.GetInnerTypes(includeSelf) ?? Array.Empty<BaseTypeDeclarationSyntax>();
	}

	/// <summary>
	/// Returns a collection of all inner types of the specified <see cref="TypeDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeDeclarationSyntax"/> to get the inner types of.</param>
	/// <param name="includeSelf">Determines whether to include the <paramref name="node"/> in the returned collection.</param>
	public static IEnumerable<BaseTypeDeclarationSyntax> GetInnerTypes(this TypeDeclarationSyntax node, bool includeSelf = false)
	{
		const int CAPACITY = 32;

		if (includeSelf)
		{
			yield return node;
		}

		BaseTypeDeclarationSyntax[] members = node.Members.OfType<BaseTypeDeclarationSyntax>().ToArray();

		if (members.Length == 0)
		{
			yield break;
		}

		Stack<BaseTypeDeclarationSyntax> stack = new(members.Length > CAPACITY ? members.Length : CAPACITY);

		foreach (BaseTypeDeclarationSyntax t in members)
		{
			stack.Push(t);
		}

		while (stack.Count > 0)
		{
			BaseTypeDeclarationSyntax t = stack.Pop();
			yield return t;

			if (t is not TypeDeclarationSyntax decl)
			{
				continue;
			}

			foreach (BaseTypeDeclarationSyntax child in decl.Members.OfType<BaseTypeDeclarationSyntax>().Reverse())
			{
				stack.Push(child);
			}
		}
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
	/// Returns the <see cref="LiteralKind"/> of the specified <see cref="ExpressionSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="ExpressionSyntax"/> to get the <see cref="LiteralKind"/> of.</param>
	public static LiteralKind GetLiteralKind(this ExpressionSyntax node)
	{
		return (node as LiteralExpressionSyntax)?.GetLiteralKind() ?? default;
	}

	/// <summary>
	/// Returns the <see cref="LiteralKind"/> of the specified <see cref="TypeSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="LiteralKind"/> of.</param>
	public static LiteralKind GetLiteralKind(this TypeSyntax node)
	{
		return (node as PredefinedTypeSyntax)?.GetLiteralKind() ?? default;
	}

	/// <summary>
	/// Returns the <see cref="LiteralKind"/> of the specified <see cref="PredefinedTypeSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="PredefinedTypeSyntax"/> to get the <see cref="LiteralKind"/> of.</param>
	public static LiteralKind GetLiteralKind(this PredefinedTypeSyntax node)
	{
		return (SyntaxKind)node.RawKind switch
		{
			SyntaxKind.BoolKeyword => LiteralKind.False,
			SyntaxKind.StringKeyword => LiteralKind.String,
			SyntaxKind.CharKeyword => LiteralKind.Character,
			SyntaxKind.ObjectKeyword => LiteralKind.Null,

			SyntaxKind.IntKeyword or
			SyntaxKind.UIntKeyword or
			SyntaxKind.LongKeyword or
			SyntaxKind.ULongKeyword or
			SyntaxKind.ShortKeyword or
			SyntaxKind.UShortKeyword or
			SyntaxKind.ByteKeyword or
			SyntaxKind.SByteKeyword or
			SyntaxKind.FloatKeyword or
			SyntaxKind.DoubleKeyword or
			SyntaxKind.DecimalKeyword
				=> LiteralKind.Number,

			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="LiteralKind"/> of the specified <see cref="LiteralExpressionSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="LiteralKind"/> of.</param>
	public static LiteralKind GetLiteralKind(this LiteralExpressionSyntax node)
	{
		return (SyntaxKind)node.RawKind switch
		{
			SyntaxKind.NumericLiteralToken => LiteralKind.Number,
			SyntaxKind.CharacterLiteralToken => LiteralKind.Character,
			SyntaxKind.StringLiteralToken => LiteralKind.String,
			SyntaxKind.DefaultKeyword => LiteralKind.Default,
			SyntaxKind.NullKeyword => LiteralKind.Null,
			SyntaxKind.FalseKeyword => LiteralKind.False,
			SyntaxKind.TrueKeyword => LiteralKind.True,
			SyntaxKind.ArgListKeyword => LiteralKind.ArgList,
			_ => default
		};
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
	/// Returns new instance of <see cref="IMemberData"/> associated with the specified <paramref name="member"/>.
	/// </summary>
	/// <param name="member"><see cref="MemberDeclarationSyntax"/> to get the data of.</param>
	/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
	public static IMemberData GetMemberData(this SyntaxNode member, ICompilationData compilation)
	{
		return member switch
		{
			ClassDeclarationSyntax => new ClassData((ClassDeclarationSyntax)member, compilation),
			StructDeclarationSyntax => new StructData((StructDeclarationSyntax)member, compilation),
			InterfaceDeclarationSyntax => new InterfaceData((InterfaceDeclarationSyntax)member, compilation),
			RecordDeclarationSyntax => new RecordData((RecordDeclarationSyntax)member, compilation),
			EnumDeclarationSyntax => new EnumData((EnumDeclarationSyntax)member, compilation),
			MethodDeclarationSyntax => new MethodData((MethodDeclarationSyntax)member, compilation),
			FieldDeclarationSyntax => new FieldData((FieldDeclarationSyntax)member, compilation, 0),
			PropertyDeclarationSyntax => new PropertyData((PropertyDeclarationSyntax)member, compilation),
			BaseNamespaceDeclarationSyntax => new NamespaceData((BaseNamespaceDeclarationSyntax)member, compilation),
			VariableDeclaratorSyntax variable => variable.Parent?.Parent switch
			{
				FieldDeclarationSyntax field => new FieldData(field, compilation, new FieldData.Properties() { Variable = variable }),
				EventFieldDeclarationSyntax @event => new EventData(@event, compilation, new EventData.Properties() { Variable = variable }),
				LocalDeclarationStatementSyntax local => new LocalData(local, compilation, new LocalData.Properties() { Variable = variable }),
				_ => new MemberData(variable, compilation)
			},
			EventDeclarationSyntax => new EventData((EventDeclarationSyntax)member, compilation),
			EventFieldDeclarationSyntax => new EventData((EventFieldDeclarationSyntax)member, compilation, 0),
			DelegateDeclarationSyntax => new DelegateData((DelegateDeclarationSyntax)member, compilation),
			ParameterSyntax => new ParameterData((ParameterSyntax)member, compilation),
			TypeParameterSyntax => new TypeParameterData((TypeParameterSyntax)member, compilation),
			IndexerDeclarationSyntax => new IndexerData((IndexerDeclarationSyntax)member, compilation),
			ConstructorDeclarationSyntax => new ConstructorData((ConstructorDeclarationSyntax)member, compilation),
			DestructorDeclarationSyntax => new DestructorData((DestructorDeclarationSyntax)member, compilation),
			OperatorDeclarationSyntax => new OperatorData((OperatorDeclarationSyntax)member, compilation),
			ConversionOperatorDeclarationSyntax => new ConversionOperatorData((ConversionOperatorDeclarationSyntax)member, compilation),
			LocalFunctionStatementSyntax => new LocalFunctionData((LocalFunctionStatementSyntax)member, compilation),
			LocalDeclarationStatementSyntax => new LocalData((LocalDeclarationStatementSyntax)member, compilation, 0),

			_ => new MemberData(member, compilation),
		};
	}

	/// <summary>
	/// Returns the <see cref="MethodKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the <see cref="TypeKind"/> associated with.</param>
	public static MethodKind GetMethodKind(this SyntaxNode node)
	{
		return node switch
		{
			BaseMethodDeclarationSyntax method => method.GetMethodKind(),
			AnonymousFunctionExpressionSyntax => MethodKind.AnonymousFunction,
			LocalFunctionStatementSyntax => MethodKind.LocalFunction,
			FunctionPointerTypeSyntax or FunctionPointerParameterListSyntax or FunctionPointerCallingConventionSyntax => MethodKind.FunctionPointerSignature,
			DelegateDeclarationSyntax => MethodKind.DelegateInvoke,
			AccessorDeclarationSyntax accessor => accessor.GetAccessorKind() switch
			{
				AccessorKind.Get => MethodKind.PropertyGet,
				AccessorKind.Set or AccessorKind.Init => MethodKind.PropertySet,
				AccessorKind.Add => MethodKind.EventAdd,
				AccessorKind.Remove => MethodKind.EventRemove,
				_ => default
			},
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="TypeKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to get the <see cref="MethodKind"/> associated with.</param>
	public static MethodKind GetMethodKind(this BaseMethodDeclarationSyntax node)
	{
		return node switch
		{
			MethodDeclarationSyntax method => method.ExplicitInterfaceSpecifier is null ? MethodKind.Ordinary : MethodKind.ExplicitInterfaceImplementation,
			OperatorDeclarationSyntax => MethodKind.UserDefinedOperator,
			ConversionOperatorDeclarationSyntax => MethodKind.Conversion,
			ConstructorDeclarationSyntax ctor => ctor.IsStatic() ? MethodKind.StaticConstructor : MethodKind.Constructor,
			DestructorDeclarationSyntax => MethodKind.Destructor,
			_ => default
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

		foreach (MemberDeclarationSyntax d in decl)
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
	/// Returns the <see cref="NamespaceStyle"/> applied to the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseNamespaceDeclarationSyntax"/> to get the <see cref="NamespaceStyle"/> applied to.</param>
	public static NamespaceStyle GetNamespaceStyle(this BaseNamespaceDeclarationSyntax node)
	{
		if (node is FileScopedNamespaceDeclarationSyntax)
		{
			return NamespaceStyle.File;
		}

		if (node.Parent is NamespaceDeclarationSyntax)
		{
			return NamespaceStyle.Nested;
		}

		return NamespaceStyle.Default;
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
	public static NumericLiteralSuffix GetNumericSuffix(this LiteralExpressionSyntax node)
	{
		return node.Token.GetNumericSuffix();
	}

	/// <summary>
	/// Returns the <see cref="OverloadableOperator"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="ExpressionSyntax"/> to get the <see cref="OverloadableOperator"/> represented by.</param>
	public static OverloadableOperator GetOperatorType(this ExpressionSyntax node)
	{
		return node switch
		{
			BinaryExpressionSyntax binary => binary.GetOperatorType(),
			PostfixUnaryExpressionSyntax postfix => postfix.GetOperatorType(),
			PrefixUnaryExpressionSyntax prefix => prefix.GetOperatorType(),
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="OverloadableOperator"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="OperatorDeclarationSyntax"/> to get the <see cref="OverloadableOperator"/> represented by.</param>
	public static OverloadableOperator GetOperatorType(this OperatorDeclarationSyntax node)
	{
		return (SyntaxKind)node.OperatorToken.RawKind switch
		{
			SyntaxKind.PlusToken => node.ParameterList.Parameters.Count > 1 ? OverloadableOperator.Addition : OverloadableOperator.UnaryPlus,
			SyntaxKind.MinusToken => node.ParameterList.Parameters.Count > 1 ? OverloadableOperator.Subtraction : OverloadableOperator.UnaryMinus,
			_ => node.OperatorToken.GetOperator()
		};
	}

	/// <summary>
	/// Returns the <see cref="OverloadableOperator"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BinaryExpressionSyntax"/> to get the <see cref="OverloadableOperator"/> represented by.</param>
	public static OverloadableOperator GetOperatorType(this BinaryExpressionSyntax node)
	{
		return (SyntaxKind)node.RawKind switch
		{
			SyntaxKind.AddExpression or
			SyntaxKind.AddAssignmentExpression
				=> OverloadableOperator.Addition,

			SyntaxKind.SubtractExpression or
			SyntaxKind.SubtractAssignmentExpression
				=> OverloadableOperator.Subtraction,

			SyntaxKind.MultiplyExpression or
			SyntaxKind.MultiplyAssignmentExpression
				=> OverloadableOperator.Multiplication,

			SyntaxKind.DivideExpression or
			SyntaxKind.DivideAssignmentExpression
				=> OverloadableOperator.Division,

			SyntaxKind.ModuloExpression or
			SyntaxKind.ModuloAssignmentExpression
				=> OverloadableOperator.Remainder,

			SyntaxKind.EqualsExpression
				=> OverloadableOperator.Equality,

			SyntaxKind.NotEqualsExpression
				=> OverloadableOperator.Inequality,

			SyntaxKind.ExclusiveOrExpression or
			SyntaxKind.ExclusiveOrAssignmentExpression
				=> OverloadableOperator.LogicalXor,

			SyntaxKind.LogicalAndExpression or
			SyntaxKind.BitwiseAndExpression or
			SyntaxKind.AndAssignmentExpression
				=> OverloadableOperator.LogicalAnd,

			SyntaxKind.LogicalOrExpression or
			SyntaxKind.BitwiseOrExpression or
			SyntaxKind.OrAssignmentExpression
				=> OverloadableOperator.LogicalOr,

			SyntaxKind.GreaterThanExpression
				=> OverloadableOperator.GreaterThan,

			SyntaxKind.GreaterThanOrEqualExpression
				=> OverloadableOperator.GreaterThanOrEqual,

			SyntaxKind.LessThanExpression
				=> OverloadableOperator.LessThan,

			SyntaxKind.LessThanOrEqualExpression
				=> OverloadableOperator.LessThanOrEqual,

			SyntaxKind.RightShiftExpression or
			SyntaxKind.RightShiftAssignmentExpression
				=> OverloadableOperator.RightShift,

			SyntaxKind.LeftShiftExpression or
			SyntaxKind.LeftShiftAssignmentExpression
				=> OverloadableOperator.LeftShift,

			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="OverloadableOperator"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="PostfixUnaryExpressionSyntax"/> to get the <see cref="OverloadableOperator"/> represented by.</param>
	public static OverloadableOperator GetOperatorType(this PostfixUnaryExpressionSyntax node)
	{
		return (SyntaxKind)node.RawKind switch
		{
			SyntaxKind.PostIncrementExpression => OverloadableOperator.Increment,
			SyntaxKind.PostDecrementExpression => OverloadableOperator.Decrement,
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="OverloadableOperator"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="PrefixUnaryExpressionSyntax"/> to get the <see cref="OverloadableOperator"/> represented by.</param>
	public static OverloadableOperator GetOperatorType(this PrefixUnaryExpressionSyntax node)
	{
		return (SyntaxKind)node.RawKind switch
		{
			SyntaxKind.PreIncrementExpression => OverloadableOperator.Increment,
			SyntaxKind.PreDecrementExpression => OverloadableOperator.Decrement,
			SyntaxKind.UnaryPlusExpression => OverloadableOperator.UnaryPlus,
			SyntaxKind.UnaryMinusExpression => OverloadableOperator.UnaryMinus,
			SyntaxKind.LogicalNotExpression => OverloadableOperator.Negation,
			SyntaxKind.BitwiseNotExpression => OverloadableOperator.Complement,
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="BasePropertyDeclarationSyntax"/> associated with the specified <see cref="AccessorDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the associated <see cref="BasePropertyDeclarationSyntax"/> of.</param>
	public static BasePropertyDeclarationSyntax? GetProperty(this AccessorDeclarationSyntax node)
	{
		return node?.Parent?.Parent as BasePropertyDeclarationSyntax;
	}

	/// <summary>
	/// Returns the <see cref="BasePropertyDeclarationSyntax"/> associated with the specified <see cref="AccessorListSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="AccessorListSyntax"/> to get the associated <see cref="BasePropertyDeclarationSyntax"/> of.</param>
	public static BasePropertyDeclarationSyntax? GetProperty(this AccessorListSyntax node)
	{
		return node.Parent as BasePropertyDeclarationSyntax;
	}

	/// <summary>
	/// Returns the kind of <see cref="PropertyAccessorKind"/> the specified <paramref name="node"/> represents.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the <see cref="AccessorKind"/> kind represented by.</param>
	public static PropertyAccessorKind GetPropertyAccessorKind(this AccessorDeclarationSyntax node)
	{
		return node.GetAccessorKind().GetPropertyAccessorKind();
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
	/// <param name="node"><see cref="LocalDeclarationStatementSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
	public static RefKind GetRefKind(this LocalDeclarationStatementSyntax node)
	{
		return node.Declaration.GetRefKind();
	}

	/// <summary>
	/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="VariableDeclarationSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
	public static RefKind GetRefKind(this VariableDeclarationSyntax node)
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
	/// <param name="node"><see cref="ParameterSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
	public static RefKind GetRefKind(this ParameterSyntax node)
	{
		SyntaxTokenList list = node.Modifiers;

		for (int i = 0; i < list.Count; i++)
		{
			SyntaxToken token = list[i];

			switch ((SyntaxKind)token.RawKind)
			{
				case SyntaxKind.InKeyword:
					return RefKind.In;

				case SyntaxKind.RefKeyword:
					return RefKind.Ref;

				case SyntaxKind.OutKeyword:
					return RefKind.Out;
			}
		}

		return default;
	}

	/// <summary>
	/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="FunctionPointerParameterSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
	public static RefKind GetRefKind(this FunctionPointerParameterSyntax node)
	{
		SyntaxTokenList list = node.Modifiers;

		for (int i = 0; i < list.Count; i++)
		{
			SyntaxToken token = list[i];

			switch ((SyntaxKind)token.RawKind)
			{
				case SyntaxKind.RefKeyword:

					if (i < list.Count - 1 && (SyntaxKind)list[i + 1].RawKind == SyntaxKind.ReadOnlyKeyword)
					{
						return RefKind.RefReadOnly;
					}

					return RefKind.Ref;

				case SyntaxKind.OutKeyword:
					return RefKind.Out;
			}
		}

		return default;
	}

	/// <summary>
	/// Returns the <see cref="RefKind"/> applied to the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseParameterSyntax"/> to get the <see cref="RefKind"/> applied to.</param>
	public static RefKind GetRefKind(this BaseParameterSyntax node)
	{
		return node switch
		{
			ParameterSyntax parameter => parameter.GetRefKind(),
			FunctionPointerParameterSyntax pointer => pointer.GetRefKind(),
			_ => default
		};
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
		if (node.ReadOnlyKeyword != default)
		{
			return RefKind.RefReadOnly;
		}

		return RefKind.Ref;
	}

	/// <summary>
	/// Returns the return type of the specified <see cref="SyntaxNode"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the return type of.</param>
	public static TypeSyntax? GetReturnType(this SyntaxNode node)
	{
		return node switch
		{
			BaseMethodDeclarationSyntax method => method.GetReturnType(),
			DelegateDeclarationSyntax @delegate => @delegate.ReturnType,
			AccessorDeclarationSyntax accessor => accessor.GetReturnType(),
			LocalFunctionStatementSyntax local => local.ReturnType,
			ParenthesizedLambdaExpressionSyntax lambda => lambda.ReturnType,
			BasePropertyDeclarationSyntax property => property.Type,
			_ => default
		};
	}

	/// <summary>
	/// Returns the return type of the specified <see cref="BaseMethodDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to get the return type of.</param>
	public static TypeSyntax? GetReturnType(this BaseMethodDeclarationSyntax node)
	{
		return node switch
		{
			MethodDeclarationSyntax method => method.ReturnType,
			OperatorDeclarationSyntax @operator => @operator.ReturnType,
			ConversionOperatorDeclarationSyntax conversion => conversion.Type,
			_ => default
		};
	}

	/// <summary>
	/// Returns the return type of the specified <see cref="AccessorDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to get the return type of.</param>
	public static TypeSyntax? GetReturnType(this AccessorDeclarationSyntax node)
	{
		if (node.GetAccessorKind().HasReturnType())
		{
			return (node.Parent?.Parent as PropertyDeclarationSyntax)?.Type;
		}

		return default;
	}

	/// <summary>
	/// Returns name of the root namespace of the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the root namespace of.</param>
	public static string? GetRootNamespace(this SyntaxNode node)
	{
		return node.GetContainingNamespaces(ReturnOrder.ParentToChild).FirstOrDefault();
	}

	/// <summary>
	/// Returns a <see cref="ConstructorDeclarationSyntax"/> of the given <paramref name="kind"/> declared in the specified <see cref="TypeDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeDeclarationSyntax"/> to get the <see cref="ConstructorDeclarationSyntax"/> of.</param>
	/// <param name="kind">Kind of special constructor to return.</param>
	public static ConstructorDeclarationSyntax? GetSpecialConstructor(this TypeDeclarationSyntax node, SpecialConstructor kind)
	{
		if (kind == SpecialConstructor.None || kind == SpecialConstructor.Default)
		{
			return default;
		}

		return node.Members
			.OfType<ConstructorDeclarationSyntax>()
			.FirstOrDefault(ctor => ctor.GetConstructorKind() == kind);
	}

	/// <summary>
	/// Returns a <see cref="ConstructorDeclarationSyntax"/> of the given <paramref name="kind"/> declared in the specified <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to get the <see cref="ConstructorDeclarationSyntax"/> of.</param>
	/// <param name="kind">Kind of special constructor to return.</param>
	public static ConstructorDeclarationSyntax? GetSpecialConstructor(this BaseTypeDeclarationSyntax node, SpecialConstructor kind)
	{
		return (node as TypeDeclarationSyntax)?.GetSpecialConstructor(kind) ?? default;
	}

	/// <summary>
	/// Returns the <see cref="StringModifiers"/> applied to the specified <see cref="LiteralExpressionSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="LiteralExpressionSyntax"/> to get the <see cref="StringModifiers"/> applied to.</param>
	public static StringModifiers GetStringModifiers(this LiteralExpressionSyntax node)
	{
		return node.Token.GetStringModifiers();
	}

	/// <summary>
	/// Returns the <see cref="StringModifiers"/> applied to the specified <see cref="InterpolatedStringExpressionSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="InterpolatedStringExpressionSyntax"/> to get the <see cref="StringModifiers"/> applied to.</param>
	public static StringModifiers GetStringModifiers(this InterpolatedStringExpressionSyntax node)
	{
		return node.StringStartToken.GetStringModifiers();
	}

	/// <summary>
	/// Returns the <see cref="StringModifiers"/> applied to the specified <see cref="ExpressionSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="ExpressionSyntax"/> to get the <see cref="StringModifiers"/> applied to.</param>
	public static StringModifiers GetStringModifiers(this ExpressionSyntax node)
	{
		return node switch
		{
			LiteralExpressionSyntax literal => literal.GetStringModifiers(),
			InterpolatedStringExpressionSyntax interpolated => interpolated.GetStringModifiers(),
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="SymbolKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the <see cref="SymbolKind"/> associated with.</param>
	public static SymbolKind GetSymbolKind(this SyntaxNode node)
	{
		return node switch
		{
			BaseNamespaceDeclarationSyntax
				=> SymbolKind.Namespace,

			BaseTypeDeclarationSyntax or
			DelegateDeclarationSyntax or
			AnonymousObjectCreationExpressionSyntax or
			TupleExpressionSyntax
				=> SymbolKind.NamedType,

			BaseMethodDeclarationSyntax or
			AnonymousFunctionExpressionSyntax or
			LocalFunctionStatementSyntax or
			AccessorDeclarationSyntax
				=> SymbolKind.Method,

			PropertyDeclarationSyntax or
			IndexerDeclarationSyntax or
			AnonymousObjectMemberDeclaratorSyntax
				 => SymbolKind.Property,

			EventDeclarationSyntax or
			EventFieldDeclarationSyntax
				=> SymbolKind.Event,

			BaseParameterSyntax
				=> SymbolKind.Parameter,

			TypeParameterSyntax
				=> SymbolKind.TypeParameter,

			ExternAliasDirectiveSyntax
				=> SymbolKind.Alias,

			EnumMemberDeclarationSyntax or
			FieldDeclarationSyntax
				=> SymbolKind.Field,

			LocalDeclarationStatementSyntax
				=> SymbolKind.Local,

			LabeledStatementSyntax or
			SwitchLabelSyntax
				=> SymbolKind.Label,

			ArrayTypeSyntax
				=> SymbolKind.ArrayType,

			PointerTypeSyntax
				=> SymbolKind.PointerType,

			FunctionPointerTypeSyntax
				=> SymbolKind.FunctionPointerType,

			QueryClauseSyntax or
			QueryContinuationSyntax or
			JoinIntoClauseSyntax
				=> SymbolKind.RangeVariable,

			DiscardPatternSyntax
				=> SymbolKind.Discard,

			DirectiveTriviaSyntax
				=> SymbolKind.Preprocessing,

			UsingDirectiveSyntax @using when @using.GetUsingKind() == UsingKind.Alias
				=> SymbolKind.Alias,

			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="ArrayTypeSyntax"/> to get the <see cref="TypeKeyword"/> represented by.</param>
	public static TypeKeyword GetTypeKeyword(this ArrayTypeSyntax node)
	{
		return node.ElementType.GetTypeKeyword();
	}

	/// <summary>
	/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="PointerTypeSyntax"/> to get the <see cref="TypeKeyword"/> represented by.</param>
	public static TypeKeyword GetTypeKeyword(this PointerTypeSyntax node)
	{
		return node.ElementType.GetTypeKeyword();
	}

	/// <summary>
	/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="NullableTypeSyntax"/> to get the <see cref="TypeKeyword"/> represented by.</param>
	public static TypeKeyword GetTypeKeyword(this NullableTypeSyntax node)
	{
		return node.ElementType.GetTypeKeyword();
	}

	/// <summary>
	/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="RefTypeSyntax"/> to get the <see cref="TypeKeyword"/> represented by.</param>
	public static TypeKeyword GetTypeKeyword(this RefTypeSyntax node)
	{
		return node.Type.GetTypeKeyword();
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
		switch (node)
		{
			case PredefinedTypeSyntax predefined:
				return predefined.GetTypeKeyword();

			case RefTypeSyntax refType:
				return refType.GetTypeKeyword();

			case NullableTypeSyntax nullable:
				return nullable.GetTypeKeyword();

			case PointerTypeSyntax pointer:
				return pointer.GetTypeKeyword();

			case ArrayTypeSyntax array:
				return array.GetTypeKeyword();
		}

		if (node.IsNint)
		{
			return TypeKeyword.NInt;
		}

		if (node.IsNuint)
		{
			return TypeKeyword.NUInt;
		}

		if (node.IsDynamic())
		{
			return TypeKeyword.Dynamic;
		}

		return default;
	}

	/// <summary>
	/// Returns the <see cref="TypeKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to get the <see cref="TypeKind"/> associated with.</param>
	public static TypeKind GetTypeKind(this SyntaxNode node)
	{
		return node switch
		{
			BaseTypeDeclarationSyntax decl => decl.GetTypeKind(),
			DelegateDeclarationSyntax => TypeKind.Delegate,
			TypeParameterSyntax => TypeKind.TypeParameter,
			TypeSyntax type => type.GetTypeKind(),
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="TypeKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="TypeKind"/> associated with.</param>
	public static TypeKind GetTypeKind(this TypeSyntax node)
	{
		if (node is NullableTypeSyntax nullable)
		{
			node = nullable.ElementType;
		}

		return node switch
		{
			ArrayTypeSyntax => TypeKind.Array,
			PointerTypeSyntax => TypeKind.Pointer,
			FunctionPointerTypeSyntax => TypeKind.FunctionPointer,
			_ => node.IsDynamic() ? TypeKind.Dynamic : default
		};
	}

	/// <summary>
	/// Returns the <see cref="TypeKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to get the <see cref="TypeKind"/> associated with.</param>
	public static TypeKind GetTypeKind(this BaseTypeDeclarationSyntax node)
	{
		return node switch
		{
			ClassDeclarationSyntax => TypeKind.Class,
			StructDeclarationSyntax => TypeKind.Struct,
			EnumDeclarationSyntax => TypeKind.Enum,
			InterfaceDeclarationSyntax => TypeKind.Interface,
			RecordDeclarationSyntax record => record.IsStruct() ? TypeKind.Struct : TypeKind.Class,
			_ => default
		};
	}

	/// <summary>
	/// Returns the <see cref="TypeParameterKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeSyntax"/> to get the <see cref="TypeParameterKind"/> associated with.</param>
	public static TypeParameterKind GetTypeParameterKind(this TypeSyntax node)
	{
		return node.Ancestors().OfType<NameMemberCrefSyntax>().Any() ? TypeParameterKind.Cref : default;
	}

	/// <summary>
	/// Returns the <see cref="TypeParameterKind"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterSyntax"/> to get the <see cref="TypeParameterKind"/> associated with.</param>
	public static TypeParameterKind GetTypeParameterKind(this TypeParameterSyntax node)
	{
		return node?.Parent?.Parent switch
		{
			TypeDeclarationSyntax or DelegateDeclarationSyntax => TypeParameterKind.Type,
			MethodDeclarationSyntax or LocalFunctionStatementSyntax => TypeParameterKind.Method,
			_ => default
		};
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
	/// Returns the <see cref="UsingKind"/> associated with the specified <see cref="UsingDirectiveSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="UsingDirectiveSyntax"/> to get the <see cref="UsingKind"/> associated with.</param>
	public static UsingKind GetUsingKind(this UsingDirectiveSyntax node)
	{
		if (node.Alias is not null)
		{
			return UsingKind.Alias;
		}

		if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
		{
			return UsingKind.Static;
		}

		return UsingKind.Ordinary;
	}

	/// <summary>
	/// Returns a <see cref="VariableDeclaratorSyntax"/> at the specified <paramref name="index"/> in the <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="FieldDeclarationSyntax"/> to get the variable of.</param>
	/// <param name="index">Index to get the <see cref="VariableDeclarationSyntax"/> at.</param>
	public static VariableDeclaratorSyntax GetVariable(this LocalDeclarationStatementSyntax node, int index)
	{
		return node.Declaration.GetVariable(index);
	}

	/// <summary>
	/// Returns a <see cref="VariableDeclaratorSyntax"/> at the specified <paramref name="index"/> in the <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="FieldDeclarationSyntax"/> to get the variable of.</param>
	/// <param name="index">Index to get the <see cref="VariableDeclarationSyntax"/> at.</param>
	public static VariableDeclaratorSyntax GetVariable(this BaseFieldDeclarationSyntax node, int index)
	{
		return node.Declaration.GetVariable(index);
	}

	/// <summary>
	/// Returns a <see cref="VariableDeclaratorSyntax"/> at the specified <paramref name="index"/> in the <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="VariableDeclarationSyntax"/> to get the variable of.</param>
	/// <param name="index">Index to get the <see cref="VariableDeclarationSyntax"/> at.</param>
	public static VariableDeclaratorSyntax GetVariable(this VariableDeclarationSyntax node, int index)
	{
		return node.Variables[index];
	}

	/// <summary>
	/// Returns the <see cref="VarianceKind"/> of the specified <see cref="TypeParameterSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterSyntax"/> to get the <see cref="VarianceKind"/> of.</param>
	public static VarianceKind GetVariance(this TypeParameterSyntax node)
	{
		return node.VarianceKeyword.GetVariance();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
	/// </summary>
	/// <param name="node"><see cref="PropertyDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
	/// <param name="accessor">Kind of accessor to check for.</param>
	public static bool HasAccessor(this PropertyDeclarationSyntax node, PropertyAccessorKind accessor)
	{
		if (node.ExpressionBody is not null)
		{
			return accessor == PropertyAccessorKind.Get;
		}

		return node.AccessorList?.HasAccessor(accessor.GetAccessorKind()) ?? false;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
	/// </summary>
	/// <param name="node"><see cref="IndexerDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
	/// <param name="accessor">Kind of accessor to check for.</param>
	public static bool HasAccessor(this IndexerDeclarationSyntax node, PropertyAccessorKind accessor)
	{
		if (node.ExpressionBody is not null)
		{
			return accessor == PropertyAccessorKind.Get;
		}

		return node.AccessorList?.HasAccessor(accessor.GetAccessorKind()) ?? false;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
	/// </summary>
	/// <param name="node"><see cref="EventDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
	/// <param name="accessor">Kind of accessor to check for.</param>
	public static bool HasAccessor(this EventDeclarationSyntax node, EventAccessorKind accessor)
	{
		return node.AccessorList?.HasAccessor(accessor.GetAccessorKind()) ?? false;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
	/// </summary>
	/// <param name="node"><see cref="BasePropertyDeclarationSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
	/// <param name="accessor">Kind of accessor to check for.</param>
	public static bool HasAccessor(this BasePropertyDeclarationSyntax node, AccessorKind accessor)
	{
		return node.AccessorList?.HasAccessor(accessor) ?? false;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> defines an <paramref name="accessor"/> of a given kind.
	/// </summary>
	/// <param name="node"><see cref="AccessorListSyntax"/> to determines whether has an <paramref name="accessor"/> of a given kind.</param>
	/// <param name="accessor">Kind of accessor to check for.</param>
	public static bool HasAccessor(this AccessorListSyntax node, AccessorKind accessor)
	{
		return node.Accessors.Any(acc => acc.GetAccessorKind() == accessor);
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
	/// Determines whether the specified <see cref="TypeParameterSyntax"/> has any generic constraints applied.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterSyntax"/> to determine whether has any generic constraints applied.</param>
	public static bool HasConstraints(this TypeParameterSyntax node)
	{
		return node.GetConstraintClause() is not null;
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeParameterSyntax"/> has a <paramref name="constraint"/> of the given kind applied.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterSyntax"/> to determine whether has a <paramref name="constraint"/> of the given kind applied.</param>
	/// <param name="constraint"><see cref="GenericConstraint"/> to check for.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static bool HasConstraints(this TypeParameterSyntax node, GenericConstraint constraint, bool includeImplicit = false)
	{
		return node.GetConstraintClause()?.HasConstraints(constraint, includeImplicit) ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeParameterConstraintClauseSyntax"/> contains a <paramref name="constraint"/> of the given kind.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterConstraintClauseSyntax"/> to determine whether contains a <paramref name="constraint"/> of the given kind.</param>
	/// <param name="constraint"><see cref="GenericConstraint"/> to check for.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static bool HasConstraints(this TypeParameterConstraintSyntax node, GenericConstraint constraint, bool includeImplicit = false)
	{
		if (!includeImplicit)
		{
			return HasConstraintExplicit(node, constraint);
		}

		return HasConstraintImplicit(node, constraint);
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeParameterConstraintClauseSyntax"/> contains a <paramref name="constraint"/> of the given kind.
	/// </summary>
	/// <param name="node"><see cref="TypeParameterConstraintClauseSyntax"/> to determine whether contains a <paramref name="constraint"/> of the given kind.</param>
	/// <param name="constraint"><see cref="GenericConstraint"/> to check for.</param>
	/// <param name="includeImplicit">Determines whether to include constraints that are implicitly applied to the <paramref name="node"/>.</param>
	public static unsafe bool HasConstraints(this TypeParameterConstraintClauseSyntax node, GenericConstraint constraint, bool includeImplicit = false)
	{
		GenericConstraint[] values = constraint.GetFlags();

		if (values.Length == 0)
		{
			return false;
		}

		Queue<GenericConstraint> queue = new(values);

		// Why function pointer instead of a Func? Because it's more fun this way!

		delegate*<TypeParameterConstraintSyntax, GenericConstraint, bool> func = includeImplicit
			? &HasConstraintImplicit
			: &HasConstraintExplicit;

		foreach (TypeParameterConstraintSyntax cons in node.Constraints)
		{
			int count = queue.Count;

			for (int i = 0; i < count; i++)
			{
				GenericConstraint value = queue.Dequeue();

				if (!func(cons, value))
				{
					queue.Enqueue(value);
				}
			}

			if (queue.Count == 0)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Returns the <see cref="DocumentationCommentTriviaSyntax"/> applied to the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> get the <see cref="DocumentationCommentTriviaSyntax"/> applied to.</param>
	public static DocumentationCommentTriviaSyntax? GetXmlDocumentation(this SyntaxNode node)
	{
		if (!node.HasLeadingTrivia)
		{
			return default;
		}

		SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();
		SyntaxTrivia token = leadingTrivia.FirstOrDefault(token => token.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || token.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

		if (token.IsKind(SyntaxKind.None))
		{
			return default;
		}

		SyntaxNode? structure = token.GetStructure();

		return structure as DocumentationCommentTriviaSyntax;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="abstract"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="abstract"/> modifier.</param>
	public static bool IsAbstract(this SyntaxNode node)
	{
		return node.GetModifiers().IsAbstract();
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="BaseMethodDeclarationSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this BaseMethodDeclarationSyntax node)
	{
		return node.ParameterList.IsArgList();
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="DelegateDeclarationSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="DelegateDeclarationSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this DelegateDeclarationSyntax node)
	{
		return node.ParameterList.IsArgList();
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="BaseParameterListSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="BaseParameterListSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this BaseParameterListSyntax node)
	{
		return node.Parameters.Any(IsArgList);
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="LocalFunctionStatementSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this LocalFunctionStatementSyntax node)
	{
		return node.ParameterList.IsArgList();
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="RecordDeclarationSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="RecordDeclarationSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this RecordDeclarationSyntax node)
	{
		return node.ParameterList?.IsArgList() ?? false;
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="AnonymousMethodExpressionSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="AnonymousMethodExpressionSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this AnonymousMethodExpressionSyntax node)
	{
		return node.ParameterList?.IsArgList() ?? false;
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="AnonymousFunctionExpressionSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="AnonymousFunctionExpressionSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this AnonymousFunctionExpressionSyntax node)
	{
		return node switch
		{
			LambdaExpressionSyntax lambda => lambda.IsArgList(),
			AnonymousMethodExpressionSyntax method => method.IsArgList(),
			_ => false
		};
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="LambdaExpressionSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="LambdaExpressionSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this LambdaExpressionSyntax node)
	{
		return (node as ParenthesizedLambdaExpressionSyntax)?.IsArgList() ?? false;
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="ParenthesizedLambdaExpressionSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="ParenthesizedLambdaExpressionSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this ParenthesizedLambdaExpressionSyntax node)
	{
		return node.ParameterList.IsArgList();
	}

	/// <summary>
	/// Determines whether any parameter of the specified <see cref="IndexerDeclarationSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="IndexerDeclarationSyntax"/> to determine whether contains any parameter representing the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this IndexerDeclarationSyntax node)
	{
		return node.ParameterList.IsArgList();
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseParameterSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="BaseParameterSyntax"/> to determine whether represents the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this BaseParameterSyntax node)
	{
		return (node as ParameterSyntax)?.IsArgList() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="ParameterSyntax"/> represents the <see langword="__arglist"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="ParameterSyntax"/> to determine whether represents the <see langword="__arglist"/> keyword.</param>
	public static bool IsArgList(this ParameterSyntax node)
	{
		return node.Type is null && node.Identifier.IsKind(SyntaxKind.ArgListKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="async"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="async"/> modifier.</param>
#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.
	public static bool IsAsync(this SyntaxNode node)
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
	{
		return node.GetModifiers().IsAsync();
	}

	/// <summary>
	/// Determines whether the specified <see cref="PropertyDeclarationSyntax"/> represents an auto-property.
	/// </summary>
	/// <param name="node"><see cref="PropertyDeclarationSyntax"/> to determine whether represents an auto-property.</param>
	public static bool IsAutoProperty(this PropertyDeclarationSyntax node)
	{
		if (node.AccessorList is null)
		{
			return false;
		}

		return node.AccessorList.IsAutoProperty();
	}

	/// <summary>
	/// Determines whether the specified <see cref="BasePropertyDeclarationSyntax"/> represents an auto-property.
	/// </summary>
	/// <param name="node"><see cref="BasePropertyDeclarationSyntax"/> to determine whether represents an auto-property.</param>
	public static bool IsAutoProperty(this BasePropertyDeclarationSyntax node)
	{
		return (node as PropertyDeclarationSyntax)?.IsAutoProperty() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="AccessorListSyntax"/> represents an auto-property accessor list.
	/// </summary>
	/// <param name="node"><see cref="AccessorListSyntax"/> to determine whether represents an auto-property accessor list.</param>
	public static bool IsAutoProperty(this AccessorListSyntax node)
	{
		foreach (AccessorDeclarationSyntax accessor in node.Accessors)
		{
			if (accessor.GetBody() is not null)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Determines whether the specified <see cref="AccessorDeclarationSyntax"/> is an auto-property accessor.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to determine whether is an auto-property accessor.</param>
	public static bool IsAutoPropertyAccessor(this AccessorDeclarationSyntax node)
	{
		return (node.Parent?.Parent as PropertyDeclarationSyntax)?.IsAutoProperty() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="ClassOrStructConstraintSyntax"/> is a class constraint.
	/// </summary>
	/// <param name="node"><see cref="ClassOrStructConstraintSyntax"/> to determine whether is a class constraint.</param>
	public static bool IsClass(this ClassOrStructConstraintSyntax node)
	{
		return node.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseTypeDeclarationSyntax"/> is a class type.
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to determine whether is a class type.</param>
	public static bool IsClass(this BaseTypeDeclarationSyntax node)
	{
		return node is ClassDeclarationSyntax || (node is RecordDeclarationSyntax record && record.IsClass());
	}

	/// <summary>
	/// Determines whether the specified <see cref="RecordDeclarationSyntax"/> is a class type.
	/// </summary>
	/// <param name="node"><see cref="RecordDeclarationSyntax"/> to determine whether is a class type.</param>
	public static bool IsClass(this RecordDeclarationSyntax node)
	{
		return node.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="SyntaxNode"/> is considered a declaration node and can be used as argument for the <c>GetDeclaredSymbol</c> method of a <see cref="SemanticModel"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether is considered a declaration node.</param>
	public static bool IsDeclaration(this SyntaxNode node)
	{
		return node is
			MemberDeclarationSyntax or
			AccessorDeclarationSyntax or
			TypeParameterSyntax or
			ParameterSyntax or
			VariableDesignationSyntax or
			AnonymousObjectCreationExpressionSyntax or
			AnonymousObjectMemberDeclaratorSyntax or
			ArgumentSyntax or
			CatchDeclarationSyntax or
			ExternAliasDirectiveSyntax or
			CompilationUnitSyntax or
			ForEachStatementSyntax or
			LabeledStatementSyntax or
			JoinIntoClauseSyntax or
			QueryClauseSyntax or
			QueryContinuationSyntax or
			SingleVariableDesignationSyntax or
			SwitchLabelSyntax or
			UsingDirectiveSyntax or
			TupleElementSyntax or
			TupleExpressionSyntax;
	}

	/// <summary>
	/// Determines whether the specified <see cref="PragmaWarningDirectiveTriviaSyntax"/> represents a <c>#pragma warning disable</c> directive.
	/// </summary>
	/// <param name="node"><see cref="PragmaWarningDirectiveTriviaSyntax"/> to determine whether represents a <c>#pragma warning disable</c> directive.</param>
	public static bool IsDisable(this PragmaWarningDirectiveTriviaSyntax node)
	{
		return node.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeSyntax"/> represents the <see langword="dynamic"/> keyword.
	/// </summary>
	/// <param name="node"><see cref="TypeSyntax"/> to determine whether represents the <see langword="dynamic"/> keyword.</param>
	public static bool IsDynamic(this TypeSyntax node)
	{
		return node is IdentifierNameSyntax { Identifier.ValueText: "dynamic" };
	}

	/// <summary>
	/// Determines whether the specified <see cref="AccessorDeclarationSyntax"/> is an event accessor.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to determine whether is an event accessor.</param>
	public static bool IsEventAccessor(this AccessorDeclarationSyntax node)
	{
		return node.Parent?.Parent is EventDeclarationSyntax;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> represents a declaration of an <see cref="IEventSymbol"/>. This applies to:
	/// <list type="bullet">
	/// <item><see cref="EventFieldDeclarationSyntax"/>.</item>
	/// <item><see cref="EventDeclarationSyntax"/></item>
	/// </list>
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether represents a declaration of an <see cref="IEventSymbol"/>.</param>
	public static bool IsEventDeclaration(this SyntaxNode node)
	{
		return node is
			EventFieldDeclarationSyntax or
			EventDeclarationSyntax;
	}

	/// <summary>
	/// Determines whether the specified <see cref="ConversionOperatorDeclarationSyntax"/> represents an explicit operator.
	/// </summary>
	/// <param name="node"><see cref="ConversionOperatorDeclarationSyntax"/> to determine whether represents an explicit operator.</param>
	public static bool IsExplicit(this ConversionOperatorDeclarationSyntax node)
	{
		return node.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ExplicitKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="ConversionOperatorMemberCrefSyntax"/> represents an explicit operator.
	/// </summary>
	/// <param name="node"><see cref="ConversionOperatorMemberCrefSyntax"/> to determine whether represents an explicit operator.</param>
	public static bool IsExplicit(this ConversionOperatorMemberCrefSyntax node)
	{
		return node.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ExplicitKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="extern"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="extern"/> modifier.</param>
	public static bool IsExtern(this SyntaxNode node)
	{
		return node.GetModifiers().IsExtern();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="fixed"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="fixed"/> modifier.</param>
	public static bool IsFixed(this SyntaxNode node)
	{
		return node.GetModifiers().IsFixed();
	}

	/// <summary>
	/// Determines whether the specified <see cref="UsingDirectiveSyntax"/> represents a <c><see langword="global"/> <see langword="using"/></c> directive.
	/// </summary>
	/// <param name="node"><see cref="UsingDirectiveSyntax"/> to determine whether represents a <c><see langword="global"/> <see langword="using"/></c> directive.</param>
	public static bool IsGlobal(this UsingDirectiveSyntax node)
	{
		return node.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="ConversionOperatorDeclarationSyntax"/> represents an implicit operator.
	/// </summary>
	/// <param name="node"><see cref="ConversionOperatorDeclarationSyntax"/> to determine whether represents an implicit operator.</param>
	public static bool IsImplicit(this ConversionOperatorDeclarationSyntax node)
	{
		return node.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="ConversionOperatorMemberCrefSyntax"/> represents an implicit operator.
	/// </summary>
	/// <param name="node"><see cref="ConversionOperatorMemberCrefSyntax"/> to determine whether represents an implicit operator.</param>
	public static bool IsImplicit(this ConversionOperatorMemberCrefSyntax node)
	{
		return node.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="in"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="in"/> modifier.</param>
	public static bool IsIn(this SyntaxNode node)
	{
		return node.GetModifiers().IsIn();
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseTypeDeclarationSyntax"/> is contained within another <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to determine whether is contained within another <see cref="BaseTypeDeclarationSyntax"/>.</param>
	public static bool IsInnerType(this BaseTypeDeclarationSyntax node)
	{
		return node.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any();
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseMethodDeclarationSyntax"/> is an iterator (contains any <see cref="YieldStatementSyntax"/>es).
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to determine whether is an iterator.</param>
	public static bool IsIterator(this BaseMethodDeclarationSyntax node)
	{
		return (node as MethodDeclarationSyntax)?.IsIterator() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="MethodDeclarationSyntax"/> is an iterator (contains any <see cref="YieldStatementSyntax"/>es).
	/// </summary>
	/// <param name="node"><see cref="MethodDeclarationSyntax"/> to determine whether is an iterator.</param>
	public static bool IsIterator(this MethodDeclarationSyntax node)
	{
		return node
			.DescendantNodes(node => node.Kind() is
				not SyntaxKind.LocalFunctionStatement and
				not SyntaxKind.AnonymousMethodExpression and
				not SyntaxKind.SimpleLambdaExpression and
				not SyntaxKind.ParenthesizedLambdaExpression &&
				node is not ExpressionSyntax
			)
			.Any(n => n is YieldStatementSyntax);
	}

	/// <summary>
	/// Determines whether the specified <see cref="LocalFunctionStatementSyntax"/> is an iterator (contains any <see cref="YieldStatementSyntax"/>es).
	/// </summary>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to determine whether is an iterator.</param>
	public static bool IsIterator(this LocalFunctionStatementSyntax node)
	{
		return node
			.DescendantNodes(node => node.Kind() is
				not SyntaxKind.LocalFunctionStatement and
				not SyntaxKind.AnonymousMethodExpression and
				not SyntaxKind.SimpleLambdaExpression and
				not SyntaxKind.ParenthesizedLambdaExpression &&
				node is not ExpressionSyntax
			)
			.Any(n => n is YieldStatementSyntax);
	}

	/// <summary>
	/// Determines whether the specified <see cref="FunctionPointerTypeSyntax"/> uses the <see langword="managed"/> calling convention.
	/// </summary>
	/// <param name="node"><see cref="FunctionPointerTypeSyntax"/> to determine whether uses the <see langword="managed"/> calling convention.</param>
	public static bool IsManaged(this FunctionPointerTypeSyntax node)
	{
		return node.CallingConvention?.IsManaged() ?? true;
	}

	/// <summary>
	/// Determines whether the specified <see cref="FunctionPointerCallingConventionSyntax"/> uses the <see langword="managed"/> calling convention.
	/// </summary>
	/// <param name="node"><see cref="FunctionPointerCallingConventionSyntax"/> to determine whether uses the <see langword="managed"/> calling convention.</param>
	public static bool IsManaged(this FunctionPointerCallingConventionSyntax node)
	{
		return node.ManagedOrUnmanagedKeyword == default || node.ManagedOrUnmanagedKeyword.IsKind(SyntaxKind.ManagedKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> represents a declaration of an <see cref="IMethodSymbol"/>. This applies to:
	/// <list type="bullet">
	/// <item><see cref="BaseMethodDeclarationSyntax"/> (and its derived types)</item>
	/// <item><see cref="AnonymousFunctionExpressionSyntax"/> (and its derived types)</item>
	/// <item><see cref="LocalFunctionStatementSyntax"/></item>
	/// <item><see cref="AccessorDeclarationSyntax"/></item>
	/// </list>
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether represents a declaration of an <see cref="IMethodSymbol"/>.</param>
	public static bool IsMethodDeclaration(this SyntaxNode node)
	{
		return node is
			BaseMethodDeclarationSyntax or
			AnonymousFunctionExpressionSyntax or
			LocalFunctionStatementSyntax or
			AccessorDeclarationSyntax;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="new"/> token.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="new"/> modifier.</param>
	public static bool IsNew(this SyntaxNode node)
	{
		return node.GetModifiers().IsNew();
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeConstraintSyntax"/> represents the <see langword="notnull"/> constraint.
	/// </summary>
	/// <param name="node"><see cref="TypeConstraintSyntax"/> to determine whether represents the <see langword="notnull"/> constraint.</param>
	public static bool IsNotNullConstraint(this TypeConstraintSyntax node)
	{
		return node.Type.IsNotNull;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="override"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="override"/> modifier.</param>
	public static bool IsOverride(this SyntaxNode node)
	{
		return node.GetModifiers().IsOverride();
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseMethodDeclarationSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this BaseMethodDeclarationSyntax node)
	{
		return node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <see cref="DelegateDeclarationSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="DelegateDeclarationSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this DelegateDeclarationSyntax node)
	{
		return node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <see cref="RecordDeclarationSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="RecordDeclarationSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this RecordDeclarationSyntax node)
	{
		return node.ParameterList is null || node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <see cref="LocalFunctionStatementSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this LocalFunctionStatementSyntax node)
	{
		return node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <see cref="AnonymousMethodExpressionSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="AnonymousMethodExpressionSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this AnonymousMethodExpressionSyntax node)
	{
		return node.ParameterList is null || node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <see cref="AnonymousFunctionExpressionSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="AnonymousFunctionExpressionSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this AnonymousFunctionExpressionSyntax node)
	{
		return node switch
		{
			LambdaExpressionSyntax lambda => lambda.IsParameterless(),
			AnonymousMethodExpressionSyntax method => method.IsParameterless(),
			_ => default
		};
	}

	/// <summary>
	/// Determines whether the specified <see cref="ParenthesizedLambdaExpressionSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="ParenthesizedLambdaExpressionSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this ParenthesizedLambdaExpressionSyntax node)
	{
		return node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <see cref="ParenthesizedLambdaExpressionSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="ParenthesizedLambdaExpressionSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this LambdaExpressionSyntax node)
	{
		if (node is SimpleLambdaExpressionSyntax)
		{
			return true;
		}

		return (node as ParenthesizedLambdaExpressionSyntax)?.IsParameterless() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseParameterListSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="BaseParameterListSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this BaseParameterListSyntax node)
	{
		return !node.Parameters.Any();
	}

	/// <summary>
	/// Determines whether the specified <see cref="IndexerDeclarationSyntax"/> is parameterless.
	/// </summary>
	/// <param name="node"><see cref="IndexerDeclarationSyntax"/> to determine whether is parameterless</param>
	public static bool IsParameterless(this IndexerDeclarationSyntax node)
	{
		return node.ParameterList.IsParameterless();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="params"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="params"/> modifier.</param>
	public static bool IsParams(this SyntaxNode node)
	{
		return node.GetModifiers().IsParams();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="partial"/> token.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="partial"/> modifier.</param>
	public static bool IsPartial(this SyntaxNode node)
	{
		return node.GetModifiers().IsPartial();
	}

	/// <summary>
	/// Determines whether the specified <see cref="AccessorDeclarationSyntax"/> is a property accessor.
	/// </summary>
	/// <param name="node"><see cref="AccessorDeclarationSyntax"/> to determine whether is a property accessor.</param>
	public static bool IsPropertyAccessor(this AccessorDeclarationSyntax node)
	{
		return node.Parent?.Parent is PropertyDeclarationSyntax or IndexerDeclarationSyntax;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="readonly"/> modifier (<see langword="ref"/> <see langword="readonly"/> does not count).
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="readonly"/> modifier.</param>
	public static bool IsReadOnly(this SyntaxNode node)
	{
		return node.GetModifiers().IsReadOnly();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="ref"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="ref"/> modifier.</param>
	public static bool IsRef(this SyntaxNode node)
	{
		return node.GetModifiers().IsRef();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="ref"/> and <see langword="readonly"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="ref"/> and <see langword="readonly"/> modifiers.</param>
	public static bool IsRefReadOnly(this SyntaxNode node)
	{
		return node.GetModifiers().IsRefReadOnly();
	}

	/// <summary>
	/// Determines whether the specified <see cref="PragmaWarningDirectiveTriviaSyntax"/> represents a <c>#pragma warning restore</c> directive.
	/// </summary>
	/// <param name="node"><see cref="PragmaWarningDirectiveTriviaSyntax"/> to determine whether represents a <c>#pragma warning restore</c> directive.</param>
	public static bool IsRestore(this PragmaWarningDirectiveTriviaSyntax node)
	{
		return node.DisableOrRestoreKeyword.IsKind(SyntaxKind.RestoreKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="sealed"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="sealed"/> modifier.</param>
	public static bool IsSealed(this SyntaxNode node)
	{
		return node.GetModifiers().IsSealed();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="static"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="static"/> modifier.</param>
	public static bool IsStatic(this SyntaxNode node)
	{
		return node.GetModifiers().IsStatic();
	}

	/// <summary>
	/// Determines whether the specified <see cref="ClassOrStructConstraintSyntax"/> is a struct constraint.
	/// </summary>
	/// <param name="node"><see cref="ClassOrStructConstraintSyntax"/> to determine whether is a struct constraint.</param>
	public static bool IsStruct(this ClassOrStructConstraintSyntax node)
	{
		return node.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseTypeDeclarationSyntax"/> is a struct type.
	/// </summary>
	/// <param name="node"><see cref="BaseTypeDeclarationSyntax"/> to determine whether is a struct type.</param>
	public static bool IsStruct(this BaseTypeDeclarationSyntax node)
	{
		return node is StructDeclarationSyntax || (node is RecordDeclarationSyntax record && record.IsStruct());
	}

	/// <summary>
	/// Determines whether the specified <see cref="RecordDeclarationSyntax"/> is a struct type.
	/// </summary>
	/// <param name="node"><see cref="RecordDeclarationSyntax"/> to determine whether is a struct type.</param>
	public static bool IsStruct(this RecordDeclarationSyntax node)
	{
		return node.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="this"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="this"/> modifier.</param>
	public static bool IsThis(this SyntaxNode node)
	{
		return node.GetModifiers().IsThis();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> as declared at the top level, meaning the <see cref="SyntaxNode.Parent"/> is either:
	/// <list type="bullet">
	/// <item><see langword="null"/></item>
	/// <item><see cref="CompilationUnitSyntax"/></item>
	/// <item><see cref="GlobalStatementSyntax"/></item>
	/// </list>
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether is at the top level.</param>
	public static bool IsTopLevel(this SyntaxNode node)
	{
		return node.Parent is
			null or
			CompilationUnitSyntax or
			GlobalStatementSyntax;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> as declared in a namespace or at the top level (see <see cref="IsTopLevel(SyntaxNode)"/> for more details).
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether is at the top level.</param>
	public static bool IsTopLevelOrInNamespace(this SyntaxNode node)
	{
		return node.Parent is
			null or
			CompilationUnitSyntax or
			BaseNamespaceDeclarationSyntax;
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeConstraintSyntax"/> represents a named type constraint.
	/// </summary>
	/// <param name="node"><see cref="TypeConstraintSyntax"/> to determine whether represents a named type constraint.</param>
	public static bool IsTypeConstraint(this TypeConstraintSyntax node)
	{
		return !node.IsUnmanagedConstraint() && !node.IsNotNullConstraint();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> represents a declaration of an <see cref="ITypeSymbol"/>. This applies to:
	/// <list type="bullet">
	/// <item><see cref="BaseTypeDeclarationSyntax"/> (and its derived types)</item>
	/// <item><see cref="DelegateDeclarationSyntax"/></item>
	/// </list>
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether represents a declaration of an <see cref="ITypeSymbol"/>.</param>
	public static bool IsTypeDeclaration(this SyntaxNode node)
	{
		return node is
			BaseTypeDeclarationSyntax or
			DelegateDeclarationSyntax;
	}

	/// <summary>
	/// Determines whether the specified <see cref="FunctionPointerTypeSyntax"/> uses the <see langword="unmanaged"/> calling convention.
	/// </summary>
	/// <param name="node"><see cref="FunctionPointerTypeSyntax"/> to determine whether uses the <see langword="unmanaged"/> calling convention.</param>
	public static bool IsUnmanaged(this FunctionPointerTypeSyntax node)
	{
		return node.CallingConvention?.IsUnmanaged() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="FunctionPointerCallingConventionSyntax"/> uses the <see langword="unmanaged"/> calling convention.
	/// </summary>
	/// <param name="node"><see cref="FunctionPointerCallingConventionSyntax"/> to determine whether uses the <see langword="unmanaged"/> calling convention.</param>
	public static bool IsUnmanaged(this FunctionPointerCallingConventionSyntax node)
	{
		return node.ManagedOrUnmanagedKeyword.IsKind(SyntaxKind.UnmanagedKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="TypeConstraintSyntax"/> represents the <see langword="unmanaged"/> constraint.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="unmanaged"/> modifier.</param>
	public static bool IsUnmanagedConstraint(this TypeConstraintSyntax node)
	{
		return node.Type.IsUnmanaged;
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="unsafe"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="unsafe"/> modifier.</param>
	public static bool IsUnsafe(this SyntaxNode node)
	{
		return node.GetModifiers().IsUnsafe();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="virtual"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="virtual"/> modifier.</param>
	public static bool IsVirtual(this SyntaxNode node)
	{
		return node.GetModifiers().IsVirtual();
	}

	/// <summary>
	/// Determines whether the specified <see cref="BaseMethodDeclarationSyntax"/> returns <see langword="void"/>.
	/// </summary>
	/// <param name="node"><see cref="BaseMethodDeclarationSyntax"/> to determine whether returns <see langword="void"/>.</param>
	public static bool IsVoid(this BaseMethodDeclarationSyntax node)
	{
		return (node as MethodDeclarationSyntax)?.IsVoid() ?? false;
	}

	/// <summary>
	/// Determines whether the specified <see cref="MethodDeclarationSyntax"/> returns <see langword="void"/>.
	/// </summary>
	/// <param name="node"><see cref="MethodDeclarationSyntax"/> to determine whether returns <see langword="void"/>.</param>
	public static bool IsVoid(this MethodDeclarationSyntax node)
	{
		return node.ReturnType is PredefinedTypeSyntax type && type.Keyword.IsKind(SyntaxKind.VoidKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="LocalFunctionStatementSyntax"/> returns <see langword="void"/>.
	/// </summary>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to determine whether returns <see langword="void"/>.</param>
	public static bool IsVoid(this LocalFunctionStatementSyntax node)
	{
		return node.ReturnType is PredefinedTypeSyntax type && type.Keyword.IsKind(SyntaxKind.VoidKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="DelegateDeclarationSyntax"/> returns <see langword="void"/>.
	/// </summary>
	/// <param name="node"><see cref="DelegateDeclarationSyntax"/> to determine whether returns <see langword="void"/>.</param>
	public static bool IsVoid(this DelegateDeclarationSyntax node)
	{
		return node.ReturnType is PredefinedTypeSyntax type && type.Keyword.IsKind(SyntaxKind.VoidKeyword);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> contains the <see langword="volatile"/> modifier.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether contains the <see langword="volatile"/> modifier.</param>
	public static bool IsVolatile(this SyntaxNode node)
	{
		return node.GetModifiers().IsVolatile();
	}

	/// <summary>
	/// Determines whether the specified <see cref="YieldStatementSyntax"/> represents a <c><see langword="yield"/> break</c> statement.
	/// </summary>
	/// <param name="node"><see cref="YieldStatementSyntax"/> to determine whether represents a <c><see langword="yield"/> break</c> statement.</param>
	public static bool IsYieldBreak(this YieldStatementSyntax node)
	{
		return node.ReturnOrBreakKeyword.IsKind(SyntaxKind.BreakKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="YieldStatementSyntax"/> represents a <c><see langword="yield"/> return</c> statement.
	/// </summary>
	/// <param name="node"><see cref="YieldStatementSyntax"/> to determine whether represents a <c><see langword="yield"/> return</c> statement.</param>
	public static bool IsYieldReturn(this YieldStatementSyntax node)
	{
		return node.ReturnOrBreakKeyword.IsKind(SyntaxKind.ReturnKeyword);
	}

	/// <summary>
	/// Determines whether the specified <see cref="SyntaxNode"/> supports more than one attribute target kind.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to determine whether supports more than one attribute target kind.</param>
	public static bool SupportsAlternativeAttributeTargets(this SyntaxNode node)
	{
		return node switch
		{
			MemberDeclarationSyntax member
				=> member.SupportsAlternativeAttributeTargets(),

			CompilationUnitSyntax or
			AccessorDeclarationSyntax or
			LambdaExpressionSyntax or
			LocalFunctionStatementSyntax
				=> true,

			_ => false
		};
	}

	/// <summary>
	/// Determines whether the specified <see cref="MemberDeclarationSyntax"/> supports more than one attribute target kind.
	/// </summary>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to determine whether supports more than one attribute target kind.</param>
	public static bool SupportsAlternativeAttributeTargets(this MemberDeclarationSyntax node)
	{
		return node switch
		{
			MethodDeclarationSyntax or
			DelegateDeclarationSyntax or
			EventFieldDeclarationSyntax or
			ConversionOperatorDeclarationSyntax or
			OperatorDeclarationSyntax or
			BaseNamespaceDeclarationSyntax
				=> true,

			PropertyDeclarationSyntax property
				=> property.IsAutoProperty(),

			_ => false
		};
	}

	/// <summary>
	/// Determines whether the specified <paramref name="node"/> can have an explicit base type.
	/// </summary>
	/// <param name="node"><see cref="TypeDeclarationSyntax"/> to check whether can have an explicit base type.</param>
	public static bool SupportsExplicitBaseType(this BaseTypeDeclarationSyntax node)
	{
		return node switch
		{
			ClassDeclarationSyntax => !node.IsStatic(),
			RecordDeclarationSyntax record => record.IsClass(),
			EnumDeclarationSyntax or InterfaceDeclarationSyntax => true,
			_ => false
		};
	}

	private static bool HasConstraintExplicit(TypeParameterConstraintSyntax node, GenericConstraint constraint)
	{
		return constraint switch
		{
			GenericConstraint.Class => node is ClassOrStructConstraintSyntax @class && @class.IsClass(),
			GenericConstraint.Struct => node is ClassOrStructConstraintSyntax @struct && @struct.IsStruct(),
			GenericConstraint.New => node is ConstructorConstraintSyntax,
			GenericConstraint.Unmanaged => node is TypeConstraintSyntax unmanaged && unmanaged.IsUnmanagedConstraint(),
			GenericConstraint.NotNull => node is TypeConstraintSyntax notnull && notnull.IsNotNullConstraint(),
			GenericConstraint.Type => node is TypeConstraintSyntax type && type.IsTypeConstraint(),
			GenericConstraint.Default => node is DefaultConstraintSyntax,
			_ => false
		};
	}

	private static bool HasConstraintImplicit(TypeParameterConstraintSyntax node, GenericConstraint constraint)
	{
		bool isValid = false;

		if (constraint.HasFlag(GenericConstraint.Class))
		{
			if (node is ClassOrStructConstraintSyntax @class)
			{
				if (!@class.IsClass())
				{
					return false;
				}
			}
			else if (!IsType())
			{
				return false;
			}

			isValid = true;
		}
		else if (constraint.HasFlag(GenericConstraint.Struct))
		{
			if (node is ClassOrStructConstraintSyntax @struct)
			{
				if (!@struct.IsStruct())
				{
					return false;
				}
			}
			else if (!IsUnmanaged())
			{
				return false;
			}

			isValid = true;
		}
		else if (constraint.HasFlag(GenericConstraint.Unmanaged))
		{
			if (!IsUnmanaged())
			{
				return false;
			}

			isValid = true;
		}
		else if (constraint.HasFlag(GenericConstraint.NotNull))
		{
			if (node is not TypeConstraintSyntax notnull || !notnull.IsNotNullConstraint())
			{
				return false;
			}

			isValid = true;
		}
		else if (constraint.HasFlag(GenericConstraint.Default))
		{
			if (node is not DefaultConstraintSyntax)
			{
				return false;
			}

			isValid = true;
		}

		if (constraint.HasFlag(GenericConstraint.Type))
		{
			if (!IsType())
			{
				return false;
			}

			isValid = true;
		}

		if (constraint.HasFlag(GenericConstraint.New))
		{
			if (node is not ConstructorConstraintSyntax)
			{
				if (node is not ClassOrStructConstraintSyntax @struct || !@struct.IsStruct())
				{
					return false;
				}
			}

			isValid = true;
		}

		return isValid;

		bool IsType()
		{
			return node is TypeConstraintSyntax type && type.IsTypeConstraint();
		}

		bool IsUnmanaged()
		{
			return node is TypeConstraintSyntax unmanaged && unmanaged.IsUnmanagedConstraint();
		}
	}
}
