// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	public partial class CodeBuilder
	{
		public CodeBuilder Field(IFieldSymbol field)
		{
		}

		public CodeBuilder Event(IEventSymbol @event)
		{
		}

		public CodeBuilder EventList()
		{

		}

		public CodeBuilder FieldList()
		{

		}

		public CodeBuilder LocalList()
		{

		}

		/// <summary>
		/// Begins declaration of an indexer.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Indexer(IPropertySymbol property, MethodStyle body = CodeGeneration.MethodStyle.Block)
		{
			Accessibility(property);
			WritePropertyModifiers(property);
			ReturnType(property);

			TextBuilder.Append("this[");

			ImmutableArray<IParameterSymbol> parameters = property.Parameters;

			if(parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					CommaSpace();
					Parameter(parameters[i]);
				}
			}

			TextBuilder.Append(']');

			return MethodBody(body);
		}

		/// <summary>
		/// Begins declaration of a property.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		/// <param name="kind">Kind of auto-property to write.</param>
		public CodeBuilder Property(IPropertySymbol property, AutoPropertyKind kind)
		{
			Accessibility(property);

			if (property.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			WritePropertyModifiers(property);
			ReturnType(property);
			SimpleName(property);

			switch (kind)
			{
				case AutoPropertyKind.GetOnly:
					TextBuilder.Append(" { get; }");
					break;

				case AutoPropertyKind.GetSet:
					TextBuilder.Append(" { get; set; }");
					break;

				case AutoPropertyKind.GetInit:
					TextBuilder.Append(" { get; init; }");
					break;
			}

			return this;
		}

		private void WritePropertyModifiers(IPropertySymbol property)
		{
			WriteExternAndVirtualityModifiers(property);

			if (property.IsReadOnlyContext())
			{
				TextBuilder.Append("readonly ");
			}

			TryUnsafe(property);
		}

		/// <summary>
		/// Begins declaration of a property.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Property(IPropertySymbol property, MethodStyle body = CodeGeneration.MethodStyle.Block)
		{
			Accessibility(property);

			if (property.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			WritePropertyModifiers(property);
			ReturnType(property);
			SimpleName(property);
			return MethodBody(body);
		}

		public CodeBuilder Constant(ILocalSymbol local)
		{
			InitBuilder();

			if(local.IsConst)
			{
				TextBuilder.Append("const ");
			}
		}

		public CodeBuilder Constant(IFieldSymbol field)
		{

		}

		/// <summary>
		/// Begins declaration of a local variable.
		/// </summary>
		/// <param name="local"><see cref="ILabelSymbol"/> to begin the declaration of.</param>
		/// <param name="format">Specifies how to format the <paramref name="local"/>.</param>
		public CodeBuilder Local(ILocalSymbol local, LocalFormat format = default)
		{
			if(local.RefKind.GetText(false) is string refKind)
			{
				TextBuilder.Append(refKind);
				TextBuilder.Append(' ');
			}

			if(format.HasFlag(LocalFormat.ImplicitType) && !local.IsRef)
			{
				TextBuilder.Append("var ");
			}
			else
			{
				Type(local.Type);
			}

			if(format.HasFlag(LocalFormat.SkipInitializer))
			{
				TextBuilder.Append(';');
				NewLine();
			}
			else
			{
				TextBuilder.Append('=');
				TextBuilder.Append(' ');
			}

			return this;
		}

		/// <summary>
		/// Writes a label statement.
		/// </summary>
		/// <param name="label"><see cref="ILabelSymbol"/> to write.</param>
		public CodeBuilder Label(ILabelSymbol label)
		{
			SimpleName(label);
			TextBuilder.Append(':');
			return this;
		}

		/// <summary>
		/// Writes accessibility modifier of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the accessibility modifier of.</param>
		public CodeBuilder Accessibility(ISymbol symbol)
		{
			Accessibility defaultAccessibility = symbol.GetDefaultAccessibility(false);

			switch (defaultAccessibility)
			{
				case Microsoft.CodeAnalysis.Accessibility.Private:

					if(_style.UseExplicitPrivate)
					{
						TextBuilder.Append("public ");
					}

					break;

				case Microsoft.CodeAnalysis.Accessibility.Internal:

					if(_style.UseExplicitInternal)
					{
						TextBuilder.Append("internal ");
					}

					break;

				case Microsoft.CodeAnalysis.Accessibility.Public:

					if(_style.InterfaceMemberStyle.HasFlag(InterfaceMemberStyle.ExplicitAccess))
					{
						TextBuilder.Append("public ");
					}

					break;

				default:
					goto case Microsoft.CodeAnalysis.Accessibility.Private;
			}

			return this;
		}

		/// <summary>
		/// Writes accessibility modifier of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the accessibility modifier of.</param>
		/// <param name="skipDefault">Determines whether to skip the default accessibility in the context of the current <paramref name="symbol"/>.</param>
		public CodeBuilder Accessibility(ISymbol symbol, bool skipDefault)
		{
			InitBuilder();

			if(skipDefault && symbol.DeclaredAccessibility == symbol.GetDefaultAccessibility())
			{
				return this;
			}

			return Accessibility(symbol.DeclaredAccessibility);
		}

		/// <summary>
		/// Writes the specified <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to write.</param>
		public CodeBuilder Array(IArrayTypeSymbol array)
		{
			ITypeSymbol element = array.ElementType;

			if (element is IArrayTypeSymbol elementArray)
			{
				Queue<IArrayTypeSymbol> childArrays = new();

				while (elementArray is not null)
				{
					childArrays.Enqueue(elementArray);
					element = elementArray.ElementType;
					elementArray = (element as IArrayTypeSymbol)!;
				}

				Type(element);
				WriteArrayBrackets(array);

				while (childArrays.Count > 0)
				{
					elementArray = childArrays.Dequeue();
					Nullability(elementArray);
					WriteArrayBrackets(elementArray);
				}
			}
			else
			{
				Type(element);
				WriteArrayBrackets(array);
			}

			Nullability(array);

			return this;

			void WriteArrayBrackets(IArrayTypeSymbol a)
			{
				int rank = a.Rank;
				TextBuilder.Append('[');

				for (int i = 1; i < rank; i++)
				{
					TextBuilder.Append(',');
				}

				TextBuilder.Append(']');
			}
		}

		/// <summary>
		/// Writes a list of base types of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write the base type list of.</param>
		public CodeBuilder BaseTypeList(INamedTypeSymbol type)
		{
			InitBuilder();

			ImmutableArray<INamedTypeSymbol> interfaces = type.Interfaces;

			bool hasBegun = false;

			if (type.HasExplicitBaseType())
			{
				Space();
				TextBuilder.Append(':');
				Space();

				Name(type.BaseType!, SymbolName.Substituted);
				hasBegun = true;
			}

			if(interfaces.Length == 0)
			{
				return this;
			}

			if(hasBegun)
			{
				CommaSpace();
			}
			else
			{
				Space();
				TextBuilder.Append(':');
				Space();
			}

			Name(interfaces[0], SymbolName.Substituted);

			for (int i = 1; i < interfaces.Length; i++)
			{
				CommaSpace();
				Name(interfaces[i], SymbolName.Substituted);
			}

			return this;
		}

		/// <summary>
		/// Writes a list of base <paramref name="types"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="INamedTypeSymbol"/> to write.</param>
		public CodeBuilder BaseTypeList(IEnumerable<INamedTypeSymbol> types)
		{
			return BaseTypeList(types.ToImmutableArray());
		}

		/// <summary>
		/// Writes a list of base <paramref name="types"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="INamedTypeSymbol"/> to write.</param>
		public CodeBuilder BaseTypeList(ImmutableArray<INamedTypeSymbol> types)
		{
			InitBuilder();

			if (types.Length == 0)
			{
				return this;
			}

			TextBuilder.Append(':');
			Space();

			Name(types[0], SymbolName.Substituted);

			for (int i = 1; i < types.Length; i++)
			{
				CommaSpace();
				Name(types[i], SymbolName.Substituted);
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of an anonymous function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder AnonymousFunction(IMethodSymbol method)
		{
			return AnonymousFunction(method, _style.LambdaStyle, _style.UseLambdaReturnType);
		}

		/// <summary>
		/// Begins declaration of an anonymous function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines what kind of anonymous function body to begin.</param>
		/// <param name="explicitType">Determines whether to apply explicit return type.</param>
		public CodeBuilder AnonymousFunction(IMethodSymbol method, LambdaStyle body = LambdaStyle.Expression, bool explicitType = false)
		{
			InitBuilder();

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (body == LambdaStyle.Method)
			{
				TextBuilder.Append("delegate");
			}
			else
			{
				if (explicitType)
				{
					ReturnType(method);
				}

				ImmutableArray<IParameterSymbol> parameters = method.Parameters;

				if (parameters.Length == 0)
				{
					TextBuilder.Append('(');
					TextBuilder.Append(')');
				}
				else if (parameters.Length == 1)
				{
					SimpleName(method.Parameters[0]);
				}
				else
				{
					TextBuilder.Append('(');
					SimpleName(method.Parameters[0]);

					for (int i = 1; i < parameters.Length; i++)
					{
						CommaSpace();
						SimpleName(method.Parameters[i]);
					}

					TextBuilder.Append(')');
				}
			}

			switch (body)
			{
				case LambdaStyle.Block:
					Space();
					TextBuilder.Append('=');
					TextBuilder.Append('>');
					BeginBlock();
					break;

				case LambdaStyle.Expression:
					Space();
					TextBuilder.Append('=');
					TextBuilder.Append('>');
					Space();
					break;

				case LambdaStyle.Method:
					BeginBlock();
					break;

				default:
					TextBuilder.Append(';');
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a class.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Class(INamedTypeSymbol type)
		{
			Accessibility(type);

			if (type.IsStatic)
			{
				TextBuilder.Append("static ");
			}
			else if (type.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}
			else if (type.IsSealed)
			{
				TextBuilder.Append("sealed ");
			}

			TryUnsafe(type);
			TryPartial(type);

			TextBuilder.Append("class ");

			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters);
				BaseTypeList(type);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				BaseTypeList(type);
			}

			return BeginBlock();
		}

		/// <summary>
		/// Begins declaration of a constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Constructor(IMethodSymbol method, MethodStyle body = CodeGeneration.MethodStyle.Block)
		{
			Accessibility(method);

			TryUnsafe(method);
			SimpleName(method.ContainingType);

			ParameterList(method);
			MethodBody(body);

			return this;
		}

		/// <summary>
		/// Begins declaration of a constructor with a <see langword="this"/> or <see langword="base"/> call.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="initializer">Determines which constructor initializer to use.</param>
		public CodeBuilder ConstructorWithInitializer(IMethodSymbol method, ConstructorInitializer initializer = default)
		{
			Accessibility(method);

			TryUnsafe(method);
			SimpleName(method.ContainingType);

			ParameterList(method);

			switch (initializer)
			{
				case ConstructorInitializer.This:
					TextBuilder.Append(" : this");
					break;

				case ConstructorInitializer.Base:
					TextBuilder.Append(" : base");
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder ConversionOperator(IMethodSymbol method, MethodStyle body = CodeGeneration.MethodStyle.Block)
		{
			Accessibility(method);

			TextBuilder.Append("static ");

			TryUnsafe(method);

			if (method.Name == WellKnownMemberNames.ImplicitConversionName)
			{
				TextBuilder.Append("implicit ");
			}
			else
			{
				TextBuilder.Append("explicit ");
			}

			TextBuilder.Append("operator ");

			Type(method.ReturnType);

			ParameterList(method);
			MethodBody(body);

			return this;
		}

		/// <summary>
		/// Begins declaration of a method of any valid kind.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		/// <exception cref="ArgumentException"><paramref name="method"/> is of kind does not support declarations.</exception>
		public CodeBuilder Declaration(IMethodSymbol method, MethodStyle body = CodeGeneration.MethodStyle.Block)
		{
			return method.MethodKind switch
			{
				MethodKind.Ordinary => Method(method, body),
				MethodKind.ReducedExtension => Method(method.ReducedFrom!, body),
				MethodKind.StaticConstructor => StaticConstructor(method, body),
				MethodKind.LocalFunction => LocalFunction(method, body),
				MethodKind.Conversion => ConversionOperator(method, body),
				MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator => Operator(method, body),
				MethodKind.ExplicitInterfaceImplementation => ExplicitInterfaceImplementation(method, body),
				MethodKind.LambdaMethod => AnonymousFunction(method, body.AsLambda()),
				MethodKind.Destructor => Destructor(method, body),
				MethodKind.Constructor => Constructor(method, body),
				MethodKind.EventAdd => Accessor(method, CodeGeneration.Accessor.Add, body),
				MethodKind.EventRemove => Accessor(method, CodeGeneration.Accessor.Remove, body),
				MethodKind.PropertyGet => Accessor(method, CodeGeneration.Accessor.Get, body),
				MethodKind.PropertySet => Accessor(method, method.IsInitOnly ? CodeGeneration.Accessor.Init : CodeGeneration.Accessor.Set, body),
				_ => throw new ArgumentException($"Method '{method}' is of kind that does not support declarations", nameof(method))
			};
		}

		/// <summary>
		/// Begins declaration of a type of any valid kind.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <exception cref="ArgumentException"><paramref name="type"/> is of kind does not support declarations.</exception>
		public CodeBuilder Declaration(INamedTypeSymbol type)
		{
			if (type.IsRecord)
			{
				return Record(type);
			}

			return type.TypeKind switch
			{
				TypeKind.Class => Class(type),
				TypeKind.Struct => Struct(type),
				TypeKind.Enum => Enum(type),
				TypeKind.Delegate => Delegate(type),
				TypeKind.Interface => Interface(type),
				_ => throw new ArgumentException($"Type '{type}' is of kind that does not support declarations", nameof(type))
			};
		}

		/// <summary>
		/// Begins declaration of a destructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Destructor(IMethodSymbol method, MethodStyle body = CodeGeneration.MethodStyle.Block)
		{
			InitBuilder();

			TryUnsafe(method);

			TextBuilder.Append('~');
			SimpleName(method.ContainingType);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			MethodBody(body);

			return this;
		}

		/// <summary>
		/// Begins declaration of an enum.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Enum(INamedTypeSymbol type)
		{
			Accessibility(type);
			TextBuilder.Append("enum ");
			SimpleName(type);

			if(type.EnumUnderlyingType is not null && type.EnumUnderlyingType.SpecialType == SpecialType.System_Int32)
			{
				Space();
				TextBuilder.Append(':');
				Space();

				TextBuilder.Append(type.EnumUnderlyingType.SpecialType.GetKeyword()!);
			}

			BeginBlock();

			return this;
		}

		/// <summary>
		/// Begins declaration of an explicit interface method implementation.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder ExplicitInterfaceImplementation(IMethodSymbol method)
		{
			return ExplicitInterfaceImplementation(method, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of an explicit interface method implementation.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder ExplicitInterfaceImplementation(IMethodSymbol method, MethodStyle body)
		{
			InitBuilder();

			if (method.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			TryExtern(method);
			TryUnsafe(method);

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (method.ExplicitInterfaceImplementations.Length > 0)
			{
				Type(method.ExplicitInterfaceImplementations[0].ContainingType);
			}

			WriteMethodHead(method, body);
			return this;
		}

		/// <summary>
		/// Begins an attribute list.
		/// </summary>
		/// <param name="type">Type of first attribute in the list.</param>
		public CodeBuilder AttributeList(INamedTypeSymbol type)
		{
			InitBuilder();

			TextBuilder.Append('[');
			return Name(type, SymbolName.Attribute);
		}

		/// <summary>
		/// Begins an attribute list with an <see cref="AttributeTarget"/>.
		/// </summary>
		/// <param name="type">Type of first attribute in the list.</param>
		/// <param name="target">Target of the attribute.</param>
		public CodeBuilder AttributeList(INamedTypeSymbol type, AttributeTarget target)
		{
			AttributeList(target);
			return Name(type, SymbolName.Attribute);
		}

		/// <summary>
		/// Begins declaration of an interface.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Interface(INamedTypeSymbol type)
		{
			Accessibility(type);

			TryUnsafe(type);
			TryPartial(type);

			TextBuilder.Append("interface ");
			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters, true);
				BaseTypeList(type.Interfaces);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				BaseTypeList(type.Interfaces);
			}

			return BeginBlock();
		}

		/// <summary>
		/// Begins declaration of a local function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder LocalFunction(IMethodSymbol method)
		{
			return LocalFunction(method, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a local function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder LocalFunction(IMethodSymbol method, MethodStyle body)
		{
			InitBuilder();

			TryStatic(method);
			TryExtern(method);
			TryUnsafe(method);
			TryAsync(method);

			WriteMethodHead(method, body);
			return this;
		}

		private void TryAsync(IMethodSymbol method)
		{
			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}
		}

		private void TryExtern(IMethodSymbol method)
		{
			if (method.IsExtern)
			{
				TextBuilder.Append("extern ");
			}
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Accessor(IMethodSymbol method)
		{
			return Accessor(method, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Accessor(IMethodSymbol method, MethodStyle body)
		{
			return Accessor(method, method.MethodKind.AsAccessor(), body);
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		public CodeBuilder Accessor(IMethodSymbol method, Accessor accessor)
		{
			return Accessor(method, accessor, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Accessor(IMethodSymbol method, Accessor accessor, MethodStyle body)
		{
			InitBuilder();

			if(method.AssociatedSymbol is null)
			{
				return this;
			}

			if(method.DeclaredAccessibility < method.AssociatedSymbol.DeclaredAccessibility)
			{
				Accessibility(method.DeclaredAccessibility);
			}

			if (method.IsReadOnly)
			{
				switch (method.AssociatedSymbol)
				{
					case IPropertySymbol property:

						if(!property.IsReadOnlyContext())
						{
							TextBuilder.Append("readonly ");
						}

						break;

					case IEventSymbol @event:

						if(!@event.IsReadOnlyContext())
						{
							TextBuilder.Append("readonly ");
						}

						break;
				}
			}

			return Accessor(accessor, body);
		}

		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Method(IMethodSymbol method)
		{
			return Method(method, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Method(IMethodSymbol method, MethodStyle body)
		{
			Accessibility(method);
			TryStatic(method);

			WriteExternAndVirtualityModifiers(method);

			if (method.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			TryUnsafe(method);

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			TryPartial(method);

			WriteMethodHead(method, body);
			return this;
		}

		private void TryStatic(IMethodSymbol method)
		{
			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}
		}

		private bool CanUseAbstractOrVirtual(ISymbol symbol)
		{
			if(symbol.ContainingType is INamedTypeSymbol type && type.TypeKind == TypeKind.Interface)
			{
				return _style.InterfaceMemberStyle.HasFlag(InterfaceMemberStyle.ExplicitVirtual);
			}

			return true;
		}

		private void WriteExternAndVirtualityModifiers(ISymbol symbol)
		{
			if (symbol.IsExtern)
			{
				TextBuilder.Append("extern ");
			}

			if (symbol.IsAbstract)
			{
				if(CanUseAbstractOrVirtual(symbol))
				{
					TextBuilder.Append("abstract ");
				}
			}
			else if (symbol.IsSealed)
			{
				TextBuilder.Append("sealed ");
			}
			else if (symbol.IsVirtual)
			{
				if (CanUseAbstractOrVirtual(symbol))
				{
					TextBuilder.Append("virtual ");
				}
			}

			if (symbol.IsOverride)
			{
				TextBuilder.Append("override ");
			}
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		public CodeBuilder Namesapce(INamespaceSymbol @namespace, bool includeParent = true)
		{
			return Namespace(@namespace, _style.NamespaceStyle, includeParent);
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		public CodeBuilder Namespace(INamespaceSymbol @namespace, NamespaceStyle type, bool includeParent = true)
		{
			InitBuilder();

			TextBuilder.Append("namespace ");

			if(includeParent && @namespace.ContainingNamespace is not null)
			{
				if(type == NamespaceStyle.Nested)
				{
					foreach (INamespaceSymbol parent in @namespace.ContainingNamespace.GetContainingNamespaces())
					{
						SimpleName(parent);
						BeginBlock();
						Indent();
					}
				}
				else
				{
					foreach (INamespaceSymbol parent in @namespace.ContainingNamespace.GetContainingNamespaces())
					{
						SimpleName(parent);
						TextBuilder.Append('.');
					}
				}

				SimpleName(@namespace.ContainingNamespace);
			}

			switch (type)
			{
				case NamespaceStyle.Default:
					TextBuilder.Append('.');
					SimpleName(@namespace);
					BeginBlock();
					break;

				case NamespaceStyle.File:
					TextBuilder.Append('.');
					SimpleName(@namespace);
					TextBuilder.Append(';');
					NewLine();
					break;

				case NamespaceStyle.Nested:
					BeginBlock();
					Indent();
					SimpleName(@namespace);
					BeginBlock();
					break;

				default:
					goto case NamespaceStyle.Default;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of an operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Operator(IMethodSymbol method)
		{
			return Operator(method, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of an operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Operator(IMethodSymbol method, MethodStyle body)
		{
			Accessibility(method);

			TextBuilder.Append("static ");

			WriteExternAndVirtualityModifiers(method);

			TryUnsafe(method);

			Type(method.ReturnType);

			TextBuilder.Append(" operator ");

			if (AnalysisUtilities.GetOperatorText(method.Name) is string operatorName)
			{
				TextBuilder.Append(operatorName);
			}

			ParameterList(method);
			MethodBody(body);

			return this;
		}

		/// <summary>
		/// Begins declaration of a record.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a declaration body.</param>
		public CodeBuilder Record(INamedTypeSymbol type, bool body = true)
		{
			return Record(type, _style.RecordStyle, body);
		}

		/// <summary>
		/// Begins declaration of a record.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <param name="style">Determines the record declaration is written.</param>
		/// <param name="body">Determines whether to begin a declaration body.</param>
		public CodeBuilder Record(INamedTypeSymbol type, RecordStyle style, bool body = true)
		{
			Accessibility(type);

			if(type.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if(type.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}

			TryUnsafe(type);
			TryPartial(type);

			TextBuilder.Append("record ");

			if(type.IsValueType)
			{
				TextBuilder.Append("struct ");
			}
			else if(style.HasFlag(RecordStyle.ExplicitClass))
			{
				TextBuilder.Append("class ");
			}

			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters);
			}

			if (style.HasFlag(RecordStyle.PrimaryConstructor) && type.GetPrimaryConstructor() is IMethodSymbol ctor)
			{
				ParameterList(ctor);
			}

			if (type.IsGenericType)
			{
				ConstraintList(type.TypeParameters);
			}

			if (body)
			{
				BeginBlock();
			}
			else
			{
				TextBuilder.Append(';');
				NewLine();
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a static constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder StaticConstructor(IMethodSymbol method)
		{
			return StaticConstructor(method, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a static constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder StaticConstructor(IMethodSymbol method, MethodStyle body)
		{
			InitBuilder();

			TextBuilder.Append("static ");

			TryUnsafe(method);

			SimpleName(method.ContainingType);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			MethodBody(body);

			return this;
		}

		/// <summary>
		/// Begins declaration of a struct.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Struct(INamedTypeSymbol type)
		{
			Accessibility(type);

			if (type.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			TryUnsafe(type);

			if (type.IsRefLikeType)
			{
				TextBuilder.Append("ref ");
			}

			TryPartial(type);

			TextBuilder.Append("struct ");
			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters);
				BaseTypeList(type);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				BaseTypeList(type);
			}

			return BeginBlock();
		}

		private void TryPartial(ISymbol symbol)
		{
			if (symbol.IsPartial())
			{
				TextBuilder.Append("partial ");
			}
		}

		/// <summary>
		/// Writes constraint of the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write the constraint of.</param>
		public CodeBuilder Constraint(ITypeParameterSymbol typeParameter)
		{
			InitBuilder();

			bool hasConstraint = false;

			if (typeParameter.HasReferenceTypeConstraint)
			{
				TextBuilder.Append("class");

				if (typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated)
				{
					TextBuilder.Append('?');
				}

				hasConstraint = true;
			}
			else if (typeParameter.HasUnmanagedTypeConstraint)
			{
				TextBuilder.Append("unmanaged");
				hasConstraint = true;
			}
			else if (typeParameter.HasValueTypeConstraint)
			{
				TextBuilder.Append("struct");
				hasConstraint = true;
			}
			else if (typeParameter.HasNotNullConstraint)
			{
				TextBuilder.Append("notnull");
				hasConstraint = true;
			}

			ImmutableArray<ITypeSymbol> constraintTypes = typeParameter.ConstraintTypes;

			if (constraintTypes.Length > 0)
			{
				EnsureComma(ref hasConstraint);
				ImmutableArray<NullableAnnotation> nullables = typeParameter.ConstraintNullableAnnotations;
				Type(constraintTypes[0], nullables[0]);

				for (int i = 1; i < constraintTypes.Length; i++)
				{
					CommaSpace();
					Type(constraintTypes[i], nullables[i]);
				}
			}

			if (typeParameter.HasConstructorConstraint)
			{
				EnsureComma(ref hasConstraint);
				TextBuilder.Append("new()");
			}

			return this;

			void EnsureComma(ref bool hasConstraint)
			{
				if (hasConstraint)
				{
					CommaSpace();
					hasConstraint = true;
				}
			}
		}

		/// <summary>
		/// Writes a list of constraints of the specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write the constraints of.</param>
		public CodeBuilder ConstraintList(IEnumerable<ITypeParameterSymbol> typeParameters)
		{
			InitBuilder();

			foreach (ITypeParameterSymbol typeParameter in typeParameters)
			{
				if (typeParameter.HasConstraint())
				{
					TextBuilder.Append(" where ");
					SimpleName(typeParameter);
					TextBuilder.Append(':');
					Space();
					Constraint(typeParameter);
				}
			}

			return this;
		}

		/// <summary>
		/// Writes default value of the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write the default value of.</param>
		public CodeBuilder DefaultValue(IParameterSymbol parameter)
		{
			InitBuilder();

			if (parameter.HasExplicitDefaultValue)
			{
				Space();
				TextBuilder.Append('=');
				Space();

				if (parameter.ExplicitDefaultValue is null)
				{
					TextBuilder.Append("default");
				}
				else
				{
					TextBuilder.Append(parameter.ExplicitDefaultValue.ToString());
				}
			}

			return this;
		}

		/// <summary>
		/// Writes a delegate declaration.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write the declaration of.</param>
		/// <exception cref="ArgumentException"><see cref="INamedTypeSymbol.DelegateInvokeMethod"/> cannot be <see langword="null"/>.</exception>
		public CodeBuilder Delegate(INamedTypeSymbol type)
		{
			if (type.DelegateInvokeMethod is null)
			{
				throw new ArgumentException("DelegateInvokeMethod cannot be null", nameof(type));
			}

			Accessibility(type);

			TryUnsafe(type);

			TextBuilder.Append("delegate ");

			ReturnType(type.DelegateInvokeMethod);

			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters, true);
				ParameterList(type.DelegateInvokeMethod);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				ParameterList(type.DelegateInvokeMethod);
			}

			TextBuilder.Append(';');
			NewLine();

			return this;
		}

		private void TryUnsafe(ISymbol symbol)
		{
			if (symbol.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="type"><see cref="IDynamicTypeSymbol"/> to write.</param>
		public CodeBuilder Dynamic(IDynamicTypeSymbol type)
		{
			return Dynamic(type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="annotation"><see cref="Microsoft.CodeAnalysis.NullableAnnotation"/> that should be used.</param>>
		public CodeBuilder Dynamic(NullableAnnotation annotation = default)
		{
			InitBuilder();

			TextBuilder.Append("dynamic");
			Nullability(annotation);
			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IFunctionPointerTypeSymbol"/> to write.</param>
		public CodeBuilder FunctionPointer(IFunctionPointerTypeSymbol pointer)
		{
			return FunctionPointer(pointer, _style.UseExplicitManaged);
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IFunctionPointerTypeSymbol"/> to write.</param>
		/// <param name="explicitManaged">Determines whether to use explicit <see langword="managed"/> keyword in function pointers.</param>
		public CodeBuilder FunctionPointer(IFunctionPointerTypeSymbol pointer, bool explicitManaged)
		{
			InitBuilder();

			IMethodSymbol signature = pointer.Signature;

			TextBuilder.Append("delegate*");

			if (signature.CallingConvention == SignatureCallingConvention.Unmanaged)
			{
				ImmutableArray<INamedTypeSymbol> callConv = signature.UnmanagedCallingConventionTypes;

				TextBuilder.Append(" unmanaged");

				if (callConv.Length > 0)
				{
					TextBuilder.Append(callConv[0].Name);

					for (int i = 1; i < callConv.Length; i++)
					{
						TextBuilder.Append(", ");
						TextBuilder.Append(callConv[1].Name);
					}
				}
			}
			else if(explicitManaged && signature.CallingConvention == SignatureCallingConvention.Default)
			{
				TextBuilder.Append(" managed");
			}

			TextBuilder.Append('<');

			ImmutableArray<IParameterSymbol> parameters = signature.Parameters;

			if (parameters.Length > 0)
			{
				WriteParameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(',');
					Space();
					WriteParameter(parameters[i]);
				}
			}

			Type(signature.ReturnType);

			TextBuilder.Append('>');

			return this;

			void WriteParameter(IParameterSymbol parameter)
			{
				if (parameter.RefKind.GetText() is string value)
				{
					TextBuilder.Append(value);
					Space();
					Type(parameter.Type);
				}
			}
		}

		/// <summary>
		/// Writes name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the name of.</param>
		/// <param name="format">Format of the name.</param>
		public CodeBuilder Name(ISymbol symbol, SymbolName format = default)
		{
			InitBuilder();

			if (symbol is INamedTypeSymbol type && KeywordType(type, format == SymbolName.SystemName))
			{
				return this;
			}

			switch (format)
			{
				case SymbolName.Default:
					SimpleName(symbol);
					break;

				case SymbolName.Generic:
					return GenericName(symbol, false, false);

				case SymbolName.VarianceGeneric:
					return GenericName(symbol, false, true);

				case SymbolName.Substituted:
					return GenericName(symbol, true, false);

				case SymbolName.Attribute:
					return AttributeName(symbol);

				default:
					goto case SymbolName.Default;
			}

			return this;
		}

		/// <summary>
		/// Writes nullability marker '?' if the specified <paramref name="type"/> is nullable.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write the nullability of.</param>
		public CodeBuilder Nullability(ITypeSymbol type)
		{
			return Nullability(type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write.</param>
		public CodeBuilder Parameter(IParameterSymbol parameter)
		{
			InitBuilder();

			if (parameter.IsThis)
			{
				TextBuilder.Append("this ");
			}

			if (parameter.RefKind.GetText() is string refKind)
			{
				TextBuilder.Append(refKind);
				TextBuilder.Append(' ');
			}

			if (parameter.IsDiscard)
			{
				TextBuilder.Append('_');
				return this;
			}

			if (parameter.IsParams)
			{
				TextBuilder.Append("params ");
			}

			Type(parameter.Type);
			SimpleName(parameter);
			DefaultValue(parameter);

			return this;
		}

		/// <summary>
		/// Writes parameter list of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to write the parameter list of.</param>
		public CodeBuilder ParameterList(IMethodSymbol method)
		{
			return ParameterList(method.Parameters, method.IsVararg);
		}

		/// <summary>
		/// Writes a list of specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="parameters">Collection of <see cref="IParameterSymbol"/> to write.</param>
		/// <param name="isArgList">Determines whether the <see langword="__arglist"/> should be written in the parameter list.</param>
		public CodeBuilder ParameterList(ImmutableArray<IParameterSymbol> parameters, bool isArgList = false)
		{
			InitBuilder();

			TextBuilder.Append('(');

			if (parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					CommaSpace();
					Parameter(parameters[i]);
				}

				if (isArgList)
				{
					CommaSpace();
					TextBuilder.Append("__arglist");
				}
			}
			else if (isArgList)
			{
				TextBuilder.Append("__arglist");
			}

			TextBuilder.Append(')');

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="parameters">Collection of <see cref="IParameterSymbol"/> to write.</param>
		/// <param name="isArgList">Determines whether the <see langword="__arglist"/> should be written in the parameter list.</param>
		public CodeBuilder ParameterList(IEnumerable<IParameterSymbol> parameters, bool isArgList = false)
		{
			return ParameterList(parameters.ToImmutableArray(), isArgList);
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to write.</param>
		public CodeBuilder Pointer(IPointerTypeSymbol pointer)
		{
			Type(pointer.PointedAtType);
			TextBuilder.Append('*');

			return this;
		}

		/// <summary>
		/// Writes the return type of the specified <paramref name="method"/> (including the <see langword="ref"/> and <see langword="ref"/> <see langword="readonly"/> modifiers).
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to write the return type of.</param>
		public CodeBuilder ReturnType(IMethodSymbol method)
		{
			InitBuilder();

			if (method.ReturnsVoid)
			{
				TextBuilder.Append("void ");
			}
			else
			{
				ReturnType(method.ReturnType, method.ReturnsByRef, method.ReturnsByRefReadonly);
			}

			return this;
		}

		private void ReturnType(ITypeSymbol type, bool isRef, bool isRefReadonly)
		{
			if (isRefReadonly)
			{
				TextBuilder.Append("ref readonly ");
			}
			else if (isRef)
			{
				TextBuilder.Append("ref ");
			}

			Type(type);
			Space();
		}

		/// <summary>
		/// Writes the return type of the specified <paramref name="property"/> (including the <see langword="ref"/> and <see langword="ref"/> <see langword="readonly"/> modifiers).
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to write the return type of.</param>
		public CodeBuilder ReturnType(IPropertySymbol property)
		{
			InitBuilder();
			ReturnType(property.Type, property.ReturnsByRef, property.ReturnsByRefReadonly);
			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder Type(ITypeSymbol type)
		{
			return Type(type, type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write.</param>
		/// <param name="annotation"><see cref="Microsoft.CodeAnalysis.NullableAnnotation"/> that should be used instead of the <paramref name="type"/>'s actual <see cref="Microsoft.CodeAnalysis.NullableAnnotation"/>.</param>>
		public CodeBuilder Type(ITypeSymbol type, NullableAnnotation annotation)
		{
			InitBuilder();

			switch (type)
			{
				case INamedTypeSymbol named:
					return Type(named, annotation);

				case IArrayTypeSymbol array:
					return Array(array);

				case IDynamicTypeSymbol:
					return Dynamic(annotation);

				case IPointerTypeSymbol pointer:
					return Pointer(pointer);

				case IFunctionPointerTypeSymbol functionPointer:
					return FunctionPointer(functionPointer);

				case ITypeParameterSymbol typeParameter:
					return TypeParameter(typeParameter, annotation);

				default:
					SimpleName(type);
					Nullability(annotation);
					return this;
			}
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write.</param>
		public CodeBuilder Type(INamedTypeSymbol type)
		{
			return Type(type, type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write.</param>
		/// <param name="annotation"><see cref="Microsoft.CodeAnalysis.NullableAnnotation"/> that should be used instead of the <paramref name="type"/>'s actual <see cref="Microsoft.CodeAnalysis.NullableAnnotation"/>.</param>>
		public CodeBuilder Type(INamedTypeSymbol type, NullableAnnotation annotation)
		{
			if (type.IsNullableValueType())
			{
				Name(type.TypeArguments[0], SymbolName.Substituted);
				TextBuilder.Append('?');
				return this;
			}

			Name(type, SymbolName.Substituted);
			Nullability(annotation);

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Collection of <see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder TypeArgumentList(ImmutableArray<ITypeSymbol> typeArguments)
		{
			InitBuilder();

			if (typeArguments.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');
			Type(typeArguments[0]);

			for (int i = 1; i < typeArguments.Length; i++)
			{
				CommaSpace();
				Type(typeArguments[0]);
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Collection of <see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder TypeArgumentList(IEnumerable<ITypeSymbol> typeArguments)
		{
			return TypeArgumentList(typeArguments.ToImmutableArray());
		}

		/// <summary>
		/// Writes the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameter(ITypeParameterSymbol typeParameter)
		{
			return TypeParameter(typeParameter, typeParameter.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write.</param>
		/// <param name="annotation"><see cref="Microsoft.CodeAnalysis.NullableAnnotation"/> that should be used instead of the <paramref name="typeParameter"/>'s actual <see cref="Microsoft.CodeAnalysis.NullableAnnotation"/>.</param>>
		public CodeBuilder TypeParameter(ITypeParameterSymbol typeParameter, NullableAnnotation annotation)
		{
			InitBuilder();

			SimpleName(typeParameter);
			Nullability(annotation);

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public CodeBuilder TypeParameterList(ImmutableArray<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			InitBuilder();

			if (typeParameters.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');

			if (includeVariance)
			{
				Variance(typeParameters[0]);
				SimpleName(typeParameters[0]);

				for (int i = 1; i < typeParameters.Length; i++)
				{
					CommaSpace();
					Variance(typeParameters[i]);
					SimpleName(typeParameters[i]);
				}
			}
			else
			{
				SimpleName(typeParameters[0]);

				for (int i = 1; i < typeParameters.Length; i++)
				{
					CommaSpace();
					SimpleName(typeParameters[i]);
				}
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes all specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		///  <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public CodeBuilder TypeParameterList(IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			return TypeParameterList(typeParameters.ToImmutableArray(), includeVariance);
		}

		/// <summary>
		/// Writes variance of the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write the variance of.</param>
		public CodeBuilder Variance(ITypeParameterSymbol typeParameter)
		{
			return Variance(typeParameter.Variance);
		}

		private CodeBuilder AttributeName(ISymbol symbol)
		{
			const string suffix = "Attribute";

			string name = symbol.GetVerbatimName();

			if (name.EndsWith(suffix) && name.Length > suffix.Length)
			{
				TextBuilder.Append(name, 0, name.Length - suffix.Length);
				return this;
			}

			TextBuilder.Append(name);

			if(symbol is INamedTypeSymbol type && type.IsGenericType)
			{
				TypeArgumentList(type.TypeArguments);
			}

			return this;
		}

		private CodeBuilder GenericName(ISymbol symbol, bool parametersOrArguments, bool includeVariance)
		{
			if (symbol is INamedTypeSymbol t)
			{
				return GenericName(t, parametersOrArguments, includeVariance);
			}

			if (symbol is IMethodSymbol m)
			{
				return GenericName(m, includeVariance, includeVariance);
			}

			TextBuilder.Append(symbol.GetVerbatimName());
			return this;
		}

		private CodeBuilder GenericName(INamedTypeSymbol type, bool parametersOrArguments, bool includeVariance)
		{
			SimpleName(type);

			if (parametersOrArguments)
			{
				TypeParameterList(type.TypeParameters, includeVariance);
			}
			else
			{
				TypeArgumentList(type.TypeArguments);
			}

			return this;
		}

		private CodeBuilder GenericName(IMethodSymbol method, bool parametersOrArguments, bool includeVariance)
		{
			SimpleName(method);

			if (parametersOrArguments)
			{
				TypeParameterList(method.TypeParameters, includeVariance);
			}
			else
			{
				TypeArgumentList(method.TypeArguments);
			}

			return this;
		}

		private bool KeywordType(INamedTypeSymbol type, bool useSystemName)
		{
			if (useSystemName)
			{
				if (type is IDynamicTypeSymbol)
				{
					TextBuilder.Append("Object");
					return true;
				}
				else if (type.SpecialType.IsKeyword())
				{
					TextBuilder.Append(type.Name);
					return true;
				}

				return false;
			}

			if (type.GetTypeKeyword() is string keyword)
			{
				TextBuilder.Append(keyword);
				return true;
			}

			return false;
		}

		private void SimpleName(ISymbol symbol)
		{
			TextBuilder.Append(symbol.GetVerbatimName());
		}

		private void WriteMethodHead(IMethodSymbol method, MethodStyle body)
		{
			ReturnType(method);
			SimpleName(method);

			if (method.IsGenericMethod)
			{
				TypeParameterList(method.TypeParameters);
				ParameterList(method);
				ConstraintList(method.TypeParameters);
			}
			else
			{
				ParameterList(method);
			}

			MethodBody(body);
		}
	}
}