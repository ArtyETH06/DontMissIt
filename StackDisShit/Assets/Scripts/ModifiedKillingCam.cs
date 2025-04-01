using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnAtFixedDistance : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject EnemyPrefab;

    [Header("Input Settings")]
    public PlayerInput playerInput;
    private InputAction touchPressAction;
    private InputAction touchPosAction;

    [Header("Spawn Settings")]
    // Distance utilisée pour placer le premier cube devant la caméra
    public float spawnDistance = 2f;
    private bool isFirstCubePlaced = false;
    // La coordonnée Z (en monde) fixée lors du premier spawn
    private float fixedZ;

    // Indique si le sol a été créé
    private bool isFloorCreated = false;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        touchPressAction = playerInput.actions["TouchPress"];
        touchPosAction = playerInput.actions["TouchPos"];
    }

    void Update()
    {
        // Vérifie si l'utilisateur vient de toucher l'écran cette frame
        if (!touchPressAction.WasPerformedThisFrame())
        {
            return;
        }

        // Récupère la position du toucher (X, Y en coordonnées écran)
        Vector2 touchPos = touchPosAction.ReadValue<Vector2>();
        Vector3 spawnPos;

        if (!isFirstCubePlaced)
        {
            // Pour le premier cube, on utilise ScreenToWorldPoint avec la distance spawnDistance
            Vector3 screenPoint = new Vector3(touchPos.x, touchPos.y, spawnDistance);
            spawnPos = cam.ScreenToWorldPoint(screenPoint);
            fixedZ = spawnPos.z;   // Enregistre la profondeur définie par le premier cube
            isFirstCubePlaced = true;

            // Instancie le premier cube
            GameObject firstCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);

            // Création du sol : on définit le niveau Y minimal à partir du bord inférieur du cube
            if (!isFloorCreated)
            {
                float floorY;
                Collider cubeCollider = firstCube.GetComponent<Collider>();
                if (cubeCollider != null)
                {
                    // Utilise le bord inférieur du collider avec une petite marge de 0.1f
                    floorY = cubeCollider.bounds.min.y - 0.1f;
                }
                else
                {
                    // Si pas de collider, on se base sur la position du spawn en y
                    floorY = spawnPos.y - 0.5f;
                }
                CreateFloor(floorY);
                isFloorCreated = true;
            }
        }
        else
        {
            // Pour les cubes suivants, on calcule le point d'intersection sur le plan à z = fixedZ
            Ray ray = cam.ScreenPointToRay(touchPos);
            float t = (fixedZ - ray.origin.z) / ray.direction.z;
            spawnPos = ray.GetPoint(t);
            Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    // Méthode pour créer un sol avec une épaisseur donnée, dont la surface supérieure se trouve à "y"
    void CreateFloor(float y)
    {
        // On crée un sol en utilisant un Cube primitif
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Définir des dimensions pour le sol (large et long pour couvrir la zone de jeu)
        float width = 10f;
        float length = 10f;
        float thickness = 0.2f;
        // Le pivot du cube est au centre, donc pour que la surface supérieure soit à "y", on positionne le sol à y - thickness/2
        Vector3 floorPos = new Vector3(0, y - thickness / 2, 0);
        floor.transform.position = floorPos;
        floor.transform.localScale = new Vector3(width, thickness, length);
        floor.name = "Floor";
    }
}
