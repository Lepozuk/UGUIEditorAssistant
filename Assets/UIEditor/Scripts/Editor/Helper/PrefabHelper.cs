
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using static UnityEditor.DragAndDrop;

namespace Editor.UIEditor {
	
	public class PrefabWin : EditorWindow
	{
		private static PrefabWin Instance;
		[MenuItem(MenuDefine.SHOW_PREFAB_WIN, false, 9)]
		private static void OpenPrefabTool()
		{
			var prefabWin = EditorWindow.GetWindow<PrefabWin>(false, "UI预制件管理器", true);
			prefabWin.autoRepaintOnSceneChange = true;
            
			prefabWin.Show();
		}
		
		private class PrefabItem
		{
			public GameObject Prefab;
			public Texture Tex;
			public string Guid;
		}

		private const int CellPadding = 4;
	    private const int CellSize = 90;

		private int TabIndex = 0;
		
		private Vector2 GUIPos = Vector2.zero;
		private bool MouseIsInside = false;
		private GUIContent Content;
		private GUIStyle TextCenterStyle;

		/// <summary>
		/// 当前分类下所包含的UI预制件
		/// </summary>
		private readonly List<PrefabItem> PrefabItemsInCurrentTab = new List<PrefabItem>();
		
		/// <summary>
		/// 当前正在拖动的对象
		/// </summary>
		private GameObject[] DraggedObjects
		{
			get
			{
				if (objectReferences == null || objectReferences.Length == 0)
				{
					return null;
				}
				
				return objectReferences.Where(x=>x as GameObject).Cast<GameObject>().ToArray();
			}
			set
			{
				if (value != null)
				{
					PrepareStartDrag();
					objectReferences = value;
					DraggedObjectIsOurs = true;
				}
				else
				{
					AcceptDrag();
				}
			}
		}

		private readonly string PrefabHelperDragItemKey = "PrefabHelper.DragItem";
		private bool DraggedObjectIsOurs
		{
			get => GetGenericData(PrefabHelperDragItemKey) != null;
			set => SetGenericData(PrefabHelperDragItemKey, value);
		}
		
		
	    private string[] assetPath = new[] {"Assets/ResourcesAssets/UI/Atoms", "Assets/ResourcesAssets/UI/Modules"};
		

		private readonly string PreviewTextureSavePath = "../Temp/UIPrefabHelper/Preview/";
		
		private readonly List<int> _currentDisplayItems = new List<int>();
		private readonly List<PrefabItem> _selections = new List<PrefabItem>();
		private const int labelAreaHeight = 48;
		private string FindPrefabKey;
		
		private void OnEnable ()
		{
			if (Instance != null)
			{
				Instance.Close();
			}
			
			Instance = this;
			
			Load(TabIndex);

			Content = new GUIContent();
			
			TextCenterStyle = new GUIStyle();
			TextCenterStyle.alignment = TextAnchor.MiddleCenter;
			TextCenterStyle.padding = new RectOffset(2, 2, 2, 2);
			TextCenterStyle.clipping = TextClipping.Clip;
			TextCenterStyle.wordWrap = true;
			TextCenterStyle.stretchWidth = false;
			TextCenterStyle.stretchHeight = false;
			TextCenterStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 0f, 0f, 0.5f);
			TextCenterStyle.normal.background = null;
		}
		
		private void OnDisable ()
		{
			Instance = null;
			ClearItemCache();
		}

		private void OnSelectionChange()
		{
			Repaint();
		}

		private void Reset()
		{
			ClearItemCache();
		}

		private void ClearItemCache()
		{
			foreach (PrefabItem item in PrefabItemsInCurrentTab)
			{
				DestroyTexture(item);
			}

			PrefabItemsInCurrentTab.Clear();
		}
		
		private void AddGUID (string guid)
		{
			var go = PrefabHelperUtil.GUIDToObject<GameObject>(guid);

			if (go == null || !go.TryGetComponent<RectTransform>(out var rect))
			{
				return;
			}
	        
			var item = new PrefabItem { Prefab = go, Guid = guid };
	        
			PrefabItemsInCurrentTab.Add(item);
	        
		}

		private void RefreshItemTex(object obj)
		{
			if (this == null) return;
			var index = (int)obj;
			if (index >= PrefabItemsInCurrentTab.Count || index <= -1) return;
			
			var item = PrefabItemsInCurrentTab[index];
			
			GeneratePreview(item);
		}
		
		private PrefabItem FindItem (GameObject go)
		{
			foreach (var item in PrefabItemsInCurrentTab)
			{
				if (item.Prefab == go)
				{
					return item;
				}
			}

			return null;
		}

