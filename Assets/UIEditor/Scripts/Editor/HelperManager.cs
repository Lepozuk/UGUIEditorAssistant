using UnityEditor;

namespace Editor.UIEditor
{
    public static class HelperManager
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            var dragHelper = new DragHelper();
            dragHelper.Init();
            
            var snapHelper = new SnapHelper();
            snapHelper.Init();
            
            var packageHelper = new PackageHelper();
            packageHelper.Init(dragHelper);
        }
    }
}