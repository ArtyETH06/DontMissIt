using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startGamePanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    [Header("UI Elements")]
    public Button startButton;
    public TMP_Text countdownText;
    public TMP_Text startMessageText;

    [Header("AR Effects")]
    public ParticleSystem startMessageParticles;
    public AudioSource audioSource;
    public AudioClip explosionSound;
    public Camera arCamera;

    void Start()
    {
        startGamePanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        countdownText.gameObject.SetActive(false);
        startMessageText.gameObject.SetActive(false);

        if (startMessageParticles != null)
            startMessageParticles.Stop();

        startButton.onClick.AddListener(() => StartCoroutine(StartGameRoutine()));
    }

    IEnumerator StartGameRoutine()
    {
        startButton.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);

        for (int i = 5; i > 0; i--)
        {
            yield return StartCoroutine(AnimateCountdown(countdownText, i.ToString()));
            yield return new WaitForSeconds(0.2f);
        }

        countdownText.gameObject.SetActive(false);
        startMessageText.gameObject.SetActive(true);

        // 💥 Particules dans l’espace devant la caméra
        if (startMessageParticles != null)
        {
            Vector3 spawnPos = arCamera.transform.position + arCamera.transform.forward * 1.5f;
            startMessageParticles.transform.position = spawnPos;
            startMessageParticles.transform.LookAt(arCamera.transform);
            startMessageParticles.Play();
        }

        // 🔊 Son d’explosion
        if (audioSource && explosionSound)
            audioSource.PlayOneShot(explosionSound);

        // 🌀 Shake de la caméra
        StartCoroutine(CameraShake(0.4f, 0.1f));

        yield return StartCoroutine(AnimateCountdown(startMessageText, "Stack\nDis\nSh*t !"));
        yield return new WaitForSeconds(2f);

        // Démarrer le jeu
        startGamePanel.SetActive(false);
        gamePanel.SetActive(true);

        yield return new WaitForSeconds(15f);

        // Fin de partie
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    IEnumerator AnimateCountdown(TMP_Text textComponent, string message)
    {
        textComponent.text = message;
        textComponent.transform.localScale = Vector3.zero;

        float duration = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, time / duration);
            textComponent.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        textComponent.transform.localScale = Vector3.one;
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = arCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            arCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        arCamera.transform.localPosition = originalPos;
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
