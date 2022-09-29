using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackThisThing : MonoBehaviour, TrackedData
{
    public string referenceTerm = "";
    public bool IncludeInSave = false;

    public string getReferenceTerm() => referenceTerm;

    public GameObject getSelf() => gameObject;

    public TrackedObject getTracked() => new TrackedObject(this, referenceTerm);

    public bool Include() => IncludeInSave;
}
