using UnityEngine;

public class ManagerInitalizer : MonoBehaviour
{
    public ReferenceStorage refStorage;

    void Awake()
    {
        _ = Logger.Instance;
        CareerRecordManager.LoadCareer(); //Load the career manager
        //ResumeManager.SetReferenceTable(refStorage);
        //Load any other managers or other shit we may need here
    }
}
