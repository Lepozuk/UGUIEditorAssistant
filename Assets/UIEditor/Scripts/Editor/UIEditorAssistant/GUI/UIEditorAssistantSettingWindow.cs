using UnityEditor;
using UnityEngine;

namespace Editor.UIEditorAssistant
{
    public class UIEditorAssistantSettingWindow : EditorWindow
    {
        
        private void OnGUI()
        {
            
            UIEditorAssistantSetting.GridSizeX = Mathf.CeilToInt(EditorGUILayout.IntSlider("网格大小_X", UIEditorAssistantSetting.GridSizeX, 2, 40) / 2) * 2;
            
            UIEditorAssistantSetting.GridSizeY = Mathf.CeilToInt(EditorGUILayout.IntSlider("网格大小_Y", UIEditorAssistantSetting.GridSizeY, 2, 40) / 2) * 2;

            UIEditorAssistantSetting.GridColor = EditorGUILayout.ColorField("网格颜色", UIEditorAssistantSetting.GridColor);
            
            UIEditorAssistantSetting.GuideColor = EditorGUILayout.ColorField("引导颜色", UIEditorAssistantSetting.GuideColor);

        }
        
        
    }
}