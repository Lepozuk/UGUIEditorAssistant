
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using Editor.UIEditor;
using static UnityEditor.DragAndDrop;

namespace Editor.UIEditor {
	
	
	public class UIPrefabPreviewWindow : EditorWindow
	{
		private const int TOP_PADDING = 24;
		
		private const int CELL_PADDING = 4;
		
	    private const int CELL_SIZE = 90;
	    
		private const int LABEL_AREA_HEIGHT = 48;
		
		private const float DOUBLE_CLICK_CHECK_VALUE = 0.05f;

	    /// <summary>
		/// 当前分类下所包含的UI预制件
		/// </summary>
		private readonly List<PrefabItem> PrefabItemsInCurrentTab = new List<PrefabItem>();

	    private readonly Dictionary<string, List<PrefabItem>> PrefabItemsInFolders = new Dictionary<string, List<PrefabItem>>();
	    
		private readonly List<PrefabItem> PrefabItemsByFiltered = new List<PrefabItem>();
		
		private readonly List<int> CurrentDisplayItems = new List<int>();
		
		
		private string mFindPrefabKey;
		
		private string[] mAssetPaths;

		private Vector2 mLastMousePos;
		
		private int mTabIndex = 0;
		
		private Vector2 mListPos = Vector2.zero;
		
		private float mLastMouseDownTime = 0f;
		
		private PrefabItem mCurrHoverItem;
		
		private PrefabItem mLastHoverItem;
		
		private bool DraggedObjectIsOurs
		{
			get => GetGenericData("PrefabHelper.DragItem") != null;
			set => SetGenericData("PrefabHelper.DragItem", value);
		}
		
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
		
		private void InitAssetPaths()
		{
			mAssetPaths = new[]
			{
				HelperSettings.AtomPath,
				HelperSettings.ModulePath
			};
		}
		
		private void OnEnable ()
		{
			InitAssetPaths();
			
			mTabIndex = 0;
			Load(mTabIndex);
		}

		private void OnFocus()
		{
			InitAssetPaths();
			Load(mTabIndex);
		}

		private void OnDisable ()
		{
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
			foreach (var item in PrefabItemsInCurrentTab)
			{
				DestroyTexture(ref item.ListTex);
			}
			PrefabItemsInCurrentTab.Clear();

			foreach (var pair in PrefabItemsInFolders)
			{
				pair.Value.Clear();
			}
			PrefabItemsInFolders.Clear();
			
		}

		private void AddGUID(string guid, string fromRootFolder)
		{
			var go = PrefabUtil.GUIDToObject<GameObject>(guid);

			if (go == null || !go.TryGetComponent<RectTransform>(out var rect))
			{
				return;
			}
	        
			PrefabItemsInCurrentTab.Add(new PrefabItem { Prefab = go, Guid = guid });

			var relativePath = AssetDatabase.GetAssetPath(go).Substring(fromRootFolder.Length + 1);
			var pathArr = relativePath.Split(Path.DirectorySeparatorChar);
			if (!PrefabItemsInFolders.TryGetValue(pathArr[0], out var list))
			{
				list = new List<PrefabItem>();
				PrefabItemsInFolders.Add(pathArr[0], list);
			}

		}

		private void RefreshItemTex(object obj)
		{
			if (this == null)
			{
				return;
			}
			
			var index = (int)obj;
			if (index >= PrefabItemsInCurrentTab.Count || index <= -1)
			{
				return;
			}
			
			var item = PrefabItemsInCurrentTab[index];
			
			GeneratePreviewTex(item);
		}
		
		private void Load (int tabIndex)
		{
			if (tabIndex < 0 || tabIndex >= mAssetPaths.Length) return;

			ClearItemCache();
			
			var folderPath = mAssetPaths[tabIndex];
			
			
			var guidArr = AssetDatabase.FindAssets("t:prefab", new string[] {folderPath});
			foreach (var guid in guidArr)
			{
				AddGUID(guid, folderPath);
			}
		}

