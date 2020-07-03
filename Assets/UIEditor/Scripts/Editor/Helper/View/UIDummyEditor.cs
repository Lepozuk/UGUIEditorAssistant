
#if UNITY_EDITOR

using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;

using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    [CustomEditor(typeof(UIDummy))]
    public class UIDummyEditor : OdinEditor
    {
        
        private static bool IsPrefab(GameObject go)
        {
            var correspondingObject = PrefabUtility.GetCorrespondingObjectFromSource(go);
            var instanceHandle = PrefabUtility.GetPrefabInstanceHandle(go);
            return correspondingObject != null && instanceHandle != null;
        }

        [CanBeNull]
        public UIDummy Dummy
        {
            get
            { 
                var mono = target as UIDummy;
                if (mono == null || IsPrefab(mono.gameObject))
                {
                    return null;
                }

                return mono;
            } 
        }

        private readonly GUIContent DummyLabel = new GUIContent("锚点列表");
        
        public override void OnInspectorGUI()
        {
             if (Dummy == null)
             {
                 return;
             }
             
             InspectorUtilities.BeginDrawPropertyTree(Tree, true);

             var property = Tree.GetPropertyAtPath("Dummys");
             if (property != null)
             {
                 property.Draw(DummyLabel);
             }
             
             InspectorUtilities.EndDrawPropertyTree(Tree);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawDummys(UIDummy component, GizmoType gizmoType)
        {
            var gameObject = component?.gameObject;
            if (gameObject == null) return;
            

            if (!(UIUtility.TryGetRootCanvas(gameObject, out var canvas) && UIUtility.TryGetRectTransform(gameObject,out var rect)))
            {
                return;
            }

            var canvasRect = UIUtility.GetCanvasRect(rect);
            
            var center = canvasRect.Center;
            var mat4x4 = canvas.transform.localToWorldMatrix;
            
            foreach (var dummy in component.Dummys)
            {
                Vector3 offset = dummy.Offset;
                DrawDummyCross(mat4x4, center + offset, dummy.AliasName, Color.red);
            }
        }

        private static void DrawDummyCross(Matrix4x4 mat4x4, Vector3 position, string label, Color color)
        {
            var oldMat = Gizmos.matrix;
            Gizmos.matrix = mat4x4;
            Gizmos.color = color;
            Gizmos.DrawLine(position + Vector3.up * 10, position + Vector3.down * 10);
            Gizmos.DrawLine(position + Vector3.left * 10, position + Vector3.right * 10);
            Gizmos.matrix = oldMat;

            if (string.IsNullOrEmpty(label)) return;
            
            oldMat = Handles.matrix;
            position += Vector3.up * 20;
            position += Vector3.right * 10;
            
            Handles.matrix = mat4x4;
            var sytle = new GUIStyle();
            sytle.normal.textColor = color;
            Handles.Label(position, label, sytle);
            Handles.matrix = oldMat;
        }
    }
}
#endif