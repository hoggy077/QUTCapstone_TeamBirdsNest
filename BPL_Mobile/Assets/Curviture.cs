using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curviture : MonoBehaviour
{
    public Vector3 RelativeCenter = new Vector3();
    public Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(new Vector3(0, 0, 10), ForceMode.Impulse);
    }

    float TimeVal { get { TimeVal_ += Time.deltaTime; return TimeVal_; } }
    float TimeVal_ = 0;
    void Update()
    {
        Vector3 Displace = gameObject.transform.position + RelativeCenter;
        rb.AddForceAtPosition(new Vector3(Mathf.Pow(TimeVal, 2),0,0)* Time.deltaTime , Displace, ForceMode.Impulse);
    }
}
