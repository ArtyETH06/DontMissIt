using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlaceMenuOnce : MonoBehaviour
{
    public float distanceFromCamera = 2f; // distance devant la caméra
    private bool hasPlaced = false;

    void Start()
    {
        Camera mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("Main Camera not found");
            return;
        }

        // Place le canvas devant la caméra
        Vector3 forwardOffset = mainCam.transform.forward.normalized * distanceFromCamera;
        transform.position = mainCam.transform.position + forwardOffset;

        // Le faire face à la caméra
        transform.LookAt(mainCam.transform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);

        // Détacher le menu de tout parent pour qu'il reste fixe
        transform.SetParent(null);
        hasPlaced = true;
    }

    void Update()
    {
        // Ne rien faire ensuite
        if (hasPlaced)
        {
            enabled = false; // Désactive le script pour toujours
        }
    }
}
