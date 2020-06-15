using System;
using UnityEditor;
using UnityEngine;

namespace Editor.UI
{
    [InitializeOnLoad]
    public static class UIEditorAssistantSettingMenu
    {
        private const string GRID_VISIBLE = "Window/UI编辑器辅助/显示舞台网格";
        
        private const string GUIDE_VISIBLE = "Window/UI编辑器辅助/显示组件引导线";
        
        private const string GRID_SNAP = "Window/UI编辑器辅助/吸附舞台网格";
        
        
        private const string SHOW_SETTINGS = "Window/UI编辑器辅助/设置...";
        
        static UIEditorAssistantSettingMenu()
        {
            UIEditorAssistantSetting.OnSettingUpdate += key =>
            {
                switch (key)
                {
                    case UIEditorAssistantSetting.KEY.GRID_VISIBLE:
                        Menu.SetChecked(GRID_VISIBLE, UIEditorAssistantSetting.GridVisible);
                        break;
                    case UIEditorAssistantSetting.KEY.GUIDE_VISIBLE:
                        Menu.SetChecked(GUIDE_VISIBLE, UIEditorAssistantSetting.GuideVisible);
                        break;
                    case UIEditorAssistantSetting.KEY.GRID_SNAP:
                        Menu.SetChecked(GRID_SNAP, UIEditorAssistantSetting.GridSnap);
                        break;
                    default:
                        break;
                }
            };
        }
        /// <summary>
        /// 显示网格
        /// </summary>
        [MenuItem(GRID_VISIBLE, false,0)]
        private static void ShowCanvasGrid() => UIEditorAssistantSetting.GridVisible = !UIEditorAssistantSetting.GridVisible;
        
        
        /// <summary>
        /// 吸附网格
        /// </summary>
        [MenuItem(GRID_SNAP)]
        private static void SnapGird() => UIEditorAssistantSetting.GridSnap = !UIEditorAssistantSetting.GridSnap;
        [MenuItem(GRID_SNAP, true)]
        private static bool SnapGrid() => UIEditorAssistantSetting.GridVisible;
        
        
        /// <summary>
        /// 显示引导线
        /// </summary>
        [MenuItem(GUIDE_VISIBLE, false, 1)]
        private static void ShowGuideline() => UIEditorAssistantSetting.GuideVisible = !UIEditorAssistantSetting.GuideVisible;
        [MenuItem(GUIDE_VISIBLE, true)]
        private static bool ShowGuideLine() => UIEditorAssistantSetting.GridVisible;
        
        /// <summary>
        /// 打开设置
        /// </summary>
        private static UIEditorAssistantSettingWindow _window;
        [MenuItem(SHOW_SETTINGS, false, 3)]
        private static void ShowWindow()
        {
            if(_window == null) {
                var window = EditorWindow.GetWindow<UIEditorAssistantSettingWindow>();
                window.titleContent = new GUIContent("UI编辑器辅助设置");
                _window = window;
                _window.maxSize = _window.minSize = new Vector2(300, 80);
                
            }
            _window.Show();
            _window.Focus();
        }

    }
}