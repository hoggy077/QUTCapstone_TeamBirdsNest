using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#region storage
[Serializable]
public class ReferenceStorage : ScriptableObject
{
    [SerializeField]
    public List<pair> references = new List<pair>();
    public int Count => references.Count;
}

[Serializable]
public class pair
{
    [SerializeField]
    public string name;
    [SerializeField]
    public Object value;
}
#endregion
