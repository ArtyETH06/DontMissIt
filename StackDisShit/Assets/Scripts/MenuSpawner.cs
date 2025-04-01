using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class MenuSpawner : MonoBehaviour
{
    public GameObject menuCanvas;

    void Start()
    {
        // Fait apparaître le menu à 1.5m devant la caméra
        Camera cam = Camera.main;
        if (cam != null && menuCanvas != null)
        {
            Vector3 pos = cam.transform.position + cam.transform.forward * 1.5f;
            menuCanvas.transform.position = pos;
            menuCanvas.transform.rotation = Quaternion.LookRotation(menuCanvas.transform.position - cam.transform.position);
        }
    }
}

