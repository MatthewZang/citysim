using UnityEngine;

public class ResidentialBuilding : Building
{
    [Header("Residential Properties")]
    public int maxResidents = 100;
    public float happinessBonus = 0f;
    public float crimeRate = 0f;
    
    private int currentResidents = 0;
    private float residentialEfficiency = 100f;

    protected override void Start()
    {
        base.Start();
        buildingName = "Residential Building";
        cost = 50000f;
        maintenanceCost = 1000f;
        energyConsumption = 50f;
        waterConsumption = 30f;
    }

    public override void OnDayUpdate()
    {
        base.OnDayUpdate();
        
        if (!isOperational) return;

        // Calculate residential efficiency based on various factors
        CalculateResidentialEfficiency();
        
        // Update population
        UpdateResidents();
        
        // Update happiness
        UpdateHappiness();
    }

    private void CalculateResidentialEfficiency()
    {
        // Base efficiency on building condition
        residentialEfficiency = condition;
        
        // Modify efficiency based on service coverage
        residentialEfficiency *= (1f + (GameManager.Instance.policeCoverage * 0.2f));
        residentialEfficiency *= (1f + (GameManager.Instance.healthcareCoverage * 0.2f));
        residentialEfficiency *= (1f + (GameManager.Instance.educationCoverage * 0.1f));
        
        // Clamp efficiency
        residentialEfficiency = Mathf.Clamp(residentialEfficiency, 0f, 100f);
    }

    private void UpdateResidents()
    {
        // Calculate target number of residents based on efficiency
        int targetResidents = Mathf.RoundToInt(maxResidents * (residentialEfficiency / 100f));
        
        // Smoothly adjust current residents
        if (targetResidents > currentResidents)
        {
            currentResidents = Mathf.Min(targetResidents, currentResidents + 2);
        }
        else if (targetResidents < currentResidents)
        {
            currentResidents = Mathf.Max(targetResidents, currentResidents - 2);
        }
        
        // Update city population
        GameManager.Instance.population = currentResidents;
    }

    private void UpdateHappiness()
    {
        // Calculate happiness based on various factors
        float happinessFactor = 0f;
        
        // Building condition affects happiness
        happinessFactor += (condition - 50f) * 0.2f;
        
        // Service coverage affects happiness
        happinessFactor += GameManager.Instance.policeCoverage * 0.3f;
        happinessFactor += GameManager.Instance.healthcareCoverage * 0.3f;
        happinessFactor += GameManager.Instance.educationCoverage * 0.2f;
        
        // Apply happiness bonus
        happinessFactor += happinessBonus;
        
        // Clamp happiness factor
        happinessFactor = Mathf.Clamp(happinessFactor, -10f, 10f);
        
        // Update city happiness
        GameManager.Instance.happiness = Mathf.Clamp(
            GameManager.Instance.happiness + happinessFactor,
            0f,
            100f
        );
    }

    public override void Repair()
    {
        base.Repair();
        residentialEfficiency = 100f;
    }

    public override void Upgrade()
    {
        base.Upgrade();
        maxResidents = Mathf.RoundToInt(maxResidents * 1.2f);
        happinessBonus += 5f;
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (!isPreviewMode)
        {
            UIManager.Instance.ShowBuildingInfo(this);
        }
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (!isPreviewMode)
        {
            UIManager.Instance.HideBuildingInfo();
        }
    }
} 