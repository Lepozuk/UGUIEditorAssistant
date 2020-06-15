using UnityEngine;

namespace Editor.UIEditor
{
    public static class UIEditorUtils
    {
        /// <summary>
        /// 获取UI的基本组件RectTransform
        /// </summary>
        internal static bool TryGetRectTransform(object target, out RectTransform rect)
        {
            rect = null;
            var go = target as GameObject;
            if (go == null || !go.TryGetComponent(typeof(RectTransform), out var comp))
            {
                return false;
            }
            
            rect = comp as RectTransform;
            return true;

        }
        
        /// <summary>
        /// 获取当前UI对象的根舞台
        /// </summary>
        internal static bool TryGetRootCanvas(GameObject obj, out Canvas output)
        {
            output = null;
            
            if(obj.TryGetComponent(typeof(Canvas), out var comp))
            {
                var canvas = comp as Canvas;
                if (canvas.isRootCanvas)
                {
                    output = canvas;
                    return true;
                }
            }
            
            return obj.transform.parent != null && TryGetRootCanvas(obj.transform.parent.gameObject, out output);
        }

        private static CanvasRect _canvasRect = new CanvasRect();
        internal static CanvasRect GetRectFromUIElement(RectTransform trans)
        {
            if (!TryGetRootCanvas(trans.gameObject, out var canvas))
            {
                _canvasRect.Rect = Rect.zero;
                _canvasRect.Offset = Vector4.zero;
                _canvasRect.AABB = Vector4.zero;
                
                return _canvasRect;
            }
            
            
            var position = trans.position;
                
            var localPosition = trans.localPosition;
                
            var pivot = trans.pivot;
            var sizeDelta = trans.sizeDelta;
            var lossyScale = trans.lossyScale;
            
            var offsetX1 = -(sizeDelta.x * pivot.x) * lossyScale.x;
            var offsetY1 = -(sizeDelta.y * pivot.y) * lossyScale.y;
            var offsetX2 = sizeDelta.x * (1f - pivot.x) * lossyScale.x;
            var offsetY2 = sizeDelta.y * (1f - pivot.y) * lossyScale.y;
            
            var x1 =  position.x + offsetX1;
            var x2 =  position.x + offsetX2;
            
            var y1 =  position.y + offsetY1;
            var y2 =  position.y + offsetY2;
                 
                 
                
            var worldToLocalMatrix = canvas.transform.worldToLocalMatrix;
            Vector2 min = worldToLocalMatrix.MultiplyPoint(new Vector3(x1, y1, 0));
            Vector2 max = worldToLocalMatrix.MultiplyPoint(new Vector3(x2, y2, 0));
            
            _canvasRect.Rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            _canvasRect.AABB = new Vector4(x1, y1, x2, y2);
            _canvasRect.Offset = new Vector4(offsetX1, offsetY1, offsetX2, offsetY2);

            return _canvasRect;
        }
    }
    
}