		private void DestroyTexture (ref Texture tex)
		{
			if (tex == null)
			{
				return;
			}
			
			DestroyImmediate(tex);
			
			tex = null;
		}

		private void UpdateVisual ()
		{
			if (DraggedObjects == null)
			{
				visualMode = DragAndDropVisualMode.Rejected;
			}
			else if (DraggedObjectIsOurs)
			{
				visualMode = DragAndDropVisualMode.Move;
			}
			else
			{
				visualMode = DragAndDropVisualMode.Copy;
			}
		}
		
		private void GeneratePreviewTex (PrefabItem item, bool isReCreate = true)
		{
			if (item == null || item.Prefab == null)
			{
				return;
			}
			
			var textureFileName = item.Prefab.name + "_" + item.Guid;
			var relativeSavePath = Path.Combine("../Caches/Textures/UIHelper/PrefabPreview",  textureFileName);
			
	        var listTexPath = Path.Combine(Application.dataPath, relativeSavePath + "_s.png");
	        var prevTexPath = Path.Combine(Application.dataPath, relativeSavePath + "_l.png");
	        if (isReCreate)
	        {
		        DestroyTexture(ref item.ListTex);
	        }
	        item.ListTex = GenerateTexture(listTexPath, isReCreate, item.Prefab, PrefabUtil.GetUIPrefabListTexture);

	        if (isReCreate)
	        {
		        DestroyTexture(ref item.PreviewTex);
	        }
	        item.PreviewTex = GenerateTexture(prevTexPath, isReCreate, item.Prefab, PrefabUtil.GetUIPrefabPreviewTexture);
		}

		private Texture GenerateTexture(string texFilePath, bool isReCreate, GameObject prefab, Func<GameObject, Texture> generateFactory)
		{
			if (!isReCreate && File.Exists(texFilePath))
			{
				return PrefabUtil.LoadTextureInLocal(texFilePath);;
			}

			var tex = generateFactory.Invoke(prefab);
			if (tex == null)
			{
				return null;
			}
			PrefabUtil.SaveTextureToPNG(tex, texFilePath);
			
			return tex;
		}
		

		private int GetCellUnderMouse (int spacingX, int spacingY)
		{
			var pos = Event.current.mousePosition + mListPos;

			
			var x = CELL_PADDING;
			var y = CELL_PADDING + TOP_PADDING;
			
			if (pos.y < y) return -1;

			var width = Screen.width - CELL_PADDING + mListPos.x;
			var height = Screen.height - CELL_PADDING + mListPos.y;
			var index = 0;
			
			
			for (; ; ++index)
			{
				var rect = new Rect(x, y, spacingX, spacingY);
				if (rect.Contains(pos))
				{
					break;
				}

				x += spacingX;
				if (x + spacingX <= width)
				{
					continue;
				}

				if (pos.x > x)
				{
					return -1;
				}
				y += spacingY;
				x = CELL_PADDING;

				if (y > height)
				{
					return -1;
				}
			}
			
			return index;
		}



		void OnGUI()
		{
			var newTab = mTabIndex;
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Toggle(newTab == 0, "原子", EditorStyles.miniButton)) newTab = 0;
			if (GUILayout.Toggle(newTab == 1, "模组", EditorStyles.miniButton)) newTab = 1;
			GUILayout.EndHorizontal();

			mLastMousePos = Event.current.mousePosition;
			
			if (mTabIndex != newTab)
			{
				Load(newTab);
			}
			DrawItemList();
			
