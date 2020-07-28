using System;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    public class SnapHelper
    {   
        /// <summary>
        /// 当前选择的根舞台
        /// </summary>
        public static Canvas SelectedRootCanvas;
        
        /// <summary>
        /// 当前选择的UI对象
        /// </summary>
        public static RectTransform SelectedUIElement;
        
        /// <summary>
        /// 静态类的构造
        /// </summary>
        public void Init()
        {
            Selection.selectionChanged += SelectionChanged;

            SceneView.duringSceneGui += OnSceneGUI;
            
            CreateUIEditorAssistantGizmoDrawer();
        }

        /// <summary>
        /// 创建一个只在编辑器周期存在的gizmo辅助，后续逻辑看OnDrawGizmo
        /// </summary>
        private void CreateUIEditorAssistantGizmoDrawer()
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
        /// 当前选择的对象发生更改
        /// </summary>
        private void SelectionChanged()
        {
            var obj = Selection.activeObject;
            if (! ( obj != null &&
                CanvasUtil.TryGetRectTransform(obj as GameObject, out SelectedUIElement) &&
                CanvasUtil.TryGetRootCanvas(SelectedUIElement.gameObject, out SelectedRootCanvas) && 
                SelectedUIElement.gameObject != SelectedRootCanvas.gameObject ) )
            {
                OnDeselect();
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        private void OnDeselect()
        {
            SelectedUIElement = null;
            SelectedRootCanvas = null;
        }
        

        ///////////////////////////////////////////////////////////////////////////////////
        /// 吸附网格 ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 编辑器模式下每帧调用
        /// </summary>
        private void OnSceneGUI(SceneView view)
        {
            if (!(Application.isEditor && SelectedUIElement != null))
            {
                return;
            }
            
            var type = Event.current.type;
            switch (type)
            {
                case EventType.KeyDown:
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.UpArrow:
                        {
                            MoveElementByGridStep(Vector3.up);
                            break;
                        }
                        case KeyCode.DownArrow:
                        {
                            MoveElementByGridStep(Vector3.down);
                            break;
                        }
                        case KeyCode.LeftArrow:
                        {
                            MoveElementByGridStep(Vector3.left);
                            break;
                        }
                        case KeyCode.RightArrow:
                        {
                            MoveElementByGridStep(Vector3.right);
                            break;
                        }
                        default: 
                            break;
                    }
                    break;
                }
                case EventType.MouseUp:
                {
                    if (Event.current.button == 0)
                    {
                        DoSnapElementToGrid();
                    }
                    break;
                }
                
                default: 
                    break;
            }
        }

        
        private void MoveElementByGridStep(Vector3 direction)
        {
            if (!GetCorrectElementPosition(out var canvasRect, out var position))
            {
                return;
            }

            var offset = new Vector3();
            offset.x = HelperSettings.GridSize.x;
            offset.y = HelperSettings.GridSize.y;
            offset.Scale(direction);
            position += offset;

            ChangeElementPositionFromGrid(SelectedUIElement, canvasRect, position);
            DoSnapElementToGrid();
        }

        private void DoSnapElementToGrid()
        {
            if (!GetCorrectElementPosition(out var canvasRect, out var position))
            {
                return;
            }

            ChangeElementPositionFromGrid(SelectedUIElement, canvasRect, position);
        }


        private void ChangeElementPositionFromGrid(RectTransform transform, CanvasRect canvasRect,
            Vector3 targetPos)
        {
            targetPos = SelectedRootCanvas.transform.localToWorldMatrix.MultiplyPoint(targetPos);
            targetPos.x -= canvasRect.PivotToSide.x;
            targetPos.y -= canvasRect.PivotToSide.w;

            SelectedUIElement.position = targetPos;
        }
        
        private bool GetCorrectElementPosition(out CanvasRect canvasRect, out Vector3 position)
        {
            canvasRect = new CanvasRect();
            position = Vector3.zero;
            
            if (!(HelperSettings.GridSnap && HelperSettings.GridVisible))
            {
                return false;
            }
            
            canvasRect = CanvasUtil.GetCanvasRect(SelectedUIElement);
            
            var rect = canvasRect.Rect;
            
            var topLeft = new Vector3(rect.xMin, rect.yMax, 0);
            
            var gridSizeX = HelperSettings.GridSize.x;
            var gridSizeY = HelperSettings.GridSize.y;
            
            var canvasHalfSize = SelectedRootCanvas.pixelRect.size * 0.5f;
            var tx = Mathf.Clamp(Convert.ToInt32(Mathf.Round(topLeft.x/gridSizeX) * gridSizeX),-canvasHalfSize.x, canvasHalfSize.x);
            var ty = Mathf.Clamp(Convert.ToInt32(Mathf.Round(topLeft.y/gridSizeY) * gridSizeY),-canvasHalfSize.y, canvasHalfSize.y);
            
            position = new Vector3(tx, ty, 0);

            return true;
        }
        

    }
}

class UIEditorAssistant_GizmoDrawer : MonoBehaviour { }
