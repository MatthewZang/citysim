using UnityEngine;

public class CommercialBuilding : Building
{
    [Header("Commercial Properties")]
    public int maxWorkers = 50;
    public float taxRate = 0.1f;
    public float businessEfficiency = 100f;
    public float customerSatisfaction = 50f;
    
    private int currentWorkers = 0;
    private float dailyIncome = 0f;

    protected override void Start()
    {
        base.Start();
        buildingName = "Commercial Building";
        cost = 75000f;
        maintenanceCost = 1500f;
        energyConsumption = 75f;
        waterConsumption = 40f;
    }

    public override void OnDayUpdate()
    {
        base.OnDayUpdate();
        
        if (!isOperational) return;

        // Calculate business efficiency
        CalculateBusinessEfficiency();
        
        // Update workers
        UpdateWorkers();
        
        // Generate income
        GenerateIncome();
        
        // Update customer satisfaction
        UpdateCustomerSatisfaction();
    }

    private void CalculateBusinessEfficiency()
    {
        // Base efficiency on building condition
        businessEfficiency = condition;
        
        // Modify efficiency based on service coverage
        businessEfficiency *= (1f + (GameManager.Instance.policeCoverage * 0.15f));
        businessEfficiency *= (1f + (GameManager.Instance.fireCoverage * 0.15f));
        businessEfficiency *= (1f + (GameManager.Instance.educationCoverage * 0.1f));
        
        // Clamp efficiency
        businessEfficiency = Mathf.Clamp(businessEfficiency, 0f, 100f);
    }

    private void UpdateWorkers()
    {
        // Calculate target number of workers based on efficiency
        int targetWorkers = Mathf.RoundToInt(maxWorkers * (businessEfficiency / 100f));
        
        // Smoothly adjust current workers
        if (targetWorkers > currentWorkers)
        {
            currentWorkers = Mathf.Min(targetWorkers, currentWorkers + 1);
        }
        else if (targetWorkers < currentWorkers)
        {
            currentWorkers = Mathf.Max(targetWorkers, currentWorkers - 1);
        }
    }

    private void GenerateIncome()
    {
        // Calculate daily income based on workers and efficiency
        dailyIncome = currentWorkers * 100f * (businessEfficiency / 100f);
        
        // Apply tax rate
        float taxAmount = dailyIncome * taxRate;
        
        // Add to city budget
        GameManager.Instance.budget += taxAmount;
    }

    private void UpdateCustomerSatisfaction()
    {
        float satisfactionFactor = 0f;
        
        // Building condition affects satisfaction
        satisfactionFactor += (condition - 50f) * 0.2f;
        
        // Service coverage affects satisfaction
        satisfactionFactor += GameManager.Instance.policeCoverage * 0.2f;
        satisfactionFactor += GameManager.Instance.fireCoverage * 0.2f;
        satisfactionFactor += GameManager.Instance.educationCoverage * 0.1f;
        
        // Update customer satisfaction
        customerSatisfaction = Mathf.Clamp(
            customerSatisfaction + satisfactionFactor,
            0f,
            100f
        );
    }

    public override void Repair()
    {
        base.Repair();
        businessEfficiency = 100f;
    }

    public override void Upgrade()
    {
        base.Upgrade();
        maxWorkers = Mathf.RoundToInt(maxWorkers * 1.2f);
        taxRate += 0.02f;
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