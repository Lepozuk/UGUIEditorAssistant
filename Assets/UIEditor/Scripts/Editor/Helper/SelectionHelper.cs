using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor.UIEditor
{
    
    public class SelectionHelper
    {
        public void Init()
        {
            CreateSelectionHelper();
        }
        
        private void CreateSelectionHelper()
        {
            var obj = GameObject.Find("UIEditorSelectionHelper");
            if(obj == null) {
                obj = new GameObject("UIEditorSelectionHelper");
            }

            if (!obj.TryGetComponent(typeof(UISelectionHelperMono), out var comp))
            {
                obj.AddComponent<UISelectionHelperMono>();
            }

            obj.hideFlags = HideFlags.DontSaveInEditor;
        }
        
    }
    
    [ExecuteInEditMode]
    class UISelectionHelperMono : MonoBehaviour
    {
        private void Update()
        {
            
            var list = new List<GameObject>();
            foreach (var obj in Selection.gameObjects)
            {
                if (CheckIsUIPrefab(obj))
                {
                    var prefabObj = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                    list.Add(prefabObj);
                }
                else
                {
                    list.Add(obj);
                }
            }

            Selection.objects = list.ToArray();
            
        }

        private bool CheckIsUIPrefab(GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!obj.TryGetComponent<RectTransform>(out var rectTrans) || obj.TryGetComponent<Canvas>(out var canvas))
            {
                return false;
            }

            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab)
            {
                return false;
            }

            return true;
        }
        
    }
}
