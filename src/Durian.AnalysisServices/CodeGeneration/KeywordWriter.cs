// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Writes single character tokens.
	/// </summary>
	public sealed class KeywordWriter
	{
		/// <inheritdoc cref="CodeBuilder.TextBuilder"/>
		public StringBuilder TextBuilder { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KeywordWriter"/> class.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write the generated code to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
		public KeywordWriter(StringBuilder builder)
		{
			if (builder is null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			TextBuilder = builder;
		}

		/// <summary>
		/// Writes the <see langword="abstract"/> keyword.
		/// </summary>
		public KeywordWriter Abstract()
		{
			TextBuilder.Append("abstract");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="add"/> keyword.
		/// </summary>
		public KeywordWriter Add()
		{
			TextBuilder.Append("add");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="alias"/> keyword.
		/// </summary>
		public KeywordWriter Alias()
		{
			TextBuilder.Append("alias");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="and"/> keyword.
		/// </summary>
		public KeywordWriter And()
		{
			TextBuilder.Append("and");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="annotations"/> keyword.
		/// </summary>
		public KeywordWriter Annotations()
		{
			TextBuilder.Append("annotations");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="__arglist"/> keyword.
		/// </summary>
		public KeywordWriter ArgList()
		{
			TextBuilder.Append("__arglist");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="as"/> keyword.
		/// </summary>
		public KeywordWriter As()
		{
			TextBuilder.Append("as");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="ascending"/> keyword.
		/// </summary>
		public KeywordWriter Ascending()
		{
			TextBuilder.Append("ascending");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="assembly"/> keyword.
		/// </summary>
		public KeywordWriter Assembly()
		{
			TextBuilder.Append("assembly");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="async"/> keyword.
		/// </summary>
#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.

		public KeywordWriter Async()
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
		{
			TextBuilder.Append("async");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="await"/> keyword.
		/// </summary>
		public KeywordWriter Await()
		{
			TextBuilder.Append("await");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="base"/> keyword.
		/// </summary>
		public KeywordWriter Base()
		{
			TextBuilder.Append("base");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="bool"/> keyword.
		/// </summary>
		public KeywordWriter Bool()
		{
			TextBuilder.Append("bool");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="break"/> keyword.
		/// </summary>
		public KeywordWriter Break()
		{
			TextBuilder.Append("break");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="by"/> keyword.
		/// </summary>
		public KeywordWriter By()
		{
			TextBuilder.Append("by");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="byte"/> keyword.
		/// </summary>
		public KeywordWriter Byte()
		{
			TextBuilder.Append("byte");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="case"/> keyword.
		/// </summary>
		public KeywordWriter Case()
		{
			TextBuilder.Append("case");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="catch"/> keyword.
		/// </summary>
		public KeywordWriter Catch()
		{
			TextBuilder.Append("catch");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="char"/> keyword.
		/// </summary>
		public KeywordWriter Char()
		{
			TextBuilder.Append("char");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="checked"/> keyword.
		/// </summary>
		public KeywordWriter Checked()
		{
			TextBuilder.Append("checked");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="checksum"/> keyword.
		/// </summary>
		public KeywordWriter Checksum()
		{
			TextBuilder.Append("checksum");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="class"/> keyword.
		/// </summary>
		public KeywordWriter Class()
		{
			TextBuilder.Append("class");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="const"/> keyword.
		/// </summary>
		public KeywordWriter Const()
		{
			TextBuilder.Append("const");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="continue"/> keyword.
		/// </summary>
		public KeywordWriter Continue()
		{
			TextBuilder.Append("continue");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="decimal"/> keyword.
		/// </summary>
		public KeywordWriter Decimal()
		{
			TextBuilder.Append("decimal");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="default"/> keyword.
		/// </summary>
		public KeywordWriter Default()
		{
			TextBuilder.Append("default");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="define"/> keyword.
		/// </summary>
		public KeywordWriter Define()
		{
			TextBuilder.Append("define");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="delegate"/> keyword.
		/// </summary>
		public KeywordWriter Delegate()
		{
			TextBuilder.Append("delegate");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="descending"/> keyword.
		/// </summary>
		public KeywordWriter Descending()
		{
			TextBuilder.Append("descending");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="disable"/> keyword.
		/// </summary>
		public KeywordWriter Disable()
		{
			TextBuilder.Append("disable");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="do"/> keyword.
		/// </summary>
		public KeywordWriter Do()
		{
			TextBuilder.Append("do");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="double"/> keyword.
		/// </summary>
		public KeywordWriter Double()
		{
			TextBuilder.Append("double");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> keyword.
		/// </summary>
		public KeywordWriter Dynamic()
		{
			TextBuilder.Append("dynamic");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="elif"/> keyword.
		/// </summary>
		public KeywordWriter Elif()
		{
			TextBuilder.Append("elif");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="else"/> keyword.
		/// </summary>
		public KeywordWriter Else()
		{
			TextBuilder.Append("else");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="enable"/> keyword.
		/// </summary>
		public KeywordWriter Enable()
		{
			TextBuilder.Append("enable");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="endif"/> keyword.
		/// </summary>
		public KeywordWriter EndIf()
		{
			TextBuilder.Append("endif");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="endregion"/> keyword.
		/// </summary>
		public KeywordWriter EndRegion()
		{
			TextBuilder.Append("endregion");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="enum"/> keyword.
		/// </summary>
		public KeywordWriter Enum()
		{
			TextBuilder.Append("enum");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="equals"/> keyword.
		/// </summary>
		public KeywordWriter Equals()
		{
			TextBuilder.Append("equals");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="error"/> keyword.
		/// </summary>
		public KeywordWriter Error()
		{
			TextBuilder.Append("error");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="event"/> keyword.
		/// </summary>
		public KeywordWriter Event()
		{
			TextBuilder.Append("event");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="explicit"/> keyword.
		/// </summary>
		public KeywordWriter Explicit()
		{
			TextBuilder.Append("explicit");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="extern"/> keyword.
		/// </summary>
		public KeywordWriter Extern()
		{
			TextBuilder.Append("extern");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="false"/> keyword.
		/// </summary>
		public KeywordWriter False()
		{
			TextBuilder.Append("false");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="field"/> keyword.
		/// </summary>
		public KeywordWriter Field()
		{
			TextBuilder.Append("field");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="finally"/> keyword.
		/// </summary>
		public KeywordWriter Finally()
		{
			TextBuilder.Append("finally");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="fixed"/> keyword.
		/// </summary>
		public KeywordWriter Fixed()
		{
			TextBuilder.Append("fixed");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="float"/> keyword.
		/// </summary>
		public KeywordWriter Float()
		{
			TextBuilder.Append("float");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="for"/> keyword.
		/// </summary>
		public KeywordWriter For()
		{
			TextBuilder.Append("for");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="foreach"/> keyword.
		/// </summary>
		public KeywordWriter ForEach()
		{
			TextBuilder.Append("foreach");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="from"/> keyword.
		/// </summary>
		public KeywordWriter From()
		{
			TextBuilder.Append("from");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="get"/> keyword.
		/// </summary>
		public KeywordWriter Get()
		{
			TextBuilder.Append("get");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="global"/> keyword.
		/// </summary>
		public KeywordWriter Global()
		{
			TextBuilder.Append("global");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="goto"/> keyword.
		/// </summary>
		public KeywordWriter GoTo()
		{
			TextBuilder.Append("goto");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="group"/> keyword.
		/// </summary>
		public KeywordWriter Group()
		{
			TextBuilder.Append("group");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="hidden"/> keyword.
		/// </summary>
		public KeywordWriter Hidden()
		{
			TextBuilder.Append("hidden");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="if"/> keyword.
		/// </summary>
		public KeywordWriter If()
		{
			TextBuilder.Append("if");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="implicit"/> keyword.
		/// </summary>
		public KeywordWriter Implicit()
		{
			TextBuilder.Append("implicit");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="in"/> keyword.
		/// </summary>
		public KeywordWriter In()
		{
			TextBuilder.Append("in");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="init"/> keyword.
		/// </summary>
		public KeywordWriter Init()
		{
			TextBuilder.Append("init");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="int"/> keyword.
		/// </summary>
		public KeywordWriter Int()
		{
			TextBuilder.Append("int");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="interface"/> keyword.
		/// </summary>
		public KeywordWriter Interface()
		{
			TextBuilder.Append("interface");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="internal"/> keyword.
		/// </summary>
		public KeywordWriter Internal()
		{
			TextBuilder.Append("internal");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="into"/> keyword.
		/// </summary>
		public KeywordWriter Into()
		{
			TextBuilder.Append("into");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="is"/> keyword.
		/// </summary>
		public KeywordWriter Is()
		{
			TextBuilder.Append("is");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="join"/> keyword.
		/// </summary>
		public KeywordWriter Join()
		{
			TextBuilder.Append("join");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="let"/> keyword.
		/// </summary>
		public KeywordWriter Let()
		{
			TextBuilder.Append("let");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="line"/> keyword.
		/// </summary>
		public KeywordWriter Line()
		{
			TextBuilder.Append("line");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="load"/> keyword.
		/// </summary>
		public KeywordWriter Load()
		{
			TextBuilder.Append("load");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="lock"/> keyword.
		/// </summary>
		public KeywordWriter Lock()
		{
			TextBuilder.Append("lock");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="long"/> keyword.
		/// </summary>
		public KeywordWriter Long()
		{
			TextBuilder.Append("long");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="__makeref"/> keyword.
		/// </summary>
		public KeywordWriter MakeRef()
		{
			TextBuilder.Append("__makeref");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="managed"/> keyword.
		/// </summary>
		public KeywordWriter Managed()
		{
			TextBuilder.Append("managed");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="method"/> keyword.
		/// </summary>
		public KeywordWriter Method()
		{
			TextBuilder.Append("method");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="module"/> keyword.
		/// </summary>
		public KeywordWriter Module()
		{
			TextBuilder.Append("module");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="nameof"/> keyword.
		/// </summary>
		public KeywordWriter NameOf()
		{
			TextBuilder.Append("nameof");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="namespace"/> keyword.
		/// </summary>
		public KeywordWriter Namespace()
		{
			TextBuilder.Append("namespace");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="new"/> keyword.
		/// </summary>
		public KeywordWriter New()
		{
			TextBuilder.Append("new");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="nint"/> keyword.
		/// </summary>
		public KeywordWriter NInt()
		{
			TextBuilder.Append("nint");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="not"/> keyword.
		/// </summary>
		public KeywordWriter Not()
		{
			TextBuilder.Append("not");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="notnull"/> keyword.
		/// </summary>
		public KeywordWriter NotNull()
		{
			TextBuilder.Append("notnull");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="nuint"/> keyword.
		/// </summary>
		public KeywordWriter NUInt()
		{
			TextBuilder.Append("nuint");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="null"/> keyword.
		/// </summary>
		public KeywordWriter Null()
		{
			TextBuilder.Append("null");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="nullable"/> keyword.
		/// </summary>
		public KeywordWriter Nullable()
		{
			TextBuilder.Append("nullable");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="object"/> keyword.
		/// </summary>
		public KeywordWriter Object()
		{
			TextBuilder.Append("object");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="on"/> keyword.
		/// </summary>
		public KeywordWriter On()
		{
			TextBuilder.Append("on");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="operator"/> keyword.
		/// </summary>
		public KeywordWriter Operator()
		{
			TextBuilder.Append("operator");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="or"/> keyword.
		/// </summary>
		public KeywordWriter Or()
		{
			TextBuilder.Append("or");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="orderby"/> keyword.
		/// </summary>
		public KeywordWriter OrderBy()
		{
			TextBuilder.Append("orderby");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="out"/> keyword.
		/// </summary>
		public KeywordWriter Out()
		{
			TextBuilder.Append("out");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="override"/> keyword.
		/// </summary>
		public KeywordWriter Override()
		{
			TextBuilder.Append("override");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="param"/> keyword.
		/// </summary>
		public KeywordWriter Param()
		{
			TextBuilder.Append("param");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="params"/> keyword.
		/// </summary>
		public KeywordWriter Params()
		{
			TextBuilder.Append("params");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="partial"/> keyword.
		/// </summary>
		public KeywordWriter Partial()
		{
			TextBuilder.Append("partial");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="pragma"/> keyword.
		/// </summary>
		public KeywordWriter Pragma()
		{
			TextBuilder.Append("pragma");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="private"/> keyword.
		/// </summary>
		public KeywordWriter Private()
		{
			TextBuilder.Append("private");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="property"/> keyword.
		/// </summary>
		public KeywordWriter Property()
		{
			TextBuilder.Append("property");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="protected"/> keyword.
		/// </summary>
		public KeywordWriter Protected()
		{
			TextBuilder.Append("protected");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="public"/> keyword.
		/// </summary>
		public KeywordWriter Public()
		{
			TextBuilder.Append("public");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="readonly"/> keyword.
		/// </summary>
		public KeywordWriter ReadOnly()
		{
			TextBuilder.Append("readonly");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="record"/> keyword.
		/// </summary>
		public KeywordWriter Record()
		{
			TextBuilder.Append("record");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="Ref"/> keyword.
		/// </summary>
		public KeywordWriter Ref()
		{
			TextBuilder.Append("ref");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="r"/> keyword.
		/// </summary>
		public KeywordWriter Reference()
		{
			TextBuilder.Append('r');

			return this;
		}

		/// <summary>
		/// Writes the <see langword="__reftype"/> keyword.
		/// </summary>
		public KeywordWriter RefType()
		{
			TextBuilder.Append("__reftype");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="__refvalue"/> keyword.
		/// </summary>
		public KeywordWriter RefValue()
		{
			TextBuilder.Append("__refvalue");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="region"/> keyword.
		/// </summary>
		public KeywordWriter Region()
		{
			TextBuilder.Append("region");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="remove"/> keyword.
		/// </summary>
		public KeywordWriter Remove()
		{
			TextBuilder.Append("remove");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="restore"/> keyword.
		/// </summary>
		public KeywordWriter Restore()
		{
			TextBuilder.Append("restore");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="return"/> keyword.
		/// </summary>
		public KeywordWriter Return()
		{
			TextBuilder.Append("return");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="sbyte"/> keyword.
		/// </summary>
		public KeywordWriter SByte()
		{
			TextBuilder.Append("sbyte");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="sealed"/> keyword.
		/// </summary>
		public KeywordWriter Sealed()
		{
			TextBuilder.Append("sealed");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="select"/> keyword.
		/// </summary>
		public KeywordWriter Select()
		{
			TextBuilder.Append("select");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="set"/> keyword.
		/// </summary>
		public KeywordWriter Set()
		{
			TextBuilder.Append("set");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="short"/> keyword.
		/// </summary>
		public KeywordWriter Short()
		{
			TextBuilder.Append("short");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="sizeof"/> keyword.
		/// </summary>
		public KeywordWriter SizeOf()
		{
			TextBuilder.Append("sizeof");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="stackalloc"/> keyword.
		/// </summary>
		public KeywordWriter StackAlloc()
		{
			TextBuilder.Append("stackalloc");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="static"/> keyword.
		/// </summary>
		public KeywordWriter Static()
		{
			TextBuilder.Append("static");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="string"/> keyword.
		/// </summary>
		public KeywordWriter String()
		{
			TextBuilder.Append("warning");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="struct"/> keyword.
		/// </summary>
		public KeywordWriter Struct()
		{
			TextBuilder.Append("struct");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="as"/> keyword.
		/// </summary>
		public KeywordWriter Switch()
		{
			TextBuilder.Append("switch");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="this"/> keyword.
		/// </summary>
		public KeywordWriter This()
		{
			TextBuilder.Append("this");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="throw"/> keyword.
		/// </summary>
		public KeywordWriter Throw()
		{
			TextBuilder.Append("throw");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="true"/> keyword.
		/// </summary>
		public KeywordWriter True()
		{
			TextBuilder.Append("true");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="try"/> keyword.
		/// </summary>
		public KeywordWriter Try()
		{
			TextBuilder.Append("try");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="type"/> keyword.
		/// </summary>
		public KeywordWriter Type()
		{
			TextBuilder.Append("type");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="typeof"/> keyword.
		/// </summary>
		public KeywordWriter TypeOf()
		{
			TextBuilder.Append("typeof");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="typevar"/> keyword.
		/// </summary>
		public KeywordWriter TypeVar()
		{
			TextBuilder.Append("typevar");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="uint"/> keyword.
		/// </summary>
		public KeywordWriter UInt()
		{
			TextBuilder.Append("uint");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="ulong"/> keyword.
		/// </summary>
		public KeywordWriter ULong()
		{
			TextBuilder.Append("ulong");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="unchecked"/> keyword.
		/// </summary>
		public KeywordWriter Unchecked()
		{
			TextBuilder.Append("unchecked");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="undef"/> keyword.
		/// </summary>
		public KeywordWriter Undef()
		{
			TextBuilder.Append("undef");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="unmanaged"/> keyword.
		/// </summary>
		public KeywordWriter Unmanaged()
		{
			TextBuilder.Append("unmanaged");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="unsafe"/> keyword.
		/// </summary>
		public KeywordWriter Unsafe()
		{
			TextBuilder.Append("unsafe");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="ushort"/> keyword.
		/// </summary>
		public KeywordWriter UShort()
		{
			TextBuilder.Append("ushort");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="using"/> keyword.
		/// </summary>
		public KeywordWriter Using()
		{
			TextBuilder.Append("using");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="value"/> keyword.
		/// </summary>
		public KeywordWriter Value()
		{
			TextBuilder.Append("value");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="var"/> keyword.
		/// </summary>
		public KeywordWriter Var()
		{
			TextBuilder.Append("var");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="virtual"/> keyword.
		/// </summary>
		public KeywordWriter Virtual()
		{
			TextBuilder.Append("virtual");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="void"/> keyword.
		/// </summary>
		public KeywordWriter Void()
		{
			TextBuilder.Append("void");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="volatile"/> keyword.
		/// </summary>
		public KeywordWriter Volatile()
		{
			TextBuilder.Append("volatile");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="warning"/> keyword.
		/// </summary>
		public KeywordWriter Warning()
		{
			TextBuilder.Append("warning");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="warnings"/> keyword.
		/// </summary>
		public KeywordWriter Warnings()
		{
			TextBuilder.Append("warnings");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="when"/> keyword.
		/// </summary>
		public KeywordWriter When()
		{
			TextBuilder.Append("when");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="where"/> keyword.
		/// </summary>
		public KeywordWriter Where()
		{
			TextBuilder.Append("where");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="while"/> keyword.
		/// </summary>
		public KeywordWriter While()
		{
			TextBuilder.Append("while");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="with"/> keyword.
		/// </summary>
		public KeywordWriter With()
		{
			TextBuilder.Append("with");

			return this;
		}

		/// <summary>
		/// Writes the <see langword="yield"/> keyword.
		/// </summary>
		public KeywordWriter Yield()
		{
			TextBuilder.Append("yield");

			return this;
		}
	}
}
