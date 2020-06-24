using UnityEditor;
using UnityEngine;
namespace Editor.UIEditor.Assistant
{
    internal static class UIEditorAssistantSetting
    {
        

        
        private static bool _gridVisible;
        private static int _gridSizeX;
        private static int _gridSizeY;
        private static Color _gridColor;
        private static bool _gridSnap;
        
        private static bool _guideVisible;
        private static Color _guideColor;

        
        [InitializeOnLoadMethod]
        private static void InitSetting()
        {
            GridVisible = EditorPrefs.GetBool(SettingConsts.Assistant.GRID_SIZE_Y, true);
            GridSizeX = EditorPrefs.GetInt(SettingConsts.Assistant.GRID_SIZE_X, 8);
            GridSizeY = EditorPrefs.GetInt(SettingConsts.Assistant.GRID_SIZE_Y, 8);
            
            var defaultGridColor = Color.gray;
            defaultGridColor.a = 0.25f;
            GridColor = ReadColor(SettingConsts.Assistant.GRID_COLOR, defaultGridColor);
            GridSnap = EditorPrefs.GetBool(SettingConsts.Assistant.GRID_SNAP, true);

            GuideVisible = EditorPrefs.GetBool(SettingConsts.Assistant.GUIDE_VISIBLE, true);
            var defaultGuideLineColor = Color.cyan;
            GuideColor = ReadColor(SettingConsts.Assistant.GUIDE_COLOR, defaultGuideLineColor);
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
                EditorPrefs.SetBool(SettingConsts.Assistant.GRID_VISIBLE, value);
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
                EditorPrefs.SetBool(SettingConsts.Assistant.GRID_SNAP, value);
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
                EditorPrefs.SetInt(SettingConsts.Assistant.GRID_SIZE_X, value);
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
                EditorPrefs.SetInt(SettingConsts.Assistant.GRID_SIZE_Y, value);
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
                WriteColor(SettingConsts.Assistant.GRID_COLOR, value);
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
                EditorPrefs.SetBool(SettingConsts.Assistant.GUIDE_VISIBLE, value);
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
                WriteColor(SettingConsts.Assistant.GUIDE_COLOR, value);
            }
        }
    }
}