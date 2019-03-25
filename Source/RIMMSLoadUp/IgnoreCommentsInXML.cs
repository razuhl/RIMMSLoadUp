/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 25.03.2019
 * Time: 15:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using Harmony;
using Verse;

namespace RIMMSLoadUp
{
	//[HarmonyPatch(typeof(LoadableXmlAsset))]
	//[HarmonyPatch(MethodType.Constructor)]
	//[HarmonyPatch(new Type[] {typeof(string),typeof(string),typeof(string)})]
	static class IgnoreCommentsInXML
	{
		static bool Prefix(LoadableXmlAsset __instance, string name, string fullFolderPath, string contents)
		{
			__instance.name = name;
			__instance.fullFolderPath = fullFolderPath;
			try {
				using(XmlReader r = XmlReader.Create(new StringReader(contents),new XmlReaderSettings(){IgnoreComments = true, IgnoreWhitespace = true})) {
					__instance.xmlDoc = new XmlDocument();
					__instance.xmlDoc.Load(r);
				}
			} catch (Exception ex) {
				Log.Warning(string.Concat(new object[]
				{
					"Exception reading ",
					name,
					" as XML: ",
					ex
				}), false);
				__instance.xmlDoc = null;
			}
			return false;
		}
	}
	
	//[HarmonyPatch(typeof(ScribeLoader))]
	//[HarmonyPatch("InitLoading")]
	static class IgnoreCommentsInXMLFromScribeLoader {
		static bool Prefix(ScribeLoader __instance, string filePath) {
			if (Scribe.mode != LoadSaveMode.Inactive)
			{
				Log.Error("Called InitLoading() but current mode is " + Scribe.mode, false);
				Scribe.ForceStop();
			}
			if (__instance.curParent != null)
			{
				Log.Error("Current parent is not null in InitLoading", false);
				__instance.curParent = null;
			}
			if (__instance.curPathRelToParent != null)
			{
				Log.Error("Current path relative to parent is not null in InitLoading", false);
				__instance.curPathRelToParent = null;
			}
			try
			{
				using (StreamReader streamReader = new StreamReader(filePath))
				{
					using(XmlReader r = XmlReader.Create(streamReader,new XmlReaderSettings(){IgnoreComments = true, IgnoreWhitespace = true})) {
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(r);
						__instance.curXmlParent = xmlDocument.DocumentElement;
					}
				}
				Scribe.mode = LoadSaveMode.LoadingVars;
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception while init loading file: ",
					filePath,
					"\n",
					ex
				}), false);
				__instance.ForceStop();
				throw;
			}
			
			return false;
		}
	}
}
