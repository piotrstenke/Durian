namespace Durian.Analysis.Data
{
	/// <summary>
	/// Provides mechanism to determine whether a value of type <typeparamref name="T"/> represents an actual desired value or <see langword="default"/> specified by the compiler.
	/// </summary>
	/// <typeparam name="T">Type of desired value.</typeparam>
	public readonly struct DefaultedValue<T>
	{
		private readonly bool _isInit;

		/// <summary>
		/// Determines whether the <see cref="Value"/> was initialized by the compiler
		/// </summary>
		public bool IsDefault => !_isInit;

		/// <summary>
		/// Actual value.
		/// </summary>
		public T? Value { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultedValue{T}"/> struct.
		/// </summary>
		/// <param name="value">Actual value.</param>
		public DefaultedValue(T? value)
		{
			Value = value;
			_isInit = true;
		}

		/// <summary>
		/// Deconstructs the current instance.
		/// </summary>
		/// <param name="value">Actual value.</param>
		/// <param name="isDefault">Determines whether the <see cref="Value"/> was initialized by the compiler</param>
		public void Deconstruct(out T? value, out bool isDefault)
		{
			value = Value;
			isDefault = IsDefault;
		}

		/// <summary>
		/// Implicitly converts a <see cref="DefaultedValue{T}"/> to a <typeparamref name="T"/>.
		/// </summary>
		/// <param name="value"><see cref="DefaultedValue{T}"/> to convert to a <typeparamref name="T"/>.</param>
		public static implicit operator T?(in DefaultedValue<T> value)
		{
			return value.Value;
		}

		/// <summary>
		/// Implicitly converts a <typeparamref name="T"/> to a <see cref="DefaultedValue{T}"/>.
		/// </summary>
		/// <param name="value"><typeparamref name="T"/> to convert to a <see cref="DefaultedValue{T}"/>.</param>
		public static implicit operator DefaultedValue<T>(T? value)
		{
			return new(value);
		}
	}
}
