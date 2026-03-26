using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private float minDistance = 0.2f;
    [SerializeField] private float maxDistance = 3f;

    private Mouse mouse;
    private Vector3 targetPosition;

    private void OnEnable()
    {
        mouse = Mouse.current;
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (mouse == null)
            return;

        float scrollDelta = mouse.scroll.ReadValue().y;
        if (scrollDelta != 0)
        {
            Vector3 scrollDirection = transform.forward * scrollDelta * scrollSpeed;
            Vector3 newPosition = transform.position + scrollDirection;

            // Clamp distance along the forward direction from a reference point (e.g., origin or parent)
            Vector3 referencePoint = transform.parent != null ? transform.parent.position : Vector3.zero;
            float distance = Vector3.Distance(newPosition, referencePoint);
            
            if (distance >= minDistance && distance <= maxDistance)
            {
                transform.position = newPosition;
            }
        }
    }
}
