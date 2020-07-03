
using UnityEditor;

namespace Editor.UIEditor.Assistant
{
    [InitializeOnLoad]
    public static class UIEditorAssistantSettingMenu
    {
        
        /// <summary>
        /// 显示网格
        /// </summary>
        [MenuItem(MenuDefine.GRID_VISIBLE, false,0)]
        private static void ShowCanvasGrid() => UIEditorAssistantSetting.GridVisible = !UIEditorAssistantSetting.GridVisible;
        [MenuItem(MenuDefine.GRID_VISIBLE, true)]
        private static bool UpdateShowCanvasGrid()
        {
            Menu.SetChecked(MenuDefine.GRID_VISIBLE, UIEditorAssistantSetting.GridVisible);
            return true;
        }
        
        /// <summary>
        /// 吸附网格
        /// </summary>
        [MenuItem(MenuDefine.GRID_SNAP)]
        private static void SnapGird() => UIEditorAssistantSetting.GridSnap = !UIEditorAssistantSetting.GridSnap;

        [MenuItem(MenuDefine.GRID_SNAP, true)]
        private static bool UpdateSnapGrid()
        {
            Menu.SetChecked(MenuDefine.GRID_SNAP, UIEditorAssistantSetting.GridSnap);
            return UIEditorAssistantSetting.GridVisible;
        }
        
        
        /// <summary>
        /// 显示引导线
        /// </summary>
        [MenuItem(MenuDefine.GUIDE_VISIBLE, false, 1)]
        private static void ShowGuideline() => UIEditorAssistantSetting.GuideVisible = !UIEditorAssistantSetting.GuideVisible;
        [MenuItem(MenuDefine.GUIDE_VISIBLE, true)]
        private static bool UpdateShowGuideLine()
        {
            Menu.SetChecked(MenuDefine.GUIDE_VISIBLE, UIEditorAssistantSetting.GuideVisible);
            return true;
        }
        

    }
}