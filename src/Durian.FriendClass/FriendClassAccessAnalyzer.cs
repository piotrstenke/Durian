using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass;

/// <summary>
/// Analyzes expressions that attempt to access members of a <see cref="Type"/> with at least one <c>Durian.FriendClassAttribute</c> specified.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FriendClassAccessAnalyzer : DurianAnalyzer<FriendClassCompilationData>
{
	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DUR0302_MemberCannotBeAccessedOutsideOfFriendClass,
		DUR0307_MemberCannotBeAccessedByChildClass,
		DUR0310_MemberCannotBeAccessedByChildClassOfFriend,
		DUR0314_DoNotAccessInheritedStaticMembers
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="FriendClassAccessAnalyzer"/> class.
	/// </summary>
	public FriendClassAccessAnalyzer()
	{
	}

	/// <inheritdoc/>
	public override void Register(IDurianAnalysisContext context, FriendClassCompilationData compilation)
	{
		context.RegisterSyntaxNodeAction(context => AnalyzeIndentifierName(context, compilation),
			SyntaxKind.IdentifierName,
			SyntaxKind.ImplicitObjectCreationExpression
		);

		context.RegisterSyntaxNodeAction(context => AnalyzeMemberDeclaration(context, compilation),
			SyntaxKind.MethodDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.IndexerDeclaration,
			SyntaxKind.EventDeclaration,
			SyntaxKind.EventFieldDeclaration
		);

		context.RegisterSyntaxNodeAction(context => AnalyzeTypeOrConstructorDeclaration(context, compilation),
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.ClassDeclaration,
			SyntaxKind.RecordDeclaration
		);
	}

	/// <inheritdoc/>
	protected override FriendClassCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
	{
		return new FriendClassCompilationData(compilation);
	}

	private static void AnalyzeIndentifierName(SyntaxNodeAnalysisContext context, FriendClassCompilationData compilation)
	{
		if (context.Node.Parent is NameMemberCrefSyntax)
		{
			return;
		}

		if (GetCurrentType(context.ContainingSymbol) is not INamedTypeSymbol currentType)
		{
			return;
		}

		ISymbol? accessedSymbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;

		if (accessedSymbol is null || !FriendClassDeclarationAnalyzer.IsInternal(accessedSymbol))
		{
			return;
		}

		Diagnostic? diagnostic = GetDiagnosticWithAccessCheck(accessedSymbol, currentType, context.Node, compilation, context.SemanticModel);

		if (diagnostic is not null)
		{
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static void AnalyzeMemberDeclaration(SyntaxNodeAnalysisContext context, FriendClassCompilationData compilation)
	{
		if (GetCurrentType(context.ContainingSymbol) is not INamedTypeSymbol currentType)
		{
			return;
		}

		ISymbol? accessedSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

		if (accessedSymbol is null || !FriendClassDeclarationAnalyzer.IsInternal(accessedSymbol))
		{
			return;
		}

		if (!HandleMemberDeclaration(context.Node, ref accessedSymbol))
		{
			return;
		}

		Diagnostic? diagnostic = GetDiagnostic(accessedSymbol, currentType, context.Node, compilation);

		if (diagnostic is not null)
		{
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static void AnalyzeTypeOrConstructorDeclaration(SyntaxNodeAnalysisContext context, FriendClassCompilationData compilation)
	{
		if (GetCurrentType(context.ContainingSymbol) is not INamedTypeSymbol currentType)
		{
			return;
		}

		IMethodSymbol? accessedCtor = GetAccessedConstructorSymbol(context.Node, context.SemanticModel, currentType);

		if (accessedCtor is null || !FriendClassDeclarationAnalyzer.IsInternal(accessedCtor))
		{
			return;
		}

		Diagnostic? diagnostic = GetDiagnostic(accessedCtor, currentType, context.Node, compilation);

		if (diagnostic is not null)
		{
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static IMethodSymbol? GetAccessedConstructorSymbol(SyntaxNode node, SemanticModel semanticModel, INamedTypeSymbol currentType)
	{
		switch (node)
		{
			case ConstructorDeclarationSyntax ctor:
			{
				if (ctor.Initializer is null)
				{
					return currentType.BaseType?.GetSpecialConstructor(SpecialConstructor.Parameterless);
				}
				else if (semanticModel.GetSymbolInfo(ctor.Initializer).Symbol is IMethodSymbol baseCtor)
				{
					return baseCtor;
				}

				break;
			}

			case ClassDeclarationSyntax @class:
			{
				if (@class.BaseList is not null && @class.BaseList.Types.Any())
				{
					return GetParameterlessConstructorFromSymbol(currentType);
				}

				break;
			}

			case RecordDeclarationSyntax record:
			{
				if (record.BaseList is not null && record.BaseList.Types.Any())
				{
					BaseTypeSyntax baseType = record.BaseList.Types[0];

					switch (baseType)
					{
						case PrimaryConstructorBaseTypeSyntax primary:
							SymbolInfo info = semanticModel.GetSymbolInfo(primary);

							if (info.Symbol is IMethodSymbol baseCtor)
							{
								return baseCtor;
							}

							break;

						case SimpleBaseTypeSyntax:
							if (record.ParameterList is not null)
							{
								return currentType.BaseType?.GetSpecialConstructor(SpecialConstructor.Parameterless);
							}

							return GetParameterlessConstructorFromSymbol(currentType);
					}
				}

				break;
			}
		}

		return null;

		static IMethodSymbol? GetParameterlessConstructorFromSymbol(INamedTypeSymbol? type)
		{
			if (type is not null &&
				type.TypeKind == TypeKind.Class &&
				type.InstanceConstructors.All(ctor => ctor.IsImplicitlyDeclared || ctor.DeclaringSyntaxReferences.Length == 0) &&
				type.BaseType?.GetSpecialConstructor(SpecialConstructor.Parameterless) is IMethodSymbol baseCtor
			)
			{
				return baseCtor;
			}

			return default;
		}
	}

	private static INamedTypeSymbol? GetAccessedType(ISymbol accessedSymbol, INamedTypeSymbol currentType)
	{
		if (accessedSymbol.ContainingType is INamedTypeSymbol accessedType &&
			!currentType.ContainsSymbol(accessedType) &&
			!SymbolEqualityComparer.Default.Equals(currentType, accessedType)
		)
		{
			return accessedType;
		}

		return null;
	}

	private static bool GetConfigurationBoolValue(
		INamedTypeSymbol accessedType,
		FriendClassCompilationData compilation,
		AttributeData? configurationAttribute,
		bool accessedConfigurationAttribute,
		string argumentName)
	{
		if (configurationAttribute is null)
		{
			if (accessedConfigurationAttribute || accessedType.GetAttribute(compilation.FriendClassConfigurationAttribute!) is not AttributeData attr)
			{
				return false;
			}

			configurationAttribute = attr;
		}

		return configurationAttribute.GetNamedArgumentValue<bool>(argumentName);
	}

	private static INamedTypeSymbol? GetCurrentType(ISymbol? symbol)
	{
		if (symbol is INamedTypeSymbol type)
		{
			return type;
		}

		return symbol?.ContainingType;
	}

	private static Diagnostic? GetDiagnostic(
		ISymbol accessedSymbol,
		INamedTypeSymbol currentType,
		SyntaxNode node,
		FriendClassCompilationData compilation
	)
	{
		if (GetAccessedType(accessedSymbol, currentType) is not INamedTypeSymbol accessedType)
		{
			return null;
		}

		if (TryGetInvalidFriendDiagnostic(
			accessedSymbol,
			currentType,
			accessedType,
			compilation,
			null,
			false,
			out DiagnosticDescriptor? descriptor)
		)
		{
			return Diagnostic.Create(
				descriptor: descriptor,
				location: node.GetLocation(),
				messageArgs: new object[] { currentType, accessedSymbol }
			);
		}

		return null;
	}

	private static Diagnostic? GetDiagnosticWithAccessCheck(
		ISymbol accessedSymbol,
		INamedTypeSymbol currentType,
		SyntaxNode node,
		FriendClassCompilationData compilation,
		SemanticModel semanticModel
	)
	{
		if (GetAccessedType(accessedSymbol, currentType) is not INamedTypeSymbol accessedType)
		{
			return null;
		}

		HandleSpecificSyntaxNodeTypes(
			node,
			semanticModel,
			compilation,
			currentType,
			ref accessedType,
			out AttributeData? configurationAttribute,
			out bool accessedConfigurationAttribute,
			out bool reportStaticAccess
		);

		if (TryGetInvalidFriendDiagnostic(
			accessedSymbol,
			currentType,
			accessedType,
			compilation,
			configurationAttribute,
			accessedConfigurationAttribute,
			out DiagnosticDescriptor? descriptor
		))
		{
			if (reportStaticAccess)
			{
				return Diagnostic.Create(
					descriptor: DUR0314_DoNotAccessInheritedStaticMembers,
					location: node.GetLocation(),
					messageArgs: new object[] { currentType }
				);
			}

			return Diagnostic.Create(
				descriptor: descriptor,
				location: node.GetLocation(),
				messageArgs: new object[] { currentType, accessedSymbol }
			);
		}

		return null;
	}

	private static bool HandleMemberDeclaration(SyntaxNode node, ref ISymbol accessedSymbol)
	{
		switch (node)
		{
			case MethodDeclarationSyntax:
			{
				if (accessedSymbol is IMethodSymbol method && method.IsOverride && method.OverriddenMethod is not null)
				{
					accessedSymbol = method.OverriddenMethod;
					return true;
				}

				break;
			}

			case PropertyDeclarationSyntax:
			case IndexerDeclarationSyntax:
			{
				if (accessedSymbol is IPropertySymbol property && property.IsOverride && property.OverriddenProperty is not null)
				{
					accessedSymbol = property.OverriddenProperty;
					return true;
				}

				break;
			}

			case EventDeclarationSyntax:
			case EventFieldDeclarationSyntax:
			{
				if (accessedSymbol is IEventSymbol @event && @event.IsOverride && @event.OverriddenEvent is not null)
				{
					accessedSymbol = @event.OverriddenEvent;
					return true;
				}

				break;
			}
		}

		return false;
	}

	private static void HandleSpecificSyntaxNodeTypes(
		SyntaxNode node,
		SemanticModel semanticModel,
		FriendClassCompilationData compilation,
		INamedTypeSymbol currentType,
		ref INamedTypeSymbol accessedType,
		out AttributeData? configurationAttribute,
		out bool accessedConfigurationAttribute,
		out bool reportStaticAccess
	)
	{
		if (node.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression is not ThisExpressionSyntax)
		{
			if (semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is INamedTypeSymbol targetType && targetType.Inherits(accessedType))
			{
				accessedType = targetType;
				reportStaticAccess = true;
				accessedConfigurationAttribute = false;
				configurationAttribute = null;

				return;
			}
			else if (semanticModel.GetTypeInfo(memberAccess.Expression).Type is INamedTypeSymbol type && type.Inherits(accessedType))
			{
				CheckIncludeInherited(type, ref accessedType, out configurationAttribute, out accessedConfigurationAttribute);

				reportStaticAccess = false;
				return;
			}
		}
		else
		{
			reportStaticAccess = false;

			if (currentType.Inherits(accessedType))
			{
				CheckIncludeInherited(currentType, ref accessedType, out configurationAttribute, out accessedConfigurationAttribute);

				return;
			}
			else
			{
				foreach (SyntaxNode ancestor in node.Ancestors())
				{
					if (ancestor is InitializerExpressionSyntax initializer)
					{
						if (semanticModel.GetTypeInfo(initializer.Parent!).Type is INamedTypeSymbol type)
						{
							CheckIncludeInherited(type, ref accessedType, out configurationAttribute, out accessedConfigurationAttribute);
							return;
						}

						break;
					}

					if (ancestor is IsPatternExpressionSyntax pattern)
					{
						if (semanticModel.GetTypeInfo(pattern.Expression).Type is INamedTypeSymbol type)
						{
							CheckIncludeInherited(type, ref accessedType, out configurationAttribute, out accessedConfigurationAttribute);
							return;
						}

						break;
					}

					if (ancestor is MemberDeclarationSyntax)
					{
						break;
					}
				}
			}
		}

		configurationAttribute = null;
		accessedConfigurationAttribute = false;
		reportStaticAccess = false;

		bool CheckIncludeInherited(
			INamedTypeSymbol target,
			ref INamedTypeSymbol accessedType,
			[NotNullWhen(true)] out AttributeData? configurationAttribute,
			out bool accessedConfigurationAttribute
		)
		{
			(INamedTypeSymbol? type, AttributeData? configAttr) = target
				.GetBaseTypes(true)
				.Select(t => (type: t, attribute: t.GetAttribute(compilation.FriendClassConfigurationAttribute!)))
				.FirstOrDefault(t => t.attribute is not null);

			bool hasConfig = false;

			if (configAttr is null)
			{
				configurationAttribute = null;
			}
			else
			{
				configurationAttribute = configAttr;

				if (configAttr.GetNamedArgumentValue<bool>(FriendClassConfigurationAttributeProvider.IncludeInherited))
				{
					accessedType = type;
					hasConfig = true;
				}
			}

			accessedConfigurationAttribute = true;
			return hasConfig;
		}
	}

	private static bool IsChildOfAccessedType(INamedTypeSymbol currentType, INamedTypeSymbol accessedType)
	{
		INamedTypeSymbol? current = currentType;

		while (current is not null)
		{
			if (IsChildOfAccessedType(current))
			{
				return true;
			}

			current = current.ContainingType;
		}

		return false;

		bool IsChildOfAccessedType(INamedTypeSymbol current)
		{
			INamedTypeSymbol? parent = current;

			while ((parent = parent!.BaseType) is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(parent, accessedType))
				{
					return true;
				}
			}

			return false;
		}
	}

	private static bool IsChildOfFriend(
		INamedTypeSymbol currentType,
		List<(AttributeData attr, INamedTypeSymbol friend)> friends,
		out int targetFriendIndex
	)
	{
		INamedTypeSymbol? current = currentType;

		while (current is not null)
		{
			if (IsChildOfFriend(current, out int index))
			{
				targetFriendIndex = index;
				return true;
			}

			current = current.ContainingType;
		}

		targetFriendIndex = default;
		return false;

		bool IsChildOfFriend(INamedTypeSymbol current, out int targetFriendIndex)
		{
			INamedTypeSymbol? parent = current;

			while ((parent = parent!.BaseType) is not null)
			{
				for (int i = 0; i < friends.Count; i++)
				{
					if (SymbolEqualityComparer.Default.Equals(friends[i].friend, parent))
					{
						targetFriendIndex = i;
						return true;
					}
				}
			}

			targetFriendIndex = default;
			return false;
		}
	}

	private static bool IsInnerClassOfFriend(INamedTypeSymbol currentType, List<(AttributeData, INamedTypeSymbol)> friends)
	{
		INamedTypeSymbol? parent = currentType;

		while ((parent = parent!.ContainingType) is not null)
		{
			foreach ((_, INamedTypeSymbol friend) in friends)
			{
				if (SymbolEqualityComparer.Default.Equals(friend, parent))
				{
					return true;
				}
			}
		}

		return false;
	}

	private static bool IsSpecifiedFriend(
		INamedTypeSymbol currentType,
		AttributeData[] attributes,
		[NotNullWhen(false)] out List<(AttributeData attribute, INamedTypeSymbol friend)>? friendList
	)
	{
		List<(AttributeData, INamedTypeSymbol)> friends = new(attributes.Length);

		foreach (AttributeData attr in attributes)
		{
			if (attr.TryGetConstructorArgumentTypeValue(0, out INamedTypeSymbol? friend) && friend is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(friend, currentType))
				{
					friendList = null;
					return true;
				}

				friends.Add((attr, friend));
			}
		}

		if (IsInnerClassOfFriend(currentType, friends))
		{
			friendList = null;
			return true;
		}

		friendList = friends;
		return false;
	}

	private static bool TryGetInvalidFriendDiagnostic(
		ISymbol accessedSymbol,
		INamedTypeSymbol currentType,
		INamedTypeSymbol accessedType,
		FriendClassCompilationData compilation,
		AttributeData? configurationAttribute,
		bool accessedConfigurationAttribute,
		[NotNullWhen(true)] out DiagnosticDescriptor? descriptor
	)
	{
		bool isProtected = accessedSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

		if (isProtected && IsChildOfAccessedType(currentType, accessedType))
		{
			descriptor = null;
			return false;
		}

		AttributeData[] attributes = accessedType.GetAttributes(compilation.FriendClassAttribute!).ToArray();

		if (attributes.Length == 0)
		{
			descriptor = null;
			return false;
		}

		if (!IsSpecifiedFriend(currentType, attributes, out List<(AttributeData attribute, INamedTypeSymbol friend)>? friends))
		{
			if (!isProtected && IsChildOfAccessedType(currentType, accessedType))
			{
				if (!GetConfigurationBoolValue(accessedType, compilation, configurationAttribute, accessedConfigurationAttribute, FriendClassConfigurationAttributeProvider.AllowChildren))
				{
					descriptor = DUR0307_MemberCannotBeAccessedByChildClass;
					return true;
				}
			}
			else if (IsChildOfFriend(currentType, friends, out int targetFriendIndex))
			{
				if (!friends[targetFriendIndex].attribute.GetNamedArgumentValue<bool>(FriendClassAttributeProvider.AllowFriendChildren))
				{
					descriptor = DUR0310_MemberCannotBeAccessedByChildClassOfFriend;
					return true;
				}
			}
			else
			{
				descriptor = DUR0302_MemberCannotBeAccessedOutsideOfFriendClass;
				return true;
			}
		}

		descriptor = null;
		return false;
	}
}
