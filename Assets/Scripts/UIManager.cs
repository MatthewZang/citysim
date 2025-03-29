using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject buildingMenuPanel;
    public GameObject buildingInfoPanel;
    
    [Header("Resource Displays")]
    public TextMeshProUGUI budgetText;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI timeText;
    
    [Header("Building Menu")]
    public Transform buildingButtonContainer;
    public GameObject buildingButtonPrefab;
    
    [Header("Building Info")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingCostText;
    public TextMeshProUGUI buildingEfficiencyText;
    public TextMeshProUGUI buildingConditionText;
    
    [Header("Buttons")]
    public Button pauseButton;
    public Button resumeButton;
    public Button speedButton;
    
    private Dictionary<BuildingType, Button> buildingButtons = new Dictionary<BuildingType, Button>();
    private Building selectedBuilding;

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
        InitializeUI();
        SetupBuildingButtons();
        UpdateAllDisplays();
    }

    private void Update()
    {
        UpdateTimeDisplay();
    }

    private void InitializeUI()
    {
        // Setup button listeners
        pauseButton.onClick.AddListener(OnPauseButtonClicked);
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        speedButton.onClick.AddListener(OnSpeedButtonClicked);
        
        // Hide building info panel initially
        buildingInfoPanel.SetActive(false);
    }

    private void SetupBuildingButtons()
    {
        // Create buttons for each building type
        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            buttonText.text = type.ToString();
            button.onClick.AddListener(() => OnBuildingButtonClicked(type));
            
            buildingButtons.Add(type, button);
        }
    }

    public void UpdateAllDisplays()
    {
        UpdateResourceDisplays();
        UpdateTimeDisplay();
    }

    private void UpdateResourceDisplays()
    {
        budgetText.text = $"Budget: ${GameManager.Instance.budget:N0}";
        populationText.text = $"Population: {GameManager.Instance.population:N0}";
        happinessText.text = $"Happiness: {GameManager.Instance.happiness:F1}%";
    }

    private void UpdateTimeDisplay()
    {
        float hours = Mathf.Floor(GameManager.Instance.gameTime);
        float minutes = (GameManager.Instance.gameTime - hours) * 60f;
        timeText.text = $"Day {GameManager.Instance.day} - {hours:00}:{minutes:00}";
    }

    public void ShowBuildingInfo(Building building)
    {
        selectedBuilding = building;
        buildingInfoPanel.SetActive(true);
        
        buildingNameText.text = building.buildingName;
        buildingCostText.text = $"Cost: ${building.cost:N0}";
        buildingEfficiencyText.text = $"Efficiency: {building.efficiency:F1}%";
        buildingConditionText.text = $"Condition: {building.condition:F1}%";
    }

    public void HideBuildingInfo()
    {
        buildingInfoPanel.SetActive(false);
        selectedBuilding = null;
    }

    private void OnBuildingButtonClicked(BuildingType type)
    {
        BuildingSystem.Instance.StartBuildingPlacement(type);
        buildingMenuPanel.SetActive(false);
    }

    private void OnPauseButtonClicked()
    {
        GameManager.Instance.PauseGame();
        pauseButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);
    }

    private void OnResumeButtonClicked()
    {
        GameManager.Instance.ResumeGame();
        resumeButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    private void OnSpeedButtonClicked()
    {
        float currentSpeed = GameManager.Instance.timeScale;
        float newSpeed = currentSpeed >= 3f ? 1f : currentSpeed + 1f;
        GameManager.Instance.SetTimeScale(newSpeed);
        speedButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Speed: {newSpeed}x";
    }

    public void ToggleBuildingMenu()
    {
        buildingMenuPanel.SetActive(!buildingMenuPanel.activeSelf);
    }

    public void ShowGameOver()
    {
        // TODO: Implement game over screen
        Debug.Log("Game Over!");
    }
} 