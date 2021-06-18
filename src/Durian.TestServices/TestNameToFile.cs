// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices
{
	/// <summary>
	/// A <see cref="IHintNameProvider"/> that returns name of the current test.
	/// </summary>
	[DebuggerDisplay("TestName = {TestName}, Counter = {Counter}")]
	public sealed class TestNameToFile : IHintNameProvider
	{
		private int _counter = 0;
		private string _current;
		private string _testName;

		/// <summary>
		/// Number that is added to the <see cref="TestName"/> when creating the file name. Each call to the <see cref="Success"/> method adds <c>1</c> to this value.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><see cref="Counter"/> cannot be less than <c>0</c>.</exception>
		public int Counter
		{
			get => _counter;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(Counter), $"{nameof(Counter)} cannot be less than 0!");
				}

				_counter = value;
				_current = value == 0 ? _testName : $"{_testName}_{value}";
			}
		}

		/// <summary>
		/// Name of the current test.
		/// </summary>
		/// <exception cref="ArgumentException"><see cref="TestName"/> cannot be <see langword="null"/> or empty.</exception>
		public string TestName
		{
			get => _testName;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(TestName)} cannot be null or empty!");
				}

				if (value != _testName)
				{
					_counter = 0;
					_testName = value;
					_current = _testName;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestNameToFile"/> class.
		/// </summary>
		public TestNameToFile()
		{
			_testName = "test";
			_current = _testName;
		}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		/// <summary>
		/// Initializes a new instance of the <see cref="TestNameToFile"/> class.
		/// </summary>
		/// <param name="testName">Name of the current test.</param>
		public TestNameToFile(string testName)
		{
			TestName = testName;
		}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		/// <inheritdoc/>
		public string GetFileName()
		{
			return _current;
		}

		/// <summary>
		/// Resets the provider to the original state when the <see cref="TestName"/> was set.
		/// </summary>
		public void Reset()
		{
			_current = _testName;
			_counter = 0;
		}

		/// <inheritdoc/>
		public void Success()
		{
			_counter++;
			_current = $"{_testName}_{_counter}";
		}

		string IHintNameProvider.GetFileName(ISymbol symbol)
		{
			return GetFileName();
		}
	}
}
