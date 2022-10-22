using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setRefT : MonoBehaviour
{
    public ReferenceStorage refTable;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ResumeManager.SetReferenceTable(refTable);
    }
}
