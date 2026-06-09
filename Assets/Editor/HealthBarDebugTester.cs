// ============================================================
//  HealthBarDebugTester.cs — Geçici test scripti
//  Sahneye ekle, Play modunda T tuşuna basarak health bar'ı test et.
//  Test bitince bu scripti silebilirsin.
// ============================================================

#if UNITY_EDITOR
using UnityEngine;
using FightingGame.UI;
using FightingGame.Core.Interfaces;

public class HealthBarDebugTester : MonoBehaviour
{
    [Header("Test Hedefi")]
    [Tooltip("Test edilecek HealthBarUI (HealthBarP1 veya P2).")]
    [SerializeField] private HealthBarUI targetHealthBar;

    [Header("Test Ayarları")]
    [SerializeField] private int testMaxHealth = 100;
    [SerializeField] private int damagePerPress = 15;

    private DummyDamageable _dummy;

    private void Start()
    {
        if (targetHealthBar == null)
        {
            Debug.LogError("[DebugTester] targetHealthBar atanmamış!");
            return;
        }

        _dummy = new DummyDamageable(testMaxHealth);
        targetHealthBar.SetTarget(_dummy);
        Debug.Log($"[DebugTester] Test başladı! T = hasar ver ({damagePerPress}), Y = iyileştir, R = reset");
    }

    private void Update()
    {
        if (_dummy == null) return;

        if (UnityEngine.Input.GetKeyDown(KeyCode.T))
        {
            _dummy.TakeDamage(damagePerPress);
            Debug.Log($"[DebugTester] Hasar: {damagePerPress} → Can: {_dummy.CurrentHealth}/{_dummy.MaxHealth} | Bar: {_dummy.SpecialMeterSegments}/3");
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Y))
        {
            _dummy.Heal(damagePerPress);
            Debug.Log($"[DebugTester] İyileşme: {damagePerPress} → Can: {_dummy.CurrentHealth}/{_dummy.MaxHealth}");
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            _dummy.Reset();
            Debug.Log("[DebugTester] Can resetlendi!");
        }
    }

    /// <summary>
    /// Test için sahte IDamageable ve ICharacter implementasyonu.
    /// </summary>
    private class DummyDamageable : IDamageable, ICharacter
    {
        private int _current;
        private int _max;
        private float _specialMeter;

        public int CurrentHealth => _current;
        public int MaxHealth     => _max;
        public bool IsAlive      => _current > 0;
        
        public FightingGame.Core.Data.CharacterData Data => null;
        public float SpecialMeter => _specialMeter;
        public int SpecialMeterSegments => Mathf.FloorToInt(_specialMeter / 100f);

        public DummyDamageable(int max)
        {
            _max = max;
            _current = max;
            _specialMeter = 0;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;
            _current = Mathf.Max(0, _current - amount);
            // Her hasarda bar dolsun (Test için hızlı dolduruyoruz)
            _specialMeter = Mathf.Min(300f, _specialMeter + (amount * 5f)); 
        }

        public void Heal(int amount)
        {
            _current = Mathf.Min(_max, _current + amount);
        }

        public void Reset() 
        {
            _current = _max;
            _specialMeter = 0f;
        }

        // ICharacter boş implementasyonlar (sadece test için gerekiyor)
        public void Walk(float direction) {}
        public void Jump() {}
        public void Punch() {}
        public void Kick() {}
        public void Shoot() {}
        public void Block(bool active) {}
        public void Crouch(bool active) {}
        public void Dash(float direction) {}
        public void ExecuteSkill1() { _specialMeter = Mathf.Max(0, _specialMeter - 100f); }
        public void ExecuteSkill2() { _specialMeter = Mathf.Max(0, _specialMeter - 200f); }
        public void ExecuteSkill3() { _specialMeter = Mathf.Max(0, _specialMeter - 300f); }
        public void SetOpponent(Transform opponentTransform) {}
    }
}
#endif
