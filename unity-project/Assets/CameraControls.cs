using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public int ZoomMultiplier;

    void Update()
    {
        //Zoom
        Camera.main.fieldOfView -= (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * ZoomMultiplier;
    }
}
