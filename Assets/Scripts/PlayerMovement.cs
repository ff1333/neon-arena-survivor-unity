using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private InputActionAsset inputActions;

    private Rigidbody2D body;
    private InputAction moveAction;
    private Vector2 input;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

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
        body.MovePosition(nextPosition);
    }

}
