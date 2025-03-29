using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject mainMenuPanel;
    public GameObject newGamePanel;
    public GameObject loadGamePanel;
    public GameObject settingsPanel;
    
    [Header("New Game Settings")]
    public TMP_InputField cityNameInput;
    public Button startGameButton;
    public Button backToMainButton;
    
    [Header("Load Game")]
    public GameObject saveSlotPrefab;
    public Transform saveSlotContainer;
    public Button backToMainFromLoadButton;
    
    private Dictionary<string, GameObject> saveSlots = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Setup button listeners
        startGameButton.onClick.AddListener(OnStartGameClicked);
        backToMainButton.onClick.AddListener(OnBackToMainClicked);
        backToMainFromLoadButton.onClick.AddListener(OnBackToMainClicked);
        
        // Show main menu initially
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowNewGamePanel()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(true);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        cityNameInput.text = "";
    }

    public void ShowLoadGamePanel()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(true);
        settingsPanel.SetActive(false);
        RefreshSaveSlots();
    }

    public void ShowSettingsPanel()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void RefreshSaveSlots()
    {
        // Clear existing slots
        foreach (var slot in saveSlots.Values)
        {
            Destroy(slot);
        }
        saveSlots.Clear();

        // Get save files
        string[] saveFiles = SaveSystem.Instance.GetSaveFiles();
        
        // Create slots for each save file
        foreach (string saveFile in saveFiles)
        {
            CreateSaveSlot(saveFile);
        }
    }

    private void CreateSaveSlot(string saveName)
    {
        GameObject slot = Instantiate(saveSlotPrefab, saveSlotContainer);
        saveSlots.Add(saveName, slot);

        // Setup slot UI
        TextMeshProUGUI nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
        nameText.text = saveName;

        // Setup slot buttons
        Button[] buttons = slot.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.name == "LoadButton")
            {
                button.onClick.AddListener(() => OnLoadGameClicked(saveName));
            }
            else if (button.name == "DeleteButton")
            {
                button.onClick.AddListener(() => OnDeleteSaveClicked(saveName));
            }
        }
    }

    private void OnStartGameClicked()
    {
        string cityName = cityNameInput.text.Trim();
        if (string.IsNullOrEmpty(cityName))
        {
            // TODO: Show error message
            Debug.LogWarning("Please enter a city name");
            return;
        }

        // Save the city name for the new game
        PlayerPrefs.SetString("CurrentCityName", cityName);
        
        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }

    private void OnLoadGameClicked(string saveName)
    {
        // Save the city name for loading
        PlayerPrefs.SetString("CurrentCityName", saveName);
        
        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }

    private void OnDeleteSaveClicked(string saveName)
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Saves", saveName + ".city");
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            RefreshSaveSlots();
        }
    }

    private void OnBackToMainClicked()
    {
        ShowMainMenu();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 