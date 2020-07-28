using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    internal static class SnapGizmo
    {
        
        ///////////////////////////////////////////////////////////////////////////////////
        /// 画辅助线 ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 隐式置入Gizmo阶段
        /// </summary>
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Active)]
        private static void DrawSnapHelperGizmo(UIEditorAssistant_GizmoDrawer drawer, GizmoType gizmoType)
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
            if (!HelperSettings.GridVisible)
            {
                return;
            }
            
            var canvas = SnapHelper.SelectedRootCanvas;
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
            {
                return;
            }

            var gridSizeX = HelperSettings.GridSize.x;
            var gridSizeY = HelperSettings.GridSize.y;
        
            var rect = canvas.pixelRect;

            var halfWidth = rect.width * 0.5f;
            var halfHeight = rect.height * 0.5f;
            
            Gizmos.color = HelperSettings.GridColor;
            
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
            if (!(HelperSettings.GridVisible && HelperSettings.GuideVisible && SnapHelper.SelectedUIElement != null))
            {
                return;
            }

            var color = HelperSettings.GuideColor;
            var canvasRect = CanvasUtil.GetCanvasRect(SnapHelper.SelectedUIElement);
            const float MAX = 100000f;
            const float MIN = -100000f;
            
            var oldMatrix = Gizmos.matrix;
            Gizmos.color = color;
            
            Gizmos.matrix = SnapHelper.SelectedRootCanvas.transform.localToWorldMatrix;

            var rect = canvasRect.Rect;
            
            Gizmos.DrawLine(new Vector3(rect.xMin, MIN, 0f), new Vector3(rect.xMin, MAX, 0f));
            Gizmos.DrawLine(new Vector3(rect.xMax, MIN, 0f), new Vector3(rect.xMax, MAX, 0f));
            
            Gizmos.DrawLine(new Vector3(MIN, rect.yMin, 0f), new Vector3(MAX, rect.yMin, 0f));
            Gizmos.DrawLine(new Vector3(MIN, rect.yMax, 0f), new Vector3(MAX, rect.yMax, 0f));

            Gizmos.matrix = oldMatrix;
        }
    }
}