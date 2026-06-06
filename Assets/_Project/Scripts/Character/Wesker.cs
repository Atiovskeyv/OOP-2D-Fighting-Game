// ============================================================
//  Wesker.cs
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
    /// Wesker — Virüs güçlerine sahip hızlı ve ölümcül ajan.
    /// <para>
    /// Skill1: Cobra Strike — Hızlı bir ileri atılma ve avuç içi darbesi.
    /// Skill2: Phantom Move (Ultimate) — Çok yüksek hızda hareket ederek rakibin arkasından saldırı.
    /// </para>
    /// </summary>
    public class Wesker : AbstractCharacter
    {
        [Header("Wesker Skills")]
        [SerializeField] private float skill1Cooldown = 4f;
        [SerializeField] private float skill2Cooldown = 12f;

        private float _skill1Timer;
        private float _skill2Timer;

        protected override void Update()
        {
            base.Update();

            // Skill cooldown'larını düşür
            if (_skill1Timer > 0f) _skill1Timer -= Time.deltaTime;
            if (_skill2Timer > 0f) _skill2Timer -= Time.deltaTime;
        }

        /// <summary>
        /// Cobra Strike — Hızlı bir ileri atılma ve avuç içi darbesi.
        /// </summary>
        public override void Skill1()
        {
            if (!IsAlive) return;
            if (_skill1Timer > 0f) return;

            _skill1Timer = skill1Cooldown;

            // TODO: İleri atılma hareketi (Dash benzeri) + hasar
            // TODO: Özel animasyon tetikleyicisi
            Debug.Log("[Wesker] Cobra Strike! (Skill1)");
        }

        /// <summary>
        /// Phantom Move (Ultimate) — Çok yüksek hızda hareket ederek rakibin arkasından saldırı.
        /// </summary>
        public override void Skill2()
        {
            if (!IsAlive) return;
            if (_skill2Timer > 0f) return;

            _skill2Timer = skill2Cooldown;

            // TODO: Işınlanma/Göz kırpma efekti + yüksek hasar
            // TODO: Özel animasyon + VFX
            Debug.Log("[Wesker] Phantom Move Ultimate! (Skill2)");
        }

        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Wesker] {previous} → {next}");
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Wesker'a özel ölüm efekti
        }
    }
}
