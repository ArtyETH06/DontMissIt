using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;

public class PlaceMenuOnce : MonoBehaviour
{
    public float distance = 2f;
    private static bool hasPlacedMenu = false;

    void Start()
    {
        if (hasPlacedMenu || Camera.main == null) return;

        Transform cam = Camera.main.transform;

        // Place le Canvas devant la caméra
        Vector3 offset = cam.forward * distance;
        transform.position = cam.position + offset;

        // Tourner vers la caméra
        transform.LookAt(cam);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);

        // Se détache pour rester fixe
        transform.SetParent(null);

        hasPlacedMenu = true; // ✅ Marque que c’est fait
        this.enabled = false; // 🔒 Bloque le script
    }
}

