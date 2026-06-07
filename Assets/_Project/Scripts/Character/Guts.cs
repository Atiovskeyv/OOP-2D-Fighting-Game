// ============================================================
//  Guts.cs
//  Namespace : FightingGame.Character
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  AbstractCharacter'dan türeyen somut karakter sınıfı.
//  Sadece Skill1 ve Skill2 override edilir.
//  Bu script karakter prefab'ının üzerine eklenir.
//
// ============================================================

using UnityEngine;

namespace FightingGame.Character
{
    /// <summary>
    /// Guts — Devasa kılıç kullanan berserker savaşçı.
    /// <para>
    /// Skill1: Kılıç Fırtınası — Geniş çaplı yay şeklinde kılıç sallar.
    /// Skill2: Berserker Zırhı (Ultimate) — Belirli süre hasar azaltma + saldırı hızı artışı.
    /// </para>
    /// </summary>
    public class Guts : AbstractCharacter
    {
        [Header("Guts — Berserker Zırhı")]
        [SerializeField] private float berserkerDuration = 5f;

        private bool  _berserkerActive;
        private float _berserkerTimer;

        protected override void Update()
        {
            base.Update();

            // Berserker Zırhı süre takibi
            if (_berserkerActive)
            {
                _berserkerTimer -= Time.deltaTime;
                if (_berserkerTimer <= 0f)
                {
                    _berserkerActive = false;
                    Debug.Log("[Guts] Berserker Zırhı sona erdi.");
                    // TODO: Buff'ları kaldır
                }
            }
        }

        /// <summary>
        /// Kılıç Fırtınası (Skill 1 - Enhanced) — Geniş çaplı yay şeklinde kılıç sallar, yüksek hasar verir.
        /// </summary>
        protected override void OnExecuteSkill1()
        {
            // TODO: Geniş hitbox + özel animasyon + hasar verme
            Debug.Log("[Guts] Kılıç Fırtınası! (Enhanced Skill 1)");
        }

        /// <summary>
        /// Combo Breaker (Skill 2 - Breaker) — Darbe alırken rakibi püskürtür.
        /// </summary>
        protected override void OnExecuteSkill2()
        {
            Debug.Log("[Guts] Berserker Savuşu! (Breaker Skill 2)");
        }

        /// <summary>
        /// Berserker Zırhı (Skill 3 - Ultimate) — Belirli süre hasar azaltma + saldırı hızı artışı.
        /// </summary>
        protected override void OnExecuteSkill3()
        {
            _berserkerActive = true;
            _berserkerTimer  = berserkerDuration;

            // TODO: Hasar azaltma buff'ı + saldırı hızı artışı
            // TODO: Özel animasyon + VFX (kırmızı aura)
            Debug.Log("[Guts] Berserker Zırhı Aktif! (Ultimate Skill 3)");
        }

#if UNITY_EDITOR
        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Guts] {previous} → {next}");
        }
#endif

        protected override void OnDeath()
        {
            base.OnDeath();
            _berserkerActive = false;
        }
    }
}
