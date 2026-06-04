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
        [Header("Guts Skills")]
        [SerializeField] private float skill1Cooldown = 4f;
        [SerializeField] private float skill2Cooldown = 12f;
        [SerializeField] private float skill2Duration = 5f;

        private float _skill1Timer;
        private float _skill2Timer;
        private bool  _berserkerActive;
        private float _berserkerTimer;

        protected override void Update()
        {
            base.Update();

            if (_skill1Timer > 0f) _skill1Timer -= Time.deltaTime;
            if (_skill2Timer > 0f) _skill2Timer -= Time.deltaTime;

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
        /// Kılıç Fırtınası — Geniş çaplı yay şeklinde kılıç sallar.
        /// </summary>
        public override void Skill1()
        {
            if (!IsAlive) return;
            if (_skill1Timer > 0f) return;

            _skill1Timer = skill1Cooldown;

            // TODO: Geniş hitbox + özel animasyon
            Debug.Log("[Guts] Kılıç Fırtınası! (Skill1)");
        }

        /// <summary>
        /// Berserker Zırhı (Ultimate) — Belirli süre hasar azaltma + saldırı hızı artışı.
        /// </summary>
        public override void Skill2()
        {
            if (!IsAlive) return;
            if (_skill2Timer > 0f) return;
            if (_berserkerActive) return;

            _skill2Timer     = skill2Cooldown;
            _berserkerActive = true;
            _berserkerTimer  = skill2Duration;

            // TODO: Hasar azaltma buff'ı + saldırı hızı artışı
            // TODO: Özel animasyon + VFX (kırmızı aura)
            Debug.Log("[Guts] Berserker Zırhı Aktif! (Skill2)");
        }

        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Guts] {previous} → {next}");
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            _berserkerActive = false;
        }
    }
}
