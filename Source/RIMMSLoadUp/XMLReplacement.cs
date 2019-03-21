/*
 * Created by SharpDevelop.
 * User: Malte Schulze
 * Date: 20.03.2019
 * Time: 16:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Harmony;
using Verse;

namespace RIMMSLoadUp
{
	[HarmonyPatch(typeof(Verse.XmlInheritance))]
	[HarmonyPatch("TryRegisterAllFrom")]
	static class TryRegisterAllFromPatch {
		static bool Prefix(LoadableXmlAsset xmlAsset, ModContentPack mod) {
			if (xmlAsset.xmlDoc == null) {
				return false;
			}
			XmlNodeList childNodes = xmlAsset.xmlDoc.DocumentElement.ChildNodes;
			foreach ( XmlNode child in childNodes ) {
				if (child.NodeType == XmlNodeType.Element) {
					XmlInheritance.TryRegister(child, mod);
				}
			}
			return false;
		}
		
		/*static long timeVanilla = 0, timeModded = 0;
		
		static bool PrefixTest(LoadableXmlAsset xmlAsset, ModContentPack mod) {
			if (xmlAsset.xmlDoc == null) {
				return false;
			}
			
			Stopwatch sw = Stopwatch.StartNew();
			XmlNodeList childNodes1 = xmlAsset.xmlDoc.DocumentElement.ChildNodes;
			for (int i = 0; i < childNodes1.Count; i++)
			{
				if (childNodes1[i].NodeType == XmlNodeType.Element)
				{
					XmlInheritance.TryRegister(childNodes1[i], mod);
				}
			}
			sw.Stop();
			timeVanilla += sw.ElapsedTicks;
			
			sw.Reset(); sw.Start();
			XmlNodeList childNodes2 = xmlAsset.xmlDoc.DocumentElement.ChildNodes;
			foreach ( XmlNode child in childNodes2 ) {
				if (child.NodeType == XmlNodeType.Element) {
					XmlInheritance.TryRegister(child, mod);
				}
			}
			sw.Stop();
			timeModded += sw.ElapsedTicks;
			
			Log.Message("Time vanilla "+timeVanilla+" timeModded "+timeModded);
			
			IEnumerator e = childNodes2.GetEnumerator();
			for (int i = 0; i < childNodes1.Count; i++)
			{
				e.MoveNext();
				if ( childNodes1[i] != e.Current )
				{
					Log.Error("Difference in processed node!");
				}
			}
			
			return false;
		}*/
	}
	
	[HarmonyPatch(typeof(Verse.LoadedModManager))]
	[HarmonyPatch("ParseAndProcessXML")]
	static class ParseAndProcessXMLPatch {
		static bool Prefix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup) {
			XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
			foreach ( XmlNode n in childNodes )
			{
				if (n.NodeType == XmlNodeType.Element)
				{
					LoadableXmlAsset loadableXmlAsset = assetlookup.TryGetValue(n, null);
					XmlInheritance.TryRegister(n, (loadableXmlAsset == null) ? null : loadableXmlAsset.mod);
				}
			}
			XmlInheritance.Resolve();
			DefPackage defPackage = new DefPackage("Unknown", string.Empty); 
			ModContentPack modContentPack = LoadedModManager.RunningMods.FirstOrFallback<ModContentPack>();
			modContentPack.AddDefPackage(defPackage);
			IEnumerator enumerator = xmlDoc.DocumentElement.ChildNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					XmlNode xmlNode = (XmlNode)enumerator.Current;
					LoadableXmlAsset loadableXmlAsset2 = assetlookup.TryGetValue(xmlNode, null);
					DefPackage defPackage2 = (loadableXmlAsset2 == null) ? defPackage : loadableXmlAsset2.defPackage;
					Def def = DirectXmlLoader.DefFromNode(xmlNode, loadableXmlAsset2);
					if (def != null)
					{
						def.modContentPack = ((loadableXmlAsset2 == null) ? modContentPack : loadableXmlAsset2.mod);
						defPackage2.AddDef(def);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			return false;
		}
	}
	
	[HarmonyPatch(typeof(Verse.ModContentPack))]
	[HarmonyPatch("LoadPatches")]
	static class LoadPatchesPatch {
		static bool Prefix(Verse.ModContentPack __instance) {
			DeepProfiler.Start("Loading all patches");
			List<PatchOperation> lst = new List<PatchOperation>();
			typeof(ModContentPack).GetField("patches", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance)
				.SetValue(__instance,lst);
			List<LoadableXmlAsset> list = DirectXmlLoader.XmlAssetsInModFolder(__instance, "Patches/").ToList<LoadableXmlAsset>();
			for (int i = 0; i < list.Count; i++)
			{
				XmlElement documentElement = list[i].xmlDoc.DocumentElement;
				if (documentElement.Name != "Patch")
				{
					Log.Error(string.Format("Unexpected document element in patch XML; got {0}, expected 'Patch'", documentElement.Name), false);
				}
				else
				{
					for (int j = 0; j < documentElement.ChildNodes.Count; j++)
					{
						XmlNode xmlNode = documentElement.ChildNodes[j];
						if (xmlNode.NodeType == XmlNodeType.Element)
						{
							if (xmlNode.Name != "Operation")
							{
								Log.Error(string.Format("Unexpected element in patch XML; got {0}, expected 'Operation'", documentElement.ChildNodes[j].Name), false);
							}
							else
							{
								PatchOperation patchOperation = DirectXmlToObject.ObjectFromXml<PatchOperation>(xmlNode, false);
								patchOperation.sourceFile = list[i].FullFilePath;
								lst.Add(patchOperation);
							}
						}
					}
				}
			}
			DeepProfiler.End();
			
			return false;
		}
	}
	
	// Verse.PatchOperationAdd
	//protected override bool ApplyWorker(XmlDocument xml)
	
	// Verse.PatchOperationAddModExtension
	//protected override bool ApplyWorker(XmlDocument xml)
	
	// Verse.PatchOperationInsert
	//protected override bool ApplyWorker(XmlDocument xml)
	
	// Verse.PatchOperationReplace
	//protected override bool ApplyWorker(XmlDocument xml)
	
	[HarmonyPatch(typeof(Verse.XmlInheritance))]
	[HarmonyPatch("RecursiveNodeCopyOverwriteElements")]
	static class RecursiveNodeCopyOverwriteElementsPatch {
		static bool Prefix(XmlNode child, XmlNode current) {
			XmlAttribute xmlAttribute = child.Attributes["Inherit"];
			if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "false")
			{
				while (current.HasChildNodes)
				{
					current.RemoveChild(current.FirstChild);
				}
				IEnumerator enumerator = child.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						XmlNode node = (XmlNode)enumerator.Current;
						XmlNode newChild = current.OwnerDocument.ImportNode(node, true);
						current.AppendChild(newChild);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				return false;
			}
			current.Attributes.RemoveAll();
			XmlAttributeCollection attributes = child.Attributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				XmlAttribute node2 = (XmlAttribute)current.OwnerDocument.ImportNode(attributes[i], true);
				current.Attributes.Append(node2);
			}
			List<XmlElement> list = new List<XmlElement>();
			XmlNode xmlNode = null;
			IEnumerator enumerator2 = child.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
					if (xmlNode2.NodeType == XmlNodeType.Text)
					{
						xmlNode = xmlNode2;
					}
					else if (xmlNode2.NodeType == XmlNodeType.Element)
					{
						list.Add((XmlElement)xmlNode2);
					}
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = (enumerator2 as IDisposable)) != null)
				{
					disposable2.Dispose();
				}
			}
			if (xmlNode != null)
			{
				List<XmlNode> lst = current.ChildNodes.Cast<XmlNode>().ToList();
				foreach ( XmlNode n in lst ) {
					if (n.NodeType != XmlNodeType.Attribute) {
						current.RemoveChild(n);
					}
				}
				XmlNode newChild2 = current.OwnerDocument.ImportNode(xmlNode, true);
				current.AppendChild(newChild2);
			}
			else if (!list.Any<XmlElement>())
			{
				bool flag = false;
				IEnumerator enumerator3 = current.ChildNodes.GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						XmlNode xmlNode4 = (XmlNode)enumerator3.Current;
						if (xmlNode4.NodeType == XmlNodeType.Element)
						{
							flag = true;
							break;
						}
					}
				}
				finally
				{
					IDisposable disposable3;
					if ((disposable3 = (enumerator3 as IDisposable)) != null)
					{
						disposable3.Dispose();
					}
				}
				if (!flag)
				{
					IEnumerator enumerator4 = current.ChildNodes.GetEnumerator();
					try
					{
						while (enumerator4.MoveNext())
						{
							XmlNode xmlNode5 = (XmlNode)enumerator4.Current;
							if (xmlNode5.NodeType != XmlNodeType.Attribute)
							{
								current.RemoveChild(xmlNode5);
							}
						}
					}
					finally
					{
						IDisposable disposable4;
						if ((disposable4 = (enumerator4 as IDisposable)) != null)
						{
							disposable4.Dispose();
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < list.Count; k++)
				{
					XmlElement xmlElement = list[k];
					if (xmlElement.Name == "li")
					{
						XmlNode newChild3 = current.OwnerDocument.ImportNode(xmlElement, true);
						current.AppendChild(newChild3);
					}
					else
					{
						XmlElement xmlElement2 = current[xmlElement.Name];
						if (xmlElement2 != null)
						{
							typeof(Verse.XmlInheritance).GetMethod("RecursiveNodeCopyOverwriteElements", 
                                 System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)
								.Invoke(null,new Object[]{xmlElement, xmlElement2});
						}
						else
						{
							XmlNode newChild4 = current.OwnerDocument.ImportNode(xmlElement, true);
							current.AppendChild(newChild4);
						}
					}
				}
			}
			
			return false;
		}
	}
}
