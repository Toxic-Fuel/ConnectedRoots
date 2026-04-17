using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraLeftClick : MonoBehaviour
{
    [SerializeField] private float dragSensitivity = 0.2f;
    [SerializeField] private float dragSmoothing = 0.05f;
    [SerializeField] private float dragStartThresholdPixels = 6f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float maxZ = 50f;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float minZ = -50f;

    private bool isDragging = false;
    private bool pendingDragStart = false;
    private Mouse mouse;
    private Vector2 smoothedDelta;
    private Vector2 deltaVelocity;
    private Vector2 dragPressScreenPosition;
    private bool suppressDragUntilRelease;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>(8);
    public float settingsDragSensitivity = 1.0f;
    [SerializeField] private SelectTile selectTile;

    private void OnEnable()
    {
        mouse = Mouse.current;
        if (selectTile == null)
        {
            selectTile = FindAnyObjectByType<SelectTile>();
        }
    }

    private void Update()
    {
        HandleDragInput();
    }

    private void HandleDragInput()
    {
        if (InGameGenerationMenu.IsAnyMenuOpen)
        {
            ResetDragState();
            return;
        }

        if (mouse == null)
            return;

        if (suppressDragUntilRelease)
        {
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                suppressDragUntilRelease = false;
                ResetDragState();
            }

            return;
        }

        if (selectTile != null && selectTile.HasSelection)
        {
            ResetDragState();
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 pressPosition = mouse.position.ReadValue();
            if (IsPointerOverUi(pressPosition))
            {
                suppressDragUntilRelease = true;
                ResetDragState();
                return;
            }

            pendingDragStart = true;
            isDragging = false;
            dragPressScreenPosition = pressPosition;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            ResetDragState();
            return;
        }

        if (pendingDragStart)
        {
            Vector2 currentPos = mouse.position.ReadValue();
            float thresholdSquared = dragStartThresholdPixels * dragStartThresholdPixels;
            if ((currentPos - dragPressScreenPosition).sqrMagnitude >= thresholdSquared)
            {
                pendingDragStart = false;
                isDragging = true;
                smoothedDelta = Vector2.zero;
                deltaVelocity = Vector2.zero;
            }
        }

        if (isDragging)
        {
            Vector2 rawDelta = mouse.delta.ReadValue();
            smoothedDelta = Vector2.SmoothDamp(smoothedDelta, rawDelta, ref deltaVelocity, dragSmoothing);
            MoveCamera(smoothedDelta);
        }
    }

    private void MoveCamera(Vector2 mouseDelta)
    {
        float moveX = -mouseDelta.x * dragSensitivity * settingsDragSensitivity;
        float moveZ = -mouseDelta.y * dragSensitivity * settingsDragSensitivity;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 worldMovement = (right * moveX) + (forward * moveZ);

        Vector3 newPosition = transform.localPosition + worldMovement;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        transform.localPosition = newPosition;
    }

    private void ResetDragState()
    {
        isDragging = false;
        pendingDragStart = false;
        smoothedDelta = Vector2.zero;
        deltaVelocity = Vector2.zero;
    }

    private bool IsPointerOverUi(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        uiRaycastResults.Clear();
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }
}
