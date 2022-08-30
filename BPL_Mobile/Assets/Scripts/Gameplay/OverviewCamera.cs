using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverviewCamera : MonoBehaviour
{
    public Camera mainCamera, overviewCamera;

    void Start()
    {
        cameraMain();
    }

    void Update()
    {
        if (Input.GetKeyDown("left"))
        {
            overHeadCamera();
        }
        else if (Input.GetKeyUp("left"))
        {
            cameraMain();
        }
    }

    public void overHeadCamera()
    {
            mainCamera.enabled = false;
            overviewCamera.enabled = true;
    }

    public void cameraMain()
    {
        mainCamera.enabled = true;
        overviewCamera.enabled = false;
    }
}
