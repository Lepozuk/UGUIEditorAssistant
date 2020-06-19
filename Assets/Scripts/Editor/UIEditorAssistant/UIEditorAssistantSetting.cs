using System;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditorAssistant
{
    internal static class UIEditorAssistantSetting
    {
        private const string GRID_VISIBLE = "UIEditorAssistant.Setting.GridVisible";
        private const string GRID_SIZE_X = "UIEditorAssistant.Setting.GridSize.X";
        private const string GRID_SIZE_Y = "UIEditorAssistant.Setting.GridSize.Y";
        private const string GRID_COLOR = "UIEditorAssistant.Setting.GridColor";
        private const string GRID_SNAP = "UIEditorAssistant.Setting.GridSnap";
        
        private const string GUIDE_VISIBLE = "UIEditorAssistant.Setting.GuideVisible";
        private const string GUIDE_COLOR = "UIEditorAssistant.Setting.GuideColor";

        
        private static bool _gridVisible;
        private static int _gridSizeX;
        private static int _gridSizeY;
        private static Color _gridColor;
        private static bool _gridSnap;
        
        private static bool _guideVisible;
        private static Color _guideColor;

        internal static Action<KEY> OnSettingUpdate;
        
        
        public enum KEY
        {
            GRID_VISIBLE,
            GRID_SNAP,
            GRID_SIZE_X,
            GRID_SIZE_Y,
            GRID_COLOR,
            GUIDE_VISIBLE,
            GUIDE_COLOR
        }
        
        [InitializeOnLoadMethod]
        private static void InitSetting()
        {
            GridVisible = EditorPrefs.GetBool(GRID_VISIBLE, true);
            GridSizeX = EditorPrefs.GetInt(GRID_SIZE_X, 8);
            GridSizeY = EditorPrefs.GetInt(GRID_SIZE_Y, 8);
            
            var defaultGridColor = Color.gray;
            defaultGridColor.a = 0.25f;
            GridColor = ReadColor(GRID_COLOR, defaultGridColor);
            GridSnap = EditorPrefs.GetBool(GRID_SNAP, true);

            GuideVisible = EditorPrefs.GetBool(GUIDE_VISIBLE, true);
            var defaultGuideLineColor = Color.cyan;
            GuideColor = ReadColor(GUIDE_COLOR, defaultGuideLineColor);
        }

        private static void WriteColor(string key, Color color)
        {
            EditorPrefs.SetFloat(key + "_r", color.r);
            EditorPrefs.SetFloat(key + "_g", color.g);
            EditorPrefs.SetFloat(key + "_b", color.b);
            EditorPrefs.SetFloat(key + "_a", color.a);
        }

        private static Color ReadColor(string key, Color color)
        {
            color.r = EditorPrefs.GetFloat(key + "_r", color.r);
            color.g = EditorPrefs.GetFloat(key + "_g", color.g);
            color.b = EditorPrefs.GetFloat(key + "_b", color.b);
            color.a = EditorPrefs.GetFloat(key + "_a", color.a);
            return color;
        }

        private static void NotifyUpdate(KEY key)
        {
            OnSettingUpdate?.Invoke(key);
        }
        
        /// <summary>
        /// 显示网格
        /// </summary>
        public static bool GridVisible
        {
            get => _gridVisible;
            set 
            {
                if (_gridVisible == value)
                {
                    return;
                }
                _gridVisible = value;
                EditorPrefs.SetBool(GRID_VISIBLE, value);
                NotifyUpdate(KEY.GRID_VISIBLE);
            }
        }
        /// <summary>
        /// 吸附网格
        /// </summary>
        public static bool GridSnap
        {
            get => _gridSnap;
            set
            {
                if (_gridSnap == value)
                {
                    return;
                }
                _gridSnap = value;
                EditorPrefs.SetBool(GRID_SNAP, value);
                NotifyUpdate(KEY.GRID_SNAP);
            }
        }
        /// <summary>
        /// 网格大小X
        /// </summary>
        public static int GridSizeX
        {
            get => _gridSizeX;
            set
            {
                value = Mathf.Max(2, Mathf.Min(40, value));

                if (_gridSizeX == value)
                {
                    return;
                }
                _gridSizeX = value;
                EditorPrefs.SetInt(GRID_SIZE_X, value);
                NotifyUpdate(KEY.GRID_SIZE_X);
            }
        }
        
        /// <summary>
        /// 网格大小X
        /// </summary>
        public static int GridSizeY
        {
            get => _gridSizeY;
            set
            {
                value = Mathf.Max(2, Mathf.Min(40, value));

                if (_gridSizeY == value)
                {
                    return;
                }
                _gridSizeY = value;
                EditorPrefs.SetInt(GRID_SIZE_Y, value);
                NotifyUpdate(KEY.GRID_SIZE_Y);
            }
        }
        
        /// <summary>
        /// 网格颜色
        /// </summary>
        public static Color GridColor
        {
            get => _gridColor;
            set
            {
                if (_gridColor == value)
                {
                    return;
                }
                _gridColor = value;
                WriteColor(GRID_COLOR, value);
                NotifyUpdate(KEY.GRID_COLOR);
            }
        }

        /// <summary>
        /// 显示引导线
        /// </summary>
        public static bool GuideVisible
        {
            get => _guideVisible;
            set 
            {
                if (_guideVisible == value)
                {
                    return;
                }
                _guideVisible = value;
                EditorPrefs.SetBool(GUIDE_VISIBLE, value);
                NotifyUpdate(KEY.GUIDE_VISIBLE);
            }
        }
        /// <summary>
        /// 引导线颜色
        /// </summary>
        public static Color GuideColor
        {
            get => _guideColor;
            set
            {
                if (_guideColor == value)
                {
                    return;
                }
                _guideColor = value;
                WriteColor(GUIDE_COLOR, value);
                NotifyUpdate(KEY.GUIDE_COLOR);
            }
        }
    }
}