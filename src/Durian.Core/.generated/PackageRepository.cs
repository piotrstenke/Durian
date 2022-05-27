//------------------------------------------------------------------------------
// <auto-generated>
//	 This code was generated by the GenerateModuleRepository project.
//
//	 Changes to this file may cause incorrect behavior and will be lost if
//	 the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Durian.Info
{
	/// <summary>
	/// Factory class of <see cref="PackageIdentity"/>s for all available Durian packages.
	/// </summary>
	public static class PackageRepository
	{
		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.Main"/> package.
		/// </summary>
		public static PackageIdentity Main
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("Main", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.Main,
						version: "3.0.0",
						type: PackageType.Unspecified,
						modules: new DurianModule[]
						{
							DurianModule.Core
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.Core"/> package.
		/// </summary>
		public static PackageIdentity Core
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("Core", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.Core,
						version: "3.0.0",
						type: PackageType.Library,
						modules: new DurianModule[]
						{
							DurianModule.Core
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.CoreAnalyzer"/> package.
		/// </summary>
		public static PackageIdentity CoreAnalyzer
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("CoreAnalyzer", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.CoreAnalyzer,
						version: "3.0.0",
						type: PackageType.Analyzer,
						modules: new DurianModule[]
						{
							DurianModule.Core
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.AnalysisServices"/> package.
		/// </summary>
		public static PackageIdentity AnalysisServices
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("AnalysisServices", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.AnalysisServices,
						version: "3.0.0",
						type: PackageType.Library,
						modules: new DurianModule[]
						{
							DurianModule.Development
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.TestServices"/> package.
		/// </summary>
		public static PackageIdentity TestServices
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("TestServices", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.TestServices,
						version: "3.0.0",
						type: PackageType.Library,
						modules: new DurianModule[]
						{
							DurianModule.Development
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.DefaultParam"/> package.
		/// </summary>
		public static PackageIdentity DefaultParam
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("DefaultParam", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.DefaultParam,
						version: "3.0.0",
						type: PackageType.Analyzer | PackageType.CodeFixLibrary | PackageType.StaticGenerator | PackageType.SyntaxBasedGenerator,
						modules: new DurianModule[]
						{
							DurianModule.DefaultParam
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.FriendClass"/> package.
		/// </summary>
		public static PackageIdentity FriendClass
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("FriendClass", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.FriendClass,
						version: "2.0.0",
						type: PackageType.Analyzer | PackageType.CodeFixLibrary | PackageType.StaticGenerator,
						modules: new DurianModule[]
						{
							DurianModule.FriendClass
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.InterfaceTargets"/> package.
		/// </summary>
		public static PackageIdentity InterfaceTargets
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("InterfaceTargets", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.InterfaceTargets,
						version: "2.0.0",
						type: PackageType.Analyzer | PackageType.StaticGenerator,
						modules: new DurianModule[]
						{
							DurianModule.InterfaceTargets
						}
					);
				}

				return package;
			}
		}

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.CopyFrom"/> package.
		/// </summary>
		public static PackageIdentity CopyFrom
		{
			get
			{
				if(!IdentityPool.Packages.TryGetValue("CopyFrom", out PackageIdentity package))
				{
					package = new(
						enumValue: DurianPackage.CopyFrom,
						version: "1.0.0",
						type: PackageType.Analyzer | PackageType.StaticGenerator | PackageType.SyntaxBasedGenerator | PackageType.CodeFixLibrary,
						modules: new DurianModule[]
						{
							DurianModule.CopyFrom
						}
					);
				}

				return package;
			}
		}
	}
}