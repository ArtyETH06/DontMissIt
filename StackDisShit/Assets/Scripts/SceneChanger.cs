using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Nom de la scène à charger (doit être ajouté dans Build Settings)
    public string sceneName;

    // Cette méthode sera appelée par l'OnClick du bouton
    public void ChangeScene()
    {
        // Vérifie que le nom de la scène n'est pas vide
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Le nom de la scène n'a pas été défini !");
        }
    }
}
