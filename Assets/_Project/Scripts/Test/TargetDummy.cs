using UnityEngine;
using FightingGame.Core.Interfaces;

public class TargetDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => _currentHealth > 0;

    private void Awake() => _currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        Debug.Log($"<color=red>DUMMY HASAR ALDI!</color> Kalan Can: {_currentHealth}");
        
        if (_currentHealth <= 0)
        {
            Debug.Log("<color=black>DUMMY YIKILDI!</color>");
            // Buraya bir partikül veya renk değişimi eklenebilir
            _currentHealth = maxHealth; // Test için canı fullesin
        }
    }
}