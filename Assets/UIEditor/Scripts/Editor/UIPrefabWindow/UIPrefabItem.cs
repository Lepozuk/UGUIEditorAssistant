using UnityEngine;

namespace Editor.UIEditor.PrefabWindow
{
    internal class UIPrefabItem
    {
        public GameObject Prefab;
        public string Guid;
        public Texture PreviewTex;
        public bool IsDynamicTex = false;
    }
}