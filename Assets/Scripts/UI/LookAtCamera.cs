using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
    }

    void Update()
    {
        transform.LookAt(mainCamera.transform);
    }
}
