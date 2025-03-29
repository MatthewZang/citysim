using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [Header("Required Prefabs")]
    public GameObject gameManagerPrefab;
    public GameObject buildingSystemPrefab;
    public GameObject uiManagerPrefab;
    public GameObject saveSystemPrefab;
    public GameObject cameraControllerPrefab;

    private void Start()
    {
        // Check if we're loading a saved game or starting a new one
        string cityName = PlayerPrefs.GetString("CurrentCityName", "");
        if (string.IsNullOrEmpty(cityName))
        {
            Debug.LogError("No city name found! Returning to main menu.");
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Initialize game systems
        InitializeGameSystems();

        // If this is a saved game, load it
        if (SaveSystem.Instance.GetSaveFiles().Contains(cityName))
        {
            SaveSystem.Instance.LoadGame(cityName);
        }
        else
        {
            // Initialize new game
            InitializeNewGame(cityName);
        }
    }

    private void InitializeGameSystems()
    {
        // Create GameManager
        if (FindObjectOfType<GameManager>() == null)
        {
            Instantiate(gameManagerPrefab);
        }

        // Create BuildingSystem
        if (FindObjectOfType<BuildingSystem>() == null)
        {
            Instantiate(buildingSystemPrefab);
        }

        // Create UIManager
        if (FindObjectOfType<UIManager>() == null)
        {
            Instantiate(uiManagerPrefab);
        }

        // Create SaveSystem
        if (FindObjectOfType<SaveSystem>() == null)
        {
            Instantiate(saveSystemPrefab);
        }

        // Create CameraController
        if (FindObjectOfType<CameraController>() == null)
        {
            Instantiate(cameraControllerPrefab);
        }
    }

    private void InitializeNewGame(string cityName)
    {
        // Set initial game state
        GameManager.Instance.budget = 1000000f;
        GameManager.Instance.population = 1000;
        GameManager.Instance.happiness = 50f;
        GameManager.Instance.day = 1;
        GameManager.Instance.gameTime = 0f;
        GameManager.Instance.timeScale = 1f;

        // Set initial service coverage
        GameManager.Instance.policeCoverage = 0f;
        GameManager.Instance.fireCoverage = 0f;
        GameManager.Instance.educationCoverage = 0f;
        GameManager.Instance.healthcareCoverage = 0f;

        // Save the initial game state
        SaveSystem.Instance.SaveGame(cityName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
} 