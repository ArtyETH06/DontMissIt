using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Pour le bouton (si nécessaire)
using TMPro;        // Pour TextMeshProUGUI
using System.Collections;

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

    [Header("UI Settings")]
    // Panel de démarrage affiché au lancement du jeu
    public GameObject startGamePanel;
    // Bouton "Démarrer" contenu dans le StartGamePanel
    public Button startButton;
    // Texte du compte à rebours et du message "StackDisSh*t" (TextMeshProUGUI)
    public TextMeshProUGUI countdownText;

    [Header("Game Over Settings")]
    // Panel de Game Over à afficher (assigné dans l'inspecteur)
    public GameObject gameOverPanel;
    // Panel de jeu qui est affiché pendant la partie
    public GameObject gamePanel;

    // Indique si le sol a été créé
    private bool isFloorCreated = false;

    private Camera cam;

    // Indique si le jeu a démarré (après le compte à rebours)
    private bool gameStarted = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        touchPressAction = playerInput.actions["TouchPress"];
        touchPosAction = playerInput.actions["TouchPos"];

        // Active le StartGamePanel et désactive les autres
        if (startGamePanel != null)
            startGamePanel.SetActive(true);
        if (gamePanel != null)
            gamePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Ajoute le listener sur le bouton "Démarrer"
        if (startButton != null)
            startButton.onClick.AddListener(() => { StartCoroutine(StartCountdown()); });
    }

    // Coroutine pour le compte à rebours et le démarrage du jeu
    IEnumerator StartCountdown()
    {
        // Cache le bouton pour éviter les doubles clics
        startButton.gameObject.SetActive(false);

        // Compte à rebours de 5 à 1
        for (int i = 5; i >= 1; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        // Affiche "StackDisSh*t"
        if (countdownText != null)
            countdownText.text = "StackDisSh*t";
        yield return new WaitForSeconds(1f);

        // Cache le StartGamePanel et affiche le GamePanel
        if (startGamePanel != null)
            startGamePanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);

        // Le jeu démarre
        gameStarted = true;
    }

    void Update()
    {
        // Le jeu ne commence qu'après le compte à rebours
        if (!gameStarted)
            return;

        // Vérifie si l'utilisateur a touché l'écran cette frame
        if (!touchPressAction.WasPerformedThisFrame())
            return;

        // Applique le cooldown : un spawn toutes les spawnCooldown secondes
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;
        lastSpawnTime = Time.time;

        // Récupère la position du toucher (X, Y en coordonnées écran)
        Vector2 touchPos = touchPosAction.ReadValue<Vector2>();
        Vector3 spawnPos;

        if (!isFirstCubePlaced)
        {
            // Pour le premier cube, utilise ScreenToWorldPoint avec spawnDistance
            Vector3 screenPoint = new Vector3(touchPos.x, touchPos.y, spawnDistance);
            spawnPos = cam.ScreenToWorldPoint(screenPoint);
            fixedZ = spawnPos.z;  // Enregistre la profondeur du premier cube
            isFirstCubePlaced = true;

            // Instancie le premier cube et lui attribue le tag "FirstCube"
            GameObject firstCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
            firstCube.tag = "FirstCube";

            // Création du sol : utilise le bord inférieur du premier cube
            if (!isFloorCreated)
            {
                float floorY;
                Collider cubeCollider = firstCube.GetComponent<Collider>();
                if (cubeCollider != null)
                {
                    // Utilise directement le bord inférieur du collider
                    floorY = cubeCollider.bounds.min.y;
                }
                else
                {
                    // En l'absence de collider, se base sur spawnPos avec une marge
                    floorY = spawnPos.y - 0.5f;
                }
                // Crée le sol centré sur le premier cube
                CreateFloor(floorY, spawnPos);
                isFloorCreated = true;
            }
        }
        else
        {
            // Pour les cubes suivants, calcule le point d'intersection sur le plan à z = fixedZ
            Ray ray = cam.ScreenPointToRay(touchPos);
            float t = (fixedZ - ray.origin.z) / ray.direction.z;
            spawnPos = ray.GetPoint(t);
            Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    // Crée un sol centré sur le premier cube.
    // "y" correspond au bord inférieur du premier cube et "firstCubePos" sert à centrer le sol en x et z.
    void CreateFloor(float y, Vector3 firstCubePos)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        float width = 10f;
        float length = 10f;
        float thickness = 0.2f;
        // Pour aligner la surface supérieure du sol avec le bord inférieur du premier cube,
        // le centre du sol doit être positionné à (y - thickness/2)
        Vector3 floorPos = new Vector3(firstCubePos.x, y - thickness / 2, firstCubePos.z);
        floor.transform.position = floorPos;
        floor.transform.localScale = new Vector3(width, thickness, length);
        floor.name = "Floor";

        // Ajoute le FloorCollisionHandler et lui assigne les panels
        FloorCollisionHandler handler = floor.AddComponent<FloorCollisionHandler>();
        handler.gameOverPanel = gameOverPanel;
        handler.gamePanel = gamePanel;
    }
}

// Classe gérant la détection des collisions sur le sol.
// Si un cube (autre que le premier) touche le sol, le jeu s'arrête : le GameOverPanel s'affiche et le GamePanel disparaît.
public class FloorCollisionHandler : MonoBehaviour
{
    public GameObject gameOverPanel; // Assigné automatiquement
    public GameObject gamePanel;     // Assigné automatiquement
    private bool gameOverTriggered = false;

    void OnCollisionEnter(Collision collision)
    {
        // Si l'objet entrant n'est pas le premier cube (tag "FirstCube")
        if (!gameOverTriggered && collision.gameObject.tag != "FirstCube")
        {
            gameOverTriggered = true;
            // Arrête le temps pour stopper le jeu
            Time.timeScale = 0f;
            // Affiche le GameOverPanel
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            else
                Debug.LogWarning("GameOverPanel n'est pas assigné dans FloorCollisionHandler.");
            // Masque le GamePanel
            if (gamePanel != null)
                gamePanel.SetActive(false);
            else
                Debug.LogWarning("GamePanel n'est pas assigné dans FloorCollisionHandler.");
        }
    }
}
