using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor.UIEditor
{
    class ReadAndWriteProxy<T> : IPropertyFieldReadWrite
    {
        private readonly Action<string, T> _writeCall;
        private readonly Func<string, T, T> _readCall;
            
        public ReadAndWriteProxy(Action<string, T> writeCall, Func<string, T, T> readCall)
        {
            _readCall = readCall;
            _writeCall = writeCall;
        }

        public object Read(string key, object defaultValue) => _readCall.Invoke(key, (T)defaultValue);

        public void Write(string key, object value) => _writeCall.Invoke(key, (T)value);
    }

    static class HelperDefines
    {
        private static readonly Dictionary<Type, IPropertyFieldReadWrite> ReadWriteDefines = new Dictionary<Type, IPropertyFieldReadWrite>();

        
        static HelperDefines()
        {
            ReadWriteDefines.Add(typeof(string), new ReadAndWriteProxy<string>(EditorPrefs.SetString, EditorPrefs.GetString));
            ReadWriteDefines.Add(typeof(int), new ReadAndWriteProxy<int>(EditorPrefs.SetInt, EditorPrefs.GetInt));
            ReadWriteDefines.Add(typeof(float), new ReadAndWriteProxy<float>(EditorPrefs.SetFloat, EditorPrefs.GetFloat));
            ReadWriteDefines.Add(typeof(bool), new ReadAndWriteProxy<bool>(EditorPrefs.SetBool, EditorPrefs.GetBool));
        }

        public static bool Contains(Type t) => ReadWriteDefines.ContainsKey(t);
        public static IPropertyFieldReadWrite Get(Type t) => (Contains(t) ? ReadWriteDefines[t] : null);
    }
    
    internal static class PropertyReadWriteGenerator
    {
        public static bool GetOne(Type pType, string key, out FieldReadWriteHelper propHelper)
        {
            if (HelperDefines.Contains(pType))
            {
                propHelper = FieldReadWriteHelper.Generate(key, HelperDefines.Get(pType));
            }
            else if(pType.IsValueType && StructReadWriteHelper.Generate(pType, key, out var readWrite))
            {
                propHelper = readWrite;
            }
            else
            {
                propHelper = null;
                throw new Exception("仅支持string, int, float, bool字段和由以上四种字段组成的结构体");
            }

            return propHelper != null;
        }
    }
    
    
    class FieldReadWriteHelper
    {
        public static FieldReadWriteHelper Generate(string key, IPropertyFieldReadWrite proxy)
        {
            var helper = new FieldReadWriteHelper
            {
                _proxy = proxy, 
                _key = key
            };

            return helper;
        }
        
        private IPropertyFieldReadWrite _proxy;
        private string _key;

        public virtual object Read(object defaultValue)
        {
            return _proxy.Read(_key, defaultValue);
        }

        public virtual void Write( object value)
        {
            _proxy.Write(_key, value);
        }
    }
    
    class StructReadWriteHelper : FieldReadWriteHelper
    {
        public static bool Generate(Type structType, string key, out FieldReadWriteHelper readWrite)
        {
            var dict = new Dictionary<FieldInfo, FieldReadWriteHelper>();
            var fields = structType.GetFields();
            
            try
            {
                foreach (var field in fields)
                {
                    if (field.IsStatic == false && PropertyReadWriteGenerator.GetOne(field.FieldType, key + "." + field.Name, out var fieldReadWrite))
                    {
                        dict[field] = fieldReadWrite;
                    }
                }

                var helper = new StructReadWriteHelper
                {
                    _readWrites = dict,
                    _type = structType, 
                };
                
                readWrite = helper;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                readWrite = null;
            }
            return readWrite != null;
        }

        private static object ReadObj(StructReadWriteHelper helper, object defaultValue) 
        {
            var obj = Activator.CreateInstance(helper._type);
            try
            {
                foreach (var pair in helper._readWrites)
                {
                    pair.Key.SetValue(obj, pair.Value.Read(pair.Key.GetValue(defaultValue)));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return obj;
        }

        private static void WriteObj(StructReadWriteHelper helper, object obj)
        {
            try
            {
                foreach (var pair in helper._readWrites)
                {
                    pair.Value.Write(pair.Key.GetValue(obj));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private Type _type;
        private Dictionary<FieldInfo, FieldReadWriteHelper> _readWrites;
        
        public override object Read(object defaultValue) => ReadObj(this, defaultValue);

        public override void Write(object value) => WriteObj(this, value);

    }
}