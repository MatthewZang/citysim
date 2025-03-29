using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public float gameTime = 0f;
    public int day = 1;
    public float timeScale = 1f;
    
    [Header("Resources")]
    public float budget = 1000000f;
    public int population = 1000;
    public float happiness = 50f;
    
    [Header("City Services")]
    public float policeCoverage = 0f;
    public float fireCoverage = 0f;
    public float educationCoverage = 0f;
    public float healthcareCoverage = 0f;

    [Header("Map")]
    public MapManager mapManager;

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
        // Make sure MapManager is assigned
        if (mapManager == null)
        {
            mapManager = GetComponent<MapManager>();
        }
    }

    private void Update()
    {
        // Update game time
        gameTime += Time.deltaTime * timeScale;
        
        // Check for day change
        if (gameTime >= 24f)
        {
            gameTime = 0f;
            day++;
            OnNewDay();
        }
    }

    private void OnNewDay()
    {
        // Calculate daily expenses and income
        CalculateDailyFinances();
        
        // Update city services
        UpdateCityServices();
        
        // Update population happiness
        UpdateHappiness();
    }

    private void CalculateDailyFinances()
    {
        // TODO: Implement tax collection and service costs
        float dailyIncome = population * 10f; // Basic tax per citizen
        float dailyExpenses = CalculateDailyExpenses();
        
        budget += dailyIncome - dailyExpenses;
    }

    private float CalculateDailyExpenses()
    {
        float expenses = 0f;
        // TODO: Calculate expenses based on city services and buildings
        return expenses;
    }

    private void UpdateCityServices()
    {
        // TODO: Update service coverage based on buildings and population
    }

    private void UpdateHappiness()
    {
        // TODO: Calculate happiness based on various factors
        float happinessFactors = 0f;
        
        // Apply happiness factors
        happiness = Mathf.Clamp(happiness + happinessFactors, 0f, 100f);
    }

    public void PauseGame()
    {
        timeScale = 0f;
    }

    public void ResumeGame()
    {
        timeScale = 1f;
    }

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Clamp(scale, 0f, 3f);
    }
} 