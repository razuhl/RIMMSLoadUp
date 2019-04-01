/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 21.03.2019
 * Time: 17:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Harmony;
using Verse;

namespace RIMMSLoadUp
{
	[HarmonyPatch(typeof(Verse.LoadedModManager))]
	[HarmonyPatch("ApplyPatches")]
	static class ApplyPatchesPatch {
		public static bool? foundLoadOnDemandAssembly;
		
		public static bool FoundConflictingAssembly {
			get {
				//If the LoadOnDemand assembly appears we skip these patches. LoadOnDemand is marking itself as "do not loop" after it cedes control to other code, causing infinite loops.
				if ( foundLoadOnDemandAssembly == null ) {
					foundLoadOnDemandAssembly = false;
					foreach (ModContentPack mod in LoadedModManager.RunningMods) {
						if ( mod.assemblies.loadedAssemblies.Find(ass=>ass.GetName().Name == "LoadOnDemand") != null ) {
							Log.Message("Skipping RIMMSLoadUp.EagerPatchCleanup --- found LoadOnDemand assembly");
							foundLoadOnDemandAssembly = true;
							break;
						}
					}
				}
				return foundLoadOnDemandAssembly.Value;
			}
		}
		
		[HarmonyPriority(Priority.Last)]
		static bool Prefix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup) {
			if ( FoundConflictingAssembly ) {
				return true;
			}
			
			foreach (ModContentPack mcp in LoadedModManager.RunningMods) {
				foreach ( PatchOperation po in mcp.Patches ) {
					try {
						po.Apply(xmlDoc);
					} catch (Exception arg) {
						Log.Error("Error in patch.Apply(): " + arg, false);
					}
				}
				foreach ( PatchOperation po in mcp.Patches ) {
					try {
						po.Complete(mcp.Name);
					} catch (Exception arg) {
						Log.Error("Error in patch.Complete(): " + arg, false);
					}
				}
				mcp.ClearPatchesCache();
				//preventing other code to trigger the reloading of patch information
				mcp.GetType().GetField("patches",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).SetValue(mcp,new List<PatchOperation>());
			}
			
			return false;
		}
	}
	
	[HarmonyPatch(typeof(Verse.LoadedModManager))]
	[HarmonyPatch("ClearCachedPatches")]
	static class ClearCachedPatchesPatch {
		[HarmonyPriority(Priority.Last)]
		static bool Prefix() {
			return ApplyPatchesPatch.FoundConflictingAssembly;
		}
	}
}