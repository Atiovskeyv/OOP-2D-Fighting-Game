// ============================================================
//  CharacterData.cs
//  Namespace : FightingGame.Core.Data
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  DESIGN PATTERN: Data-Driven Design + ScriptableObject Pattern
//
//  ScriptableObject nedir?
//  -----------------------
//  MonoBehaviour'dan farklı olarak bir GameObject'e bağlı olmayan,
//  doğrudan Project penceresinde .asset dosyası olarak yaşayan
//  Unity veri konteyneridir.
//
//  Neden ScriptableObject kullanıyoruz?
//  -------------------------------------
//  • Her karakter tipi için ayrı prefab kopyası oluşturmak yerine
//    tek bir prefab + farklı CharacterData asset'leri yeterli olur.
//  • Veri ve davranış birbirinden ayrılır (Separation of Concerns).
//  • Inspector'dan tasarımcılar kod yazmadan yeni karakter oluşturabilir.
//  • Aynı asset birden fazla nesne tarafından referans alınabilir;
//    bellek'te yalnızca bir kopya tutulur (flyweight benzeri davranış).
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

        [Header("Animation Settings")]
        public float locomotionDampTime = 0.1f;
        public float startDampTime = 0.05f; // Kalkış hızı (Daha küçük = Daha hızlı kalkış)
        public float stopDampTime = 0.15f;  // Duruş hızı (Daha küçük = Daha hızlı duruş)

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

        [Tooltip("Zıplama kuvveti. Rigidbody2D.AddForce ile kullanılır.")]
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

        // ── Savunma ─────────────────────────────────────────────

        [Header("Defense")]
        [Tooltip("Sabit hasar azaltma değeri. TakeDamage içinde çıkarılır.")]
        [Min(0)]
        public int armor = 5;

        // ── Validasyon ──────────────────────────────────────────
 
#if UNITY_EDITOR
        /// <summary>
        /// Inspector'da bir değer değiştirildiğinde Unity bu metodu çağırır.
        /// Tutarsız veri girişlerine karşı erken uyarı verir.
        /// </summary>
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