using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor.PrefabWindow
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
    
    internal class UIPrefabWindow : OdinEditorWindow
    {

        [ShowInInspector, EnumToggleButtons, HideLabel, OnValueChanged("OnDisplayTypeChanged")]
        private UIPrefabWindowDisplayType DisplayType = UIPrefabWindowDisplayType.原子;

        private void OnDisplayTypeChanged()
        {
            Debug.Log("Current:" + DisplayType);
            if (DisplayType != UIPrefabWindowDisplayType.设置)
            {
                string listFoler;
                switch (DisplayType)
                {
                    case UIPrefabWindowDisplayType.原子:
                        listFoler = settingView.AtomPath;
                        break;
                    case UIPrefabWindowDisplayType.模组:
                        listFoler = settingView.ModulePath;
                        break;
                    default:
                        return;
                }
                itemList.ItemListFolder = listFoler;
            }
        }
        
        [ShowInInspector, HideReferenceObjectPicker, HideLabel, InlineProperty, ShowIf("DisplayType", UIPrefabWindowDisplayType.设置)]
        private UIPrefabSettingView settingView = new UIPrefabSettingView();
        
        [ShowInInspector, HideReferenceObjectPicker, HideLabel, InlineProperty,HideIf("DisplayType", UIPrefabWindowDisplayType.设置)]
        private UIPrefabItemList itemList = new UIPrefabItemList();
        
        

        
    }
}