		private void Load (int tabIndex)
		{
			if (tabIndex < 0 || tabIndex >= assetPath.Length) return;

			ClearItemCache();
			
			var folderPath = assetPath[tabIndex];
			var guidArr = AssetDatabase.FindAssets("t:prefab", new string[] {folderPath});
			foreach (var guid in guidArr)
			{
				AddGUID(guid);
			}
		}

		private void DestroyTexture (PrefabItem prefabItem)
		{
			if (prefabItem == null ||  prefabItem.Tex == null) return;
			
			DestroyImmediate(prefabItem.Tex);
			prefabItem.Tex = null;
		}

		private void UpdateVisual ()
		{
			if (DraggedObjects == null) visualMode = DragAndDropVisualMode.Rejected;
			else if (DraggedObjectIsOurs) visualMode = DragAndDropVisualMode.Move;
			else visualMode = DragAndDropVisualMode.Copy;
		}
		
		private void GeneratePreview (PrefabItem item, bool isReCreate = true)
		{
			if (item == null || item.Prefab == null)
			{
				return;
			}
			
			var textureFileName = item.Prefab.name + ".png";
			var relativeSavePath = Path.Combine(Path.Combine(PreviewTextureSavePath, TabIndex.ToString()), textureFileName);
	        var previewTexPath = Path.Combine(Application.dataPath, relativeSavePath);
	        
	        if (!isReCreate && File.Exists(previewTexPath))
	        {
	            item.Tex = PrefabHelperUtil.LoadTextureInLocal(previewTexPath);
	            return;
	        }
	        
	        Debug.Log("CreateTex:" + textureFileName);
	        
	        var tex = PrefabHelperUtil.GetAssetPreview(item.Prefab);
	        if (tex == null)
	        {
	            return;
	        }
	        
	        DestroyTexture(item);
	        item.Tex = tex;
	        PrefabHelperUtil.SaveTextureToPNG(tex, previewTexPath);
		}

		private int GetCellUnderMouse (int spacingX, int spacingY)
		{
			var pos = Event.current.mousePosition + GUIPos;

			var topPadding = 24;
			var x = CellPadding;
			var y = CellPadding + topPadding;
			
			if (pos.y < y) return -1;

			var width = Screen.width - CellPadding + GUIPos.x;
			var height = Screen.height - CellPadding + GUIPos.y;
			var index = 0;

			
			for (; ; ++index)
			{
				Rect rect = new Rect(x, y, spacingX, spacingY);
				if (rect.Contains(pos))
				{
					break;
				}

				x += spacingX;

				if (x + spacingX > width)
				{
					if (pos.x > x)
					{
						return -1;
					}
					y += spacingY;
					x = CellPadding;

					if (y + spacingY > height)
					{
						return -1;
					}
				}
			}
			
			return index;
		}



		void OnGUI()
		{
			var newTab = TabIndex;
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Toggle(newTab == 0, "原子", EditorStyles.miniButton)) newTab = 0;
			if (GUILayout.Toggle(newTab == 1, "模组", EditorStyles.miniButton)) newTab = 1;
			if (GUILayout.Toggle(newTab == 99, "设置", EditorStyles.miniButton)) newTab = 99;
			GUILayout.EndHorizontal();
			
			if (newTab == 99)
			{
				
			} 
			else
			{
				if (TabIndex != newTab)
				{
					Load(newTab);
				}

				DrawItemList();
			}
			
