using UnityEngine;

[CreateAssetMenu(menuName = "Neon Arena/Player Upgrade")]
public class PlayerUpgradeData : ScriptableObject
{
    [SerializeField] private string title;
    [SerializeField, TextArea] private string description;
    [SerializeField] private UpgradeEffectType effectType;
    [SerializeField] private float value;
    
    public string Title => title;
    public string Description => description;
    public UpgradeEffectType EffectType => effectType;

    public float Value => value;
}   
    
