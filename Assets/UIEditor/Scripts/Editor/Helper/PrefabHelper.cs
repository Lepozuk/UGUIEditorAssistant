
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using UnityEditorInternal.VersionControl;
using static UnityEditor.DragAndDrop;

namespace Editor.UIEditor {
	
	[ExecuteInEditMode]
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
			public Texture ListTex;
			public Texture PreviewTex;
			public string Guid;

			public Rect ListItemRect;
		}

		private const int TopPadding = 24;
		private const int CellPadding = 4;
	    private const int CellSize = 90;
	    
		private int TabIndex = 0;
		
		private Vector2 GUIPos = Vector2.zero;
		
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
		
		private readonly string PreviewTextureSavePath = "../Temp/PreviewCache/UIEditor";
		
		private readonly List<int> _currentDisplayItems = new List<int>();
		private readonly List<PrefabItem> _selections = new List<PrefabItem>();
		private const int labelAreaHeight = 48;
		private string FindPrefabKey;
		
		private string[] AssetPaths;

		private const string ATOM_PATH = "UIPrefabHelper.AtomPath";
		private const string MODULE_PATH = "UIPrefabHelper.ModulePath";
		private void InitAssetPaths()
		{
			AssetPaths = new[]
			{
				EditorPrefs.GetString(ATOM_PATH, "Assets/ResourcesAssets/UI/Atoms"),
				EditorPrefs.GetString(MODULE_PATH, "Assets/ResourcesAssets/UI/Modules")
			};
		}
		
		private void OnEnable ()
		{
			if (Instance != null)
			{
				Instance.Close();
			}
			
			Instance = this;

			InitAssetPaths();
			
			TabIndex = 0;
			Load(TabIndex);
			
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
				DestroyTexture(item.ListTex);
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
			if (tabIndex < 0 || tabIndex >= AssetPaths.Length) return;

			ClearItemCache();
			
			var folderPath = AssetPaths[tabIndex];
			
			
			var guidArr = AssetDatabase.FindAssets("t:prefab", new string[] {folderPath});
			foreach (var guid in guidArr)
			{
				AddGUID(guid);
			}
		}

		private void DestroyTexture (Texture tex)
		{
			if (tex == null) return;
			
			DestroyImmediate(tex);
			tex = null;
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
			
			var textureFileName = item.Prefab.name + "_" + item.Guid;
			var relativeSavePath = Path.Combine(PreviewTextureSavePath,  textureFileName);
			
	        var listTexPath = Path.Combine(Application.dataPath, relativeSavePath + "_s.png");
	        var prevTexPath = Path.Combine(Application.dataPath, relativeSavePath + "_l.png");
	        if (isReCreate) DestroyTexture(item.ListTex);
	        item.ListTex = GenerateTexture(listTexPath, isReCreate, item.Prefab, PrefabHelperUtil.GetUIPrefabListTexture);
	        
	        if (isReCreate) DestroyTexture(item.PreviewTex);
	        item.PreviewTex = GenerateTexture(prevTexPath, isReCreate, item.Prefab, PrefabHelperUtil.GetUIPrefabPreviewTexture);
		}

		private Texture GenerateTexture(string texFilePath, bool isReCreate, GameObject prefab, Func<GameObject, Texture> generateFactory)
		{
			if (!isReCreate && File.Exists(texFilePath))
			{
				return PrefabHelperUtil.LoadTextureInLocal(texFilePath);;
			}

			var tex = generateFactory.Invoke(prefab);
			if (tex == null)
			{
				return null;
			}
			PrefabHelperUtil.SaveTextureToPNG(tex, texFilePath);
			
			return tex;
		}
		

		private int GetCellUnderMouse (int spacingX, int spacingY)
		{
			var pos = Event.current.mousePosition + GUIPos;

			
			var x = CellPadding;
			var y = CellPadding + TopPadding;
			
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

					if (y > height)
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
				DrawPathSelector();
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
		
		
		private void DrawPathSelector()
		{
			DrawPathArea(0, ATOM_PATH, "原子");
			DrawPathArea(1, MODULE_PATH, "模组");
		}

		private void DrawPathArea(int index, string editorPerfKey, string typeDef)
		{
			GUILayout.Space(4);
			//// 原子目录
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField(typeDef + "目录", AssetPaths[index], EditorStyles.label);
				if (GUILayout.Button("修改", EditorStyles.miniButton, GUILayout.Width(60f)))
				{
					var path = EditorUtility.OpenFolderPanel("请选择"+typeDef+"的目录", Application.dataPath, "");
					if (path.StartsWith(Application.dataPath))
					{
						path = "Assets" + path.Substring(Application.dataPath.Length);
						EditorPrefs.SetString(editorPerfKey, path);
						AssetPaths[index] = path;
					}
					else if(!string.IsNullOrEmpty(path))
					{
						ShowNotification(new GUIContent("请选择当前项目中Assets的中的目录"));	
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		
		
		private float lastMouseDownTimestamp = 0f;
		private const float DOUBLE_CLICK_CHECK_VALUE = 0.05f;
		private const float SHOW_PREVIEW_TEXTURE_INTERVAL = 1f;
		
		private float HoverTimeDelta = 0f;
		private PrefabItem CurrentHoverItem;
		private PrefabItem LastHoverItem;
		private bool needDrawHoverToolTip;
		
		
		private bool GetCurrentHoverItem(int indexUnderMouse, out PrefabItem item)
		{
			item = null;
			if(indexUnderMouse < 0 || indexUnderMouse>=_currentDisplayItems.Count) return false;
			
			var index = _currentDisplayItems[indexUnderMouse];
			if (index != -1 && index < PrefabItemsInCurrentTab.Count)
			{
				item = PrefabItemsInCurrentTab[index];
			}
			
			return item != null;
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
			
			var winRect = new Rect(0,0, Screen.width, Screen.height);
			
			var indexUnderMouse = GetCellUnderMouse(spacingX, spacingY);
			
			var eligibleToDrag = (currentEvent.mousePosition.y < Screen.height - 20);
			
			var nowTime = Time.time;

			
			_selections.Clear();
			
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

			
			var hasHoverItem = GetCurrentHoverItem(indexUnderMouse, out var hoveredPrefabItem);
			CurrentHoverItem = hoveredPrefabItem;
			
			switch (type)
			{
				case EventType.MouseDown:
				{
					if (nowTime - lastMouseDownTimestamp < DOUBLE_CLICK_CHECK_VALUE)
					{
						if (hasHoverItem)
						{
							Debug.Log("Now Open: " + AssetDatabase.GetAssetPath(hoveredPrefabItem.Prefab));
							AssetDatabase.OpenAsset(hoveredPrefabItem.Prefab);
						}
					}
					lastMouseDownTimestamp = nowTime;
					break;
				}
				case EventType.MouseDrag:
				{
					if (hasHoverItem && eligibleToDrag)
					{
						if (DraggedObjectIsOurs)
						{
							StartDrag(hoveredPrefabItem.Prefab.name);
						}
						currentEvent.Use();
					}
					
					break;
				}
				case EventType.DragUpdated:
				{
					UpdateVisual();
					currentEvent.Use();
					break;
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

					var itemBoxRect = new Rect(x, y, CellSize, CellSize);
					var iconRect = itemBoxRect;
					iconRect.xMin += 2f;
					iconRect.xMax -= 2f;
					iconRect.yMin += 2f;
					iconRect.yMax -= 2f;
					itemBoxRect.yMax -= 1f;
					
					var itemBoxRectCheck = new Rect(itemBoxRect);
					itemBoxRectCheck.y -=  GUIPos.y;
					if (winRect.Overlaps(itemBoxRectCheck))
					{
						if (item.ListTex == null)
						{
							GeneratePreview(item, false);
						}
						
						//Content.tooltip = !isDragging ? item.Prefab.name : "";
						//Content.image = item.PreviewTex;
						
						GUI.color = Color.white;
						GUI.backgroundColor = normal;
						
						if (GUI.Button(itemBoxRect, new GUIContent(), "Button"))
						{
							if (item != null && currentEvent.button == 1)
							{
								ContextMenu.AddItem("刷新", false, RefreshItemTex, index);
								ContextMenu.Show();
							}
						}

						item.ListItemRect = itemBoxRectCheck;
						
						GUI.DrawTexture(iconRect, PrefabHelperUtil.BackdropTexture);
						GUI.DrawTexture(iconRect, item.ListTex);
					}

					var labelRect = new Rect(itemBoxRect.x, itemBoxRect.y + itemBoxRect.height, itemBoxRect.width, labelAreaHeight);
					var labelRectCheck = new Rect(labelRect);
					labelRectCheck.y -=  GUIPos.y;
					if(winRect.Overlaps(labelRectCheck)) 
					{
						GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
						GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
						GUI.Label(labelRect, item.Prefab.name, "ProgressBarBack");
						GUI.contentColor = Color.white;
						GUI.backgroundColor = Color.white;
					}
					
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
	        
	        if(needDrawHoverToolTip && CurrentHoverItem != null)
	        {
		        var previewTex = CurrentHoverItem.PreviewTex;
		        var pos = Event.current.mousePosition;
		        pos.x -= previewTex.width * 0.5f;
			        
		        var toolTipRect = new Rect(pos.x, pos.y, previewTex.width, previewTex.height);
		        
		        var outlineRect = new Rect(toolTipRect);
		        outlineRect.x -= 4;
		        outlineRect.y -= 4;
		        outlineRect.width += 8;
		        outlineRect.height += 8;
		        
		        GUI.DrawTexture(outlineRect, PrefabHelperUtil.BorderTexture);
		        GUI.DrawTexture(toolTipRect, PrefabHelperUtil.BackdropTexture);
		        GUI.DrawTexture(toolTipRect, previewTex);
	        }
	        
	        
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

		private void Update()
		{
			if (CurrentHoverItem == null)
			{
				HoverTimeDelta = 0f;
				LastHoverItem = null;
				needDrawHoverToolTip = false;
				return;
			}
			
			if(CurrentHoverItem == LastHoverItem)
			{
				HoverTimeDelta += Time.deltaTime;
				//// 检查是否需要显示预览的大贴图
				needDrawHoverToolTip = HoverTimeDelta > SHOW_PREVIEW_TEXTURE_INTERVAL;
				if (needDrawHoverToolTip)
				{
					Repaint();
				}
			}
			else
			{
				needDrawHoverToolTip = false;
				HoverTimeDelta = 0f;
				LastHoverItem = CurrentHoverItem;
			}

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