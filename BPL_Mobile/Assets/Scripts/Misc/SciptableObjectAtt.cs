using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface SerializeScriptable
{
    SerializeScriptableData getData();
    void parseData();
}

[Serializable]
public struct SerializeScriptableData
{
    public object raw;
    public Type sco;
}