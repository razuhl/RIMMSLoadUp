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
using System.Xml;
using Harmony;
using Verse;

namespace RIMMSLoadUp
{
	/// <summary>
	/// Description of EagerPatchCleanup.
	/// </summary>
	[HarmonyPatch(typeof(Verse.LoadedModManager))]
	[HarmonyPatch("ApplyPatches")]
	static class ApplyPatchesPatch {
		static bool Prefix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup) {
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
			}
			
			return false;
		}
	}
	
	[HarmonyPatch(typeof(Verse.LoadedModManager))]
	[HarmonyPatch("ClearCachedPatches")]
	static class ClearCachedPatchesPatch {
		static bool Prefix() {
			return false;
		}
	}
}