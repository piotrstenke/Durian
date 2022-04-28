// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public partial class CodeBuilder
	{
		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration for.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginMethodDeclaration(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			TextBuilder.Append(AnalysisUtilities.GetAccessiblityText(method.DeclaredAccessibility));
			TextBuilder.Append(' ');

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if (method.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}
			else if (method.IsSealed)
			{
				TextBuilder.Append("sealed ");
			}

			if (method.IsOverride)
			{
				TextBuilder.Append("override ");
			}
			else if (method.IsVirtual)
			{
				TextBuilder.Append("virtual ");
			}

			if (method.IsExtern)
			{
				TextBuilder.Append("extern");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (method.IsPartial())
			{
				TextBuilder.Append("partial ");
			}

			if (method.ReturnsVoid)
			{
				TextBuilder.Append("void ");
			}
			else
			{
				if (method.ReturnsByRefReadonly)
				{
					TextBuilder.Append("ref readonly ");
				}
				else if (method.ReturnsByRef)
				{
					TextBuilder.Append("ref ");
				}

				TextBuilder.Append(method.ReturnType.GetGenericName(GenericSubstitution.TypeArguments));
				TextBuilder.Append(' ');
			}

			TextBuilder.Append(method.Name);

			if (method.IsGenericMethod)
			{
				TypeParameterList(method.TypeParameters);
			}

			WriteParameters(method.Parameters);

			return this;
		}

		public void WriteParameters(ImmutableArray<IParameterSymbol> parameters)
		{
			TextBuilder.Append('(');

			if (parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					Parameter(parameters[i]);
				}
			}

			TextBuilder.Append(')');
		}

		public void ParameterList(IParameterSymbol[] parameters)
		{
			WriteParameters(parameters.ToImmutableArray());
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>.</exception>
		public CodeBuilder BeginNamespace(INamespaceSymbol @namespace, NamespaceScope type = NamespaceScope.Default)
		{
			BeginNamespace_Internal(@namespace.JoinNamespaces())

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write.</param>
		public CodeBuilder Parameter(IParameterSymbol parameter)
		{
			if (parameter.RefKind != RefKind.None)
			{
				switch (parameter.RefKind)
				{
					case RefKind.In:
						TextBuilder.Append("in ");
						break;

					case RefKind.Out:
						TextBuilder.Append("out ");
						break;

					case RefKind.Ref:
						TextBuilder.Append("ref ");
						break;
				}
			}

			return Type(parameter.Type);
		}

		/// <summary>
		/// Writes all specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameterList(ImmutableArray<ITypeParameterSymbol> typeParameters)
		{
			if (typeParameters.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');
			TextBuilder.Append(typeParameters[0].Name);

			for (int i = 1; i < typeParameters.Length; i++)
			{
				TextBuilder.Append(' ');
				TextBuilder.Append(',');
				TextBuilder.Append(typeParameters[i].Name);
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes all specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameterList(ITypeParameterSymbol[] typeParameters)
		{
			if (typeParameters.Length == 0)
			{
				return this;
			}

			return TypeParameterList(typeParameters.ToImmutableArray());
		}


		/// <summary>
		/// Writes the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameter(ITypeParameterSymbol typeParameter)
		{
			TextBuilder.Append(typeParameter.Name);
			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
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
					CheckNullable(elementArray);
					WriteArrayBrackets(elementArray);
				}
			}
			else
			{
				Type(element);
				WriteArrayBrackets(array);
			}

			CheckNullable(array);

			return this;

			void CheckNullable(IArrayTypeSymbol a)
			{
				if (a.NullableAnnotation == NullableAnnotation.Annotated)
				{
					TextBuilder.Append('?');
				}
			}

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
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IFunctionPointerTypeSymbol"/> to write.</param>
		public CodeBuilder FunctionPointer(IFunctionPointerTypeSymbol pointer)
		{
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

			TextBuilder.Append('<');

			ImmutableArray<IParameterSymbol> parameters = signature.Parameters;

			if (parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(", ");
					Parameter(parameters[i]);
				}
			}

			Type(signature.ReturnType);

			TextBuilder.Append('>');

			return this;
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
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="type"><see cref="IDynamicTypeSymbol"/> to write.</param>
		public CodeBuilder Dynamic(IDynamicTypeSymbol type)
		{
			TextBuilder.Append("dynamic");

			if (type.NullableAnnotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public CodeBuilder Type(ITypeSymbol type)
		{
			switch (type)
			{
				case INamedTypeSymbol named:
					Type(named);
					return this;

				case IArrayTypeSymbol array:
					Array(array);
					return this;

				case IDynamicTypeSymbol:
					TextBuilder.Append("dynamic");
					break;

				case IPointerTypeSymbol pointer:
					Pointer(pointer);
					return this;

				case IFunctionPointerTypeSymbol functionPointer:
					FunctionPointer(functionPointer);
					return this;

				default:
					TextBuilder.Append(type.Name);
					break;
			}

			if (type.NullableAnnotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public CodeBuilder Type(INamedTypeSymbol type)
		{
			if (type.IsValueType && type.ConstructedFrom is not null && type.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T && type.TypeArguments.Length > 0)
			{
				string name = type.TypeArguments[0].GetGenericName(GenericSubstitution.TypeArguments);
				TextBuilder.Append(AnalysisUtilities.TypeToKeyword(name));
				TextBuilder.Append('?');
			}
			else
			{
				string name = type.TypeArguments.Length > 0 ? type.GetGenericName(GenericSubstitution.TypeArguments) : type.Name;
				TextBuilder.Append(AnalysisUtilities.TypeToKeyword(name));

				if (type.NullableAnnotation == NullableAnnotation.Annotated)
				{
					TextBuilder.Append('?');
				}
			}

			return this;
		}
	}
}