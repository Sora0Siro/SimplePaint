using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camChange : MonoBehaviour
{
    public Camera cam;

    public void ChangeCamSize(float size)
    {
        ChangeCamSize((int)size);
    }
    public void ChangeCamSize(int size)
    {
        cam.orthographicSize = size;
    }
}