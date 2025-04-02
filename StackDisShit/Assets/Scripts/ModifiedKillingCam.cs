using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SpawnAtFixedDistance : MonoBehaviour
{
    // Pour permettre l'accès global aux variables importantes
    public static SpawnAtFixedDistance instance;

    [Header("Prefabs")]
    public GameObject EnemyPrefab;

    [Header("Cube Materials")]
    // Liste des matériaux que le cube utilisera aléatoirement
    public Material[] cubeMaterials;

    [Header("Physics Materials")]
    // PhysicMaterial à appliquer aux cubes pour éviter qu'ils glissent
    public PhysicMaterial cubePhysicsMaterial;

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
    // Texte pour le compte à rebours (avec effet de zoom)
    public TextMeshProUGUI countdownText;
    // Texte pour afficher le message final ("StackDisSh*t")
    public TextMeshProUGUI finalMessageText;

    [Header("Game Panels")]
    // Panel de jeu affiché pendant la partie
    public GameObject gamePanel;
    // Panel de Game Over affiché en cas de défaite
    public GameObject gameOverPanel;
    // Texte dans le GameOverPanel pour afficher le message ("Perdu ! Vous avez fait un score de: X")
    public TextMeshProUGUI gameOverText;

    [Header("Score & Particles")]
    // Texte affichant le score ("Score: X")
    public TextMeshProUGUI scoreText;
    // Prefab des particules à instancier autour d'un cube pour signaler l'ajout de score
    public GameObject particleEffectPrefab;

    [Header("Audio Settings")]
    // Audio clip à jouer quand le texte final s'affiche (final message audio)
    public AudioClip finalMessageAudio;
    // Audio clip de musique de fond, joué une fois le jeu lancé
    public AudioClip backgroundMusicAudio;
    // Audio clip à jouer dès que l'utilisateur appuie sur le bouton "Démarrer"
    public AudioClip startButtonAudio;
    // Audio clip à jouer quand le joueur perd (Game Over)
    public AudioClip gameOverAudio;
    // AudioSource pour la musique de fond (initialisé par le script)
    [HideInInspector] public AudioSource bgAudioSource;

    [Header("Restart Settings")]
    // Bouton "Recommencer" présent dans le GameOverPanel
    public Button restartButton;

    // Indique si le sol a été créé
    private bool isFloorCreated = false;
    private Camera cam;
    // La caméra fixe pour les spawns afin de garder une position stable dans le monde
    private Camera fixedCamera;
    // Indique si le jeu a démarré (après le compte à rebours)
    private bool gameStarted = false;
    // Score du joueur
    public int score = 0;

    void Start()
    {
        instance = this;

        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        // Ajoute un AudioSource pour la musique de fond
        bgAudioSource = gameObject.AddComponent<AudioSource>();
        bgAudioSource.loop = true;
        bgAudioSource.playOnAwake = false;

        touchPressAction = playerInput.actions["TouchPress"];
        touchPosAction = playerInput.actions["TouchPos"];

        // Affiche le StartGamePanel et désactive les autres panels
        if (startGamePanel != null)
            startGamePanel.SetActive(true);
        if (gamePanel != null)
            gamePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        if (finalMessageText != null)
            finalMessageText.gameObject.SetActive(false);

        // Initialise le score
        if (scoreText != null)
            scoreText.text = "Score: 0";

        // Ajoute le listener sur le bouton "Démarrer"
        if (startButton != null)
            startButton.onClick.AddListener(() =>
            {
                if (startButtonAudio != null)
                    AudioSource.PlayClipAtPoint(startButtonAudio, cam.transform.position);
                StartCoroutine(StartCountdown());
            });
        // Ajoute le listener sur le bouton "Recommencer" du GameOverPanel
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    IEnumerator StartCountdown()
    {
        startButton.gameObject.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.transform.localScale = Vector3.one;
        }

        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                float duration = 1f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    countdownText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, elapsed / duration);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                countdownText.transform.localScale = Vector3.one;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (finalMessageAudio != null)
            AudioSource.PlayClipAtPoint(finalMessageAudio, cam.transform.position);

        if (finalMessageText != null)
        {
            finalMessageText.gameObject.SetActive(true);
            finalMessageText.text = "Stack\nDis\nSh*t";
            float finalDuration = 1f;
            float finalElapsed = 0f;
            finalMessageText.transform.localScale = Vector3.one;
            while (finalElapsed < finalDuration)
            {
                finalMessageText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, finalElapsed / finalDuration);
                finalElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            finalMessageText.transform.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(1f);
            finalMessageText.gameObject.SetActive(false);
        }

        // Crée une caméra fixe pour les spawns
        GameObject fixedCamObj = new GameObject("FixedCamera");
        fixedCamera = fixedCamObj.AddComponent<Camera>();
        fixedCamera.CopyFrom(cam);
        fixedCamera.transform.position = cam.transform.position;
        fixedCamera.transform.rotation = cam.transform.rotation;
        fixedCamera.enabled = false;

        if (backgroundMusicAudio != null)
        {
            bgAudioSource.clip = backgroundMusicAudio;
            bgAudioSource.Play();
        }

        if (startGamePanel != null)
            startGamePanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);

        gameStarted = true;
    }

    void Update()
    {
        if (!gameStarted)
            return;

        if (!touchPressAction.WasPerformedThisFrame())
            return;

        if (Time.time - lastSpawnTime < spawnCooldown)
            return;
        lastSpawnTime = Time.time;

        Vector2 touchPos = touchPosAction.ReadValue<Vector2>();
        Vector3 spawnPos;

        if (!isFirstCubePlaced)
        {
            Vector3 screenPoint = new Vector3(touchPos.x, touchPos.y, spawnDistance);
            spawnPos = fixedCamera.ScreenToWorldPoint(screenPoint);
            fixedZ = spawnPos.z;
            isFirstCubePlaced = true;

            GameObject firstCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
            // Force l'orientation horizontale
            firstCube.transform.rotation = Quaternion.identity;
            if (cubeMaterials != null && cubeMaterials.Length > 0)
            {
                Renderer rend = firstCube.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = cubeMaterials[Random.Range(0, cubeMaterials.Length)];
            }
            firstCube.tag = "FirstCube";
            // Applique le PhysicMaterial pour éviter le glissement
            Collider firstCollider = firstCube.GetComponent<Collider>();
            if (firstCollider != null && cubePhysicsMaterial != null)
            {
                firstCollider.material = cubePhysicsMaterial;
            }

            if (!isFloorCreated)
            {
                float floorY;
                Collider cubeCollider = firstCube.GetComponent<Collider>();
                if (cubeCollider != null)
                    floorY = cubeCollider.bounds.min.y;
                else
                    floorY = spawnPos.y - 0.5f;
                CreateFloor(floorY, spawnPos);
                isFloorCreated = true;
            }
        }
        else
        {
            Ray ray = fixedCamera.ScreenPointToRay(touchPos);
            float t = (fixedZ - ray.origin.z) / ray.direction.z;
            spawnPos = ray.GetPoint(t);
            GameObject newCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
            if (cubeMaterials != null && cubeMaterials.Length > 0)
            {
                Renderer rend = newCube.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = cubeMaterials[Random.Range(0, cubeMaterials.Length)];
            }
            // Applique le PhysicMaterial aux cubes suivants
            Collider cubeCol = newCube.GetComponent<Collider>();
            if (cubeCol != null && cubePhysicsMaterial != null)
            {
                cubeCol.material = cubePhysicsMaterial;
            }
            CubeScoreHandler cs = newCube.AddComponent<CubeScoreHandler>();
            cs.Initialize(this, particleEffectPrefab);
        }
    }

    void CreateFloor(float y, Vector3 firstCubePos)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        float width = 10f;
        float length = 10f;
        float thickness = 0.2f;
        Vector3 floorPos = new Vector3(firstCubePos.x, y - thickness / 2, firstCubePos.z);
        floor.transform.position = floorPos;
        floor.transform.localScale = new Vector3(width, thickness, length);
        floor.transform.rotation = Quaternion.identity;
        floor.name = "Floor";

        Renderer rend = floor.GetComponent<Renderer>();
        Material mat = rend.material;
        Color col = mat.color;
        col.a = 0f;
        mat.color = col;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        FloorCollisionHandler handler = floor.AddComponent<FloorCollisionHandler>();
        handler.gameOverPanel = gameOverPanel;
        handler.gamePanel = gamePanel;
        handler.gameOverText = gameOverText;
    }

    public void AddScore(int amount, Vector3 pos)
    {
        score += amount;
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
        if (particleEffectPrefab != null)
        {
            int count = 10;
            float radius = 0.5f;
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), Random.Range(-radius, radius));
                Instantiate(particleEffectPrefab, pos + offset, Quaternion.identity);
            }
        }
    }

    public class CubeScoreHandler : MonoBehaviour
    {
        private SpawnAtFixedDistance spawnManager;
        private GameObject particleEffectPrefab;
        public void Initialize(SpawnAtFixedDistance manager, GameObject particlePrefab)
        {
            spawnManager = manager;
            particleEffectPrefab = particlePrefab;
            StartCoroutine(ScoreCoroutine());
        }
        IEnumerator ScoreCoroutine()
        {
            yield return new WaitForSecondsRealtime(3f);
            if (Time.timeScale != 0 && spawnManager != null)
            {
                spawnManager.AddScore(1, transform.position);
            }
            Destroy(this);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public class FloorCollisionHandler : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject gamePanel;
    public TextMeshProUGUI gameOverText;
    private bool gameOverTriggered = false;

    void OnCollisionEnter(Collision collision)
    {
        if (!gameOverTriggered && collision.gameObject.tag != "FirstCube")
        {
            gameOverTriggered = true;
            if (SpawnAtFixedDistance.instance != null && SpawnAtFixedDistance.instance.bgAudioSource != null)
                SpawnAtFixedDistance.instance.bgAudioSource.Stop();
            if (SpawnAtFixedDistance.instance != null && SpawnAtFixedDistance.instance.gameOverAudio != null)
                AudioSource.PlayClipAtPoint(SpawnAtFixedDistance.instance.gameOverAudio, Camera.main.transform.position);
            Time.timeScale = 0f;
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            else
                Debug.LogWarning("GameOverPanel n'est pas assigné dans FloorCollisionHandler.");
            if (gamePanel != null)
                gamePanel.SetActive(false);
            else
                Debug.LogWarning("GamePanel n'est pas assigné dans FloorCollisionHandler.");
            if (gameOverText != null && SpawnAtFixedDistance.instance != null)
                gameOverText.text = "Perdu ! Vous avez fait un score de: " + SpawnAtFixedDistance.instance.score.ToString();
            else
                Debug.LogWarning("GameOverText n'est pas assigné dans FloorCollisionHandler.");
        }
    }
}
