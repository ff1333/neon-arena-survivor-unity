using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float moveSpeed = 6f;
    [SerializeField, Min(0.5f)] private float maximumMoveSpeed = 9f;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField, Min(0f)] private float edgePadding = 0.45f;

    private Rigidbody2D body;
    private InputAction moveAction;
    private Vector2 input;

    public float MoveSpeed => moveSpeed;
    public float MaximumMoveSpeed => maximumMoveSpeed;
    public bool CanIncreaseMoveSpeed =>
        moveSpeed < maximumMoveSpeed - 0.001f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        maximumMoveSpeed = Mathf.Max(0.5f, maximumMoveSpeed);
        moveSpeed = Mathf.Clamp(moveSpeed, 0.5f, maximumMoveSpeed);

        if (inputActions == null)
        {
            Debug.LogError("PlayerMovement requires an Input Action Asset.", this);
            return;
        }

        if (arenaBounds == null)
        {
            Debug.LogError("PlayerMovement requires ArenaBounds.", this);
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
        Vector2 nextPosition =
            body.position + input * moveSpeed * Time.fixedDeltaTime;

        if (arenaBounds != null)
        {
            nextPosition = arenaBounds.ClampPoint(nextPosition, edgePadding);
        }

        body.MovePosition(nextPosition);
    }

    public void AddMoveSpeed(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        moveSpeed = Mathf.Min(maximumMoveSpeed, moveSpeed + amount);
    }
}