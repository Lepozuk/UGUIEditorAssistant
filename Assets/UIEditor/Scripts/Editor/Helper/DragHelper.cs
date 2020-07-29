using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    public class DragHelper
    {
        
        public void Init()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            //当松开鼠标时
            if (Event.current.type != EventType.DragPerform || DragAndDrop.objectReferences.Length != 1) return;
            
            DragAndDrop.AcceptDrag();
            if (HandleDragAsset(sceneView, DragAndDrop.objectReferences[0]))
            {
                Event.current.Use();
            }
        }
        
        private bool HandleDragAsset(SceneView sceneView, Object handleObj)
        {
            if (handleObj == null)
            {
                return false;
            }
            
            var e = Event.current;
            var cam = sceneView.camera;
            Vector3 mouse_abs_pos = e.mousePosition;
            mouse_abs_pos.y = cam.pixelHeight - mouse_abs_pos.y;
            mouse_abs_pos = sceneView.camera.ScreenToWorldPoint(mouse_abs_pos);

            
            if (PrefabUtility.GetPrefabAssetType(handleObj) == PrefabAssetType.NotAPrefab)
            {
                return false;
            }
            
            var prefabObj = handleObj as GameObject;
            if (prefabObj != null || !prefabObj.TryGetComponent<RectTransform>(out var rectTrans))
            {
                Transform parentTrans = GetContainerUnderMouse(mouse_abs_pos);
                
                if (parentTrans == null)
                {
                    sceneView.ShowNotification(new GUIContent("请确保当前场景内存在Canvas对象"));
                    return true;
                }

                var newObj = PrefabUtility.InstantiatePrefab(prefabObj, parentTrans) as GameObject;
                newObj.transform.position = mouse_abs_pos;
                
                //生成唯一的节点名字
                newObj.name = GenerateUniqueName(parentTrans.gameObject, handleObj.name);
                
                mouse_abs_pos.z = parentTrans.position.z;
                newObj.transform.position = mouse_abs_pos;
                
                newObj.transform.localScale = Vector3.one;
                Selection.activeGameObject = newObj;
                return true;
            }

            return false;
        }
        
        //生成parent下的唯一控件名
        private string GenerateUniqueName(GameObject parent, string type)
        {
            var widgets = parent.GetComponentsInChildren<RectTransform>();
            var test_num = 1;
            var test_name = type+"_"+test_num;
            RectTransform uiBase = null;
            var prevent_death_count = 0;//防止死循环
            do {
                test_name = type+"_"+test_num;
                uiBase = System.Array.Find(widgets, p => p.gameObject.name==test_name);
                test_num = test_num + UnityEngine.Random.Range(1, (prevent_death_count+1)*2);
                if (prevent_death_count++ >= 100)
                    break;
            } while (uiBase != null);
        
            return test_name;
        }
        
        private Transform GetContainerUnderMouse(Vector3 mouse_abs_pos, GameObject ignore_obj = null)
        {
            var act = Selection.activeObject as GameObject;
            if (act != null && act.TryGetComponent<RectTransform>(out var rectTrans))
            {
                return rectTrans;
            }
            
            var list = new List<RectTransform>();
            var containers = Transform.FindObjectsOfType<Canvas>();
            var corners = new Vector3[4];
            foreach (var item in containers)
            {
                if (ignore_obj == item.gameObject )
                    continue;
                var trans = item.transform as RectTransform;
                if (trans != null)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    trans.GetWorldCorners(corners);
                    if (mouse_abs_pos.x >= corners[0].x && mouse_abs_pos.y <= corners[1].y && mouse_abs_pos.x <= corners[2].x && mouse_abs_pos.y >= corners[3].y)
                    {
                        list.Add(trans);
                    }
                }
            }
            
            if (list.Count <= 0) return null;
            
            list.Sort((a, b) => { return (a.GetSiblingIndex() == b.GetSiblingIndex()) ? 0 : ((a.GetSiblingIndex() < b.GetSiblingIndex()) ? 1 : -1); });
            
            return GetRootLayout(list[0]);
        }

        private Transform GetRootLayout(Transform trans)
        {
            Transform result = null;
            var canvas = trans.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                foreach (var item in canvas.transform.GetComponentsInChildren<RectTransform>())
                {
                    if (canvas.isRootCanvas)
                    {
                        result = item;
                        break;
                    }
                }
            }
            return result;
        }
    }
}