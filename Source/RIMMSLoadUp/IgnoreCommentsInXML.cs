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
	[HarmonyPatch(typeof(LoadableXmlAsset))]
	[HarmonyPatch(MethodType.Constructor)]
	[HarmonyPatch(new Type[] {typeof(string),typeof(string),typeof(string)})]
	static class IgnoreCommentsInXML
	{
		[HarmonyPriority(Priority.Last)]
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
}
