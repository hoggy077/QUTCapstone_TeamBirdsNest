using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;
    static public CameraZoom instance;
    public bool zoom = false;
    private float normalZoom;
    private float zoomedZoom = 26f;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        cam = GetComponent<Camera>();
        normalZoom = cam.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        // Zoom In
        if(zoom)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomedZoom, 4f * Time.deltaTime);
        }

        // Zoom Out
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalZoom, 4f * Time.deltaTime);
        }
    }
}
