// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection.Metadata;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	public partial class CodeBuilder
	{
		/// <summary>
		/// Writes accessibility modifier of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the accessibility modifier of.</param>
		public CodeBuilder Accessibility(ISymbol symbol)
		{
			return Accessibility(symbol, Style.UseExplicitDefaultAccessibility);
		}

		/// <summary>
		/// Writes accessibility modifier of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the accessibility modifier of.</param>
		/// <param name="explicitDefaultAccessibility">Determines whether to apply an accessibility modifier even if it is default in the current context.</param>
		public CodeBuilder Accessibility(ISymbol symbol, bool explicitDefaultAccessibility)
		{
			InitBuilder();

			Accessibility defaultAccessibility = symbol.GetDefaultAccessibility(false);

			if(symbol.DeclaredAccessibility != defaultAccessibility && symbol.DeclaredAccessibility.GetText() is string keyword)
			{
				TextBuilder.Append(keyword);
				TextBuilder.Append(' ');
				return this;
			}

			if(!explicitDefaultAccessibility)
			{
				return this;
			}

			switch (defaultAccessibility)
			{
				case Microsoft.CodeAnalysis.Accessibility.Private:

					if (Style.UseExplicitPrivate)
					{
						TextBuilder.Append("private ");
					}

					break;

				case Microsoft.CodeAnalysis.Accessibility.Internal:

					if (Style.UseExplicitInternal)
					{
						TextBuilder.Append("internal ");
					}

					break;

				case Microsoft.CodeAnalysis.Accessibility.Public:

					if (Style.InterfaceMemberStyle.HasFlag(InterfaceMemberStyle.ExplicitAccess))
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
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Accessor(IMethodSymbol method)
		{
			return Accessor(method, Style.MethodStyle);
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
			return Accessor(method, accessor, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Accessor(IMethodSymbol method, Accessor accessor, MethodStyle body)
		{
			Indent();

			if (method.AssociatedSymbol is null)
			{
				return this;
			}

			if (method.DeclaredAccessibility < method.AssociatedSymbol.DeclaredAccessibility)
			{
				Accessibility(method.DeclaredAccessibility);
			}

			if (method.IsReadOnly)
			{
				switch (method.AssociatedSymbol)
				{
					case IPropertySymbol property:

						if (!property.IsReadOnlyContext())
						{
							TextBuilder.Append("readonly ");
						}

						break;

					case IEventSymbol @event:

						if (!@event.IsReadOnlyContext())
						{
							TextBuilder.Append("readonly ");
						}

						break;
				}
			}

			return Accessor(accessor, body);
		}

		/// <summary>
		/// Begins declaration of an anonymous function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder AnonymousFunction(IMethodSymbol method)
		{
			return AnonymousFunction(method, Style.LambdaStyle, Style.UseLambdaReturnType);
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
					SimpleName_Internal(method.Parameters[0]);
				}
				else
				{
					TextBuilder.Append('(');
					SimpleName_Internal(method.Parameters[0]);

					for (int i = 1; i < parameters.Length; i++)
					{
						CommaSpace();
						SimpleName_Internal(method.Parameters[i]);
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
		/// Begins an attribute list.
		/// </summary>
		/// <param name="type">Type of first attribute in the list.</param>
		public CodeBuilder AttributeList(INamedTypeSymbol type)
		{
			if (Style.UseExplicitAttributeTargets)
			{
				return AttributeList(type, type.GetAttributeTargetKind());
			}

			Indent();
			TextBuilder.Append('[');
			return AttributeName(type);
		}

		/// <summary>
		/// Begins an attribute list with an <see cref="AttributeTarget"/>.
		/// </summary>
		/// <param name="type">Type of first attribute in the list.</param>
		/// <param name="target">Target of the attribute.</param>
		public CodeBuilder AttributeList(INamedTypeSymbol type, AttributeTarget target)
		{
			AttributeList(target);
			return AttributeName(type);
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

			if (interfaces.Length == 0)
			{
				return this;
			}

			if (hasBegun)
			{
				CommaSpace();
			}
			else
			{
				Space();
				TextBuilder.Append(':');
				Space();
			}

			GenericName(interfaces[0], true, false);

			for (int i = 1; i < interfaces.Length; i++)
			{
				CommaSpace();
				GenericName(interfaces[i], true, false);
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

			GenericName(types[0], true, false);

			for (int i = 1; i < types.Length; i++)
			{
				CommaSpace();
				GenericName(types[i], true, false);
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a class.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Class(INamedTypeSymbol type)
		{
			Indent();
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

			SimpleName_Internal(type);

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
		/// Begins declaration of a constant.
		/// </summary>
		/// <param name="local"><see cref="ILabelSymbol"/> to begin the declaration of.</param>
		/// <param name="includeValue">Determines whether to include </param>
		public CodeBuilder Constant(ILocalSymbol local, bool includeValue = true)
		{
			Indent();

			WriteConstantHead(local, local.Type);

			if (includeValue)
			{
				WriteConstantValue(local.ConstantValue, local.Type);
				ColonNewLine();
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a constant.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to begin the declaration of.</param>
		/// <param name="includeValue">Determines whether to write the value of the constant.</param>
		public CodeBuilder Constant(IFieldSymbol field, bool includeValue = true)
		{
			Indent();
			Accessibility(field);

			WriteConstantHead(field, field.Type);

			if (includeValue)
			{
				WriteConstantValue(field.ConstantValue, field.Type);
				ColonNewLine();
			}

			return this;
		}

		/// <summary>
		/// Writes a list of constants separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="IFieldSymbol"/>s to write as constants.</param>
		public CodeBuilder ConstantList(ImmutableArray<IFieldSymbol> fields)
		{
			if (fields.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();

			IFieldSymbol first = fields[0];

			Accessibility(first);

			TextBuilder.Append("const ");
			Type(first.Type);
			Space();
			WriteConstant(first);

			for (int i = 1; i < fields.Length; i++)
			{
				CommaSpace();
				WriteConstant(fields[i]);
			}

			ColonNewLine();

			return this;

			void WriteConstant(IFieldSymbol field)
			{
				SimpleName_Internal(field);

				VariableInitializer();

				if (field.HasConstantValue)
				{
					WriteConstantValue(field.ConstantValue, field.Type);
				}
			}
		}

		/// <summary>
		/// Writes a list of constants separated by commas.
		/// </summary>
		/// <param name="locals">Collection of <see cref="ILocalSymbol"/>s to write as constants.</param>
		public CodeBuilder ConstantList(ImmutableArray<ILocalSymbol> locals)
		{
			if (locals.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();

			ILocalSymbol first = locals[0];

			TextBuilder.Append("const ");
			Type(first.Type);
			Space();
			WriteConstant(first);

			for (int i = 1; i < locals.Length; i++)
			{
				CommaSpace();
				WriteConstant(locals[i]);
			}

			ColonNewLine();

			return this;

			void WriteConstant(ILocalSymbol local)
			{
				SimpleName_Internal(local);

				VariableInitializer();

				if (local.HasConstantValue)
				{
					WriteConstantValue(local.ConstantValue, local.Type);
				}
			}
		}

		/// <summary>
		/// Writes a list of constants separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="IFieldSymbol"/>s to write as constants.</param>
		public CodeBuilder ConstantList(IEnumerable<IFieldSymbol> fields)
		{
			return ConstantList(fields.ToImmutableArray());
		}

		/// <summary>
		/// Writes a list of constants separated by commas.
		/// </summary>
		/// <param name="locals">Collection of <see cref="ILocalSymbol"/>s to write as constants.</param>
		public CodeBuilder ConstantList(IEnumerable<ILocalSymbol> locals)
		{
			return ConstantList(locals.ToImmutableArray());
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
					SimpleName_Internal(typeParameter);
					TextBuilder.Append(':');
					Space();
					Constraint(typeParameter);
				}
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Constructor(IMethodSymbol method)
		{
			return Constructor(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Constructor(IMethodSymbol method, MethodStyle body)
		{
			Indent();
			Accessibility(method);

			TryUnsafe(method);
			SimpleName_Internal(method.ContainingType);

			ParameterList(method);
			MethodBody(body);

			return this;
		}

		/// <summary>
		/// Begins declaration of a constructor with a <see langword="this"/> or <see langword="base"/> call.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="initializer">Determines which constructor initializer to use.</param>
		public CodeBuilder Constructor(IMethodSymbol method, ConstructorInitializer initializer)
		{
			Indent();
			Accessibility(method);

			TryUnsafe(method);
			SimpleName_Internal(method.ContainingType);

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
		public CodeBuilder ConversionOperator(IMethodSymbol method)
		{
			return ConversionOperator(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder ConversionOperator(IMethodSymbol method, MethodStyle body)
		{
			Indent();
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
		/// <param name="symbol"><see cref="ISymbol"/> to begin the declaration of.</param>
		/// <exception cref="ArgumentException"><paramref name="symbol"/> is of kind does not support declarations.</exception>
		public CodeBuilder Declaration(ISymbol symbol)
		{
			return symbol switch
			{
				IMethodSymbol method => Declaration(method),
				INamedTypeSymbol type => Declaration(type),
				IFieldSymbol field => Field(field),
				IPropertySymbol property => Property(property),
				IEventSymbol @event => Event(@event),
				ILocalSymbol local => Local(local),
				ILabelSymbol label => Label(label),
				_ => throw new ArgumentException($"Member '{symbol}' is of kind that does not support declarations", nameof(symbol))
			};
		}

		/// <summary>
		/// Begins declaration of a method of any valid kind.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Declaration(IMethodSymbol method)
		{
			return Declaration(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a method of any valid kind.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		/// <exception cref="ArgumentException"><paramref name="method"/> is of kind does not support declarations.</exception>
		public CodeBuilder Declaration(IMethodSymbol method, MethodStyle body)
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
		/// Writes default value of the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write the default value of.</param>
		public CodeBuilder DefaultValue(IParameterSymbol parameter)
		{
			InitBuilder();

			if (parameter.HasExplicitDefaultValue)
			{
				VariableInitializer();

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

			Indent();
			Accessibility(type);

			TryUnsafe(type);

			TextBuilder.Append("delegate ");

			ReturnType(type.DelegateInvokeMethod);

			SimpleName_Internal(type);

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

			ColonNewLine();

			return this;
		}

		/// <summary>
		/// Begins declaration of a destructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Destructor(IMethodSymbol method)
		{
			return Destructor(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a destructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Destructor(IMethodSymbol method, MethodStyle body)
		{
			Indent();

			TryUnsafe(method);

			TextBuilder.Append('~');
			SimpleName_Internal(method.ContainingType);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			MethodBody(body);

			return this;
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
		/// Begins declaration of an enum.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Enum(INamedTypeSymbol type)
		{
			return Enum(type, Style.EnumStyle.HasFlag(EnumStyle.ExplicitInt32));
		}

		/// <summary>
		/// Begins declaration of an enum.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <param name="explicitInt32">Determines whether to write the base type <see cref="int"/> type.</param>
		public CodeBuilder Enum(INamedTypeSymbol type, bool explicitInt32)
		{
			Indent();
			Accessibility(type);
			TextBuilder.Append("enum ");
			SimpleName_Internal(type);

			if (type.EnumUnderlyingType is not null && (!explicitInt32 || type.EnumUnderlyingType.SpecialType != SpecialType.System_Int32))
			{
				Space();
				TextBuilder.Append(':');
				Space();

				TextBuilder.Append(type.EnumUnderlyingType.SpecialType.GetText()!);
			}

			BeginBlock();

			return this;
		}

		/// <summary>
		/// Begins declaration of an enum field.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to begin the declaration of.</param>
		public CodeBuilder EnumField(IFieldSymbol field)
		{
			return EnumField(field, Style.EnumStyle.HasFlag(EnumStyle.ExplicitValues));
		}

		/// <summary>
		/// Begins declaration of an enum field.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to begin the declaration of.</param>
		/// <param name="explicitValue">Determines whether to write explicit value of the field.</param>
		public CodeBuilder EnumField(IFieldSymbol field, bool explicitValue)
		{
			Indent();

			SimpleName_Internal(field);

			if (explicitValue)
			{
				VariableInitializer();
			}
			else
			{
				TextBuilder.Append(',');
				NewLine();
			}

			return this;
		}

		/// <summary>
		/// Writes a list of enum fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="ILocalSymbol"/>s to write as constants.</param>
		/// <param name="isFlags">Determines whether the specified fields are located in a flags enum.</param>
		public CodeBuilder EnumFieldList(ImmutableArray<IFieldSymbol> fields, bool isFlags = false)
		{
			if (isFlags)
			{
				return EnumFieldList(fields, 0, isFlags);
			}

			if (fields.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();
			SimpleName_Internal(fields[0]);

			for (int i = 1; i < fields.Length; i++)
			{
				TextBuilder.Append(',');
				NewLine();
				Indent();
				SimpleName_Internal(fields[i]);
			}

			return NewLine();
		}

		/// <summary>
		/// Writes a list of enum fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="ILocalSymbol"/>s to write as constants.</param>
		/// <param name="startValue">Value of the first field.</param>
		/// <param name="isFlags">Determines whether the specified fields are located in a flags enum.</param>
		public CodeBuilder EnumFieldList(ImmutableArray<IFieldSymbol> fields, int startValue, bool isFlags = false)
		{
			if (fields.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();
			SimpleName_Internal(fields[0]);
			VariableInitializer();
			TextBuilder.Append(startValue);

			if (isFlags)
			{
				for (int i = 1; i < fields.Length; i++)
				{
					TextBuilder.Append(',');
					NewLine();
					Indent();
					SimpleName_Internal(fields[i]);
					VariableInitializer();
					TextBuilder.Append(1 << (i - 1));
				}
			}
			else
			{
				for (int i = 1; i < fields.Length; i++)
				{
					TextBuilder.Append(',');
					NewLine();
					Indent();
					SimpleName_Internal(fields[i]);
					VariableInitializer();
					TextBuilder.Append(startValue + i);
				}
			}

			return NewLine();
		}

		/// <summary>
		/// Writes a list of enum fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="IFieldSymbol"/>s to write as constants.</param>
		/// <param name="source">Source of explicit values of the enum.</param>
		/// <param name="isFlags">Determines whether the specified fields are located in a flags enum.</param>
		public CodeBuilder EnumFieldList(ImmutableArray<IFieldSymbol> fields, EnumValueSource source, bool isFlags = false)
		{
			if (source == EnumValueSource.Constant)
			{
				return EnumFieldList(fields, 0, isFlags);
			}

			Indent();

			SimpleName_Internal(fields[0]);

			if (source == EnumValueSource.Symbol)
			{
				WriteConstantValue(fields[0]);

				for (int i = 1; i < fields.Length; i++)
				{
					IFieldSymbol field = fields[i];
					WriteFieldHead(field);
					WriteConstantValue(field);
				}
			}
			else
			{
				for (int i = 1; i < fields.Length; i++)
				{
					WriteFieldHead(fields[i]);
				}
			}

			return NewLine();

			void WriteFieldHead(IFieldSymbol field)
			{
				TextBuilder.Append(',');
				NewLine();
				Indent();
				SimpleName_Internal(field);
			}

			void WriteConstantValue(IFieldSymbol field)
			{
				if (field.ConstantValue is null)
				{
					return;
				}

				VariableInitializer();

				if (field.ConstantValue is IFormattable formattable)
				{
					TextBuilder.Append(formattable.ToString("M", CultureInfo.InvariantCulture));
				}
				else
				{
					TextBuilder.Append(field.ConstantValue.ToString());
				}
			}
		}

		/// <summary>
		/// Writes a list of enum fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="ILocalSymbol"/>s to write as constants.</param>
		/// <param name="isFlags">Determines whether the specified fields are located in a flags enum.</param>
		public CodeBuilder EnumFieldList(IEnumerable<IFieldSymbol> fields, bool isFlags = false)
		{
			return EnumFieldList(fields.ToImmutableArray(), isFlags);
		}

		/// <summary>
		/// Writes a list of enum fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="ILocalSymbol"/>s to write as constants.</param>
		/// <param name="startValue">Value of the first field.</param>
		/// <param name="isFlags">Determines whether the specified fields are located in a flags enum.</param>
		public CodeBuilder EnumFieldList(IEnumerable<IFieldSymbol> fields, int startValue, bool isFlags = false)
		{
			return EnumFieldList(fields.ToImmutableArray(), startValue, isFlags);
		}

		/// <summary>
		/// Writes a list of enum fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="IFieldSymbol"/>s to write as constants.</param>
		/// <param name="source">Source of explicit values of the enum.</param>
		/// <param name="isFlags">Determines whether the specified fields are located in a flags enum.</param>
		public CodeBuilder EnumFieldList(IEnumerable<IFieldSymbol> fields, EnumValueSource source, bool isFlags = false)
		{
			return EnumFieldList(fields.ToImmutableArray(), source, isFlags);
		}

		/// <summary>
		/// Begins declaration of an event.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to begin the declaration of.</param>
		/// <param name="includeAccessors">Determines whether to begin declaration of event accessors.</param>
		public CodeBuilder Event(IEventSymbol @event, bool includeAccessors = false)
		{
			Indent();

			WriteEventHead(@event);
			SimpleName_Internal(@event);

			if (includeAccessors)
			{
				BeginBlock();
			}
			else
			{
				ColonNewLine();
			}

			return this;
		}

		/// <summary>
		/// Writes a list of field-like events separated by commas.
		/// </summary>
		/// <param name="events">Collection of <see cref="IEventSymbol"/>s to write.</param>
		public CodeBuilder EventList(ImmutableArray<IEventSymbol> events)
		{
			if (events.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();
			WriteEventHead(events[0]);
			SimpleName_Internal(events[0]);

			for (int i = 1; i < events.Length; i++)
			{
				CommaSpace();
				SimpleName_Internal(events[i]);
			}

			ColonNewLine();
			return this;
		}

		/// <summary>
		/// Writes a list of field-like events separated by commas.
		/// </summary>
		/// <param name="events">Collection of <see cref="IEventSymbol"/>s to write.</param>
		public CodeBuilder EventList(IEnumerable<IEventSymbol> events)
		{
			return EventList(events.ToImmutableArray());
		}

		/// <summary>
		/// Begins declaration of an explicit interface method implementation.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder ExplicitInterfaceImplementation(IMethodSymbol method)
		{
			return ExplicitInterfaceImplementation(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of an explicit interface method implementation.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder ExplicitInterfaceImplementation(IMethodSymbol method, MethodStyle body)
		{
			Indent();

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
		/// Begins declaration of a field.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to begin the declaration of.</param>
		/// <param name="initializer">Determines whether to include field initializer.</param>
		public CodeBuilder Field(IFieldSymbol field, bool initializer = false)
		{
			Indent();

			WriteFieldHead(field);

			SimpleName_Internal(field);

			if (initializer)
			{
				VariableInitializer();
			}
			else
			{
				ColonNewLine();
			}

			return this;
		}

		/// <summary>
		/// Writes a list of fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="IFieldSymbol"/>s to write.</param>
		public CodeBuilder FieldList(ImmutableArray<IFieldSymbol> fields)
		{
			if (fields.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();
			WriteFieldHead(fields[0]);
			SimpleName_Internal(fields[0]);

			for (int i = 1; i < fields.Length; i++)
			{
				CommaSpace();
				SimpleName_Internal(fields[i]);
			}

			ColonNewLine();
			return this;
		}

		/// <summary>
		/// Writes a list of fields separated by commas.
		/// </summary>
		/// <param name="fields">Collection of <see cref="IFieldSymbol"/>s to write.</param>
		public CodeBuilder FieldList(IEnumerable<IFieldSymbol> fields)
		{
			return FieldList(fields.ToImmutableArray());
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IFunctionPointerTypeSymbol"/> to write.</param>
		public CodeBuilder FunctionPointer(IFunctionPointerTypeSymbol pointer)
		{
			return FunctionPointer(pointer, Style.UseExplicitManaged);
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
			else if (explicitManaged && signature.CallingConvention == SignatureCallingConvention.Default)
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
		/// Begins declaration of an indexer.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		public CodeBuilder Indexer(IPropertySymbol property)
		{
			return Indexer(property, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of an indexer.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Indexer(IPropertySymbol property, MethodStyle body)
		{
			Indent();

			Accessibility(property);
			WritePropertyModifiers(property);
			ReturnType(property);

			TextBuilder.Append("this[");

			ImmutableArray<IParameterSymbol> parameters = property.Parameters;

			if (parameters.Length > 0)
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
		/// Begins declaration of an interface.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Interface(INamedTypeSymbol type)
		{
			Indent();
			Accessibility(type);

			TryUnsafe(type);
			TryPartial(type);

			TextBuilder.Append("interface ");
			SimpleName_Internal(type);

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
		/// Writes a label statement.
		/// </summary>
		/// <param name="label"><see cref="ILabelSymbol"/> to write.</param>
		public CodeBuilder Label(ILabelSymbol label)
		{
			Indent();
			SimpleName_Internal(label);
			TextBuilder.Append(':');
			return this;
		}

		/// <summary>
		/// Begins declaration of a local variable.
		/// </summary>
		/// <param name="local"><see cref="ILabelSymbol"/> to begin the declaration of.</param>
		/// <param name="skipInitializer">Determines whether to skip initializer of the local variable.</param>
		public CodeBuilder Local(ILocalSymbol local, bool skipInitializer = false)
		{
			return Local(local, skipInitializer, Style.UseImplicitType);
		}

		/// <summary>
		/// Begins declaration of a local variable.
		/// </summary>
		/// <param name="local"><see cref="ILabelSymbol"/> to begin the declaration of.</param>
		/// <param name="skipInitializer">Determines whether to skip initializer of the local variable.</param>
		/// <param name="implicitType">Determines whether to write the <see langword="var"/> instead of explicit type.</param>
		public CodeBuilder Local(ILocalSymbol local, bool skipInitializer, bool implicitType)
		{
			Indent();

			WriteLocalHead(local, implicitType);

			SimpleName_Internal(local);

			if (skipInitializer)
			{
				ColonNewLine();
			}
			else
			{
				TextBuilder.Append('=');
				TextBuilder.Append(' ');
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a local function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder LocalFunction(IMethodSymbol method)
		{
			return LocalFunction(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a local function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder LocalFunction(IMethodSymbol method, MethodStyle body)
		{
			Indent();

			TryStatic(method);
			TryExtern(method);
			TryUnsafe(method);
			TryAsync(method);

			WriteMethodHead(method, body);
			return this;
		}

		/// <summary>
		/// Writes a list of local variables separated by commas.
		/// </summary>
		/// <param name="locals">Collection of <see cref="ILocalSymbol"/>s to write.</param>
		public CodeBuilder LocalList(ImmutableArray<ILocalSymbol> locals)
		{
			if (locals.Length == 0)
			{
				InitBuilder();
				return this;
			}

			Indent();
			WriteLocalHead(locals[0], false);
			SimpleName_Internal(locals[0]);

			for (int i = 1; i < locals.Length; i++)
			{
				CommaSpace();
				SimpleName_Internal(locals[i]);
			}

			ColonNewLine();

			return this;
		}

		/// <summary>
		/// Writes a list of local variables separated by commas.
		/// </summary>
		/// <param name="locals">Collection of <see cref="ILocalSymbol"/>s to write.</param>
		public CodeBuilder LocalList(IEnumerable<ILocalSymbol> locals)
		{
			return LocalList(locals.ToImmutableArray());
		}

		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Method(IMethodSymbol method)
		{
			return Method(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Method(IMethodSymbol method, MethodStyle body)
		{
			Indent();
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

		/// <summary>
		/// Writes name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the name of.</param>
		/// <param name="format">Format of the name.</param>
		public CodeBuilder Name(ISymbol symbol, SymbolName format = default)
		{
			InitBuilder();

			if (symbol is INamedTypeSymbol type && KeywordType(type, format != SymbolName.SystemName && Style.UseBuiltInAliases))
			{
				return this;
			}

			switch (format)
			{
				case SymbolName.Default:
					SimpleName_Internal(symbol);
					break;

				case SymbolName.Generic:
					return GenericName(symbol, false, false);

				case SymbolName.VarianceGeneric:
					return GenericName(symbol, false, true);

				case SymbolName.Substituted:
					return GenericName(symbol, true, false);

				case SymbolName.Attribute:
					return AttributeName(symbol);

				case SymbolName.Qualified:
					return QualifiedName(symbol, true);

				case SymbolName.GlobalQualified:
					return QualifiedName(symbol, true, true);

				default:
					goto case SymbolName.Default;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		public CodeBuilder Namespace(INamespaceSymbol @namespace, bool includeParent = true)
		{
			return Namespace(@namespace, Style.NamespaceStyle, includeParent);
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		public CodeBuilder Namespace(INamespaceSymbol @namespace, NamespaceStyle type, bool includeParent = true)
		{
			Indent();

			TextBuilder.Append("namespace ");

			if (includeParent)
			{
				if (type == NamespaceStyle.Nested)
				{
					foreach (INamespaceSymbol parent in @namespace.GetContainingNamespaces())
					{
						SimpleName_Internal(parent);
						BeginBlock();
						Indent();
						TextBuilder.Append("namespace ");
					}
				}
				else
				{
					foreach (INamespaceSymbol parent in @namespace.GetContainingNamespaces())
					{
						SimpleName_Internal(parent);
						TextBuilder.Append('.');
					}
				}
			}

			SimpleName_Internal(@namespace);

			if (type == NamespaceStyle.File)
			{
				ColonNewLine();
			}
			else
			{
				BeginBlock();
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
		/// Begins declaration of an operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder Operator(IMethodSymbol method)
		{
			return Operator(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of an operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Operator(IMethodSymbol method, MethodStyle body)
		{
			Indent();
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
			Space();
			SimpleName_Internal(parameter);
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
		/// Begins declaration of a property.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		/// <param name="kind">Kind of auto-property to write.</param>
		public CodeBuilder Property(IPropertySymbol property, AutoPropertyKind kind)
		{
			Accessibility(property);

			TryStatic(property);
			WritePropertyModifiers(property);
			ReturnType(property);
			SimpleName_Internal(property);

			switch (kind)
			{
				case AutoPropertyKind.GetOnly:
					BeginAccessor();
					WriteAccessor(property.GetMethod, "get");
					EndAccessor();
					break;

				case AutoPropertyKind.GetSet:
					BeginAccessor();
					WriteAccessor(property.GetMethod, "get");
					WriteAccessor(property.SetMethod, "set");
					EndAccessor();
					break;

				case AutoPropertyKind.GetInit:
					BeginAccessor();
					WriteAccessor(property.GetMethod, "get");
					WriteAccessor(property.SetMethod, "init");
					EndAccessor();
					break;
			}

			return this;

			void BeginAccessor()
			{
				Space();
				TextBuilder.Append('{');
				Space();
			}

			void EndAccessor()
			{
				TextBuilder.Append('}');
				NewLine();
			}

			void WriteAccessor(IMethodSymbol? accessor, string name)
			{
				if (accessor is not null && accessor.DeclaredAccessibility != property.DeclaredAccessibility)
				{
					Accessibility(accessor);
				}

				TextBuilder.Append(name);
				TextBuilder.Append(';');
				Space();
			}
		}

		/// <summary>
		/// Begins declaration of a property.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		public CodeBuilder Property(IPropertySymbol property)
		{
			return Property(property, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a property.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Property(IPropertySymbol property, MethodStyle body)
		{
			Indent();
			Accessibility(property);
			TryStatic(property);
			WritePropertyModifiers(property);
			ReturnType(property);
			SimpleName_Internal(property);
			return MethodBody(body);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the qualified name of.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters.</param>
		public CodeBuilder QualifiedName(ISymbol symbol, bool useArguments = false)
		{
			InitBuilder();

			foreach (INamespaceSymbol @namespace in symbol.GetContainingNamespaces())
			{
				SimpleName_Internal(@namespace);
				TextBuilder.Append('.');
			}

			if (symbol.ContainingType is not null)
			{
				if (useArguments)
				{
					foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
					{
						SimpleName_Internal(type);
						TypeArgumentList(type.TypeArguments);
						TextBuilder.Append('.');
					}
				}
				else
				{
					foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
					{
						SimpleName_Internal(type);
						TypeParameterList(type.TypeParameters);
						TextBuilder.Append('.');
					}
				}
			}

			return GenericName(symbol, true, false);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the qualified name of.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters.</param>
		/// <param name="globalAlias">Determines whether to include the global alias.</param>
		public CodeBuilder QualifiedName(ISymbol symbol, bool useArguments, bool globalAlias)
		{
			if (globalAlias)
			{
				return QualifiedName(symbol, "global");
			}

			return QualifiedName(symbol, useArguments);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="symbol"/> using the given <paramref name="alias"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the qualified name of.</param>
		/// <param name="alias"><see cref="IAliasSymbol"/> to include in the qualified name.</param>
		public CodeBuilder QualifiedName(ISymbol symbol, IAliasSymbol alias)
		{
			return QualifiedName(symbol, alias.Name);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="symbol"/> using the given <paramref name="alias"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the qualified name of.</param>
		/// <param name="alias">Alias to include in the qualified name.</param>
		public CodeBuilder QualifiedName(ISymbol symbol, string alias)
		{
			InitBuilder();
			TextBuilder.Append(alias);
			TextBuilder.Append(':');
			TextBuilder.Append(':');

			return QualifiedName(symbol);
		}

		/// <summary>
		/// Begins declaration of a record.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a declaration body.</param>
		public CodeBuilder Record(INamedTypeSymbol type, bool body = true)
		{
			return Record(type, Style.RecordStyle, body);
		}

		/// <summary>
		/// Begins declaration of a record.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <param name="style">Determines the record declaration is written.</param>
		/// <param name="body">Determines whether to begin a declaration body.</param>
		public CodeBuilder Record(INamedTypeSymbol type, RecordStyle style, bool body = true)
		{
			Indent();
			Accessibility(type);

			if (type.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if (type.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}

			TryUnsafe(type);
			TryPartial(type);

			TextBuilder.Append("record ");

			if (type.IsValueType)
			{
				TextBuilder.Append("struct ");
			}
			else if (style.HasFlag(RecordStyle.ExplicitClass))
			{
				TextBuilder.Append("class ");
			}

			SimpleName_Internal(type);

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
				ColonNewLine();
			}

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
		/// Writes the return type of the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to write the return type of.</param>
		public CodeBuilder ReturnType(IEventSymbol @event)
		{
			InitBuilder();
			ReturnType(@event.Type, false, false);
			return this;
		}

		/// <summary>
		/// Writes name of the <paramref name="symbol"/> without type parameters.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the name of.</param>
		public CodeBuilder SimpleName(ISymbol symbol)
		{
			return SimpleName(symbol, Style.UseBuiltInAliases);
		}

		/// <summary>
		/// Writes name of the <paramref name="symbol"/> without type parameters.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the name of.</param>
		/// <param name="useAliases">Determines whether to use aliases of built-in types instead of concrete types (e.g. <see langword="int"/> instead of <c>Int32</c>.</param>
		public CodeBuilder SimpleName(ISymbol symbol, bool useAliases)
		{
			InitBuilder();

			if (symbol is not INamedTypeSymbol type || !KeywordType(type, !useAliases))
			{
				SimpleName_Internal(symbol);
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a static constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		public CodeBuilder StaticConstructor(IMethodSymbol method)
		{
			return StaticConstructor(method, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a static constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder StaticConstructor(IMethodSymbol method, MethodStyle body)
		{
			Indent();

			TextBuilder.Append("static ");

			TryUnsafe(method);

			SimpleName_Internal(method.ContainingType);
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
			Indent();
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
			SimpleName_Internal(type);

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
					SimpleName_Internal(type);
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
				Type(typeArguments[i]);
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

			SimpleName_Internal(typeParameter);
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
				SimpleName_Internal(typeParameters[0]);

				for (int i = 1; i < typeParameters.Length; i++)
				{
					CommaSpace();
					Variance(typeParameters[i]);
					SimpleName_Internal(typeParameters[i]);
				}
			}
			else
			{
				SimpleName_Internal(typeParameters[0]);

				for (int i = 1; i < typeParameters.Length; i++)
				{
					CommaSpace();
					SimpleName_Internal(typeParameters[i]);
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
		/// Writes a <see langword="using"/> directive.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to include using the <see langword="using"/> directive.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder Using(INamespaceSymbol @namespace, bool isGlobal = false)
		{
			Indent();

			WriteUsing(isGlobal);
			QualifiedName(@namespace);
			ColonNewLine();

			return this;
		}

		/// <summary>
		/// Writes a <see langword="using"/> directive with an alias.
		/// </summary>
		/// <param name="target"><see cref="INamespaceOrTypeSymbol"/> to include using the <see langword="using"/> directive.</param>
		/// <param name="alias">Alias to write.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder UsingAlias(INamespaceOrTypeSymbol target, string alias, bool isGlobal = false)
		{
			Indent();

			WriteUsing(isGlobal);
			TextBuilder.Append(alias);
			Space();
			TextBuilder.Append('=');
			Space();

			QualifiedName(target);
			ColonNewLine();

			return this;
		}

		/// <summary>
		/// Writes a <see langword="using"/> directive with an alias.
		/// </summary>
		/// <param name="alias"><see cref="IAliasSymbol"/> to use to write the <see langword="using"/> directive.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder UsingAlias(IAliasSymbol alias, bool isGlobal = false)
		{
			return UsingAlias(alias.Target, alias.Name, isGlobal);
		}

		/// <summary>
		/// Writes a <see langword="using"/> <see langword="static"/> directive.
		/// </summary>
		/// <param name="target"><see cref="INamespaceOrTypeSymbol"/> to include using the <see langword="using"/> <see langword="static"/> directive.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder UsingStatic(INamespaceOrTypeSymbol target, bool isGlobal = false)
		{
			Indent();

			WriteUsing(isGlobal);
			TextBuilder.Append("static ");
			QualifiedName(target);
			ColonNewLine();

			return this;
		}

		/// <summary>
		/// Writes variance of the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write the variance of.</param>
		public CodeBuilder Variance(ITypeParameterSymbol typeParameter)
		{
			return Variance(typeParameter.Variance);
		}

		/// <summary>
		/// Writes an XML-compatible name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the XML compatible name of.</param>
		public CodeBuilder XmlName(ISymbol symbol)
		{
			switch (symbol)
			{
				case IMethodSymbol method:
					return XmlName(method);

				case INamedTypeSymbol type:
					return XmlName(type);

				case IPropertySymbol property:
					return XmlName(property);

				default:
					SimpleName_Internal(symbol);
					return this;
			}
		}

		/// <summary>
		/// Writes an XML-compatible name of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write the XML compatible name of.</param>
		public CodeBuilder XmlName(INamedTypeSymbol type)
		{
			InitBuilder();

			SimpleName_Internal(type);

			if (!type.IsGenericType || type.TypeParameters.Length == 0)
			{
				return this;
			}

			return XmlTypeParameterList(type.TypeParameters);
		}

		/// <summary>
		/// Writes an XML-compatible name of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to write the XML compatible name of.</param>
		public CodeBuilder XmlName(IMethodSymbol method)
		{
			InitBuilder();

			switch (method.MethodKind)
			{
				case MethodKind.Constructor:
					SimpleName_Internal(method.ContainingType);
					break;

				case MethodKind.UserDefinedOperator:

					if (method.GetOperatorToken() is not string op)
					{
						goto default;
					}

					Write("operator ");
					Write(op);

					break;

				case MethodKind.Conversion:

					string keyword;

					if (method.IsImplicitOperator())
					{
						keyword = "implicit";
					}
					else if (method.IsExplicitOperator())
					{
						keyword = "explicit";
					}
					else
					{
						goto default;
					}

					Write(keyword);
					Space();
					XmlType(method.ReturnType);

					break;

				default:
					SimpleName_Internal(method);

					if (method.IsGenericMethod && method.TypeParameters.Length > 0)
					{
						XmlTypeParameterList(method.TypeParameters);
					}

					break;
			}

			TextBuilder.Append('(');
			WriteParameterTypeList(method.Parameters);
			TextBuilder.Append(')');

			return this;
		}

		/// <summary>
		/// Writes an XML-compatible name of the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to write the XML compatible name of.</param>
		public CodeBuilder XmlName(IPropertySymbol property)
		{
			InitBuilder();

			if (!property.IsIndexer)
			{
				SimpleName_Internal(property);
				return this;
			}

			TextBuilder.Append("this[");
			WriteParameterTypeList(property.Parameters);
			TextBuilder.Append(']');

			return this;
		}

		/// <summary>
		/// Writes an XML-compatible list of specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder XmlTypeParameterList(ImmutableArray<ITypeParameterSymbol> typeParameters)
		{
			InitBuilder();

			TextBuilder.Append('{');

			SimpleName_Internal(typeParameters[0]);

			for (int i = 1; i < typeParameters.Length; i++)
			{
				CommaSpace();
				SimpleName_Internal(typeParameters[i]);
			}

			TextBuilder.Append('}');

			return this;
		}

		/// <summary>
		/// Writes an XML-compatible list of specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder XmlTypeParameterList(IEnumerable<ITypeParameterSymbol> typeParameters)
		{
			return XmlTypeParameterList(typeParameters.ToImmutableArray());
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

			if (symbol is INamedTypeSymbol type && type.IsGenericType)
			{
				TypeArgumentList(type.TypeArguments);
			}

			return this;
		}

		private bool CanUseAbstractOrVirtual(ISymbol symbol)
		{
			if (symbol.ContainingType is INamedTypeSymbol type && type.TypeKind == TypeKind.Interface)
			{
				return Style.InterfaceMemberStyle.HasFlag(InterfaceMemberStyle.ExplicitVirtual);
			}

			return true;
		}

		private void ColonNewLine()
		{
			TextBuilder.Append(';');
			TextBuilder.AppendLine();
		}

		private CodeBuilder GenericName(ISymbol symbol, bool parametersOrArguments, bool includeVariance)
		{
			if (symbol is INamedTypeSymbol t)
			{
				return GenericName(t, parametersOrArguments, includeVariance);
			}

			if (symbol is IMethodSymbol m)
			{
				return GenericName(m, parametersOrArguments, includeVariance);
			}

			SimpleName_Internal(symbol);
			return this;
		}

		private CodeBuilder GenericName(INamedTypeSymbol type, bool parametersOrArguments, bool includeVariance)
		{
			SimpleName_Internal(type);

			if (parametersOrArguments)
			{
				TypeArgumentList(type.TypeArguments);
			}
			else
			{
				TypeParameterList(type.TypeParameters, includeVariance);
			}

			return this;
		}

		private CodeBuilder GenericName(IMethodSymbol method, bool parametersOrArguments, bool includeVariance)
		{
			SimpleName_Internal(method);

			if (parametersOrArguments)
			{
				TypeArgumentList(method.TypeArguments);
			}
			else
			{
				TypeParameterList(method.TypeParameters, includeVariance);
			}

			return this;
		}

		private bool KeywordType(INamedTypeSymbol type, bool useAliases)
		{
			if (useAliases)
			{
				if (type.GetTypeKeyword() is string keyword)
				{
					TextBuilder.Append(keyword);
					return true;
				}

				return false;
			}

			if (type is IDynamicTypeSymbol)
			{
				TextBuilder.Append("dynamic");
				return true;
			}
			else if (type.SpecialType.IsKeyword())
			{
				TextBuilder.Append(type.Name);
				return true;
			}

			return false;
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

		private void SimpleName_Internal(ISymbol symbol)
		{
			TextBuilder.Append(symbol.GetVerbatimName());
		}

#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.
		private void TryAsync(IMethodSymbol method)
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
		{
			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}
		}

		private void TryExtern(ISymbol symbol)
		{
			if (symbol.IsExtern)
			{
				TextBuilder.Append("extern ");
			}
		}

		private void TryPartial(ISymbol symbol)
		{
			if (symbol.IsPartial())
			{
				TextBuilder.Append("partial ");
			}
		}

		private void TryStatic(ISymbol symbol)
		{
			if (symbol.IsStatic)
			{
				TextBuilder.Append("static ");
			}
		}

		private void TryUnsafe(ISymbol symbol)
		{
			if (symbol.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}
		}

		private void VariableInitializer()
		{
			Space();
			TextBuilder.Append('=');
			Space();
		}

		private void WriteConstantHead(ISymbol symbol, ITypeSymbol type)
		{
			TextBuilder.Append("const ");

			Type(type);
			Space();
			SimpleName_Internal(symbol);

			VariableInitializer();
		}

		private void WriteConstantValue(object? constant, ITypeSymbol type)
		{
			if (constant is not null)
			{
				if (constant is IFormattable formattable)
				{
					TextBuilder.Append(formattable.ToString("N", CultureInfo.InvariantCulture));
				}
				else
				{
					TextBuilder.Append(constant.ToString());
				}

				return;
			}

			TextBuilder.Append("default(");
			SimpleName(type);
			TextBuilder.Append(')');
		}

		private void WriteEventHead(IEventSymbol @event)
		{
			Accessibility(@event);

			TryStatic(@event);
			WriteExternAndVirtualityModifiers(@event);

			if (@event.IsReadOnlyContext())
			{
				TextBuilder.Append("readonly ");
			}

			TryUnsafe(@event);

			TextBuilder.Append("event ");

			ReturnType(@event);
		}

		private void WriteExternAndVirtualityModifiers(ISymbol symbol)
		{
			TryExtern(symbol);

			if (symbol.IsAbstract)
			{
				if (CanUseAbstractOrVirtual(symbol))
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

		private void WriteFieldHead(IFieldSymbol field)
		{
			Accessibility(field);

			TryStatic(field);

			if (field.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}
			else if (field.IsConst)
			{
				TextBuilder.Append("const ");
			}
			else if (field.IsVolatile)
			{
				TextBuilder.Append("volatile ");
			}
			else if (field.IsFixedSizeBuffer)
			{
				TextBuilder.Append("fixed ");
			}

			TryUnsafe(field);

			if (field.IsVolatile)
			{
				TextBuilder.Append("volatile ");
			}

			Type(field.Type);
			Space();
		}

		private void WriteLocalHead(ILocalSymbol local, bool implicitType)
		{
			if (local.RefKind.GetText(false) is string refKind)
			{
				TextBuilder.Append(refKind);
				Space();
			}

			if (local.IsConst)
			{
				TextBuilder.Append("const ");
			}

			if (implicitType && !local.IsRef && !local.IsConst)
			{
				TextBuilder.Append("var ");
			}
			else
			{
				Type(local.Type);
				Space();
			}
		}

		private void WriteMethodHead(IMethodSymbol method, MethodStyle body)
		{
			ReturnType(method);
			SimpleName_Internal(method);

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

		private void WriteParameterType(IParameterSymbol parameter)
		{
			if (parameter.RefKind.GetText() is string refKind)
			{
				TextBuilder.Append(refKind);
				TextBuilder.Append(' ');
			}

			XmlType(parameter.Type);
		}

		private void WriteParameterTypeList(ImmutableArray<IParameterSymbol> parameters)
		{
			if (parameters.Length > 0)
			{
				WriteParameterType(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					CommaSpace();
					WriteParameterType(parameters[i]);
				}
			}
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

		private void WriteUsing(bool isGlobal)
		{
			if (isGlobal)
			{
				TextBuilder.Append("global ");
			}

			TextBuilder.Append("using ");
		}

		private void XmlType(ITypeSymbol type)
		{
			int start = TextBuilder.Length;

			Type(type);

			int end = TextBuilder.Length;

			if (start < end)
			{
				TextBuilder.Replace('<', '{', start, end - start);
				TextBuilder.Replace('>', '}', start, end - start);
			}
		}
	}
}
