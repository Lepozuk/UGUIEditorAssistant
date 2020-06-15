

using System;
using System.Diagnostics.Contracts;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    [InitializeOnLoad]
    public static class UIEditorAssistant
    {   
        /// <summary>
        /// 静态类的构造
        /// </summary>
        static UIEditorAssistant()
        {
            Selection.selectionChanged += SelectionChanged;

            SceneView.duringSceneGui += OnSceneGUI;
            
            CreateUIEditorAssistantGizmoDrawer();
        }

        /// <summary>
        /// 创建一个只在编辑器周期存在的gizmo辅助，后续逻辑看OnDrawGizmo
        /// </summary>
        private static void CreateUIEditorAssistantGizmoDrawer()
        {
            var obj = GameObject.Find("UIEditorAssistant");
            if(obj == null) {
                obj = new GameObject("UIEditorAssistant");
            }

            if (!obj.TryGetComponent(typeof(UIEditorAssistant_GizmoDrawer), out var comp))
            {
               obj.AddComponent<UIEditorAssistant_GizmoDrawer>();
            }

            obj.hideFlags = HideFlags.DontSaveInEditor;
        }

        /// <summary>
        /// 当前选择的根舞台
        /// </summary>
        private static Canvas _selectedRootCanvas;
        
        /// <summary>
        /// 当前选择的UI对象
        /// </summary>
        private static RectTransform _selectedUIElement;
        
        /// <summary>
        /// 当前选择的对象发生更改
        /// </summary>
        private static void SelectionChanged()
        {
            var obj = Selection.activeObject;
            if (! ( obj != null &&
                UIEditorUtils.TryGetRectTransform(obj, out _selectedUIElement) &&
                UIEditorUtils.TryGetRootCanvas(_selectedUIElement.gameObject, out _selectedRootCanvas) && 
                _selectedUIElement.gameObject != _selectedRootCanvas.gameObject ) )
            {
                OnDeselect();
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        private static void OnDeselect()
        {
            _selectedUIElement = null;
            _selectedRootCanvas = null;
        }
        

        ///////////////////////////////////////////////////////////////////////////////////
        /// 吸附网格 ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 编辑器模式下每帧调用
        /// </summary>
        private static void OnSceneGUI(SceneView view)
        {
            if (!(Application.isEditor && _selectedUIElement != null))
            {
                return;
            }
            
            if (Event.current.button != 0)
            {
                return;
            }
            
            EventType type = Event.current.type;
            switch (type)
            {
                case EventType.MouseUp:
                {
                    SnapElementToGrid();
                    break;
                }
            }
        }
        
        private static void SnapElementToGrid()
        {
            if (!(UIEditorAssistantSetting.GridSnap && UIEditorAssistantSetting.GridVisible))
            {
                return;
            }

            var trans = _selectedUIElement;
            
            var position = trans.position;
                
            var localPosition = trans.localPosition;
                
            var pivot = trans.pivot;
            var sizeDelta = trans.sizeDelta;
            var lossyScale = trans.lossyScale;
                
            var xOffset = position.x - localPosition.x;
            var yOffset = position.y - localPosition.y;
                
            var currX = ((sizeDelta.x * pivot.x) * lossyScale.x) + position.x;
            var currY = ((sizeDelta.y * (1-pivot.y)) * lossyScale.y) + position.y;

            Vector3 currPos = new Vector3(currX, currY, trans.position.z);
            currPos = _selectedRootCanvas.transform.worldToLocalMatrix.MultiplyPoint(currPos);
            
            var gridSize = UIEditorAssistantSetting.GridSize;
            
            var canvasHalfSize = _selectedRootCanvas.pixelRect.size * 0.5f;
            
            var targetX = Mathf.Clamp(Convert.ToInt32(Mathf.Round(currPos.x/gridSize)*gridSize),-canvasHalfSize.x, canvasHalfSize.x);
            var targetY = Mathf.Clamp(Convert.ToInt32(Mathf.Round(currPos.y/gridSize)*gridSize),-canvasHalfSize.y, canvasHalfSize.y);

            var targetPos = new Vector3(targetX, targetY, 0);
            
            targetPos = _selectedRootCanvas.transform.localToWorldMatrix.MultiplyPoint(new Vector3(targetX, targetY, 0));
            targetPos.x -= ((sizeDelta.x * pivot.x) * lossyScale.x);
            targetPos.y -= ((sizeDelta.y * (1 - pivot.y)) * lossyScale.y);
            
            trans.position = targetPos;

        }
        
        
        ///////////////////////////////////////////////////////////////////////////////////
        /// 画辅助线 ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 隐式置入Gizmo阶段
        /// </summary>
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Active)]
        private static void OnDrawGizmo(UIEditorAssistant_GizmoDrawer drawer, GizmoType gizmoType)
        {
            DrawCanvasGrids();
            DrawElementGuideline();
        }
        
        ////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 画当前根舞台的网格
        /// </summary>
        private static void DrawCanvasGrids()
        {
            if (!UIEditorAssistantSetting.GridVisible)
            {
                return;
            }
            
            var canvas = _selectedRootCanvas;
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
            {
                return;
            }

            var gridSize = UIEditorAssistantSetting.GridSize;
        
            var rect = canvas.pixelRect;

            var halfWidth = rect.width * 0.5f;
            var halfHeight = rect.height * 0.5f;
            
            Gizmos.color = UIEditorAssistantSetting.GridColor;
            
            var lastMatrix = Gizmos.matrix;
            Gizmos.matrix = canvas.transform.localToWorldMatrix;
            
            /// 画竖线
            Gizmos.DrawLine(new Vector3( 0, -halfHeight,0), new Vector3(0, halfHeight ,0));
            for( var x = gridSize; x < halfWidth; x+=gridSize)
            {
                Gizmos.DrawLine(new Vector3(  x, -halfHeight,0), new Vector3(  x, halfHeight ,0));
                Gizmos.DrawLine(new Vector3( -x, -halfHeight,0), new Vector3( -x, halfHeight ,0));
            }
            Gizmos.DrawLine(new Vector3(  halfWidth, -halfHeight,0), new Vector3( halfWidth, halfHeight ,0));
            Gizmos.DrawLine(new Vector3( -halfWidth, -halfHeight,0), new Vector3(-halfWidth, halfHeight ,0));
            
            
            /// 画横线
            Gizmos.DrawLine(new Vector3( -halfWidth, 0,0), new Vector3(halfWidth, 0 ,0));
            for( var y = gridSize; y < halfHeight; y+=gridSize)
            {
                Gizmos.DrawLine(new Vector3( -halfWidth,  y,0), new Vector3(halfWidth,  y,0));
                Gizmos.DrawLine(new Vector3( -halfWidth, -y,0), new Vector3(halfWidth, -y,0));
            }
            Gizmos.DrawLine(new Vector3( -halfWidth,  halfHeight,0), new Vector3( halfWidth,  halfHeight,0));
            Gizmos.DrawLine(new Vector3( -halfWidth, -halfHeight,0), new Vector3( halfWidth, -halfHeight,0));

            Gizmos.matrix = lastMatrix;
        }
        
        /// <summary>
        /// 画当前UI单元的辅助线
        /// </summary>
        private static void DrawElementGuideline()
        {
            if (!(UIEditorAssistantSetting.GridVisible && UIEditorAssistantSetting.GuideVisible && _selectedUIElement != null))
            {
                return;
            }

            var color = UIEditorAssistantSetting.GuideColor;
            var rect = UIEditorUtils.GetRectFromUIElement(_selectedUIElement);
            const float MAX = 100000f;
            const float MIN = -100000f;
            
            var oldMatrix = Gizmos.matrix;
            Gizmos.color = color;
            
            Gizmos.matrix = _selectedRootCanvas.transform.localToWorldMatrix;
            Gizmos.DrawLine(new Vector3(rect.xMin, MIN, 0f), new Vector3(rect.xMin, MAX, 0f));
            Gizmos.DrawLine(new Vector3(rect.xMax, MIN, 0f), new Vector3(rect.xMax, MAX, 0f));
            
            Gizmos.DrawLine(new Vector3(MIN, rect.yMin, 0f), new Vector3(MAX, rect.yMin, 0f));
            Gizmos.DrawLine(new Vector3(MIN, rect.yMax, 0f), new Vector3(MAX, rect.yMax, 0f));

            Gizmos.matrix = oldMatrix;
        }

        

    }
}

class UIEditorAssistant_GizmoDrawer : MonoBehaviour { }
