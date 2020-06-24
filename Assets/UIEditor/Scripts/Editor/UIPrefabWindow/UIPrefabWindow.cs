using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    internal static class UIPrefabWindowUtil
    {
        private static UIPrefabWindow _window;
        [MenuItem(MenuDefine.SHOW_PREFAB_WIN)]
        private static void OpenUIPrefabWindow()
        {
            if (_window == null)
            {
                var win = _window = ScriptableObject.CreateInstance<UIPrefabWindow>();
                win.titleContent = new GUIContent("UI预制件管理器");
            }
            
            _window.Show();
            _window.Focus();
        }
        
        internal static void ShowNotification(GUIContent content)
        {
            if(_window != null) {
                _window.ShowNotification(content);
            }
        }
    }
    
    public class UIPrefabWindow : OdinEditorWindow
    {
        [ShowInInspector, EnumToggleButtons, HideLabel]
        internal UIPrefabType DisplayType = UIPrefabType.原子;

        [ShowInInspector, HideReferenceObjectPicker, HideLabel, InlineProperty, ShowIf("DisplayType", UIPrefabType.设置)]
        internal UIPrefabSettingView SettingView = new UIPrefabSettingView();

        
    }

    internal enum UIPrefabType
    {
        原子,
        模组,
        设置
    }

    
    internal class UIPrefabSettingView
    {
        
        
        private const string UI_PREFAB_ROOT = "Assets/Prefabs/UI"; 
        
        [Title("UI预制件路径设置", Bold = false)]
        
        [LabelText("原子存放目录")] 
        [FolderPath(ParentFolder = UI_PREFAB_ROOT, RequireExistingPath = true)]
        [ShowInInspector]
        public string AtomPath{
            get => EditorPrefs.GetString(SettingConsts.UIPrefabs.ATOM_PATH, UI_PREFAB_ROOT + "/Atoms");
            set
            {
                if (ValidateAssetPath(ref value))
                {
                    EditorPrefs.SetString(SettingConsts.UIPrefabs.ATOM_PATH, value);
                } 
            }
        }
        
        [LabelText("模组存放目录")] 
        [FolderPath(ParentFolder = UI_PREFAB_ROOT, RequireExistingPath = true)]
        [ShowInInspector]
        public string ModulePath {
            get => EditorPrefs.GetString(SettingConsts.UIPrefabs.MODULE_PATH, UI_PREFAB_ROOT + "/Modules");
            set
            {
                if (ValidateAssetPath(ref value))
                {
                    EditorPrefs.SetString(SettingConsts.UIPrefabs.MODULE_PATH, value);
                } 
            }
        }
        

        private bool ValidateAssetPath(ref string path)
        {
            if (path.IndexOf("..") != 0)
            {
                return true;
            }
            
            UIPrefabWindowUtil.ShowNotification(new GUIContent("请选择项目内" + UI_PREFAB_ROOT + "的子目录"));
            path = "";
            return false;

        }
    }
    
    internal class UIPrefabItem
    {
        
    }
    
}