using System;
using UnityEngine;



[RequireComponent(typeof(RectTransform))]
public class UIDummy : MonoBehaviour
{
    public DummyDefine[] Dummys;
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
    
