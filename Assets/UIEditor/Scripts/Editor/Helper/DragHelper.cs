using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    public static class DragHelper
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            //当松开鼠标时
            if (Event.current.type == EventType.DragPerform && DragAndDrop.objectReferences.Length > 0) {
                DragAndDrop.AcceptDrag();
                foreach (var item in DragAndDrop.objectReferences)
                {
                    HandleDragAsset(sceneView, item);
                }
                Event.current.Use();
            }
        }
        
        static void HandleDragAsset(SceneView sceneView, Object handleObj)
        {
            Event e = Event.current;
            Camera cam = sceneView.camera;
            Vector3 mouse_abs_pos = e.mousePosition;
            mouse_abs_pos.y = cam.pixelHeight - mouse_abs_pos.y;
            mouse_abs_pos = sceneView.camera.ScreenToWorldPoint(mouse_abs_pos);

            GameObject new_obj = GameObject.Instantiate(handleObj) as GameObject;
            if (new_obj != null)
            {
                Undo.RegisterCreatedObjectUndo(new_obj, "create obj on drag prefab");
                new_obj.transform.position = mouse_abs_pos;
                GameObject ignore_obj = new_obj;

                Transform container_trans = GetContainerUnderMouse(mouse_abs_pos, ignore_obj);
                
                if (container_trans == null)
                {
                    sceneView.ShowNotification(new GUIContent("请确保当前场景内存在Canvas对象"));
                    GameObject.DestroyImmediate(new_obj);
                    return;
                }

                new_obj.transform.SetParent(container_trans);
                mouse_abs_pos.z = container_trans.position.z;
                
                new_obj.transform.position = mouse_abs_pos;
                new_obj.transform.localScale = Vector3.one;
                Selection.activeGameObject = new_obj;
                
                //生成唯一的节点名字
                new_obj.name = GenerateUniqueName(container_trans.gameObject, handleObj.name);
            }
        }
        
        //生成parent下的唯一控件名
        public static string GenerateUniqueName(GameObject parent, string type)
        {
            var widgets = parent.GetComponentsInChildren<RectTransform>();
            int test_num = 1;
            string test_name = type+"_"+test_num;
            RectTransform uiBase = null;
            int prevent_death_count = 0;//防止死循环
            do {
                test_name = type+"_"+test_num;
                uiBase = System.Array.Find(widgets, p => p.gameObject.name==test_name);
                test_num = test_num + UnityEngine.Random.Range(1, (prevent_death_count+1)*2);
                if (prevent_death_count++ >= 100)
                    break;
            } while (uiBase != null);
        
            return test_name;
        }
        
        static public Transform GetContainerUnderMouse(Vector3 mouse_abs_pos, GameObject ignore_obj = null)
        {
            
            List<RectTransform> list = new List<RectTransform>();
            Canvas[] containers = Transform.FindObjectsOfType<Canvas>();
            Vector3[] corners = new Vector3[4];
            foreach (var item in containers)
            {
                if (ignore_obj == item.gameObject )
                    continue;
                RectTransform trans = item.transform as RectTransform;
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

        public static Transform GetRootLayout(Transform trans)
        {
            Transform result = null;
            Canvas canvas = trans.GetComponentInParent<Canvas>();
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