			TabIndex = newTab;
		}
		
		private void DrawItemList() 
		{
			var currentEvent = Event.current;
			var type = currentEvent.type;
			
			var x = CellPadding;
			var y = CellPadding;
			
			var width = Screen.width - CellPadding;

			var spacingX = CellSize + CellPadding;
			var spacingY = spacingX + labelAreaHeight;
			
			

			var draggedGameObjects = DraggedObjects;
			var isDragging = draggedGameObjects != null;
			var indexUnderMouse = GetCellUnderMouse(spacingX, spacingY);
			
	  
			 if (draggedGameObjects != null)
			 {
		 		foreach (var gameObject in draggedGameObjects)
		 		{
		 			var result = FindItem(gameObject);
		 			
		 			if (result != null)
		 			{
		 				_selections.Add(result);
		 			}
		 		}
			 }
			
			 var eligibleToDrag = (currentEvent.mousePosition.y < Screen.height - 20);
			
			 switch (type)
			 {
				 case EventType.MouseDrag:
				 {
					 MouseIsInside = true;
					 if (indexUnderMouse != -1 && eligibleToDrag)
					 {
						 if (DraggedObjectIsOurs)
						 {
							 StartDrag("PrefabHelper");
						 }
						 currentEvent.Use();
					 }

					 break;
				 }
				 case EventType.DragUpdated:
				 {
					 MouseIsInside = true;
					 UpdateVisual();
					 currentEvent.Use();
					 break;
				 }
			 }

			 if (!MouseIsInside)
			{
				_selections.Clear();
				draggedGameObjects = null;
			}

			_currentDisplayItems.Clear();
			
			for (int i = 0; i < PrefabItemsInCurrentTab.Count; i++)
			{
				var item = PrefabItemsInCurrentTab[i];
				if (item.Prefab == null)
				{
					PrefabItemsInCurrentTab.RemoveAt(i);
					continue;
				}
				
				if (string.IsNullOrEmpty(FindPrefabKey) || 
				    (PrefabItemsInCurrentTab[i].Prefab != null && PrefabItemsInCurrentTab[i].Prefab.name.IndexOf(FindPrefabKey, System.StringComparison.CurrentCultureIgnoreCase) != -1))
				{
					_currentDisplayItems.Add(i);
				}
			}
			

			if (eligibleToDrag && type == EventType.MouseDown && indexUnderMouse > -1)
			{
				GUIUtility.keyboardControl = 0;

				if (currentEvent.button == 0 && indexUnderMouse < _currentDisplayItems.Count)
				{
					var index = _currentDisplayItems[indexUnderMouse];
					if (index != -1 && index < PrefabItemsInCurrentTab.Count)
					{
						_selections.Add(PrefabItemsInCurrentTab[index]);
						DraggedObjects = _selections.Select(item => item.Prefab).ToArray();
						draggedGameObjects = _selections.Select(item => item.Prefab).ToArray();
						currentEvent.Use();
					}
				}
			}
			
	        GUIPos = EditorGUILayout.BeginScrollView(GUIPos);
			{
				var normal = new Color(1f, 1f, 1f, 0f);
				
				for (var i = 0; i < _currentDisplayItems.Count; i++)
				{
					var index = _currentDisplayItems[i];
					var item = PrefabItemsInCurrentTab[index];

					if (item.Prefab == null)
					{
						PrefabItemsInCurrentTab.RemoveAt(index);
						continue;
					}

					var rect = new Rect(x, y, CellSize, CellSize);
					
					var inner = rect;
					inner.xMin += 2f;
					inner.xMax -= 2f;
					inner.yMin += 2f;
					inner.yMax -= 2f;
					rect.yMax -= 1f;
 
					Content.tooltip = !isDragging?item.Prefab.name:"";
					
					GUI.color = Color.white;
					GUI.backgroundColor = normal;
					
					if (GUI.Button(rect, Content, "Button"))
					{
						if (item != null && currentEvent.button == 1)
						{
	                        ContextMenu.AddItem("刷新", false, RefreshItemTex, index);
	                        ContextMenu.Show();
	                    }
					}

	                if (item.Tex == null)
					{
						GeneratePreview(item, false);
					}
	                
					GUI.DrawTexture(inner, item.Tex);
					
					GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
					GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
					GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, labelAreaHeight), item.Prefab.name, "ProgressBarBack");
					GUI.contentColor = Color.white;
					GUI.backgroundColor = Color.white;
					
					x += spacingX;

					if (x + spacingX <= width)
					{
						continue;
					}
					
					y += spacingY;
					x = CellPadding;
				}
				
	            GUILayout.Space(y + spacingY);
			}
			
	        EditorGUILayout.EndScrollView();
	        
	        //// 搜索过滤
	        GUILayout.BeginHorizontal();
	        {
	            FindPrefabKey = EditorGUILayout.TextField("", FindPrefabKey, "SearchTextField", GUILayout.Width(Screen.width - 20f));
	            if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
	            {
		            FindPrefabKey = "";
	                GUIUtility.keyboardControl = 0;
	            }

	        }
	        GUILayout.EndHorizontal();
			
		}
	}
	internal static class ContextMenu
	{
	    private static List<string> mEntries = new List<string>();
	    private static GenericMenu mMenu;

	    public static void AddItem(string item, bool isChecked, GenericMenu.MenuFunction2 callback, object args)
	    {
	        if (callback != null)
	        {
		        if (mMenu == null)
		        {
			        mMenu = new GenericMenu();
		        }
	            var count = 0;

	            foreach (var str in mEntries)
	            {
		            if (str == item)
		            {
			            ++count;
		            }
	            }
	            
	            mEntries.Add(item);

	            if (count > 0)
	            {
		            item += " [" + count + "]";
	            }
	            
	            mMenu.AddItem(new GUIContent(item), isChecked, callback, args);
	        }
	        else
	        {
		        AddDisabledItem(item);
	        }
		}

        public static void Show()
        {
	        if (mMenu == null) return;
	        
	        mMenu.ShowAsContext();
	        mMenu = null;
	        mEntries.Clear();
        }

        private static void AddDisabledItem(string item)
        {
	        if (mMenu == null)
	        {
		        mMenu = new GenericMenu();
	        }
	        
	        mMenu.AddDisabledItem(new GUIContent(item));
        }
	}
}