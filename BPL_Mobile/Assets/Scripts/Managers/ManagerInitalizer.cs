using UnityEngine;

public class ManagerInitalizer : MonoBehaviour
{
    void Awake()
    {
        CareerRecordManager.LoadCareer(); //Load the career manager

        //Load any other managers or other shit we may need here
    }
}
