using System;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    public class UIHelperSettingWindow : EditorWindow
    {
        private Vector2 FixedVec2(Vector2 vec2, float size)
        {
            vec2.x = Convert.ToInt32(vec2.x / size) * size;
            vec2.y = Convert.ToInt32(vec2.y / size) * size;
            return vec2;
        }
        
        private void OnGUI()
        {
            // 网格大小
            HelperSettings.GridSize = FixedVec2(EditorGUILayout.Vector2Field("网格大小", HelperSettings.GridSize),2);
            
            // 网格颜色
            GUILayout.Space(4);
            HelperSettings.GridColor = EditorGUILayout.ColorField("网格颜色", HelperSettings.GridColor);
            
            // 引导颜色
            GUILayout.Space(4);
            HelperSettings.GuideColor = EditorGUILayout.ColorField("引导颜色", HelperSettings.GuideColor);
            
            EditorGUILayout.Separator();
            
            //// 原子目录
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("原子目录", HelperSettings.AtomPath, EditorStyles.label);
                if (GUILayout.Button("修改", EditorStyles.miniButton, GUILayout.Width(60f)))
                {
                    var path = EditorUtility.OpenFolderPanel("请选择原子的目录", Application.dataPath, "");
                    if (path.StartsWith(Application.dataPath))
                    {
                        HelperSettings.AtomPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else if(!string.IsNullOrEmpty(path))
                    {
                        ShowNotification(new GUIContent("请选择当前项目中Assets的中的目录"));	
                    }
                }
            }
            GUILayout.EndHorizontal();
            
            //// 模组目录
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("模组目录", HelperSettings.ModulePath, EditorStyles.label);
                if (GUILayout.Button("修改", EditorStyles.miniButton, GUILayout.Width(60f)))
                {
                    var path = EditorUtility.OpenFolderPanel("请选择模组的目录", Application.dataPath, "");
                    if (path.StartsWith(Application.dataPath))
                    {
                        HelperSettings.ModulePath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else if(!string.IsNullOrEmpty(path))
                    {
                        ShowNotification(new GUIContent("请选择当前项目中Assets的中的目录"));	
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        public static Vector2 WindowSize => new Vector2(360, 160);
    }
}