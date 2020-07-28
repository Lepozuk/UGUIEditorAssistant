using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    internal static class UIHelperContextMenu
    {
        private static List<string> mEntries = new List<string>();
        private static GenericMenu mMenu;

        public static void AddSeparator(string path)
        {
            if (mMenu == null)
            {
                mMenu = new GenericMenu();
            }
            mMenu.AddSeparator(path);
        }


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