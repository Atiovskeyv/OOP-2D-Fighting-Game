// ============================================================
//  TargetDummy.cs
//  Namespace : FightingGame.Test
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Saldırı/hasar sistemini test etmek için minimal IDamageable
//  implementasyonu. AbstractCharacter olmadan da hasar zincirinin
//  doğru çalıştığını doğrulamak için kullanılır.
//
//  IDamageable sözleşmesine uyar:
//    • amount <= 0 ise erken çıkış (negatif hasar = heal değildir)
//    • Ölü durumda hasar uygulanmaz
//    • CurrentHealth asla negatife düşmez
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Interfaces;

namespace FightingGame.Test
{
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 100;

        [Header("Test Options")]
        [Tooltip("Can sıfırlandığında dummy'yi tam canla geri yükle. " +
                 "Sadece test/debug için; production karakterlerinde Die() çağrılmalı.")]
        [SerializeField] private bool autoRespawnOnDeath = true;

        private int _currentHealth;

        public int  CurrentHealth => _currentHealth;
        public int  MaxHealth     => maxHealth;
        public bool IsAlive       => _currentHealth > 0;

        private void Awake() => _currentHealth = maxHealth;

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            Debug.Log($"<color=red>[Dummy] Hasar aldı: {amount} → Kalan can: {_currentHealth}/{maxHealth}</color>", this);

            if (_currentHealth > 0) return;

            Debug.Log("<color=black>[Dummy] Yıkıldı!</color>", this);

            if (autoRespawnOnDeath)
            {
                _currentHealth = maxHealth;
                Debug.Log($"<color=green>[Dummy] Auto-respawn → {_currentHealth}/{maxHealth}</color>", this);
            }
        }
    }
}
