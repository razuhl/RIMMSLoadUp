/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 25.03.2019
 * Time: 14:18
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Harmony;
using Verse;

namespace RIMMSLoadUp
{
	//[HarmonyPatch(typeof(Verse.LoadedModManager))]
	//[HarmonyPatch("ParseAndProcessXML")]
	static class DumpCombinedXML
	{
		static void Postfix(XmlDocument xmlDoc)
		{
			SaveXMLToFile("combinedXml.xml", xmlDoc);
		}
		
		static void SaveXMLToFile(string fileName, XmlDocument xml) {
			FileInfo file = new FileInfo(Path.Combine(GenFilePaths.SaveDataFolderPath, fileName));
			try {
				using (FileStream fs = file.OpenWrite()) 
				using (var writer = XmlWriter.Create(fs, new XmlWriterSettings{Indent = false,OmitXmlDeclaration = true,NewLineHandling = NewLineHandling.Replace}))
				{
					xml.WriteTo(writer);
				}
			} catch (Exception e) {
				Log.Error("Failed to save xml to file \""+file.FullName+"\"! " + e);
			}
		}
	}
}
