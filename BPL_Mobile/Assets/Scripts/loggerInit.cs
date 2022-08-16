using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loggerInit : MonoBehaviour
{
    public bool RunLogger = false;
    void Start()
    {
        if (RunLogger)
            _ = Logger.Instance;
    }

    int i = 0;
    string test(int index) => $"log {index}";
    private void Update()
    {
        if (i >= 300)
            return;

        if (i % 2 == 0)
            Debug.Log(test(i));
        else
            Debug.LogWarning(test(i));
        i++;
    }
}
