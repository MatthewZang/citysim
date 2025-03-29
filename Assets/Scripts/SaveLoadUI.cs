using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SaveLoadUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject saveLoadPanel;
    public GameObject saveSlotPrefab;
    public Transform saveSlotContainer;
    public TMP_InputField saveNameInput;
    public Button saveButton;
    public Button loadButton;
    public Button closeButton;
    
    private Dictionary<string, GameObject> saveSlots = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Setup button listeners
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        // Hide panel initially
        saveLoadPanel.SetActive(false);
    }

    public void ShowSaveLoadPanel()
    {
        saveLoadPanel.SetActive(true);
        RefreshSaveSlots();
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
                button.onClick.AddListener(() => OnLoadSlotClicked(saveName));
            }
            else if (button.name == "DeleteButton")
            {
                button.onClick.AddListener(() => OnDeleteSlotClicked(saveName));
            }
        }
    }

    private void OnSaveButtonClicked()
    {
        string saveName = saveNameInput.text.Trim();
        if (string.IsNullOrEmpty(saveName))
        {
            // TODO: Show error message
            Debug.LogWarning("Please enter a save name");
            return;
        }

        SaveSystem.Instance.SaveGame(saveName);
        RefreshSaveSlots();
        saveNameInput.text = "";
    }

    private void OnLoadButtonClicked()
    {
        string saveName = saveNameInput.text.Trim();
        if (string.IsNullOrEmpty(saveName))
        {
            // TODO: Show error message
            Debug.LogWarning("Please enter a save name");
            return;
        }

        SaveSystem.Instance.LoadGame(saveName);
        OnCloseButtonClicked();
    }

    private void OnLoadSlotClicked(string saveName)
    {
        saveNameInput.text = saveName;
        OnLoadButtonClicked();
    }

    private void OnDeleteSlotClicked(string saveName)
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Saves", saveName + ".city");
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            RefreshSaveSlots();
        }
    }

    private void OnCloseButtonClicked()
    {
        saveLoadPanel.SetActive(false);
        saveNameInput.text = "";
    }
} 