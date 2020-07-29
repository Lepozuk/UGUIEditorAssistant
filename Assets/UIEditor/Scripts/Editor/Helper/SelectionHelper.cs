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
            var hasChanged = false;
            var list = new List<GameObject>();
            foreach (var obj in Selection.gameObjects)
            {
                if (CheckIsUIPrefab(obj, out var prefabObj))
                {
                    if (prefabObj != obj)
                    {
                        hasChanged = true;
                    }

                    if (list.IndexOf(prefabObj) < 0)
                    {
                        list.Add(prefabObj);
                    }
                }
                else
                {
                    list.Add(obj);
                }
            }

            if (hasChanged)
            {
                Selection.objects = list.ToArray();
            }
            
        }

        private bool CheckIsUIPrefab(GameObject obj, out GameObject prefabObj)
        {
            prefabObj = null;
            if (obj == null)
            {
                return false;
            }
            if (!obj.TryGetComponent<RectTransform>(out var rectTrans))
            {
                return false;
            }

            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab)
            {
                return false;
            }
            prefabObj = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (prefabObj.TryGetComponent<Canvas>(out var canvas))
            {
                if (canvas.isRootCanvas)
                {
                    return false;
                }
            }
            
            return true;
        }
        
    }
}
