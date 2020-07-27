
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor.Assistant
{
    [InitializeOnLoad]
    public static class UIEditorAssistantSettingMenu
    {
        
        /// <summary>
        /// 显示网格
        /// </summary>
        [MenuItem(MenuDefine.GRID_VISIBLE, false,0)]
        private static void ShowCanvasGrid() => HelperSettings.GridVisible = !HelperSettings.GridVisible;
        [MenuItem(MenuDefine.GRID_VISIBLE, true)]
        private static bool UpdateShowCanvasGrid()
        {
            Menu.SetChecked(MenuDefine.GRID_VISIBLE, HelperSettings.GridVisible);
            return true;
        }
        
        /// <summary>
        /// 吸附网格
        /// </summary>
        [MenuItem(MenuDefine.GRID_SNAP)]
        private static void SnapGird() => HelperSettings.GridSnap = !HelperSettings.GridSnap;

        [MenuItem(MenuDefine.GRID_SNAP, true)]
        private static bool UpdateSnapGrid()
        {
            Menu.SetChecked(MenuDefine.GRID_SNAP, HelperSettings.GridSnap);
            return HelperSettings.GridVisible;
        }
        
        
        /// <summary>
        /// 显示引导线
        /// </summary>
        [MenuItem(MenuDefine.GUIDE_VISIBLE, false, 1)]
        private static void ShowGuideline() => HelperSettings.GuideVisible = !HelperSettings.GuideVisible;
        [MenuItem(MenuDefine.GUIDE_VISIBLE, true)]
        private static bool UpdateShowGuideLine()
        {
            Menu.SetChecked(MenuDefine.GUIDE_VISIBLE, HelperSettings.GuideVisible);
            return true;
        }
        
        /// <summary>
        /// 打开辅助设置
        /// </summary>
        private static UIHelperSettingWindow _helperSettingWin;
        [MenuItem(MenuDefine.SHOW_SETTINGS, false, 99)]
        private static void ShowSettingWindow()
        {
            if(_helperSettingWin == null) {
                var window = _helperSettingWin = EditorWindow.GetWindow<UIHelperSettingWindow>();
                window.titleContent = new GUIContent("UI编辑器辅助设置");
                _helperSettingWin.maxSize = _helperSettingWin.minSize = UIHelperSettingWindow.WindowSize;
                
            }
            _helperSettingWin.Show();
            _helperSettingWin.Focus();
        }
        
        private static UIPrefabPreviewWindow prefabPreviewWin;
        [MenuItem(MenuDefine.SHOW_PREFAB_WIN, false, 9)]
        private static void ShowPrefabPreviewWindow()
        {
            if(prefabPreviewWin == null) {
                prefabPreviewWin = EditorWindow.GetWindow<UIPrefabPreviewWindow>(false, "UI预制件管理器", true);
                prefabPreviewWin.autoRepaintOnSceneChange = true;
            }
            prefabPreviewWin.Show();
            prefabPreviewWin.Focus();
        }
    }
}