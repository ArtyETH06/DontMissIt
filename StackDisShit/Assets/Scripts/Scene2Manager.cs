using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class Scene2Manager : MonoBehaviour
{
    public ARRaycastManager RaycastManager;
    public TrackableType TypeToTrack = TrackableType.PlaneWithinBounds;
    public GameObject PrefabToInstantiate;
    public PlayerInput PlayerInput;

    public List<Material> Materials;

    private InputAction touchPressAction;
    private InputAction touchPosAction;
    private InputAction touchPhaseAction;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> instantiatedCubes = new List<GameObject>();
    
    [SerializeField] private TMP_Text countText;
    private int cubeCount;

    void Start()
    {
        cubeCount = 0;
        UpdateCountText();

        touchPressAction = PlayerInput.actions["TouchPress"];
        touchPosAction = PlayerInput.actions["TouchPos"];
        touchPhaseAction = PlayerInput.actions["TouchPhase"];
    }

    void Update()
    {
        if (touchPressAction.WasPerformedThisFrame())
        {
            var touchPhase = touchPhaseAction.ReadValue<UnityEngine.InputSystem.TouchPhase>();
            if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                OnTouch();
            }
        }
    }

    private void OnTouch()
    {
        Vector2 touchPos = touchPosAction.ReadValue<Vector2>();

        if (RaycastManager.Raycast(touchPos, hits, TypeToTrack))
        {
            ARRaycastHit firstHit = hits[0];
            GameObject cube = Instantiate(PrefabToInstantiate, firstHit.pose.position, firstHit.pose.rotation);
            instantiatedCubes.Add(cube);
            cubeCount += 1;
            UpdateCountText();
        }
    }

    private void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = "Cubes: " + cubeCount;
        }
    }

    public void ChangeColor()
    {
        foreach (GameObject cube in instantiatedCubes)
        {
            int randomIndex = Random.Range(0, Materials.Count);
            Material randomMaterial = Materials[randomIndex];

            MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = randomMaterial;
            }
        }
    }
}
