//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GenerateModuleRepository project.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace Durian.Info
{
	/// <summary>
	/// Factory class of <see cref="TypeIdentity"/>s for all available Durian <see cref="Type"/>s.
	/// </summary>
	public static class TypeRepository
	{
		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Generator.EnableModuleAttribute</c> type.
		/// </summary>
		public static TypeIdentity EnableModuleAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("EnableModuleAttribute", out TypeIdentity type))
				{
					type = new(
						name: "EnableModuleAttribute",
						@namespace: "Durian.Generator",
						modules: new DurianModule[]
						{
							DurianModule.Core,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Generator.DurianGeneratedAttribute</c> type.
		/// </summary>
		public static TypeIdentity DurianGeneratedAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("DurianGeneratedAttribute", out TypeIdentity type))
				{
					type = new(
						name: "DurianGeneratedAttribute",
						@namespace: "Durian.Generator",
						modules: new DurianModule[]
						{
							DurianModule.Core,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.DefaultParamAttribute</c> type.
		/// </summary>
		public static TypeIdentity DefaultParamAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("DefaultParamAttribute", out TypeIdentity type))
				{
					type = new(
						name: "DefaultParamAttribute",
						@namespace: "Durian",
						modules: new DurianModule[]
						{
							DurianModule.DefaultParam,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Configuration.DefaultParamConfigurationAttribute</c> type.
		/// </summary>
		public static TypeIdentity DefaultParamConfigurationAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("DefaultParamConfigurationAttribute", out TypeIdentity type))
				{
					type = new(
						name: "DefaultParamConfigurationAttribute",
						@namespace: "Durian.Configuration",
						modules: new DurianModule[]
						{
							DurianModule.DefaultParam,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> type.
		/// </summary>
		public static TypeIdentity DefaultParamScopedConfigurationAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("DefaultParamScopedConfigurationAttribute", out TypeIdentity type))
				{
					type = new(
						name: "DefaultParamScopedConfigurationAttribute",
						@namespace: "Durian.Configuration",
						modules: new DurianModule[]
						{
							DurianModule.DefaultParam,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Configuration.DPMethodConvention</c> type.
		/// </summary>
		public static TypeIdentity DPMethodConvention
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("DPMethodConvention", out TypeIdentity type))
				{
					type = new(
						name: "DPMethodConvention",
						@namespace: "Durian.Configuration",
						modules: new DurianModule[]
						{
							DurianModule.DefaultParam,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Configuration.DPTypeConvention</c> type.
		/// </summary>
		public static TypeIdentity DPTypeConvention
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("DPTypeConvention", out TypeIdentity type))
				{
					type = new(
						name: "DPTypeConvention",
						@namespace: "Durian.Configuration",
						modules: new DurianModule[]
						{
							DurianModule.DefaultParam,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.FriendClassAttribute</c> type.
		/// </summary>
		public static TypeIdentity FriendClassAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("FriendClassAttribute", out TypeIdentity type))
				{
					type = new(
						name: "FriendClassAttribute",
						@namespace: "Durian",
						modules: new DurianModule[]
						{
							DurianModule.FriendClass,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.Configuration.FriendClassConfigurationAttribute</c> type.
		/// </summary>
		public static TypeIdentity FriendClassConfigurationAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("FriendClassConfigurationAttribute", out TypeIdentity type))
				{
					type = new(
						name: "FriendClassConfigurationAttribute",
						@namespace: "Durian.Configuration",
						modules: new DurianModule[]
						{
							DurianModule.FriendClass,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.InterfaceTargets</c> type.
		/// </summary>
		public static TypeIdentity InterfaceTargets
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("InterfaceTargets", out TypeIdentity type))
				{
					type = new(
						name: "InterfaceTargets",
						@namespace: "Durian",
						modules: new DurianModule[]
						{
							DurianModule.InterfaceTargets,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.InterfaceTargetsAttribute</c> type.
		/// </summary>
		public static TypeIdentity InterfaceTargetsAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("InterfaceTargetsAttribute", out TypeIdentity type))
				{
					type = new(
						name: "InterfaceTargetsAttribute",
						@namespace: "Durian",
						modules: new DurianModule[]
						{
							DurianModule.InterfaceTargets,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.CopyFromTypeAttribute</c> type.
		/// </summary>
		public static TypeIdentity CopyFromTypeAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("CopyFromTypeAttribute", out TypeIdentity type))
				{
					type = new(
						name: "CopyFromTypeAttribute",
						@namespace: "Durian",
						modules: new DurianModule[]
						{
							DurianModule.CopyFrom,
						}
					);
				}

				return type;
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> for the <c>Durian.CopyFromMethodAttribute</c> type.
		/// </summary>
		public static TypeIdentity CopyFromMethodAttribute
		{
			get
			{
				if(!IdentityPool.Types.TryGetValue("CopyFromMethodAttribute", out TypeIdentity type))
				{
					type = new(
						name: "CopyFromMethodAttribute",
						@namespace: "Durian",
						modules: new DurianModule[]
						{
							DurianModule.CopyFrom,
						}
					);
				}

				return type;
			}
		}
	}
}