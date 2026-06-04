// ============================================================
//  Homelander.cs
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
    /// Homelander — Lazer gözlü süper kahraman.
    /// <para>
    /// Skill1: Lazer Bakış — Uzun menzilli ışın saldırısı.
    /// Skill2: Süper İniş (Ultimate) — Havadan süzülerek yere çakılma + AoE.
    /// </para>
    /// </summary>
    public class Homelander : AbstractCharacter
    {
        [Header("Homelander Skills")]
        [SerializeField] private float skill1Cooldown = 3.5f;
        [SerializeField] private float skill2Cooldown = 15f;

        private float _skill1Timer;
        private float _skill2Timer;

        protected override void Update()
        {
            base.Update();

            if (_skill1Timer > 0f) _skill1Timer -= Time.deltaTime;
            if (_skill2Timer > 0f) _skill2Timer -= Time.deltaTime;
        }

        /// <summary>
        /// Lazer Bakış — Uzun menzilli ışın saldırısı.
        /// </summary>
        public override void Skill1()
        {
            if (!IsAlive) return;
            if (_skill1Timer > 0f) return;

            _skill1Timer = skill1Cooldown;

            // TODO: Raycast-based lazer saldırısı
            // TODO: Özel animasyon + lazer VFX
            Debug.Log("[Homelander] Lazer Bakış! (Skill1)");
        }

        /// <summary>
        /// Süper İniş (Ultimate) — Havadan süzülerek yere çakılma + AoE.
        /// </summary>
        public override void Skill2()
        {
            if (!IsAlive) return;
            if (_skill2Timer > 0f) return;

            _skill2Timer = skill2Cooldown;

            // TODO: Fly up + slam down + shockwave AoE
            // TODO: Özel animasyon + VFX + kamera sarsıntısı
            Debug.Log("[Homelander] Süper İniş Ultimate! (Skill2)");
        }

        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Homelander] {previous} → {next}");
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Homelander'a özel ölüm efekti
        }
    }
}
