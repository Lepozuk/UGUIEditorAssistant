using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace UIExtension
{
    [RequireComponent(typeof(RectTransform))]
    public class UIDummy : MonoBehaviour
    {
        [SerializeField]
        public List<DummyDefine> Dummys = new List<DummyDefine>();
    }

    [Serializable]
    public class DummyDefine
    {
#if UNITY_EDITOR
        [LabelText("别名")]
        public string AliasName = "";
        [LabelText("类型")]
        public DummyType Type = DummyType.RED_POINT;
        [LabelText("偏移")][Tooltip("相对于Pivot中心的的偏移量")]
        public Vector2 Offset = Vector2.zero;
#else
        public string AliasName = "";
        public DummyType Type = DummyType.RED_POINT;
        public Vector2 Offset = Vector2.zero;
#endif
    }
    
    public enum DummyType
    {
        RED_POINT = 1,
    }
    
}