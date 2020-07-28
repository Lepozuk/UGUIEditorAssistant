using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    public class PackageHelper
    {
        private DragHelper mDrag;
        
        public void Init(DragHelper drag)
        {
            mDrag = drag;
            
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private SceneView mCurrScene;
        private void OnSceneGUI(SceneView obj)
        {
            mCurrScene = obj;
            var evt = Event.current;
            if (!(evt.button == 1 && evt.type == EventType.MouseUp))
            {
                return;
            }

            ShowPackageMenu();
            evt.Use();
        }
        private void ShowPackageMenu()
        {
            
            var objs = new List<GameObject>();
            
            Transform parentTrans = null;
            foreach (var obj in Selection.gameObjects)
            {
                if (CanvasUtil.TryGetRectTransform(obj, out var rect))
                {
                    if(parentTrans != null && parentTrans != obj.transform.parent)
                    {
                        EditorUtility.DisplayDialog("出错了", "不能跨节点打包", "好的");
                        return;
                    }
                    
                    parentTrans = obj.transform.parent;
                    objs.Add(obj);
                }
            }

            if (objs.Count <= 0)
            {
                return;
            }
            
            UIHelperContextMenu.AddItem("打包成原子", false, PackageAtom, objs);
            UIHelperContextMenu.AddItem("打包成模组", false, PackageModule, objs);
            if (objs.Count == 1 && objs[0].transform.childCount > 0)
            {
                UIHelperContextMenu.AddSeparator("");
                UIHelperContextMenu.AddItem("打散", false, Unpack, objs[0]);
            }
            
            UIHelperContextMenu.Show();
        }

        private void Unpack(object arg)
        {
            var gameObj = (GameObject) arg;
            var currTrans = gameObj.transform;
            var toTrans = gameObj.transform.parent;

            if (PrefabUtil.IsPrefab(gameObj))
            {
                PrefabUtility.UnpackPrefabInstance(gameObj, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
            
            var childrenTransform = gameObj.transform.GetComponentsInChildren<Transform>(true);
            foreach (var trans in childrenTransform)
            {
                if (trans.parent != currTrans || trans == currTrans)
                {
                    continue;
                }
                trans.SetParent(toTrans, true);
            }
            
            GameObject.DestroyImmediate(gameObj);
        }

        private void PackageAtom(object arg) => SavePrefabWindow.ShowPopup("原子", CheckNameAlreadyExist, SavePrefab, new object[] { HelperSettings.AtomPath, arg });
        
        private void PackageModule(object arg) => SavePrefabWindow.ShowPopup("模组", CheckNameAlreadyExist, SavePrefab, new object[] {HelperSettings.ModulePath, arg});

       
        private bool CheckNameAlreadyExist(string name)
        {
            return false;
        }

        private void SavePrefab(string name, object param0)
        {
            object[] args = (object[])param0;
            var folderPath = (string)args[0];
            var gameobjects = (List<GameObject>) args[1];
            
            var fileName = Path.GetFileName(name);
            var path = Path.Combine(folderPath, name) + ".prefab";
            
            //
            Debug.Log("SavePrefab:'" + path  + "' with filename:'" + fileName + "'");

            if (MakeGroup(gameobjects, fileName, out var root))
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                AssetDatabase.Refresh();
                
                var prefabAsset = AssetDatabase.LoadMainAssetAtPath(path);
                var prefabObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                prefabObj.transform.SetParent(root.transform.parent);
                prefabObj.transform.localPosition = root.transform.localPosition;
                GameObject.DestroyImmediate(root);
                
            }
        }

        private bool MakeGroup(List<GameObject> gameObjects, string name, out GameObject groupRoot)
        {
            groupRoot = null;
            if (gameObjects == null || gameObjects.Count == 0)
            {
                return false;
            }
            
            groupRoot = new GameObject(name);
            
            Undo.IncrementCurrentGroup();
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Make prefab asset");
            Undo.RegisterCreatedObjectUndo(groupRoot, "Create prefab asset root object");
            

            var groupRect = groupRoot.AddComponent<RectTransform>();
            groupRect.transform.SetParent(gameObjects[0].transform.parent);
            
            var leftTopPos = new Vector2(99999, -99999);
            var rightBottomPos = new Vector2(-99999, 99999);
            foreach (var obj in gameObjects)
            {
                var bounds = PrefabUtil.GetBounds(obj);
                var boundMin= obj.transform.parent.InverseTransformPoint(bounds.min);
                var boundMax = obj.transform.parent.InverseTransformPoint(bounds.max);
                leftTopPos.x = Mathf.Min(leftTopPos.x, boundMin.x);
                leftTopPos.y = Mathf.Max(leftTopPos.y, boundMax.y);
                rightBottomPos.x = Mathf.Max(rightBottomPos.x, boundMax.x);
                rightBottomPos.y = Mathf.Min(rightBottomPos.y, boundMin.y);
            }
            
            groupRect.sizeDelta = new Vector2(rightBottomPos.x - leftTopPos.x, leftTopPos.y - rightBottomPos.y);
            leftTopPos.x += groupRect.sizeDelta.x / 2;
            leftTopPos.y -= groupRect.sizeDelta.y / 2;
            groupRect.localPosition = leftTopPos;
            groupRect.localScale = Vector3.one;

            //需要先生成好root节点和设置好它的坐标和大小才可以把选中的节点挂进来，注意要先排好序，不然层次就乱了
            GameObject[] sorted_objs = gameObjects.OrderBy(x => x.transform.GetSiblingIndex()).ToArray();
            for (int i = 0; i < sorted_objs.Length; i++)
            {
                Undo.SetTransformParent(sorted_objs[i].transform, groupRect, "Move child item to group");
            }
            
            Selection.activeGameObject = groupRoot;
            
            Undo.CollapseUndoOperations(undoGroupIndex);
            
            return true;
        }
        
    }
}

internal class SavePrefabWindow : EditorWindow
{
    private static SavePrefabWindow mWin;
    public static void ShowPopup(string type, Func<string, bool> checkCall, Action<string, object> callback, object callbackArgs = null)
    {
        if(mWin == null){
            var win = mWin = GetWindow<SavePrefabWindow>();
            win.maxSize = win.minSize = new Vector2(360, 120);
        }
        
        if (mWin)
        {
            mWin.mType = type;
            mWin.mCheckCall = checkCall;
            mWin.mCallback = callback;
            mWin.mCallbackArgs = callbackArgs;
            
            mWin.titleContent = new GUIContent("请给" + type + "取个名字");
            
            mWin.mPrefabName = "";
            mWin.Show();
            mWin.Focus();
        }
    }


    private string mType;
    private string mPrefabName;
    private string mSavePath;
    
    private Func<string, bool> mCheckCall;
    
    private Action<string, object> mCallback;
    private object mCallbackArgs;
    

    private void OnGUI() 
    {
        GUILayout.Space(10);

        var nameStyle = new GUIStyle(GUI.skin.textField);
        nameStyle.margin = new RectOffset(15,15,15,15);
        nameStyle.fontSize = 20;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.fixedHeight = 30f;
        
        GUILayout.Space(10);
        
        
        mPrefabName = GUILayout.TextField(mPrefabName, nameStyle);
        var fixName = Regex.Replace(mPrefabName, @"[^(a-zA-Z0-9)_\-\\\/]", "");
        if (fixName != mPrefabName)
        {
            mPrefabName = fixName;
            Debug.Log("Fix Name: " + fixName);
            Repaint();
            return;
        }
        GUILayout.Space(10);
        
        var buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fixedWidth = Screen.width * 0.5f;
        buttonStyle.fixedHeight = 24;
        buttonStyle.margin = new RectOffset(Mathf.CeilToInt(Screen.width*0.25f),Mathf.CeilToInt(Screen.width*0.25f),0,0);
        
        GUILayout.BeginHorizontal(new GUIStyle() {alignment = TextAnchor.MiddleCenter});
        {
            if (GUILayout.Button("保存" + mType, buttonStyle) && OnClickSavePrefab())
            {
                Close();
            }
        }
        GUILayout.EndHorizontal();
    }
 
    private bool OnClickSavePrefab() {
         
        if (string.IsNullOrEmpty(mPrefabName)) {
            EditorUtility.DisplayDialog("出错了", "不输入个名字，不给保存哦", "好的");
            return false;
        }

        mPrefabName = mPrefabName.Trim();
        
        if (mCheckCall.Invoke(mPrefabName))
        {
            EditorUtility.DisplayDialog("出错了", "取的名字已经有人用了哦", "好的");
            return false;
        }
        mCallback?.Invoke(mPrefabName, mCallbackArgs);

        return true;

    }

    private void OnLostFocus()
    {
        Close();
    }

}