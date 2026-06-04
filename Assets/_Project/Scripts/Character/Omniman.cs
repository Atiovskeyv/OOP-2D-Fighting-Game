// ============================================================
//  Omniman.cs
//  Namespace : FightingGame.Character
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  AbstractCharacter'dan türeyen somut karakter sınıfı.
//  Walk, Jump, Attack, Block, Dash, Crouch → default davranış
//  (AbstractCharacter'dan miras). Sadece Skill1 ve Skill2
//  override edilir.
//
//  Bu script karakter prefab'ının üzerine eklenir.
//
// ============================================================

using UnityEngine;

namespace FightingGame.Character
{
    /// <summary>
    /// Omniman — Süper güçlü uçucu savaşçı.
    /// <para>
    /// Skill1: Hız Darbesi — Süper hızla düşmana uçar, yakın mesafe hasar verir.
    /// Skill2: Yere Çakma (Ultimate) — Havadan yere vurarak alan hasarı verir.
    /// </para>
    /// </summary>
    public class Omniman : AbstractCharacter
    {
        // ── Skill Cooldown'ları ──────────────────────────────────
        [Header("Omniman Skills")]
        [SerializeField] private float skill1Cooldown = 3f;
        [SerializeField] private float skill2Cooldown = 10f;
        [SerializeField] private float skill1Damage   = 35f;
        [SerializeField] private float skill2Damage   = 60f;

        private float _skill1Timer;
        private float _skill2Timer;

        protected override void Update()
        {
            base.Update();

            // Skill cooldown'larını tick et
            if (_skill1Timer > 0f) _skill1Timer -= Time.deltaTime;
            if (_skill2Timer > 0f) _skill2Timer -= Time.deltaTime;
        }

        /// <summary>
        /// Hız Darbesi — Süper hızla düşmana uçar, yakın mesafe hasar verir.
        /// </summary>
        public override void Skill1()
        {
            if (!IsAlive) return;
            if (_skill1Timer > 0f) return;

            _skill1Timer = skill1Cooldown;

            // TODO: Dash-like movement toward opponent + damage
            // TODO: Özel animasyon trigger'ı
            Debug.Log($"[Omniman] Hız Darbesi! (Skill1)");
        }

        /// <summary>
        /// Yere Çakma (Ultimate) — Havadan yere vurarak alan hasarı verir.
        /// </summary>
        public override void Skill2()
        {
            if (!IsAlive) return;
            if (_skill2Timer > 0f) return;

            _skill2Timer = skill2Cooldown;

            // TODO: Jump up + slam down + AoE damage
            // TODO: Özel animasyon trigger'ı + VFX
            Debug.Log($"[Omniman] Yere Çakma Ultimate! (Skill2)");
        }

        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Omniman] {previous} → {next}");
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Omniman'a özel ölüm efekti
        }
    }
}
