using UnityEngine;

public class CanvasDistanceController : MonoBehaviour
{
    // Référence au Canvas en mode World Space à positionner
    [SerializeField] private GameObject canvas;
    // Distance souhaitée entre la caméra et le canvas (modifiable depuis l'inspecteur)
    [SerializeField] private float canvasDistance = 2f;

    void LateUpdate()
    {
        if (canvas != null)
        {
            // Positionne le canvas à une distance fixe devant la caméra
            canvas.transform.position = transform.position + transform.forward * canvasDistance;
            // Oriente le canvas pour qu'il fasse face à la caméra
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - transform.position);
        }
    }
}
