namespace Durian.Analysis
{
	/// <summary>
	/// Provides mechanism for building an object step-by-step.
	/// </summary>
	/// <typeparam name="TResult">Type of object being built.</typeparam>
	public interface IBuilder<out TResult>
	{
		/// <summary>
		/// Determines whether the current state of the builder is valid for creation of the object being built.
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// Actually builds the objects.
		/// </summary>
		/// <exception cref="BuilderException">Cannot build on object while state of the builder is not valid.</exception>
		TResult Build();
	}
}
