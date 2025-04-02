using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;

using UnityEngine;

public class PositionMenuAtLaunch : MonoBehaviour
{
    [Tooltip("Distance souhaitée entre la caméra et le menu (en mètres)")]
    public float distanceFromCamera = 15f;

    [Tooltip("Marge supplémentaire au-dessus du bord inférieur de la vue (en mètres)")]
    public float bottomMargin = 0.1f;

    private RectTransform rectTransform;

    void Start()
    {
        // On récupère la caméra principale
        Camera cam = Camera.main;
        if(cam == null)
        {
            Debug.LogError("Main Camera introuvable !");
            return;
        }
        
        // On récupère le RectTransform du Canvas
        rectTransform = GetComponent<RectTransform>();
        if(rectTransform == null)
        {
            Debug.LogError("RectTransform introuvable sur " + gameObject.name);
            return;
        }
        
        // On calcule le point en bas au centre de la vue de la caméra à la distance souhaitée
        Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, distanceFromCamera));
        
        // On calcule la position cible : d'abord, on prend la position de la caméra + la direction forward * distance
        Vector3 targetPosition = cam.transform.position + cam.transform.forward * distanceFromCamera;
        
        // Calcul de la hauteur du Canvas en monde (en tenant compte de son scale)
        float canvasHeight = rectTransform.rect.height * rectTransform.lossyScale.y;
        
        // Si le pivot est (0.5, 0.5), la position du Canvas correspond à son centre.
        // Le bord inférieur du Canvas est alors à targetPosition.y - canvasHeight/2.
        // On veut que ce bord inférieur soit égal à bottomCenter.y + bottomMargin.
        // On ajuste donc la hauteur de la position cible :
        targetPosition.y = bottomCenter.y + bottomMargin + canvasHeight / 2f;
        
        // On positionne le Canvas
        transform.position = targetPosition;
        
        // On fait en sorte que le Canvas fasse face à la caméra
        Vector3 lookDirection = cam.transform.position - transform.position;
        // Pour éviter une rotation verticale (si tu préfères le fixer horizontalement)
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(-lookDirection);
        
        // Optionnel : détacher le Canvas de son parent pour qu'il reste fixe dans le monde
        transform.SetParent(null);
    }
}
