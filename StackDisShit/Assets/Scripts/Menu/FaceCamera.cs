using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public float distanceFromCamera = 2f;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        // Place le Canvas devant la caméra
        Vector3 forwardOffset = cameraTransform.forward.normalized * distanceFromCamera;
        transform.position = cameraTransform.position + forwardOffset;

        // Le fait regarder vers la caméra
        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0); // Inverse pour que le texte soit lisible

        // Détache le Canvas s’il était parenté
        transform.SetParent(null);
    }
}
