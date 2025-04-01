using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform cameraTransform;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            // Position devant la caméra à 1.5m
            Vector3 offset = cameraTransform.forward * 1.5f;
            transform.position = cameraTransform.position + offset;

            // Rotation vers la caméra (ignorer Y si besoin)
            transform.LookAt(cameraTransform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}
