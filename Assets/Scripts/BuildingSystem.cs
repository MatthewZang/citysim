using UnityEngine;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    [Header("Building Settings")]
    public LayerMask groundLayer;
    public float gridSize = 1f;
    public float maxBuildingHeight = 100f;
    
    [Header("Building Prefabs")]
    public GameObject residentialPrefab;
    public GameObject commercialPrefab;
    public GameObject industrialPrefab;
    public GameObject policeStationPrefab;
    public GameObject fireStationPrefab;
    public GameObject schoolPrefab;
    public GameObject hospitalPrefab;

    private Camera mainCamera;
    private GameObject currentBuildingPreview;
    private BuildingType selectedBuildingType;
    private bool isPlacingBuilding = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isPlacingBuilding)
        {
            UpdateBuildingPreview();
            
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
            }
        }
    }

    public void StartBuildingPlacement(BuildingType type)
    {
        selectedBuildingType = type;
        isPlacingBuilding = true;
        
        // Create preview
        GameObject prefab = GetBuildingPrefab(type);
        if (prefab != null)
        {
            currentBuildingPreview = Instantiate(prefab);
            currentBuildingPreview.GetComponent<Building>().SetPreviewMode(true);
        }
    }

    public void CancelBuildingPlacement()
    {
        isPlacingBuilding = false;
        if (currentBuildingPreview != null)
        {
            Destroy(currentBuildingPreview);
            currentBuildingPreview = null;
        }
    }

    private void UpdateBuildingPreview()
    {
        if (currentBuildingPreview == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 position = hit.point;
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;
            position.y = 0f;

            currentBuildingPreview.transform.position = position;
        }
    }

    private void PlaceBuilding()
    {
        if (currentBuildingPreview == null) return;

        // Check if we can afford the building
        Building building = currentBuildingPreview.GetComponent<Building>();
        if (GameManager.Instance.budget >= building.cost)
        {
            // Deduct cost
            GameManager.Instance.budget -= building.cost;
            
            // Place the actual building
            GameObject actualBuilding = Instantiate(GetBuildingPrefab(selectedBuildingType));
            actualBuilding.transform.position = currentBuildingPreview.transform.position;
            actualBuilding.transform.rotation = currentBuildingPreview.transform.rotation;
            
            // Clean up preview
            Destroy(currentBuildingPreview);
            currentBuildingPreview = null;
            
            // Update city services
            UpdateCityServices();
        }
        else
        {
            // TODO: Show "Not enough money" message
            Debug.Log("Not enough money to place building!");
        }
    }

    private GameObject GetBuildingPrefab(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Residential:
                return residentialPrefab;
            case BuildingType.Commercial:
                return commercialPrefab;
            case BuildingType.Industrial:
                return industrialPrefab;
            case BuildingType.PoliceStation:
                return policeStationPrefab;
            case BuildingType.FireStation:
                return fireStationPrefab;
            case BuildingType.School:
                return schoolPrefab;
            case BuildingType.Hospital:
                return hospitalPrefab;
            default:
                return null;
        }
    }

    private void UpdateCityServices()
    {
        // TODO: Update service coverage based on placed buildings
    }
}

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    PoliceStation,
    FireStation,
    School,
    Hospital
} 