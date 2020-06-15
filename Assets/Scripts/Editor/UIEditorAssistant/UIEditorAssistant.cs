

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
        /// 当前选择的跟舞台的矩形区域
        /// </summary>
        private static Rect _selectedRootRect;
        /// <summary>
        /// 当前选择的根舞台的网格基于_selectRootRect的单位大小
        /// </summary>
        private static float _selectedRootGridUnit;
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
            if (obj != null &&
                UIEditorUtils.TryGetRectTransform(obj, out _selectedUIElement) &&
                UIEditorUtils.TryGetRootCanvas(_selectedUIElement.gameObject, out _selectedRootCanvas) && 
                _selectedUIElement.gameObject != _selectedRootCanvas.gameObject)
            {
                _selectedRootRect = GetRectFromUIElement(_selectedRootCanvas.transform as RectTransform);
                _selectedRootGridUnit = _selectedRootRect.width / (_selectedRootCanvas.pixelRect.width / UIEditorAssistantSetting.GridSize);
            }
            else
            {
                OnDeselect();
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        private static void OnDeselect()
        {
            _selectedRootGridUnit = 0f;
            _selectedRootRect = Rect.zero;
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
            
            //吸附网格的逻辑实现
            var rect = GetRectFromUIElement(_selectedUIElement);
            rect.x = Mathf.Round(rect.x / _selectedRootGridUnit) * _selectedRootGridUnit;
            rect.y = Mathf.Round(rect.y / _selectedRootGridUnit) * _selectedRootGridUnit;
            
            
            SetRectToUIElement(rect, _selectedUIElement);
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

            var GridSize = UIEditorAssistantSetting.GridSize;
        
            var rect = canvas.pixelRect;

            var halfWidth = rect.width * 0.5f;
            var halfHeight = rect.height * 0.5f;
            
            Gizmos.color = UIEditorAssistantSetting.GridColor;
            
            var lastMatrix = Gizmos.matrix;
            
            Gizmos.matrix = canvas.transform.localToWorldMatrix;
            
            /// 画纵线
            Gizmos.DrawLine(new Vector3( 0, -halfHeight,0), new Vector3(0, halfHeight ,0));
            for( var x = GridSize; x < halfWidth; x+=GridSize)
            {
                Gizmos.DrawLine(new Vector3(  x, -halfHeight,0), new Vector3(  x, halfHeight ,0));
                Gizmos.DrawLine(new Vector3( -x, -halfHeight,0), new Vector3( -x, halfHeight ,0));
            }
            Gizmos.DrawLine(new Vector3(  halfWidth, -halfHeight,0), new Vector3( halfWidth, halfHeight ,0));
            Gizmos.DrawLine(new Vector3( -halfWidth, -halfHeight,0), new Vector3(-halfWidth, halfHeight ,0));
            
            
            /// 画横线
            Gizmos.DrawLine(new Vector3( -halfWidth, 0,0), new Vector3(halfWidth, 0 ,0));
            for( var y = GridSize; y < halfHeight; y+=GridSize)
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
            var rect = GetRectFromUIElement(_selectedUIElement);
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

        private static void SetRectToUIElement(Rect rect, RectTransform trans)
        {
            // var rectPos = rect.position;
            // rectPos = _selectedRootCanvas.transform.localToWorldMatrix.MultiplyPoint(rectPos);
            //
            // var sizeDelta = trans.sizeDelta;
            // var pivot = trans.pivot;
            // var lossyScale = trans.lossyScale;
            //
            // var tx = (rectPos.x - (sizeDelta.x * pivot.x) * lossyScale.x);
            // var ty = (rectPos.y + (sizeDelta.y * pivot.y) * lossyScale.y);
            //
            // trans.position = new Vector2(tx, ty);
            //
            // var position = trans.position;
            // var localPosition = trans.localPosition;
            // 
            // var pivot = trans.pivot;
            // var lossyScale = trans.lossyScale;
            //
            //
            // var xOffset = position.x - localPosition.x;
            // var yOffset = position.y - localPosition.y;
            //
            //
            // var localX = rect.x + (sizeDelta.x * pivot.x) * lossyScale.x + xOffset;
            // var localY = rect.y + (sizeDelta.y * pivot.y) * lossyScale.y + yOffset;
            //
            // trans.localPosition = new Vector3(localX, localY, trans.localPosition.z);
        }
        
        private static Rect GetRectFromUIElement(RectTransform trans)
        {
            var position = trans.position;
            
            var localPosition = trans.localPosition;
            var sizeDelta = trans.sizeDelta;
            var pivot = trans.pivot;
            var lossyScale = trans.lossyScale;

            var xOffset = position.x - localPosition.x;
            var yOffset = position.y - localPosition.y;

            var x1 = (localPosition.x - (sizeDelta.x * pivot.x) * lossyScale.x) + xOffset;
            var x2 = (localPosition.x + (sizeDelta.x * (1f - pivot.x) * lossyScale.x)) + xOffset;
            var y1 = (localPosition.y - (sizeDelta.y * pivot.y) * lossyScale.y) + yOffset;
            var y2 = (localPosition.y + (sizeDelta.y * (1f - pivot.y) * lossyScale.y)) + yOffset;
            
            var worldToLocalMatrix = _selectedRootCanvas.transform.worldToLocalMatrix;
            Vector2 min = worldToLocalMatrix.MultiplyPoint(new Vector3(x1, y1, 0));
            Vector2 max = worldToLocalMatrix.MultiplyPoint(new Vector3(x2, y2, 0));
            
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }


        private static void DrawRectGuideline(Rect rect, Color color)
        {
            
        }
    }
}

class UIEditorAssistant_GizmoDrawer : MonoBehaviour { }