			mTabIndex = newTab;
			
		}
		
		
		private bool GetCurrentHoverItem(int indexUnderMouse, out PrefabItem item)
		{
			item = null;
			if(indexUnderMouse < 0 || indexUnderMouse>=CurrentDisplayItems.Count) return false;
			
			var index = CurrentDisplayItems[indexUnderMouse];
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
			
			var x = CELL_PADDING;
			var y = CELL_PADDING;
	
			
			var width = Screen.width - CELL_PADDING;

			var spacingX = CELL_SIZE + CELL_PADDING;
			var spacingY = spacingX + LABEL_AREA_HEIGHT;
			
			var winRect = new Rect(0,0, Screen.width, Screen.height);
			
			var indexUnderMouse = GetCellUnderMouse(spacingX, spacingY);
			
			var eligibleToDrag = (currentEvent.mousePosition.y < Screen.height - 20);
			
			var nowTime = Time.time;

			
			PrefabItemsByFiltered.Clear();
			
			CurrentDisplayItems.Clear();
			
			for (int i = 0; i < PrefabItemsInCurrentTab.Count; i++)
			{
				var item = PrefabItemsInCurrentTab[i];
				if (item.Prefab == null)
				{
					PrefabItemsInCurrentTab.RemoveAt(i);
					continue;
				}
				
				if (string.IsNullOrEmpty(mFindPrefabKey) || 
				    (PrefabItemsInCurrentTab[i].Prefab != null && PrefabItemsInCurrentTab[i].Prefab.name.IndexOf(mFindPrefabKey, System.StringComparison.CurrentCultureIgnoreCase) != -1))
				{
					CurrentDisplayItems.Add(i);
				}
			}

			
			var hasHoverItem = GetCurrentHoverItem(indexUnderMouse, out var hoveredPrefabItem);
			mCurrHoverItem = hoveredPrefabItem;
			
			switch (type)
			{
				case EventType.MouseDown:
				{
					if (nowTime - mLastMouseDownTime < DOUBLE_CLICK_CHECK_VALUE)
					{
						if (hasHoverItem)
						{
							Debug.Log("Now Open: " + AssetDatabase.GetAssetPath(hoveredPrefabItem.Prefab));
							AssetDatabase.OpenAsset(hoveredPrefabItem.Prefab);
						}
					}
					mLastMouseDownTime = nowTime;
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
				default: break;
			}
			

			if (eligibleToDrag && type == EventType.MouseDown && indexUnderMouse > -1)
			{
				GUIUtility.keyboardControl = 0;

				if (currentEvent.button == 0 && indexUnderMouse < CurrentDisplayItems.Count)
				{
					var index = CurrentDisplayItems[indexUnderMouse];
					if (index != -1 && index < PrefabItemsInCurrentTab.Count)
					{
						DraggedObjects = new []{ PrefabItemsInCurrentTab[index].Prefab };
						currentEvent.Use();
					}
				}
			}
			
	        mListPos = EditorGUILayout.BeginScrollView(mListPos);
			{
				var normal = new Color(1f, 1f, 1f, 0f);
				
				for (var i = 0; i < CurrentDisplayItems.Count; i++)
				{
					var index = CurrentDisplayItems[i];
					var item = PrefabItemsInCurrentTab[index];

					if (item.Prefab == null)
					{
						PrefabItemsInCurrentTab.RemoveAt(index);
						continue;
					}

					var itemBoxRect = new Rect(x, y, CELL_SIZE, CELL_SIZE);
					var iconRect = itemBoxRect;
					iconRect.xMin += 2f;
					iconRect.xMax -= 2f;
					iconRect.yMin += 2f;
					iconRect.yMax -= 2f;
					itemBoxRect.yMax -= 1f;
					
					var itemBoxRectCheck = new Rect(itemBoxRect);
					itemBoxRectCheck.y -=  mListPos.y;
					
					if (winRect.Overlaps(itemBoxRectCheck))
					{
						if (item.ListTex == null)
						{
							GeneratePreviewTex(item, false);
						}
						
						GUI.color = Color.white;
						GUI.backgroundColor = normal;
						
						if (GUI.Button(itemBoxRect, new GUIContent(), "Button"))
						{
							if (item != null && currentEvent.button == 1)
							{
								UIHelperContextMenu.AddItem("刷新", false, RefreshItemTex, index);
								UIHelperContextMenu.Show();
							}
						}

						item.ListItemRect = itemBoxRectCheck;
						
						GUI.DrawTexture(iconRect, PrefabUtil.BackdropTexture);
						GUI.DrawTexture(iconRect, item.ListTex);
					}
					else
					{
						item.ListItemRect = Rect.zero;
					}

					var labelRect = new Rect(itemBoxRect.x, itemBoxRect.y + itemBoxRect.height, itemBoxRect.width, LABEL_AREA_HEIGHT);
					var labelRectCheck = new Rect(labelRect);
					labelRectCheck.y -=  mListPos.y;
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
					x = CELL_PADDING;
				}
				
	            GUILayout.Space(y + spacingY);
			}
			
	        EditorGUILayout.EndScrollView();
	        
	        //// 搜索过滤
	        GUILayout.BeginHorizontal();
	        {
	            mFindPrefabKey = EditorGUILayout.TextField("", mFindPrefabKey, "SearchTextField", GUILayout.Width(Screen.width - 20f));
	            if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
	            {
		            mFindPrefabKey = "";
	                GUIUtility.keyboardControl = 0;
	            }

	        }
	        GUILayout.EndHorizontal();
			
		}
		
		private void Update()
		{
			
			if (mCurrHoverItem == null)
			{
				PrefabTipPopup.Hide();
			}
			else
			{
				if (mLastHoverItem != mCurrHoverItem)
				{
					PrefabTipPopup.Hide();
				}

				
				
				PrefabTipPopup.Show(mCurrHoverItem, Time.fixedDeltaTime, this.position.position + mLastMousePos);
				GetWindow<UIPrefabPreviewWindow>().Focus();
			}

			mLastHoverItem = mCurrHoverItem;
		}
	}
}

