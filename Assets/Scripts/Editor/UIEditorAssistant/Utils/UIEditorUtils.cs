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
    }
    
}