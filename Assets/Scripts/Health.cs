using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public bool IsDead => Current <= 0f;

    public event Action<float, float> Changed;
    public event Action Died;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void ResetHealth(float newMax)
    {
        maxHealth = Mathf.Max(1f, newMax);
        Current = maxHealth;
        Changed?.Invoke(Current, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        
        Current = Mathf.Max(0f, Current - amount);
        Changed?.Invoke(Current, maxHealth);
        if (IsDead)
        {
            Died?.Invoke();
        }
    }

    public void RestoreFull()
    {
        Current = maxHealth;
        Changed?.Invoke(Current, maxHealth);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        Current = Mathf.Min(maxHealth, Current + amount);
        Changed?.Invoke(Current, maxHealth);
    }

    public void AddMaxHealth(float amount)
    {
        if (amount <= 0f)
        { 
            return;
        }

        maxHealth += amount;
        Current += amount;
        Changed?.Invoke(Current, maxHealth);
    }
}
