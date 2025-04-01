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
    // Texte dans le GameOverPanel pour afficher le message final ("Perdu ! Vous avez fait un score de: X")
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
    // AudioSource pour la musique de fond (initialisé par le script)
    [HideInInspector] public AudioSource bgAudioSource;

    [Header("Restart Settings")]
    // Bouton "Recommencer" présent dans le GameOverPanel
    public Button restartButton;

    // Indique si le sol a été créé
    private bool isFloorCreated = false;
    private Camera cam;
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
                // Joue l'audio associé au bouton dès le clic
                if (startButtonAudio != null)
                    AudioSource.PlayClipAtPoint(startButtonAudio, cam.transform.position);
                StartCoroutine(StartCountdown());
            });
        // Ajoute le listener sur le bouton "Recommencer" du GameOverPanel
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    // Coroutine pour le compte à rebours et le démarrage du jeu avec effet de zoom et lecture audio finale
    IEnumerator StartCountdown()
    {
        // Cache le bouton pour éviter plusieurs clics
        startButton.gameObject.SetActive(false);

        // Active le texte du compte à rebours
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.transform.localScale = Vector3.one;
        }

        // Compte à rebours de 3 à 1
        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                float duration = 1f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    // Effet de zoom du scale de 1 à 1.5
                    countdownText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, elapsed / duration);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                countdownText.transform.localScale = Vector3.one;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Cache le texte du compte à rebours
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Avant d'afficher le texte final, joue l'audio final
        if (finalMessageAudio != null)
            AudioSource.PlayClipAtPoint(finalMessageAudio, cam.transform.position);

        // Affiche le texte final avec effet de zoom
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

        // Démarre la musique de fond
        if (backgroundMusicAudio != null)
        {
            bgAudioSource.clip = backgroundMusicAudio;
            bgAudioSource.Play();
        }

        // Masque le StartGamePanel et affiche le GamePanel
        if (startGamePanel != null)
            startGamePanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);

        // Le jeu démarre
        gameStarted = true;
    }

    void Update()
    {
        // Le jeu ne démarre qu'après le compte à rebours
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

            // Instancie le premier cube, lui attribue le tag "FirstCube" et lui assigne un matériau aléatoire
            GameObject firstCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
            if (cubeMaterials != null && cubeMaterials.Length > 0)
            {
                Renderer rend = firstCube.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = cubeMaterials[Random.Range(0, cubeMaterials.Length)];
            }
            firstCube.tag = "FirstCube";

            // Création du sol : utilise le bord inférieur du premier cube pour définir la position du sol
            if (!isFloorCreated)
            {
                float floorY;
                Collider cubeCollider = firstCube.GetComponent<Collider>();
                if (cubeCollider != null)
                    floorY = cubeCollider.bounds.min.y;
                else
                    floorY = spawnPos.y - 0.5f;
                // Crée le sol centré sur le premier cube
                CreateFloor(floorY, spawnPos);
                isFloorCreated = true;
            }
        }
        else
        {
            // Pour les cubes suivants, calcule la position sur le plan à z = fixedZ
            Ray ray = cam.ScreenPointToRay(touchPos);
            float t = (fixedZ - ray.origin.z) / ray.direction.z;
            spawnPos = ray.GetPoint(t);
            GameObject newCube = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
            // Assigne un matériau aléatoire au cube
            if (cubeMaterials != null && cubeMaterials.Length > 0)
            {
                Renderer rend = newCube.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = cubeMaterials[Random.Range(0, cubeMaterials.Length)];
            }
            // Ajoute un composant pour gérer l'ajout du score et l'effet de particules après 3 secondes
            CubeScoreHandler cs = newCube.AddComponent<CubeScoreHandler>();
            cs.Initialize(this, particleEffectPrefab);
        }
    }

    // Crée le sol (invisible) centré sur le premier cube.
    // "y" correspond au bord inférieur du premier cube, et "firstCubePos" sert à centrer le sol en x et z.
    void CreateFloor(float y, Vector3 firstCubePos)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        float width = 10f;
        float length = 10f;
        float thickness = 0.2f;
        // Positionne le sol pour que sa surface supérieure soit alignée avec le bord inférieur du premier cube
        Vector3 floorPos = new Vector3(firstCubePos.x, y - thickness / 2, firstCubePos.z);
        floor.transform.position = floorPos;
        floor.transform.localScale = new Vector3(width, thickness, length);
        floor.name = "Floor";

        // Rend le sol 100% transparent (il existe mais n'est pas visible)
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

        // Ajoute le FloorCollisionHandler et lui assigne les panels et le texte Game Over
        FloorCollisionHandler handler = floor.AddComponent<FloorCollisionHandler>();
        handler.gameOverPanel = gameOverPanel;
        handler.gamePanel = gamePanel;
        handler.gameOverText = gameOverText;
    }

    // Méthode appelée pour mettre à jour le score et afficher des particules partout autour du cube.
    public void AddScore(int amount, Vector3 pos)
    {
        score += amount;
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
        if (particleEffectPrefab != null)
        {
            // Instancie plusieurs copies du système de particules autour de la position donnée
            int count = 10;
            float radius = 0.5f; // Ajuste le rayon selon tes besoins
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), Random.Range(-radius, radius));
                Instantiate(particleEffectPrefab, pos + offset, Quaternion.identity);
            }
        }
    }

    // Composant ajouté aux cubes (autres que le premier) pour gérer l'ajout du score après 3 secondes.
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
            // Attend 3 secondes en temps réel
            yield return new WaitForSecondsRealtime(3f);
            // Si le jeu est toujours actif, ajoute 1 point et affiche l'effet de particules
            if (Time.timeScale != 0 && spawnManager != null)
            {
                spawnManager.AddScore(1, transform.position);
            }
            Destroy(this);
        }
    }

    // Fonction pour recommencer le jeu en rechargeant la scène active
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

// Classe gérant la détection des collisions sur le sol.
// Si un cube (autre que le premier, identifié par le tag "FirstCube") touche le sol, le jeu s'arrête.
public class FloorCollisionHandler : MonoBehaviour
{
    public GameObject gameOverPanel; // Assigné automatiquement
    public GameObject gamePanel;     // Assigné automatiquement
    public TextMeshProUGUI gameOverText; // Texte à mettre à jour dans le GameOverPanel
    private bool gameOverTriggered = false;

    void OnCollisionEnter(Collision collision)
    {
        // Si l'objet entrant n'est pas le premier cube (tag "FirstCube")
        if (!gameOverTriggered && collision.gameObject.tag != "FirstCube")
        {
            gameOverTriggered = true;
            // Arrête la musique de fond
            if (SpawnAtFixedDistance.instance != null && SpawnAtFixedDistance.instance.bgAudioSource != null)
                SpawnAtFixedDistance.instance.bgAudioSource.Stop();
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
            // Met à jour le texte du GameOverPanel avec le score final
            if (gameOverText != null && SpawnAtFixedDistance.instance != null)
                gameOverText.text = "Perdu ! Vous avez fait un score de: " + SpawnAtFixedDistance.instance.score.ToString();
            else
                Debug.LogWarning("GameOverText n'est pas assigné dans FloorCollisionHandler.");
        }
    }
}
