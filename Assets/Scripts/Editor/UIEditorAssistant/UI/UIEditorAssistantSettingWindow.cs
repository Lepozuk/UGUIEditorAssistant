using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor.UI
{
    public class UIEditorAssistantSettingWindow : EditorWindow
    {
        
        private void OnGUI()
        {
            UIEditorAssistantSetting.GridSize = Mathf.CeilToInt(EditorGUILayout.IntSlider("网格大小", UIEditorAssistantSetting.GridSize, 4, 40) / 4) * 4;

            UIEditorAssistantSetting.GridColor = EditorGUILayout.ColorField("网格颜色", UIEditorAssistantSetting.GridColor);
            
            UIEditorAssistantSetting.GuideColor = EditorGUILayout.ColorField("引导颜色", UIEditorAssistantSetting.GuideColor);

        }
        
        
    }
}