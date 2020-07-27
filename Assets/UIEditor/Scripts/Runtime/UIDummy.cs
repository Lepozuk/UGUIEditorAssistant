using System;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(RectTransform))]
public class UIDummy : MonoBehaviour
{   
    [SerializeField]
    private List<DummyDefine> _dummys = new List<DummyDefine>();
    public List<DummyDefine> Dummys
    {
        get => _dummys;
        set
        {
            #if UNITY_EDITOR
                _dummys = value;
            #endif
        }
    }
}

[Serializable]
public class DummyDefine
{
    public string AliasName = "";
    public DummyType Type = DummyType.RED_POINT;
    public Vector2 Offset = Vector2.zero;
}

public enum DummyType
{
    RED_POINT = 1,
}
    
