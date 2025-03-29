using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    private string savePath;
    private const string SAVE_FOLDER = "Saves";
    private const string SAVE_EXTENSION = ".city";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create saves directory if it doesn't exist
            savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(string saveName)
    {
        GameData gameData = new GameData();
        
        // Save game state
        gameData.gameTime = GameManager.Instance.gameTime;
        gameData.day = GameManager.Instance.day;
        gameData.timeScale = GameManager.Instance.timeScale;
        
        // Save resources
        gameData.budget = GameManager.Instance.budget;
        gameData.population = GameManager.Instance.population;
        gameData.happiness = GameManager.Instance.happiness;
        
        // Save city services
        gameData.policeCoverage = GameManager.Instance.policeCoverage;
        gameData.fireCoverage = GameManager.Instance.fireCoverage;
        gameData.educationCoverage = GameManager.Instance.educationCoverage;
        gameData.healthcareCoverage = GameManager.Instance.healthcareCoverage;
        
        // Save buildings
        gameData.buildings = new List<BuildingData>();
        Building[] buildings = FindObjectsOfType<Building>();
        foreach (Building building in buildings)
        {
            BuildingData buildingData = new BuildingData
            {
                position = building.transform.position,
                rotation = building.transform.rotation,
                buildingType = GetBuildingType(building),
                condition = building.condition,
                efficiency = building.efficiency,
                isOperational = building.isOperational
            };
            gameData.buildings.Add(buildingData);
        }

        // Save to file
        string filePath = Path.Combine(savePath, saveName + SAVE_EXTENSION);
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, gameData);
        }
    }

    public void LoadGame(string saveName)
    {
        string filePath = Path.Combine(savePath, saveName + SAVE_EXTENSION);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Save file not found: {filePath}");
            return;
        }

        GameData gameData;
        using (FileStream stream = new FileStream(filePath, FileMode.Open))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            gameData = (GameData)formatter.Deserialize(stream);
        }

        // Load game state
        GameManager.Instance.gameTime = gameData.gameTime;
        GameManager.Instance.day = gameData.day;
        GameManager.Instance.timeScale = gameData.timeScale;
        
        // Load resources
        GameManager.Instance.budget = gameData.budget;
        GameManager.Instance.population = gameData.population;
        GameManager.Instance.happiness = gameData.happiness;
        
        // Load city services
        GameManager.Instance.policeCoverage = gameData.policeCoverage;
        GameManager.Instance.fireCoverage = gameData.fireCoverage;
        GameManager.Instance.educationCoverage = gameData.educationCoverage;
        GameManager.Instance.healthcareCoverage = gameData.healthcareCoverage;
        
        // Clear existing buildings
        Building[] existingBuildings = FindObjectsOfType<Building>();
        foreach (Building building in existingBuildings)
        {
            Destroy(building.gameObject);
        }
        
        // Load buildings
        foreach (BuildingData buildingData in gameData.buildings)
        {
            GameObject prefab = GetBuildingPrefab(buildingData.buildingType);
            if (prefab != null)
            {
                GameObject building = Instantiate(prefab);
                building.transform.position = buildingData.position;
                building.transform.rotation = buildingData.rotation;
                
                Building buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null)
                {
                    buildingComponent.condition = buildingData.condition;
                    buildingComponent.efficiency = buildingData.efficiency;
                    buildingComponent.isOperational = buildingData.isOperational;
                }
            }
        }
    }

    private BuildingType GetBuildingType(Building building)
    {
        if (building is ResidentialBuilding) return BuildingType.Residential;
        if (building is CommercialBuilding) return BuildingType.Commercial;
        if (building is IndustrialBuilding) return BuildingType.Industrial;
        // Add other building types here
        return BuildingType.Residential;
    }

    private GameObject GetBuildingPrefab(BuildingType type)
    {
        return BuildingSystem.Instance.GetBuildingPrefab(type);
    }

    public string[] GetSaveFiles()
    {
        if (!Directory.Exists(savePath))
        {
            return new string[0];
        }
        
        string[] files = Directory.GetFiles(savePath, "*" + SAVE_EXTENSION);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        return files;
    }
}

[System.Serializable]
public class GameData
{
    public float gameTime;
    public int day;
    public float timeScale;
    
    public float budget;
    public int population;
    public float happiness;
    
    public float policeCoverage;
    public float fireCoverage;
    public float educationCoverage;
    public float healthcareCoverage;
    
    public List<BuildingData> buildings;
}

[System.Serializable]
public class BuildingData
{
    public Vector3 position;
    public Quaternion rotation;
    public BuildingType buildingType;
    public float condition;
    public float efficiency;
    public bool isOperational;
} 