using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField, Min(0f)] private float edgePadding = 0.45f;

    private Rigidbody2D body;
    private InputAction moveAction;
    private Vector2 input;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

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
        Vector2 nextPosition = body.position + input * moveSpeed * Time.fixedDeltaTime;
        
        if (arenaBounds != null)
        {
            nextPosition = arenaBounds.ClampPoint(nextPosition, edgePadding);
        }

        body.MovePosition(nextPosition);
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Max(0.5f, moveSpeed + amount);
    }

}
