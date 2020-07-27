using System;
using System.Collections.Generic;
using UnityEngine;
namespace Editor.UIEditor
{
    public static class HelperSettings
    {
        
        private static readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        private static HelperSettingProperty<T> GetProperty<T>(string storeKey, T defaultValue)
        {
            if (!_properties.TryGetValue(storeKey, out var property))
            {
                property = new HelperSettingProperty<T>(storeKey, defaultValue);
                _properties.Add(storeKey, property);
            }
            
            var rst = property as HelperSettingProperty<T>;
            if (rst == null)
            {
                throw new Exception("键 '" + storeKey + "' 当前已经被指定为 'HelperSettingProperty<" + typeof(T).Name + ">'.");
            }
            return rst;
        }
        
        
        /// <summary>
        /// 显示网格
        /// </summary>
        public static bool GridVisible { get => _gridVisible.Get(); set => _gridVisible.Set(value); }
        private static readonly HelperSettingProperty<bool> _gridVisible =
            GetProperty<bool>(SettingConsts.Assistant.GRID_VISIBLE, true);
        
        
        /// <summary>
        /// 吸附网格
        /// </summary>
        public static bool GridSnap { get => _gridSnap.Get(); set => _gridSnap.Set(value); }
        private static readonly HelperSettingProperty<bool> _gridSnap =
            GetProperty<bool>(SettingConsts.Assistant.GRID_SNAP, true);
        
        
        /// <summary>
        /// 网格大小X
        /// </summary>
        public static Vector2 GridSize { get => _gridSize.Get(); set => _gridSize.Set(value); }
        private static readonly HelperSettingProperty<Vector2> _gridSize =
            GetProperty<Vector2>(SettingConsts.Assistant.GRID_SIZE, new Vector2(8, 8));
        
        
        /// <summary>
        /// 网格颜色
        /// </summary>
        public static Color GridColor { get => _gridColor.Get(); set => _gridColor.Set(value); }
        private static readonly HelperSettingProperty<Color> _gridColor =
            GetProperty<Color>(SettingConsts.Assistant.GRID_COLOR, Color.gray);

        
        /// <summary>
        /// 显示引导线
        /// </summary>
        public static bool GuideVisible {get => _guideVisible.Get(); set => _guideVisible.Set(value); }
        private static readonly HelperSettingProperty<bool> _guideVisible =
            GetProperty<bool>(SettingConsts.Assistant.GUIDE_VISIBLE, true);
        
        
        /// <summary>
        /// 引导线颜色
        /// </summary>
        public static Color GuideColor { get => _guideColor.Get(); set => _guideColor.Set(value); }
        private static readonly HelperSettingProperty<Color> _guideColor = 
            GetProperty<Color>(SettingConsts.Assistant.GUIDE_COLOR, Color.cyan);

        
        /// <summary>
        /// 原子的保存地址
        /// </summary>
        public static string AtomPath { get => _atomPath.Get(); set => _atomPath.Set(value); }
        private static readonly HelperSettingProperty<string> _atomPath = 
            GetProperty<string>(SettingConsts.UIPrefabs.ATOM_PATH, "Assets/ResourcesAssets/UI/Atoms");
        
        
        /// <summary>
        /// 模组的保存地址
        /// </summary>
        public static string ModulePath { get => _modulePath.Get(); set => _modulePath.Set(value); }
        private static readonly HelperSettingProperty<string> _modulePath = 
            GetProperty<string>(SettingConsts.UIPrefabs.MODULE_PATH, "Assets/ResourcesAssets/UI/Modules");
    }
}