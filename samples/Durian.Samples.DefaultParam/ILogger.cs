using System;

namespace Durian.Samples.DefaultParam
{
	public interface ILogger<T> where T : IEquatable<string>
	{
		void Clear();
		void Error(T message);
		void Info(T message);
		void Save(string path);
		void Warning(T message);
	}
}
