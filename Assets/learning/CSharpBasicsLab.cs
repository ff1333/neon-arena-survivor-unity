using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CSharpBasicsLab : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxArmor = 80;


    private readonly List<string> backpack = new List<string>();
    private readonly Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    private PlayerStats stats;
    private void Start()
    {
        stats = new PlayerStats(maxHealth, maxArmor);
        stats.ArmorChanged += OnArmorChanged;
        stats.HealthChanged += OnHealthChanged;
        stats.Died += OnDied;
        stats.Revied += OnRevied;


        //AddItem("Potion");
        //AddItem("Coin");
        //AddItem("Potion");
        //PrintBackpack();

        //RemoveItem("Coin");
        //RemoveItem("Apple");
        //RemoveItem("Potion");
        //PrintBackpack();

        stats.TakeDamage(30);
        //stats.Heal(10);
        stats.TakeDamage(100);
        stats.TakeDamage(100);
        stats.TakeDamage(100);
        stats.Revive();
    }

    private void OnDestroy()
    {
        if (stats == null)
        {
            return;
        }
        stats.ArmorChanged -= OnArmorChanged;
        stats.HealthChanged -= OnHealthChanged;
        stats.Died -= OnDied;
        stats.Revied -= OnRevied;
    }
    
    private void OnArmorChanged(int current, int maximum)
    {
        Debug.Log($"Armor: {current}/{maximum}");
    }
    private void OnHealthChanged(int current, int maximum)
    {
        Debug.Log($"Health: {current}/{maximum}");
    }

    private void OnDied()
    {
        Debug.Log("Player died");
    }
    private void OnRevied()
    {
        Debug.Log("Player revied");
    }

    private void AddItem(string itemName)
    {
        backpack.Add(itemName);

        if (itemCounts.TryGetValue(itemName, out int count))
        {
            itemCounts[itemName] = count +1;
        }
        else
        {
            itemCounts.Add(itemName, 1);
        }
    }
    private void RemoveItem(string itemName)
    {
        if (!itemCounts.ContainsKey(itemName))
        {
            Debug.LogWarning($"{itemName} is not in backpack!");
            return;
        }


        if (itemCounts.TryGetValue(itemName, out int count))
        {
            backpack.Remove(itemName);
            itemCounts[itemName] = count -1;
            if (itemCounts[itemName] == 0)
            {
                itemCounts.Remove(itemName);
            }
        }
    }

    private bool HasItem(string itemName)
    {
        return itemCounts.ContainsKey(itemName);
    }

    private void PrintBackpack()
    {
        for (int i = 0; i < backpack.Count; i++)
        {
            Debug.Log($"slot {i}: {backpack[i]}");
        }

        foreach (KeyValuePair<string, int> pair in itemCounts)
        {
            Debug.Log($"{pair.Key} x {pair.Value}");
        }

        Debug.Log($"Has Coin: {HasItem("Coin")}");
    }
}
