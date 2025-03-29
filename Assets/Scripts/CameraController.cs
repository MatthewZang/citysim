using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    public float rotationSpeed = 100f;
    public float zoomSpeed = 500f;
    
    [Header("Boundaries")]
    public float minHeight = 10f;
    public float maxHeight = 100f;
    public float minZoom = 10f;
    public float maxZoom = 100f;
    
    [Header("Smooth Movement")]
    public float smoothTime = 0.3f;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    
    private float currentZoom;
    private float targetZoom;
    private float currentRotation;
    private float targetRotation;

    private void Start()
    {
        targetPosition = transform.position;
        currentZoom = transform.position.y;
        targetZoom = currentZoom;
        currentRotation = transform.eulerAngles.y;
        targetRotation = currentRotation;
    }

    private void Update()
    {
        HandleInput();
        UpdateCamera();
    }

    private void HandleInput()
    {
        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
        movement = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * movement;
        targetPosition += movement;

        // Rotation
        if (Input.GetKey(KeyCode.Q))
        {
            targetRotation += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            targetRotation -= rotationSpeed * Time.deltaTime;
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }

        // Clamp position
        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
    }

    private void UpdateCamera()
    {
        // Smooth position update
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );

        // Smooth rotation update
        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime / smoothTime);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, currentRotation, 0f);

        // Smooth zoom update
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime / smoothTime);
        Vector3 pos = transform.position;
        pos.y = currentZoom;
        transform.position = pos;
    }

    public void FocusOnPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
    }

    public void SetRotation(float angle)
    {
        targetRotation = angle;
    }

    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
} 