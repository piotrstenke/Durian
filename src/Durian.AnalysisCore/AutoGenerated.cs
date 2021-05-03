using System.Text;

namespace Durian
{
	/// <summary>
	/// Contains various utility methods that generate text of the &lt;auto-generated&gt; header or the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/>.
	/// </summary>
	public static class AutoGenerated
	{
		/// <inheritdoc cref="GetHeader(string, string)"/>
		public static string GetHeader()
		{
			return GetHeader(null, null);
		}

		/// <inheritdoc cref="GetHeader(string, string)"/>
		public static string GetHeader(string? generatorName)
		{
			return GetHeader(generatorName, null);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is written at the beginning of every auto-generated C# file.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		public static string GetHeader(string? generatorName, string? version)
		{
			string name;

			if (string.IsNullOrWhiteSpace(generatorName))
			{
				name = "a source generator";
			}
			else if (string.IsNullOrWhiteSpace(version))
			{
				name = $"the {generatorName} class";
			}
			else
			{
				name = $"the {generatorName} class (version {version})";
			}

			return
$@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by {name}.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that combines the <paramref name="input"/> and the header returned by the <see cref="GetHeader()"/> method.
		/// </summary>
		/// <param name="input"><see cref="string"/> to apply the header to.</param>
		public static string ApplyHeader(string? input)
		{
			return ApplyHeader(input, null, null);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that combines the <paramref name="input"/> and the header returned by the <see cref="GetHeader(string?)"/> method.
		/// </summary>
		/// <param name="input"><see cref="string"/> to apply the header to.</param>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		public static string ApplyHeader(string? input, string? generatorName)
		{
			return ApplyHeader(input, generatorName, null);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that combines the <paramref name="input"/> and the header returned by the <see cref="GetHeader(string?, string?)"/> method.
		/// </summary>
		/// <param name="input"><see cref="string"/> to apply the header to.</param>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		public static string ApplyHeader(string? input, string? generatorName, string? version)
		{
			return $"{GetHeader(generatorName, version)}{(string.IsNullOrEmpty(input) ? string.Empty : $"\n{input}")}";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/> with the specified <paramref name="generatorName"/> and <paramref name="version"/> as its arguments.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		public static string GetGeneratedCodeAttribute(string? generatorName, string? version)
		{
			return $"[global::System.CodeDom.Compiler.GeneratedCode(\"{generatorName ?? string.Empty}\", \"{version ?? string.Empty}\")]";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the <c>Durian.Generator.GeneratedFromAttribute</c> with the specified <paramref name="source"/> as its argument.
		/// </summary>
		/// <param name="source">Member this code was generated from.</param>
		public static string GetGeneratedFromAttribute(string? source)
		{
			return $"[global::Durian.Generator.GeneratedFrom(\"{source}\")]";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the <c>Durian.Generator.DurianGeneratedAttribute</c>.
		/// </summary>
		public static string GetDurianGeneratedAttribute()
		{
			return "[global::Durian.Generator.DurianGenerated]";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that combines the results of the <see cref="GetGeneratedCodeAttribute(string?, string?)"/> and <see cref="GetDurianGeneratedAttribute"/> methods with the specified <paramref name="indent"/> applied.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		/// <param name="indent">Number of tab characters to apply before the attributes (first attribute is not affected).</param>
		public static string GetCodeGenerationAttributes(string? generatorName, string? version, int indent)
		{
			StringBuilder sb = new();
			sb.AppendLine(GetGeneratedCodeAttribute(generatorName, version));
			Indent(sb, indent);
			sb.AppendLine(GetDurianGeneratedAttribute());

			return sb.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> that combines the results of the <see cref="GetGeneratedCodeAttribute(string?, string?)"/>, <see cref="GetDurianGeneratedAttribute"/> and <see cref="GetGeneratedFromAttribute(string?)"/> methods with the specified <paramref name="indent"/> applied.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		/// <param name="source">Member this code was generated from.</param>
		/// <param name="indent">Number of tab characters to apply before the attributes (first attribute is not affected).</param>
		public static string GetCodeGenerationAttributes(string? generatorName, string? version, string? source, int indent)
		{
			StringBuilder sb = new();
			sb.AppendLine(GetGeneratedCodeAttribute(generatorName, version));
			Indent(sb, indent);
			sb.AppendLine(GetDurianGeneratedAttribute());
			Indent(sb, indent);
			sb.AppendLine(GetGeneratedFromAttribute(source));

			return sb.ToString();
		}

		/// <inheritdoc cref="GetInheritdoc(string?)"/>
		public static string GetInheritdoc()
		{
			return "/// <inheritdoc/>";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains the inheritdoc tag.
		/// </summary>
		/// <param name="source">Source to put in the '<c>cref</c>' attribute.</param>
		public static string GetInheritdoc(string? source)
		{
			if (source is null)
			{
				return GetInheritdoc();
			}

			return $"/// <inheritdoc cref=\"{source}\"/>";
		}

		private static void Indent(StringBuilder sb, int indent)
		{
			for (int i = 0; i < indent; i++)
			{
				sb.Append('\t');
			}
		}
	}
}
