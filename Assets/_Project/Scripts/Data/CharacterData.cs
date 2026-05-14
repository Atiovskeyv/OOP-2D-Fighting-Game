// ============================================================
//  CharacterData.cs
//  Namespace : FightingGame.Core.Data
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  DESIGN PATTERN: Data-Driven Design + ScriptableObject Pattern
//
//  Mevcut dosyanın güncellenmiş versiyonu.
//  Eklenen: Skill data alanları (skill1/skill2 cooldown, damage)
//
// ============================================================

using UnityEngine;

namespace FightingGame.Core.Data
{
    [CreateAssetMenu(
        fileName = "New_CharacterData",
        menuName  = "FightingGame/Character Data",
        order     = 0)]
    public class CharacterData : ScriptableObject
    {
        // ── Genel Bilgiler ───────────────────────────────────────

        [Header("General")]
        [Tooltip("Karakter adı. UI ve log mesajlarında kullanılır.")]
        public string characterName = "Unnamed";

        // ── Sağlık ──────────────────────────────────────────────

        [Header("Health")]
        [Tooltip("Başlangıç ve maksimum can puanı.")]
        [Min(1)]
        public int maxHealth = 100;

        // ── Hareket ─────────────────────────────────────────────

        [Header("Movement")]
        [Tooltip("Yatay hareket hızı (birim/saniye).")]
        [Min(0f)]
        public float moveSpeed = 5f;

        [Tooltip("Zıplama kuvveti.")]
        [Min(0f)]
        public float jumpForce = 10f;

        // ── Saldırı ─────────────────────────────────────────────

        [Header("Attack")]
        [Tooltip("Temel saldırı hasarı. Zırh/direnç hesabından önce uygulanır.")]
        [Min(0)]
        public int attackPower = 20;

        [Tooltip("İki ardışık saldırı arasındaki minimum süre (saniye).")]
        [Min(0f)]
        public float attackCooldown = 0.5f;

        [Tooltip("Saldırının etki menzili (metre).")]
        [Min(0f)]
        public float attackRange = 1.5f;

        // ── Dash ────────────────────────────────────────────────

        [Header("Dash")]
        [Tooltip("İki ardışık dash arasındaki minimum bekleme süresi (saniye).")]
        [Min(0f)]
        public float dashCooldown = 0.8f;

        // ── Savunma ─────────────────────────────────────────────

        [Header("Defense")]
        [Tooltip("Sabit hasar azaltma değeri. TakeDamage içinde çıkarılır.")]
        [Min(0)]
        public int armor = 5;

        [Tooltip("Hasar alındığında Hit state'inde kalınacak süre (saniye). " +
                 "Bu süre dolunca karakter otomatik olarak Idle/Jump state'ine döner.")]
        [Min(0f)]
        public float hitStunDuration = 0.4f;

        // ── Skill Data (YENİ) ───────────────────────────────────

        [Header("Skill 1 (Q Skill)")]
        [Tooltip("Skill 1 bekleme süresi (saniye).")]
        [Min(0f)]
        public float skill1Cooldown = 3f;

        [Tooltip("Skill 1 hasar değeri.")]
        [Min(0)]
        public int skill1Damage = 30;

        [Header("Skill 2 (Ultimate)")]
        [Tooltip("Skill 2 (Ultimate) bekleme süresi (saniye).")]
        [Min(0f)]
        public float skill2Cooldown = 10f;

        [Tooltip("Skill 2 hasar değeri.")]
        [Min(0)]
        public int skill2Damage = 50;

        // ── Validasyon ──────────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (maxHealth <= 0)
                Debug.LogWarning($"[CharacterData] '{characterName}': maxHealth sıfır veya negatif olamaz.");

            if (attackPower < armor)
                Debug.LogWarning($"[CharacterData] '{characterName}': attackPower, armor değerinden küçük. " +
                                 "Bu karakter hiç hasar veremeyebilir.");
        }
#endif
    }
}