class PrefabItem
{
	public GameObject Prefab;
	public Texture ListTex;
	public Texture PreviewTex;
	public string Guid;
	public Rect ListItemRect;
}

class PrefabTipPopup : EditorWindow
{
	
	private static readonly float SHOW_TIME_INTERVAL = 0.5f;
	private static PrefabTipPopup _winInst;
	
	private static float _showTimeDelta = 0f;
	public static void Hide()
	{
		_showTimeDelta = 0f;
		
		if (_winInst != null)
		{
			_winInst.Close();
			_winInst._item = null;
		}
		
	}

	
	public static void Show(PrefabItem item, float deltaTime, Vector2 pos)
	{
		_showTimeDelta += deltaTime;
		
		if (_showTimeDelta < SHOW_TIME_INTERVAL|| item == null || item.PreviewTex == null )
		{
			return;
		}
		
		var size = new Vector2(item.PreviewTex.width + 8, item.PreviewTex.height + 8);
		
		if(_winInst == null) {
			_winInst = CreateInstance<PrefabTipPopup>();
		}

		_winInst.position = new Rect(pos.x, pos.y, size.x, size.y);
		
		_winInst.maxSize = _winInst.minSize = size;
		_winInst._item = item;
		_winInst.ShowPopup();
		
	}

	private PrefabItem _item;


	public void OnGUI()
	{
		if (_item == null) return;
		
		var previewTex = _item.PreviewTex;
			        
		var toolTipRect = new Rect(4, 4, previewTex.width, previewTex.height);
		        
		var outlineRect = new Rect(toolTipRect);
		outlineRect.x -= 4;
		outlineRect.y -= 4;
		outlineRect.width += 8;
		outlineRect.height += 8;
		        
		GUI.DrawTexture(outlineRect, PrefabUtil.BorderTexture);
		GUI.DrawTexture(toolTipRect, PrefabUtil.BackdropTexture);
		GUI.DrawTexture(toolTipRect, previewTex);
	}
}