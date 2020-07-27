using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIEditor.Scripts.Editor.Helper.Utils
{
    public class GUIDUtil
    {
        private static readonly Type ObjectType = typeof(Object);
        private static readonly Type GameObjectType = typeof(GameObject);
        private static readonly Type ComponentType = typeof(Component);
        /// <summary>
        /// 通过guid查找asset资源
        /// </summary>
        /// <param name="guid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GUIDToObject<T>(string guid) where T : Object
        {
            Object obj = null;
            
            if (!string.IsNullOrEmpty(guid))
            {
                var id = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid)).GetInstanceID();
                if (id == 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    obj = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath(path, ObjectType);
                }
                else
                {
                    obj = EditorUtility.InstanceIDToObject(id);
                }
            }
            
            if (obj == null) return null;
            var tType = typeof(T);
            var objType = obj.GetType();
            if (objType == tType || objType.IsSubclassOf(tType)) return obj as T;

            if (objType != GameObjectType || !tType.IsSubclassOf(ComponentType)) return null;
            
            return (obj as GameObject)?.GetComponent(typeof(T)) as T;
        }

    }
}