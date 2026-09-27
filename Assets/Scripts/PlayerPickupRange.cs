using UnityEngine;

public class PlayerPickupRange : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float currentRadius = 3f;
    [SerializeField, Min(0.1f)] private float maximumRadius = 6f;

    public float CurrentRadius => currentRadius;
    public float MaximumRadius => maximumRadius;
    public bool CanIncrease => currentRadius < maximumRadius - 0.001f;

    private void Awake()
    {
        maximumRadius = Mathf.Max(0.1f, maximumRadius);
        currentRadius = Mathf.Clamp(currentRadius, 0.1f, maximumRadius);
    }

    public void AddRadius(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentRadius = Mathf.Min(maximumRadius, currentRadius + amount);
    }
}