// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using static Durian.Analysis.DiagnosticReceiver.Composite;

namespace Durian.Analysis.Logging
{
	public partial class LoggableDiagnosticReceiver
	{
		/// <summary>
		/// <see cref="LoggableDiagnosticReceiver"/> that combines multiple <see cref="IDiagnosticReceiver"/>s.
		/// </summary>
		public sealed class Composite : LoggableDiagnosticReceiver, ICompositeDiagnosticReceiver
		{
			private readonly List<Entry> _receivers;

			/// <inheritdoc/>
			public int NumReceivers => _receivers.Count;

			/// <inheritdoc cref="Composite(ILoggableGenerator, NodeOutput)"/>
			public Composite(ILoggableGenerator generator) : base(generator)
			{
				_receivers = new();
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Composite"/> class.
			/// </summary>
			/// <param name="generator"><see cref="Composite"/> that will log the received <see cref="Diagnostic"/>s.</param>
			/// <param name="nodeOutput">Determines what to output when a <see cref="SyntaxNode"/> is being logged.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
			public Composite(ILoggableGenerator generator, NodeOutput nodeOutput) : base(generator, nodeOutput)
			{
				_receivers = new();
			}

			/// <inheritdoc cref="Composite(ILoggableGenerator, NodeOutput, IEnumerable{IDiagnosticReceiver})"/>
			public Composite(ILoggableGenerator generator, IEnumerable<IDiagnosticReceiver> diagnosticReceivers) : this(generator)
			{
				AddReceivers(diagnosticReceivers);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Composite"/> class.
			/// </summary>
			/// <param name="generator"><see cref="Composite"/> that will log the received <see cref="Diagnostic"/>s.</param>
			/// <param name="nodeOutput">Determines what to output when a <see cref="SyntaxNode"/> is being logged.</param>
			/// <param name="diagnosticReceivers">A collection of <see cref="IDiagnosticReceiver"/>s to add to the current resolver.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="diagnosticReceivers"/> is <see langword="null"/>.</exception>
			/// <exception cref="ArgumentException">Collection contains <see langword="null"/> objects. -or- Collection contains <see cref="IDiagnosticReceiver"/>s that are already present in the current receiver.</exception>
			public Composite(ILoggableGenerator generator, NodeOutput nodeOutput, IEnumerable<IDiagnosticReceiver> diagnosticReceivers) : this(generator, nodeOutput)
			{
				AddReceivers(diagnosticReceivers);
			}

			/// <inheritdoc/>
			public void AddReceiver(IDiagnosticReceiver diagnosticReceiver)
			{
				AddReceiver(diagnosticReceiver, false);
			}

			/// <summary>
			/// Adds the specified <paramref name="diagnosticReceiver"/> to the current receiver.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to add to the current receiver.</param>
			/// <param name="push">
			/// Determines when diagnostics should be reported to the specified <paramref name="diagnosticReceiver"/>:
			/// <see langword="false"/> for when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called,
			/// <see langword="true"/> for when the <see cref="Push"/> method is called.
			/// </param>
			/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
			/// <exception cref="ArgumentException"><paramref name="diagnosticReceiver"/> is already present in the current receiver.</exception>
			public void AddReceiver(IDiagnosticReceiver diagnosticReceiver, bool push)
			{
				if (ContainsReceiver(diagnosticReceiver))
				{
					throw new ArgumentException("Target is already present in the current receiver", nameof(diagnosticReceiver));
				}

				lock (_receivers)
				{
					_receivers.Add(new Entry(diagnosticReceiver, push));
				}
			}

			/// <inheritdoc/>
			public void AddReceivers(IEnumerable<IDiagnosticReceiver> diagnosticReceivers)
			{
				if (diagnosticReceivers is null)
				{
					throw new ArgumentNullException(nameof(diagnosticReceivers));
				}

				lock (_receivers)
				{
					foreach (IDiagnosticReceiver diagnostic in diagnosticReceivers)
					{
						if (diagnostic is null)
						{
							throw new ArgumentException("Collection contains null objects", nameof(diagnosticReceivers));
						}

						if (_receivers.Exists(entry => entry.DiagnosticReceiver.Equals(diagnostic)))
						{
							throw new ArgumentException("Collection contains objects that are already present in the current receiver", nameof(diagnosticReceivers));
						}

						_receivers.Add(new Entry(diagnostic, false));
					}
				}
			}

			/// <inheritdoc/>
			public bool ContainsReceiver(IDiagnosticReceiver diagnosticReceiver)
			{
				if (diagnosticReceiver is null)
				{
					throw new ArgumentNullException(nameof(diagnosticReceiver));
				}

				if(_receivers.Count > 0)
				{
					lock (_receivers)
					{
						return _receivers.Exists(entry => entry.DiagnosticReceiver.Equals(diagnosticReceiver));
					}
				}

				return false;
			}

			/// <summary>
			/// Returns all <see cref="IDiagnosticReceiver"/> added to the current receiver.
			/// </summary>
			/// <param name="push">
			/// Determines which set of <see cref="IDiagnosticReceiver"/>s to return:
			/// <see langword="false"/> for <see cref="IDiagnosticReceiver"/>s that report diagnostics when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called,
			/// <see langword="true"/> for <see cref="IDiagnosticReceiver"/>s that report diagnostics when the <see cref="Push"/> method is called.
			/// </param>
			public IEnumerable<IDiagnosticReceiver> GetDiagnosticReceivers(bool push)
			{
				if (_receivers.Count == 0)
				{
					return Array.Empty<IDiagnosticReceiver>();
				}

				List<IDiagnosticReceiver> diagnosticReceivers = new(_receivers.Count);

				lock (_receivers)
				{
					foreach (Entry entry in _receivers)
					{
						if (entry.Push == push)
						{
							diagnosticReceivers.Add(entry.DiagnosticReceiver);
						}
					}
				}

				return diagnosticReceivers;
			}

			/// <inheritdoc/>
			public IEnumerator<IDiagnosticReceiver> GetEnumerator()
			{
				foreach (Entry entry in _receivers)
				{
					yield return entry.DiagnosticReceiver;
				}
			}

			/// <inheritdoc/>
			public IEnumerable<IDiagnosticReceiver> GetReceivers()
			{
				if (_receivers.Count == 0)
				{
					return Array.Empty<IDiagnosticReceiver>();
				}

				List<IDiagnosticReceiver> diagnosticReceivers = new(_receivers.Count);

				lock (_receivers)
				{
					foreach (Entry entry in _receivers)
					{
						diagnosticReceivers.Add(entry.DiagnosticReceiver);
					}
				}

				return diagnosticReceivers;
			}

			/// <summary>
			/// Determines whether diagnostics are reported for the specified <paramref name="diagnosticReceiver"/> when the <see cref="Push"/> method is called.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to check when diagnostics are reported for.</param>
			/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
			/// <exception cref="ArgumentException"><paramref name="diagnosticReceiver"/> is not present in the current receiver.</exception>
			public bool IsPush(IDiagnosticReceiver diagnosticReceiver)
			{
				if (diagnosticReceiver is null)
				{
					throw new ArgumentNullException(nameof(diagnosticReceiver));
				}

				if (_receivers.Count > 0)
				{
					lock (_receivers)
					{
						for (int i = 0; i < _receivers.Count; i++)
						{
							Entry entry = _receivers[i];

							if (entry.DiagnosticReceiver.Equals(diagnosticReceiver))
							{
								return entry.Push;
							}
						}
					}
				}

				throw new ArgumentException("Target is not present in the current receiver", nameof(diagnosticReceiver));
			}

			/// <inheritdoc/>
			public bool RemoveReceiver(IDiagnosticReceiver diagnosticReceiver)
			{
				if (diagnosticReceiver is null)
				{
					throw new ArgumentNullException(nameof(diagnosticReceiver));
				}

				lock (_receivers)
				{
					int index = _receivers.FindIndex(entry => entry.DiagnosticReceiver.Equals(diagnosticReceiver));

					if (index == -1)
					{
						return false;
					}

					_receivers.RemoveAt(index);
					return true;
				}
			}

			/// <inheritdoc/>
			public void RemoveReceivers(IEnumerable<IDiagnosticReceiver> diagnosticReceivers)
			{
				if (diagnosticReceivers is null)
				{
					throw new ArgumentNullException(nameof(diagnosticReceivers));
				}

				lock (_receivers)
				{
					foreach (IDiagnosticReceiver diagnostic in diagnosticReceivers)
					{
						if (diagnostic is null)
						{
							throw new ArgumentException("Collection contains null objects", nameof(diagnosticReceivers));
						}

						int index = _receivers.FindIndex(entry => entry.DiagnosticReceiver.Equals(diagnostic));

						if (index > -1)
						{
							_receivers.RemoveAt(index);
						}
					}
				}
			}

			/// <inheritdoc/>
			public void RemoveReceivers()
			{
				lock (_receivers)
				{
					_receivers.Clear();
				}
			}

			/// <inheritdoc/>
			public override void ReportDiagnostic(Diagnostic diagnostic)
			{
				base.ReportDiagnostic(diagnostic);

				if (_receivers.Count > 0)
				{
					lock (_receivers)
					{
						foreach (Entry entry in _receivers)
						{
							if (!entry.Push)
							{
								entry.DiagnosticReceiver.ReportDiagnostic(diagnostic);
							}
						}
					}
				}
			}

			/// <inheritdoc/>
			public override void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
			{
				base.ReportDiagnostic(descriptor, location, messageArgs);

				if (_receivers.Count > 0)
				{
					lock (_receivers)
					{
						foreach (Entry entry in _receivers)
						{
							if (!entry.Push)
							{
								entry.DiagnosticReceiver.ReportDiagnostic(descriptor, location, messageArgs);
							}
						}
					}
				}
			}

			/// <inheritdoc/>
			public override void SetTargetNode(CSharpSyntaxNode? node, string? hintName)
			{
				base.SetTargetNode(node, hintName);

				if (_receivers.Count > 0)
				{
					lock (_receivers)
					{
						foreach (Entry entry in _receivers)
						{
							if (entry.DiagnosticReceiver is INodeDiagnosticReceiver d)
							{
								d.SetTargetNode(node!, hintName!);
							}
						}
					}
				}
			}

			/// <summary>
			/// Changes the method that reports diagnostics for the specified <paramref name="diagnosticReceiver"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to switch the target report method for.</param>
			/// <param name="push">
			/// Determines when diagnostics should be reported to the specified <paramref name="diagnosticReceiver"/>:
			/// <see langword="false"/> for when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called,
			/// <see langword="true"/> for when the <see cref="Push"/> method is called.
			/// </param>
			/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
			/// <exception cref="ArgumentException"><paramref name="diagnosticReceiver"/> is not present in the current receiver.</exception>
			public void SwitchPush(IDiagnosticReceiver diagnosticReceiver, bool push)
			{
				if (diagnosticReceiver is null)
				{
					throw new ArgumentNullException(nameof(diagnosticReceiver));
				}

				if (_receivers.Count > 0)
				{
					lock (_receivers)
					{
						for (int i = 0; i < _receivers.Count; i++)
						{
							Entry entry = _receivers[i];

							if (entry.DiagnosticReceiver.Equals(diagnosticReceiver))
							{
								entry = new Entry(diagnosticReceiver, push);
								_receivers[i] = entry;
								return;
							}
						}
					}
				}

				throw new ArgumentException("Target is not present in the current receiver", nameof(diagnosticReceiver));
			}

			/// <inheritdoc/>
			protected override void ReportDiagnostics(Diagnostic[] diagnostics)
			{
				if (_receivers.Count == 0)
				{
					return;
				}

				foreach (IDiagnosticReceiver diagnosticReceiver in GetDiagnosticReceivers(true))
				{
					foreach (Diagnostic diagnostic in diagnostics)
					{
						diagnosticReceiver.ReportDiagnostic(diagnostic);
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
