using UnityEngine;

public class IndustrialBuilding : Building
{
    [Header("Industrial Properties")]
    public int maxWorkers = 100;
    public float productionRate = 100f;
    public float pollutionLevel = 0f;
    public float resourceEfficiency = 100f;
    
    private int currentWorkers = 0;
    private float dailyProduction = 0f;
    private float dailyPollution = 0f;

    protected override void Start()
    {
        base.Start();
        buildingName = "Industrial Building";
        cost = 100000f;
        maintenanceCost = 2000f;
        energyConsumption = 150f;
        waterConsumption = 80f;
    }

    public override void OnDayUpdate()
    {
        base.OnDayUpdate();
        
        if (!isOperational) return;

        // Calculate resource efficiency
        CalculateResourceEfficiency();
        
        // Update workers
        UpdateWorkers();
        
        // Generate production
        GenerateProduction();
        
        // Update pollution
        UpdatePollution();
    }

    private void CalculateResourceEfficiency()
    {
        // Base efficiency on building condition
        resourceEfficiency = condition;
        
        // Modify efficiency based on service coverage
        resourceEfficiency *= (1f + (GameManager.Instance.fireCoverage * 0.2f));
        resourceEfficiency *= (1f + (GameManager.Instance.educationCoverage * 0.1f));
        
        // Clamp efficiency
        resourceEfficiency = Mathf.Clamp(resourceEfficiency, 0f, 100f);
    }

    private void UpdateWorkers()
    {
        // Calculate target number of workers based on efficiency
        int targetWorkers = Mathf.RoundToInt(maxWorkers * (resourceEfficiency / 100f));
        
        // Smoothly adjust current workers
        if (targetWorkers > currentWorkers)
        {
            currentWorkers = Mathf.Min(targetWorkers, currentWorkers + 2);
        }
        else if (targetWorkers < currentWorkers)
        {
            currentWorkers = Mathf.Max(targetWorkers, currentWorkers - 2);
        }
    }

    private void GenerateProduction()
    {
        // Calculate daily production based on workers and efficiency
        dailyProduction = currentWorkers * productionRate * (resourceEfficiency / 100f);
        
        // Add to city budget (industrial tax)
        float industrialTax = dailyProduction * 0.15f;
        GameManager.Instance.budget += industrialTax;
    }

    private void UpdatePollution()
    {
        // Calculate pollution based on production and efficiency
        dailyPollution = dailyProduction * 0.1f * (1f - (resourceEfficiency / 100f));
        
        // Update pollution level
        pollutionLevel = Mathf.Min(100f, pollutionLevel + dailyPollution);
        
        // Pollution affects city happiness
        float pollutionImpact = pollutionLevel * 0.1f;
        GameManager.Instance.happiness = Mathf.Max(0f, GameManager.Instance.happiness - pollutionImpact);
    }

    public override void Repair()
    {
        base.Repair();
        resourceEfficiency = 100f;
    }

    public override void Upgrade()
    {
        base.Upgrade();
        maxWorkers = Mathf.RoundToInt(maxWorkers * 1.2f);
        productionRate *= 1.1f;
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