using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEditorWindow : OdinEditorWindow
{
    private static UIEditorWindow _win;
    [MenuItem("Window/UI编辑器...",false,-1)] 
    private static void OpenUIEditorWindow()
    {
        if (_win == null)
        {
            _win = OdinEditorWindow.CreateWindow<UIEditorWindow>();
            _win.titleContent = new GUIContent("UI编辑器");
            _win.minSize = new Vector2(1366, 768);
        }
        _win.Show();
        _win.Focus();
    }

    [HorizontalGroup(0.5f), InlineEditor(InlineEditorModes.LargePreview)] 
    public Scene selectedCanvas;

}
