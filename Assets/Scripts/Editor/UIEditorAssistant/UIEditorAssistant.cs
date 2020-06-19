

using System;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditorAssistant
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
                UIUtility.TryGetRectTransform(obj as GameObject, out _selectedUIElement) &&
                UIUtility.TryGetRootCanvas(_selectedUIElement.gameObject, out _selectedRootCanvas) && 
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
                    DoSnapElementToGrid();
                    break;
                }
            }
        }

        private static void DoSnapElementToGrid()
        {
            if (!(UIEditorAssistantSetting.GridSnap && UIEditorAssistantSetting.GridVisible))
            {
                return;
            }

            var canvasRect = UIUtility.GetCanvasRect(_selectedUIElement);
            var rect = canvasRect.Rect;
            
            var topLeft = new Vector3(rect.xMin, rect.yMax, 0);
            
            var gridSizeX = UIEditorAssistantSetting.GridSizeX;
            var gridSizeY = UIEditorAssistantSetting.GridSizeY;
            
            var canvasHalfSize = _selectedRootCanvas.pixelRect.size * 0.5f;
            var tx = Mathf.Clamp(Convert.ToInt32(Mathf.Round(topLeft.x/gridSizeX) * gridSizeX),-canvasHalfSize.x, canvasHalfSize.x);
            var ty = Mathf.Clamp(Convert.ToInt32(Mathf.Round(topLeft.y/gridSizeY) * gridSizeY),-canvasHalfSize.y, canvasHalfSize.y);
            var targetPos = new Vector3(tx, ty, 0);
            
            targetPos = _selectedRootCanvas.transform.localToWorldMatrix.MultiplyPoint(targetPos);
            targetPos.x -= canvasRect.PivotToSide.x;
            targetPos.y -= canvasRect.PivotToSide.w;

            _selectedUIElement.position = targetPos;

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

            var gridSizeX = UIEditorAssistantSetting.GridSizeX;
            var gridSizeY = UIEditorAssistantSetting.GridSizeY;
        
            var rect = canvas.pixelRect;

            var halfWidth = rect.width * 0.5f;
            var halfHeight = rect.height * 0.5f;
            
            Gizmos.color = UIEditorAssistantSetting.GridColor;
            
            var lastMatrix = Gizmos.matrix;
            Gizmos.matrix = canvas.transform.localToWorldMatrix;
            
            
            /// 画竖线
            Gizmos.DrawLine(new Vector3( 0, -halfHeight,0), new Vector3(0, halfHeight ,0));
            for( var x = gridSizeX; x < halfWidth; x+=gridSizeX)
            {
                Gizmos.DrawLine(new Vector3(  x, -halfHeight,0), new Vector3(  x, halfHeight ,0));
                Gizmos.DrawLine(new Vector3( -x, -halfHeight,0), new Vector3( -x, halfHeight ,0));
            }
            Gizmos.DrawLine(new Vector3(  halfWidth, -halfHeight,0), new Vector3( halfWidth, halfHeight ,0));
            Gizmos.DrawLine(new Vector3( -halfWidth, -halfHeight,0), new Vector3(-halfWidth, halfHeight ,0));
            
            
            /// 画横线
            Gizmos.DrawLine(new Vector3( -halfWidth, 0,0), new Vector3(halfWidth, 0 ,0));
            for( var y = gridSizeY; y < halfHeight; y+=gridSizeY)
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
            var canvasRect = UIUtility.GetCanvasRect(_selectedUIElement);
            const float MAX = 100000f;
            const float MIN = -100000f;
            
            var oldMatrix = Gizmos.matrix;
            Gizmos.color = color;
            
            Gizmos.matrix = _selectedRootCanvas.transform.localToWorldMatrix;

            var rect = canvasRect.Rect;
            
            Gizmos.DrawLine(new Vector3(rect.xMin, MIN, 0f), new Vector3(rect.xMin, MAX, 0f));
            Gizmos.DrawLine(new Vector3(rect.xMax, MIN, 0f), new Vector3(rect.xMax, MAX, 0f));
            
            Gizmos.DrawLine(new Vector3(MIN, rect.yMin, 0f), new Vector3(MAX, rect.yMin, 0f));
            Gizmos.DrawLine(new Vector3(MIN, rect.yMax, 0f), new Vector3(MAX, rect.yMax, 0f));

            Gizmos.matrix = oldMatrix;
        }

        

    }
}

class UIEditorAssistant_GizmoDrawer : MonoBehaviour { }
