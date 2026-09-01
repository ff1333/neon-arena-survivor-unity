using System;
using System.Diagnostics;
using UnityEngine.Rendering;

[Serializable]
public class PlayerStats
{
    private int currentHealth;
    private int currentArmor;

    public int MaxHealth { get; }
    public int MaxArmor { get; }
    public int CurrentHealth => currentHealth;
    public int CurrentArmor => currentArmor;
    public bool IsDied => currentHealth <= 0;

    public event Action<int, int> ArmorChanged;
    public event Action<int, int> HealthChanged;
    public event Action Died;
    public event Action Revied;

    public PlayerStats(int maxHealth, int maxArmor)
    {
        if (maxHealth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth));
        }
        if (maxArmor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxArmor));
        }

        MaxHealth = maxHealth;
        currentHealth = maxHealth;
        MaxArmor = maxArmor;
        currentArmor = maxArmor;
    }

    public void TakeDamage(int amount)
    {
        if ( amount  <= 0 || IsDied)
        {
            return;
        }
        if (currentArmor == 0)
        {
            currentHealth = Math.Max(0, currentHealth - amount);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }
        if (currentArmor != 0)
        {
            if (currentArmor - amount / 2 > 0)
            {
                currentArmor -= amount / 2;
                ArmorChanged?.Invoke(currentArmor, MaxArmor);
            }
            else
            {
                currentHealth = Math.Max(0, currentHealth + (currentArmor - amount / 2));
                HealthChanged?.Invoke(currentHealth, MaxHealth);
                currentArmor = 0;
                ArmorChanged?.Invoke(currentArmor, MaxArmor);
            }
            
        }
        
        if (IsDied)
        {
            Died?.Invoke();
        }
    }




















    //public void TakeDamage(int amount)
    //{
    //    if (amount <= 0 || IsDead) 
    //    {
    //        return;
    //    }
    //    if (currentArmor == 0)
    //    {
    //        currentHealth = Math.Max(0, currentHealth - amount);
    //        HealthChanged?.Invoke(currentHealth, MaxHealth);
    //    }
    //    if (currentArmor > 0)
    //    {
    //        if (currentArmor - amount / 2 > 0)
    //        {
    //            currentArmor = Math.Max(0, currentArmor - amount / 2);

    //            ArmorChanged?.Invoke(currentArmor, MaxArmor);
    //        }
            
            
    //        else
    //        {
    //            currentHealth = Math.Max(0, currentHealth + (currentArmor - amount / 2));
    //            HealthChanged?.Invoke(currentHealth, MaxHealth);
    //            currentArmor = 0;
    //        }
    //    }

      

    //    if (IsDead)
    //    {
    //        Died?.Invoke();
    //    }
        
    //}

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDied)
        {
            return;
        }
        currentHealth = Math.Min(MaxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void Revive()
    {
        if (IsDied)
        {
            currentHealth = MaxHealth / 2;
            Revied?.Invoke();
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            ArmorChanged?.Invoke(currentArmor, MaxArmor);
        }
    }

}
