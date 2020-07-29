using UnityEditor;

namespace Editor.UIEditor
{
    public static class HelperManager
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            new DragHelper().Init();
            
            new SnapHelper().Init();
            
            new PackageHelper().Init();
            
            new SelectionHelper().Init();
        }
    }
}