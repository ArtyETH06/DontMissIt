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

    [Header("Cooldown Settings")]
    // Intervalle minimum entre deux spawns (en secondes)
    public float spawnCooldown = 3f;
    // Heure du dernier spawn
    private float lastSpawnTime = -Mathf.Infinity;

    [Header("Game Over Settings")]
    // Panel de Game Over à afficher (assigné via l'inspecteur)
    public GameObject gameOverPanel;

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
            return;

        // Appliquer le cooldown : un spawn toutes les spawnCooldown secondes
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;
        lastSpawnTime = Time.time;

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

            // Instancie le premier cube et lui assigne le tag "FirstCube"
            GameObject firstCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
            firstCube.tag = "FirstCube";

            // Création du sol : on définit le niveau Y minimal à partir du bord inférieur du premier cube
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
                    // Si pas de collider, on se base sur la position du spawn en Y
                    floorY = spawnPos.y - 0.5f;
                }
                // On passe également la position du premier cube pour positionner correctement le sol (x et z)
                CreateFloor(floorY, spawnPos);
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
    // Le sol sera centré sur le premier cube (pour les coordonnées x et z)
    void CreateFloor(float y, Vector3 firstCubePos)
    {
        // Création d'un sol via un Cube primitif
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Dimensions du sol
        float width = 10f;
        float length = 10f;
        float thickness = 0.2f;
        // Le pivot du cube est au centre, donc pour que la surface supérieure soit à "y",
        // on positionne le sol à (firstCubePos.x, y - thickness/2, firstCubePos.z)
        Vector3 floorPos = new Vector3(firstCubePos.x, y - thickness / 2, firstCubePos.z);
        floor.transform.position = floorPos;
        floor.transform.localScale = new Vector3(width, thickness, length);
        floor.name = "Floor";

        // Ajoute le FloorCollisionHandler automatiquement et lui assigne le panel GameOver
        FloorCollisionHandler handler = floor.AddComponent<FloorCollisionHandler>();
        handler.gameOverPanel = gameOverPanel;
    }
}

// Classe gérant la détection des collisions sur le sol
public class FloorCollisionHandler : MonoBehaviour
{
    // Panel de Game Over à afficher (assigné via le script principal)
    public GameObject gameOverPanel;
    private bool gameOverTriggered = false;

    void OnCollisionEnter(Collision collision)
    {
        // Si l'objet entrant en collision n'est pas le premier cube (tag différent de "FirstCube")
        if (!gameOverTriggered && collision.gameObject.tag != "FirstCube")
        {
            gameOverTriggered = true;
            // Arrête le temps pour stopper le jeu (optionnel)
            Time.timeScale = 0f;
            // Affiche le panel de Game Over si assigné
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("GameOverPanel n'est pas assigné dans FloorCollisionHandler.");
            }
        }
    }
}
