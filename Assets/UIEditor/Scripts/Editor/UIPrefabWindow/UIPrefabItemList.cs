using Sirenix.OdinInspector;

namespace Editor.UIEditor.PrefabWindow
{
    public class UIPrefabItemList
    {
        private string _itemListFolder;
        public string ItemListFolder
        {
            get => _itemListFolder;
            set
            {
                _itemListFolder = value;
                FindItemStr = "";
            }
        }
        [Button("刷新列表")]
        private void OnRefresh()
        {
            
        }
        
        [HorizontalGroup("searchArea")]
        [ShowInInspector, HideLabel, SuffixLabel("请输入搜索关键字", true)]
        private string FindItemStr;
        
        [HorizontalGroup("searchArea")]
        [Button("搜索", ButtonSizes.Small)]
        private void OnFind()
        {
            
        }
    }
}