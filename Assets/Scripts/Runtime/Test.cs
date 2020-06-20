using System;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Runtime
{
    public class Test : MonoBehaviour
    {
        [ValueDropdown("GetSystemLayers")]
        public string layer = SortingLayer.GetLayerValueFromName("Default").ToString();

        private string[] GetSystemLayers()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }
        
        [Button("Open Tag Manager")]
        public void DoSomething()
        {
            SettingsService.OpenProjectSettings("Project/Tags and Layers");
        }
    }
}