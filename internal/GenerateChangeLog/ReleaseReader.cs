using System;
using System.Collections.Generic;
using System.IO;

internal class ReleaseReader : IDisposable
{
	private StreamReader _reader;
	private bool _disposedValue;
	private int _currentLine;
	private HashSet<string> _members;
	private string? _cachedLine;

	public bool IsEnd => _reader.EndOfStream;

	public ReleaseReader(string filePath)
	{
		_reader = new StreamReader(filePath);
		_members = new();
	}

	~ReleaseReader()
	{
		Dispose(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			_reader.Dispose();
			_disposedValue = true;

			if (disposing)
			{
				_currentLine = default;
				_members = null!;
				_reader = null!;
			}
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public HashSet<string> ReadEntries()
	{
		while (!IsEnd)
		{
			MemberAction action = GetCurrentAction();

			switch (action)
			{
				case MemberAction.Add:
					AddMembers();
					break;
				case MemberAction.Move:
					MoveMembers();
					break;
				case MemberAction.Remove:
					RemoveMembers();
					break;
				default:
					break;
			}
		}

		return _members;
	}

	public void AddMembers()
	{
		string? line;

		while ((line = ReadLine()) is not null && !ShouldStop(line))
		{
			if (ShouldSkip(line) || !ValidateEntry(line))
			{
				continue;
			}

			int index = line.IndexOf('-');
			string member = line[(index + 1)..].TrimStart();

			if (!_members.Add(member))
			{
				Console.WriteLine($"Member '{member}' already added! At line: {_currentLine}");
			}
		}
	}

	public void MoveMembers()
	{
		string? line;

		while ((line = ReadLine()) is not null && !ShouldStop(line))
		{
			if (ShouldSkip(line) || !ValidateEntry(line))
			{
				continue;
			}

			int arrow = line.IndexOf("->");

			if (arrow == -1)
			{
				Console.WriteLine($"Wrong entry format! Expected move expression (->) At line {_currentLine}");
				continue;
			}

			string original = line.Substring(0, arrow).TrimEnd();
			string target = line.Substring(arrow + 2, line.Length - arrow - 2).TrimStart();

			if (original == target)
			{
				continue;
			}

			if (_members.Contains(target))
			{
				Console.WriteLine($"Tried to move member '{original}' to '{target}', but '{target}' already exists! At line {_currentLine}");
				continue;
			}

			if (!_members.Remove(original))
			{
				Console.WriteLine($"Tried to move unknown member '{original}'! At line {_currentLine}");
				continue;
			}

			_members.Add(target);
		}
	}

	public void RemoveMembers()
	{
		string? line;

		while ((line = ReadLine()) is not null && !ShouldStop(line))
		{
			if (ShouldSkip(line) || !ValidateEntry(line))
			{
				continue;
			}

			if (!_members.Remove(line))
			{
				Console.WriteLine($"Member '{line}' does not exist! At line: {_currentLine}");
			}
		}
	}

	public MemberAction GetCurrentAction()
	{
		string? line;

		while ((line = ReadLine()) is not null)
		{
			_currentLine++;

			if (!line.StartsWith("###"))
			{
				continue;
			}

			if (line.Contains("Added"))
			{
				return MemberAction.Add;
			}

			if (line.Contains("Moved"))
			{
				return MemberAction.Move;
			}

			if (line.Contains("Removed"))
			{
				return MemberAction.Remove;
			}

			Console.WriteLine($"Unknown action specified at line {_currentLine}: {line}");
			break;
		}

		return MemberAction.None;
	}

	private bool ValidateEntry(string line)
	{
		if (!line.StartsWith('-'))
		{
			Console.WriteLine($"Entry must start with an '-'! At line: {_currentLine}");
			return false;
		}

		return true;
	}

	private bool ShouldStop(string line)
	{
		if (line.StartsWith("#"))
		{
			_cachedLine = line;
			return true;
		}

		return false;
	}

	private bool ShouldSkip(string line)
	{
		_currentLine++;

		if (string.IsNullOrWhiteSpace(line))
		{
			return true;
		}

		return false;
	}

	private string? ReadLine()
	{
		if (_cachedLine is not null)
		{
			string temp = _cachedLine;
			_cachedLine = null;
			return temp;
		}

		return _reader.ReadLine()?.Trim();
	}
}
