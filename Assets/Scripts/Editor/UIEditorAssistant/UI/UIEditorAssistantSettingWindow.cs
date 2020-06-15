using UnityEditor;
using UnityEngine;

namespace Editor.UI
{
    public class UIEditorAssistantSettingWindow : EditorWindow
    {
        

        private int _gridSize = 8;
        private Color _gridColor = Color.gray;
        private Color _guideColor = Color.cyan;
        
        private void Awake()
        {
            _gridColor = UIEditorAssistantSetting.GridColor;
        }
        
        private void OnGUI()
        {
            _gridSize = Mathf.CeilToInt(EditorGUILayout.IntSlider("网格大小", _gridSize, 4, 32) / 4) * 4;
            UIEditorAssistantSetting.GridSize = _gridSize;
            
            _gridColor = EditorGUILayout.ColorField("网格颜色", _gridColor);
            
            _guideColor = EditorGUILayout.ColorField("引导颜色", _guideColor);

            if (!GUILayout.Button("确认")) return;
            
            ChangeColors();
            Close();
        }

        private void ChangeColors()
        {
            UIEditorAssistantSetting.GridColor = _gridColor;
            UIEditorAssistantSetting.GuideColor = _guideColor;
        }
        
        
    }
}