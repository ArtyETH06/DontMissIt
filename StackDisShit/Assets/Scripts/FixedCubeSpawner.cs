using UnityEngine;

public class FixedCubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public Vector3 spawnPosition = new Vector3(0f, 0.5f, 0f); // Coordonnées dans le monde
    public Quaternion spawnRotation = Quaternion.identity;

    public void SpawnCube()
    {
        if (cubePrefab != null)
        {
            Instantiate(cubePrefab, spawnPosition, spawnRotation);
        }
        else
        {
            Debug.LogWarning("Aucun prefab assigné dans le spawner.");
        }
    }
}
