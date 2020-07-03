using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor.Assistant
{
    public class UIEditorAssistantSettingWindow : OdinEditorWindow
    {
        /// <summary>
        /// 打开设置
        /// </summary>
        private static UIEditorAssistantSettingWindow _window;
        [MenuItem(MenuDefine.SHOW_SETTINGS, false, 3)]
        private static void ShowWindow()
        {
            if(_window == null) {
                var window = _window = GetWindow<UIEditorAssistantSettingWindow>();
                window.titleContent = new GUIContent("UI编辑器辅助设置");
                _window.maxSize = _window.minSize = new Vector2(360, 120);
                
            }
            _window.Show();
            _window.Focus();
        }
        
        [ShowInInspector]
        [LabelText("网格大小_X"), PropertyRange(2f, 40f)]
        public float GridSizeX
        {
            get => UIEditorAssistantSetting.GridSizeX;
            set => UIEditorAssistantSetting.GridSizeX = Mathf.CeilToInt(value / 2) * 2;
        }
        
        [ShowInInspector]
        [LabelText("网格大小_Y"), PropertyRange(2f, 40f)]
        public float GridSizeY
        {
            get => UIEditorAssistantSetting.GridSizeY;
            set => UIEditorAssistantSetting.GridSizeY = Mathf.CeilToInt(value / 2) * 2;
        }
        [ShowInInspector, LabelText("网格颜色"), ColorPalette("Underwater")]
        public Color GridColor
        {
            get => UIEditorAssistantSetting.GridColor;
            set => UIEditorAssistantSetting.GridColor = value;
        }

        [ShowInInspector, LabelText("引导颜色"), ColorPalette("Breeze")]
        public Color GuideColor
        {
            get => UIEditorAssistantSetting.GuideColor;
            set => UIEditorAssistantSetting.GuideColor = value;
        }

    }
}