using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField, Min(0f)] private float edgePadding = 0.45f;

    private Rigidbody2D body;
    private InputAction moveAction;
    private Camera mainCamera;
    private Vector2 input;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (inputActions == null)
        {
            Debug.LogError("PlayerMovement requires an Input Action Asset. Assign Assets/InputSystem_Actions.inputactions in the Player Inspector.", this);
            return;
        }

        moveAction = inputActions.FindAction("Player/Move", true);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
    }

    private void Update()
    {
        if (moveAction == null)
        {
            return;
        }

        input = moveAction.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = body.position + input * moveSpeed * Time.fixedDeltaTime;
        
        if (mainCamera != null && mainCamera.orthographic)
        {
            float halfHeight = Mathf.Max(0f, mainCamera.orthographicSize - edgePadding);
            float halfWidth = Mathf.Max(0f, mainCamera.orthographicSize * mainCamera.aspect - edgePadding);
            Vector3 cameraPosition = mainCamera.transform.position;
            nextPosition.x = Mathf.Clamp(nextPosition.x, cameraPosition.x - halfWidth, cameraPosition.x + halfWidth);
            nextPosition.y = Mathf.Clamp(nextPosition.y, cameraPosition.y - halfHeight, cameraPosition.y + halfHeight);
        }

        body.MovePosition(nextPosition);
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Max(0.5f, moveSpeed + amount);
    }

}
