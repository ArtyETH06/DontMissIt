using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class PlaceMenuInFront : MonoBehaviour
{
    public float distanceFromCamera = 1.5f;

    void Start()
    {
        Transform cam = Camera.main.transform;

        // Positionne le menu devant la caméra
        transform.position = cam.position + cam.forward * distanceFromCamera;

        // (Optionnel) Le fait tourner pour qu’il regarde la caméra au début
        transform.LookAt(new Vector3(cam.position.x, transform.position.y, cam.position.z));
    }
}
