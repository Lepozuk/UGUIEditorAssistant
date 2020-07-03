using UnityEngine;

namespace Editor
{
    public static class UIUtility
    {
        /// <summary>
        /// 获取UI的基本组件RectTransform
        /// </summary>
        /// <param name="target">GameObject</param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool TryGetRectTransform(GameObject gameObject, out RectTransform output)
        {
            output = null;
            if (gameObject == null || !gameObject.TryGetComponent(typeof(RectTransform), out var comp))
            {
                return false;
            }
            
            output = comp as RectTransform;
            return true;

        }
        
        /// <summary>
        /// 获取当前UI对象的根舞台
        /// </summary>
        public static bool TryGetRootCanvas(GameObject gameObject, out Canvas output)
        {
            output = null;
            
            if(gameObject.TryGetComponent(typeof(Canvas), out var comp))
            {
                var canvas = comp as Canvas;
                if (canvas.isRootCanvas)
                {
                    output = canvas;
                    return true;
                }
            }
            
            return gameObject.transform.parent != null && TryGetRootCanvas(gameObject.transform.parent.gameObject, out output);
        }

        
        public static CanvasRect GetCanvasRect(RectTransform trans)
        {

            var output = new CanvasRect();

            if (!TryGetRootCanvas(trans.gameObject, out var canvas))
            {
                return output;
            }
            
            var position = trans.position;
                
                
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
            
            var center = worldToLocalMatrix.MultiplyPoint(position);
            
            output.Rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            output.PivotToSide = new Vector4(offsetX1, offsetY1, offsetX2, offsetY2);
            output.Center = center;
            return output;
            
        }
    }
    
    public struct CanvasRect
    {
        public Rect Rect;
        public Vector4 PivotToSide;
        public Vector3 Center;
    }
}