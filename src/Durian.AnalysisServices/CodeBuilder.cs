// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// A wrapper for the <see cref="StringBuilder"/> class that helps generating C# code.
	/// </summary>
	[DebuggerDisplay("{TextBuilder}")]
	public sealed partial class CodeBuilder
	{
		private int _currentIndent;
		private int _currentLength;

		/// <summary>
		/// Determines whether the last method call has changed state of the builder.
		/// </summary>
		public bool Changed => _currentLength != TextBuilder.Length;

		/// <summary>
		/// Current indentation level.
		/// </summary>
		public int CurrentIndent
		{
			get => _currentIndent;
			set
			{
				if (value < 0)
				{
					_currentIndent = 0;
				}
				else
				{
					_currentIndent = value;
				}
			}
		}

		/// <summary>
		/// Builder that write single keywords.
		/// </summary>
		public KeywordWriter Keyword { get; }

		/// <summary>
		/// Builder that writes single literals.
		/// </summary>
		public LiteralWriter Literal { get; }

		/// <summary>
		/// Style configuration applied to the current builder.
		/// </summary>
		public CodeBuilderStyleConfiguration Style { get; }

		/// <summary>
		/// <see cref="StringBuilder"/> to write the generated code to.
		/// </summary>
		public StringBuilder TextBuilder { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="style">Style configuration applied to the current builder.</param>
		public CodeBuilder(CodeBuilderStyleConfiguration? style = default) : this(true, default, style)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write the generated code to.</param>
		/// <param name="style">Style configuration applied to the current builder.</param>
		public CodeBuilder(StringBuilder builder, CodeBuilderStyleConfiguration? style = default) : this(true, Validate(builder), style)
		{
		}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		internal CodeBuilder(bool requireChildBuilder, StringBuilder? builder = default, CodeBuilderStyleConfiguration? style = default)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			Style = style ?? CodeBuilderStyleConfiguration.Default();
			TextBuilder = builder ?? new();

			if (requireChildBuilder)
			{
				Literal = new(TextBuilder);
				Keyword = new(TextBuilder);
			}
		}

		/// <summary>
		/// Writes an accessibility modifier.
		/// </summary>
		/// <param name="accessibility">Accessibility modifier to write.</param>
		public CodeBuilder Accessibility(Accessibility accessibility)
		{
			InitBuilder();

			if (accessibility.GetText() is string keyword)
			{
				TextBuilder.Append(keyword);
				Space();
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of auto-property accessors.
		/// </summary>
		public CodeBuilder Accessor(AutoPropertyKind kind)
		{
			InitBuilder();

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

				case AutoPropertyKind.SetOnly:
					TextBuilder.Append(" { set; }");
					break;

				case AutoPropertyKind.InitOnly:
					TextBuilder.Append(" { init; }");
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		public CodeBuilder Accessor(AccessorKind accessor)
		{
			return Accessor(accessor, Style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Accessor(AccessorKind accessor, MethodStyle body)
		{
			InitBuilder();

			if (accessor.GetText() is string value)
			{
				TextBuilder.Append(value);
			}

			return MethodBody(body);
		}

		/// <summary>
		/// Begins an attribute list.
		/// </summary>
		public CodeBuilder AttributeList()
		{
			Indent();
			TextBuilder.Append('[');
			return this;
		}

		/// <summary>
		/// Begins an attribute list with an <see cref="AttributeTarget"/>.
		/// </summary>
		/// <param name="target">Target of the attribute.</param>
		public CodeBuilder AttributeList(AttributeTarget target)
		{
			Indent();

			if (target.GetText() is not string text)
			{
				return AttributeList();
			}

			TextBuilder.Append('[');
			TextBuilder.Append(text);
			TextBuilder.Append(':');
			TextBuilder.Append(' ');

			return this;
		}

		/// <summary>
		/// Begins a new scope.
		/// </summary>
		public CodeBuilder BeginBlock()
		{
			InitBuilder();

			NewLine();
			Indent();
			TextBuilder.Append('{');
			NewLine();
			CurrentIndent++;

			return this;
		}

		/// <summary>
		/// Clears the builder.
		/// </summary>
		public CodeBuilder Clear()
		{
			InitBuilder();

			TextBuilder.Clear();
			return this;
		}

		/// <summary>
		/// Begins a member declaration using the specified raw text.
		/// </summary>
		/// <param name="member">Raw text to write.</param>
		public CodeBuilder Declaration(string member)
		{
			InitBuilder();

			TextBuilder.Append(member);
			BeginBlock();

			return this;
		}

		/// <summary>
		/// Decrements the value of the <see cref="CurrentIndent"/>.
		/// </summary>
		public CodeBuilder DecrementIndent()
		{
			InitBuilder();

			CurrentIndent--;
			return this;
		}

		/// <summary>
		/// Ends all the remaining scope.
		/// </summary>
		public CodeBuilder EndAllBlocks()
		{
			InitBuilder();

			int length = CurrentIndent;

			for (int i = 0; i < length; i++)
			{
				CurrentIndent--;
				Indent();
				TextBuilder.AppendLine("}");
			}

			return this;
		}

		/// <summary>
		/// Ends an attribute list.
		/// </summary>
		public CodeBuilder EndAttributeList()
		{
			InitBuilder();

			TextBuilder.Append(']');
			NewLine();
			return this;
		}

		/// <summary>
		/// Ends the current scope.
		/// </summary>
		public CodeBuilder EndBlock()
		{
			CurrentIndent--;
			Indent();
			TextBuilder.AppendLine("}");

			return this;
		}

		/// <summary>
		/// Increments the value of the <see cref="CurrentIndent"/>.
		/// </summary>
		public CodeBuilder IncrementIndent()
		{
			InitBuilder();
			CurrentIndent++;
			return this;
		}

		/// <summary>
		/// Applies indentation according to the value of <see cref="CurrentIndent"/>.
		/// </summary>
		public CodeBuilder Indent()
		{
			Indent(_currentIndent);
			return this;
		}

		/// <summary>
		/// Applies indentation according to the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">Indentation level to apply.</param>
		public CodeBuilder Indent(int value)
		{
			InitBuilder();

			for (int i = 0; i < value; i++)
			{
				TextBuilder.Append('\t');
			}

			return this;
		}

		/// <summary>
		/// Begins a method body.
		/// </summary>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder MethodBody(MethodStyle body)
		{
			InitBuilder();

			switch (body)
			{
				case MethodStyle.Block:
					BeginBlock();
					break;

				case MethodStyle.Expression:
					TextBuilder.Append(" => ");
					break;

				default:
					ColonNewLine();
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace">Name of namespace to begin declaration of.</param>
		public CodeBuilder Namespace(string @namespace)
		{
			return Namespace(@namespace, Style.NamespaceStyle);
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace">Name of namespace to begin declaration of.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		public CodeBuilder Namespace(string @namespace, NamespaceStyle type)
		{
			InitBuilder();

			TextBuilder.Append("namespace ");
			TextBuilder.Append(@namespace);

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
		/// Writes a new line.
		/// </summary>
		public CodeBuilder NewLine()
		{
			InitBuilder();
			TextBuilder.AppendLine();
			return this;
		}

		/// <summary>
		/// Writes a <paramref name="number"/> of new lines.
		/// </summary>
		/// <param name="number">Number of new lines to write.</param>
		public CodeBuilder NewLine(int number)
		{
			InitBuilder();

			for (int i = 0; i < number; i++)
			{
				TextBuilder.AppendLine();
			}

			return this;
		}

		/// <summary>
		/// Writes nullability marker if the <paramref name="annotation"/> is equal to <see cref="NullableAnnotation.Annotated"/>.
		/// </summary>
		/// <param name="annotation"><see cref="NullableAnnotation"/> to write.</param>
		public CodeBuilder Nullability(NullableAnnotation annotation)
		{
			InitBuilder();

			if (annotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}

			return this;
		}

		/// <summary>
		/// Writes a space.
		/// </summary>
		public CodeBuilder Space()
		{
			InitBuilder();

			TextBuilder.Append(' ');
			return this;
		}

		/// <summary>
		/// Writes a <paramref name="number"/> of spaces.
		/// </summary>
		/// <param name="number">Number of spaces to write.</param>
		public CodeBuilder Space(int number)
		{
			InitBuilder();

			for (int i = 0; i < number; i++)
			{
				TextBuilder.Append(' ');
			}

			return this;
		}

		/// <summary>
		/// Writes a tab.
		/// </summary>
		public CodeBuilder Tab()
		{
			InitBuilder();

			TextBuilder.Append('\t');
			return this;
		}

		/// <summary>
		/// Writes a <paramref name="number"/> of tabs.
		/// </summary>
		/// <param name="number">Number of tabs to write.</param>
		public CodeBuilder Tab(int number)
		{
			InitBuilder();

			for (int i = 0; i < number; i++)
			{
				TextBuilder.Append('\t');
			}

			return this;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return TextBuilder.ToString();
		}

		/// <summary>
		/// Writes a <see langword="using"/> directive.
		/// </summary>
		/// <param name="namespace">Name of namespace to include using the <see langword="using"/> directive.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder Using(string @namespace, bool isGlobal = false)
		{
			Indent();

			WriteUsing(isGlobal);
			TextBuilder.Append(@namespace);

			ColonNewLine();
			return this;
		}

		/// <summary>
		/// Writes a <see langword="using"/> directive with an alias.
		/// </summary>
		/// <param name="target">name of type or namespace to include using the <see langword="using"/> directive.</param>
		/// <param name="alias">Alias to write.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder UsingAlias(string target, string alias, bool isGlobal = false)
		{
			Indent();

			WriteUsing(isGlobal);

			TextBuilder.Append(alias);
			TextBuilder.Append(' ');
			TextBuilder.Append('=');
			TextBuilder.Append(' ');
			TextBuilder.Append(target);

			ColonNewLine();
			return this;
		}

		/// <summary>
		/// Writes a <see langword="using"/> <see langword="static"/> directive.
		/// </summary>
		/// <param name="target">Name of type or namespace to include using the <see langword="using"/> <see langword="static"/> directive.</param>
		/// <param name="isGlobal">Determines whether the <see langword="using"/> directive is <see langword="global"/>.</param>
		public CodeBuilder UsingStatic(string target, bool isGlobal = false)
		{
			Indent();

			WriteUsing(isGlobal);
			TextBuilder.Append("static ");
			TextBuilder.Append(target);

			ColonNewLine();

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="variance"/>.
		/// </summary>
		/// <param name="variance">Kind of variance to write.</param>
		public CodeBuilder Variance(VarianceKind variance)
		{
			InitBuilder();

			if (variance.GetText() is string value)
			{
				TextBuilder.Append(value);
				TextBuilder.Append(' ');
			}

			return this;
		}

		/// <summary>
		/// Appends the <paramref name="value"/> to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>
		public CodeBuilder Write(char value)
		{
			InitBuilder();

			TextBuilder.Append(value);
			return this;
		}

		/// <summary>
		/// Appends the <paramref name="value"/> to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>
		public CodeBuilder Write(string value)
		{
			InitBuilder();

			TextBuilder.Append(value);
			return this;
		}

		/// <summary>
		/// Appends the <paramref name="value"/> followed by a new line to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>v
		public CodeBuilder WriteLine(char value)
		{
			InitBuilder();

			TextBuilder.Append(value);
			TextBuilder.AppendLine();
			return this;
		}

		/// <summary>
		/// Appends the <paramref name="value"/> followed by a new line to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>v
		public CodeBuilder WriteLine(string value)
		{
			InitBuilder();

			TextBuilder.AppendLine(value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static StringBuilder Validate(StringBuilder builder)
		{
			if (builder is null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			return builder;
		}

		private void CommaSpace()
		{
			TextBuilder.Append(',');
			Space();
		}

		private void InitBuilder()
		{
			_currentLength = TextBuilder.Length;
		}
	}
}