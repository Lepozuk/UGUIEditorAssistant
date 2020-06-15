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
        
        
        internal static Rect GetRectFromUIElement(RectTransform trans)
        {
            if (!TryGetRootCanvas(trans.gameObject, out var canvas))
            {
                return Rect.zero;
            }
            
            
            var position = trans.position;
                
            var localPosition = trans.localPosition;
                
            var pivot = trans.pivot;
            var sizeDelta = trans.sizeDelta;
            var lossyScale = trans.lossyScale;
                
            var xOffset = position.x - localPosition.x;
            var yOffset = position.y - localPosition.y;
                
            var x1 = (localPosition.x - (sizeDelta.x * pivot.x) * lossyScale.x) + xOffset;
            var y1 = (localPosition.y - (sizeDelta.y * pivot.y) * lossyScale.y) + yOffset;
            var x2 = (localPosition.x + (sizeDelta.x * (1f - pivot.x) * lossyScale.x)) + xOffset;
            var y2 = (localPosition.y + (sizeDelta.y * (1f - pivot.y) * lossyScale.y)) + yOffset;
                 
                 
                
            var worldToLocalMatrix = canvas.transform.worldToLocalMatrix;
            Vector2 min = worldToLocalMatrix.MultiplyPoint(new Vector3(x1, y1, 0));
            Vector2 max = worldToLocalMatrix.MultiplyPoint(new Vector3(x2, y2, 0));
                
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);

        }
    }
    
}