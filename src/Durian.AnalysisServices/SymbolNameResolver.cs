// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains classes that implement the <see cref="ISymbolNameResolver"/> interface.
	/// </summary>
	public static class SymbolNameResolver
	{
		private static ISymbolNameResolver _default = Verbatim.Instance;

		/// <summary>
		/// Default <see cref="ISymbolNameResolver"/>.
		/// </summary>
		/// <remarks>At application start, <see cref="Verbatim.Instance"/> is the default.</remarks>
		/// <exception cref="ArgumentNullException">Value is <see langword="null"/>.</exception>
		public static ISymbolNameResolver Default
		{
			get => _default;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_default = value;
			}
		}

		/// <summary>
		/// Returns a globally accessible <see cref="ISymbolNameResolver"/> that handles the specified <see cref="SymbolName"/> format.
		/// </summary>
		/// <param name="format"><see cref="SymbolName"/> format to get the <see cref="ISymbolNameResolver"/> for.</param>
		/// <param name="reuseBuilder">Determines whether to reuse the internal <see cref="CodeBuilder"/> of the <see cref="ISymbolNameResolver"/> is a <see cref="WithBuilder"/>.</param>
		public static ISymbolNameResolver GetResolver(SymbolName format, bool reuseBuilder = false)
		{
			if (format == SymbolName.Default)
			{
				return Verbatim.Instance;
			}

			return new WithBuilder(format, reuseBuilder);
		}

		/// <summary>
		/// Base class for resolvers that use a <see cref="CodeBuilder"/> internally.
		/// </summary>
		public class WithBuilder : ISymbolNameResolver
		{
			private readonly CodeBuilder? _builder;

			/// <summary>
			/// Determines whether to reuse an instance of the <see cref="CodeBuilder"/> class.
			/// </summary>
			public bool ReuseBuilder { get; }

			/// <summary>
			/// <see cref="SymbolName"/> format this resolver supports.
			/// </summary>
			public SymbolName SupportedNameFormat { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="WithBuilder"/> class.
			/// </summary>
			/// <param name="supportedNameFormat"><see cref="SymbolName"/> format this resolver supports.</param>
			public WithBuilder(SymbolName supportedNameFormat)
			{
				SupportedNameFormat = supportedNameFormat;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithBuilder"/> class.
			/// </summary>
			/// <param name="supportedNameFormat"><see cref="SymbolName"/> format this resolver supports.</param>
			/// <param name="reuseBuilder">Determines whether to reuse an instance of the <see cref="CodeBuilder"/> class.</param>
			public WithBuilder(SymbolName supportedNameFormat, bool reuseBuilder)
			{
				SupportedNameFormat = supportedNameFormat;
				ReuseBuilder = reuseBuilder;

				if (reuseBuilder)
				{
					_builder = new(false);
				}
			}

			/// <inheritdoc/>
			public virtual string ResolveName(ISymbol symbol)
			{
				string result;

				if (_builder is null)
				{
					CodeBuilder builder = new(false);
					builder.Name(symbol, SupportedNameFormat);
					result = builder.ToString();
				}
				else
				{
					_builder.Name(symbol, SupportedNameFormat);
					result = _builder.ToString();
					_builder.Clear();
				}

				return result;
			}

			/// <inheritdoc/>
			public virtual string ResolveName(IMemberData member)
			{
				string result;

				if (_builder is null)
				{
					CodeBuilder builder = new(false);
					builder.Name(member, SupportedNameFormat);
					result = builder.ToString();
				}
				else
				{
					_builder.Name(member, SupportedNameFormat);
					result = _builder.ToString();
					_builder.Clear();
				}

				return result;
			}

			/// <inheritdoc/>
			public virtual string ResolveName(ISymbolOrMember member)
			{
				if (member.HasMember)
				{
					return ResolveName(member.Member);
				}

				return ResolveName(member.Symbol);
			}
		}

		/// <summary>
		/// <see cref="ISymbolNameResolver"/> that calls the <see cref="SymbolExtensions.GetVerbatimName(ISymbol)"/> method.
		/// </summary>
		public sealed class Verbatim : ISymbolNameResolver
		{
			/// <summary>
			/// Instance of the <see cref="Verbatim"/> class accessible globally.
			/// </summary>
			public static Verbatim Instance { get; } = new();

			/// <summary>
			/// Initializes a new instance of the <see cref="Verbatim"/> class.
			/// </summary>
			public Verbatim()
			{
			}

			/// <inheritdoc/>
			public string ResolveName(ISymbol symbol)
			{
				return symbol.GetVerbatimName();
			}

			/// <inheritdoc/>
			public string ResolveName(IMemberData member)
			{
				return member.Name;
			}

			/// <inheritdoc/>
			public string ResolveName(ISymbolOrMember member)
			{
				if (member.HasMember)
				{
					return ResolveName(member.Member);
				}

				return ResolveName(member.Symbol);
			}
		}

		/// <summary>
		/// <see cref="ISymbolNameResolver"/> that uses the <see cref="ISymbol.Name"/> property.
		/// </summary>
		public sealed class Normal : ISymbolNameResolver
		{
			/// <summary>
			/// Instance of the <see cref="Normal"/> class accessible globally.
			/// </summary>
			public static Normal Instance { get; } = new();

			/// <summary>
			/// Initializes a new instance of the <see cref="Normal"/> class.
			/// </summary>
			public Normal()
			{
			}

			/// <inheritdoc/>
			public string ResolveName(ISymbol symbol)
			{
				return symbol.Name;
			}

			/// <inheritdoc/>
			public string ResolveName(IMemberData member)
			{
				return member.Symbol.Name;
			}

			/// <inheritdoc/>
			public string ResolveName(ISymbolOrMember member)
			{
				if (member.HasMember)
				{
					return ResolveName(member.Member);
				}

				return ResolveName(member.Symbol);
			}
		}
	}
}