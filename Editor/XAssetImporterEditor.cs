using System;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Flexy.Core.Editor
{
    [CustomEditor( typeof(XAssetImporter), true), CanEditMultipleObjects]
    public class XAssetImporterEditor : ScriptedImporterEditor
    {
        //Remove mostly all importer UI from inspector. may be we can do better there
		
        protected override Boolean needsApplyRevert => false;

        public override void OnInspectorGUI() { }
        protected override void OnHeaderGUI() { }

        public override Boolean UseDefaultMargins() => false;
    }
}
