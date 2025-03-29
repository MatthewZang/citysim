using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Properties")]
    public string buildingName;
    public float cost;
    public float maintenanceCost;
    public float energyConsumption;
    public float waterConsumption;
    
    [Header("Building Stats")]
    public float efficiency = 100f;
    public float condition = 100f;
    public bool isOperational = true;
    
    [Header("Visual Settings")]
    public Material previewMaterial;
    private Material originalMaterial;
    private MeshRenderer meshRenderer;
    private bool isPreviewMode = false;

    protected virtual void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
    }

    public virtual void SetPreviewMode(bool preview)
    {
        isPreviewMode = preview;
        if (meshRenderer != null)
        {
            meshRenderer.material = preview ? previewMaterial : originalMaterial;
        }
    }

    public virtual void OnDayUpdate()
    {
        if (!isOperational) return;

        // Calculate daily costs
        float dailyCost = maintenanceCost + (energyConsumption * 0.1f) + (waterConsumption * 0.05f);
        GameManager.Instance.budget -= dailyCost;

        // Update building condition
        UpdateCondition();
    }

    protected virtual void UpdateCondition()
    {
        // Buildings deteriorate over time
        condition = Mathf.Max(0f, condition - 0.1f);

        // If condition is too low, building becomes non-operational
        if (condition < 20f)
        {
            isOperational = false;
            efficiency = 0f;
        }
    }

    public virtual void Repair()
    {
        float repairCost = (100f - condition) * 100f;
        if (GameManager.Instance.budget >= repairCost)
        {
            GameManager.Instance.budget -= repairCost;
            condition = 100f;
            isOperational = true;
            efficiency = 100f;
        }
    }

    public virtual void Upgrade()
    {
        float upgradeCost = cost * 0.5f;
        if (GameManager.Instance.budget >= upgradeCost)
        {
            GameManager.Instance.budget -= upgradeCost;
            efficiency = Mathf.Min(150f, efficiency + 10f);
        }
    }

    protected virtual void OnMouseEnter()
    {
        if (!isPreviewMode)
        {
            // TODO: Show building info UI
            Debug.Log($"Hovering over {buildingName}");
        }
    }

    protected virtual void OnMouseExit()
    {
        if (!isPreviewMode)
        {
            // TODO: Hide building info UI
        }
    }
} 