using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public float distance = 15f;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            Vector3 offset = cameraTransform.forward * distance;
            transform.position = cameraTransform.position + offset;

            Vector3 lookDirection = transform.position - cameraTransform.position;
            lookDirection.y = 0; // Ignore la hauteur pour ne pas tourner vers le bas/haut
            transform.rotation = Quaternion.LookRotation(lookDirection);

            transform.parent = null; // se détache de tout parent
        }
    }
}
