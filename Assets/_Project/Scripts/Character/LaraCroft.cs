// ============================================================
//  LaraCroft.cs
//  Namespace : FightingGame.Character
//  GEÇİCİ — Omniman'dan kopyalanmış placeholder script.
// ============================================================

using UnityEngine;

namespace FightingGame.Character
{
    /// <summary>
    /// Lara Croft — Geçici karakter sınıfı (test amaçlı).
    /// </summary>
    public class LaraCroft : AbstractCharacter
    {
        [Header("LaraCroft Skills")]
        [SerializeField] private float skill1Cooldown = 3f;
        [SerializeField] private float skill2Cooldown = 10f;

        private float _skill1Timer;
        private float _skill2Timer;

        protected override void Update()
        {
            base.Update();

            if (_skill1Timer > 0f) _skill1Timer -= Time.deltaTime;
            if (_skill2Timer > 0f) _skill2Timer -= Time.deltaTime;
        }

        public override void Skill1()
        {
            if (!IsAlive) return;
            if (_skill1Timer > 0f) return;

            _skill1Timer = skill1Cooldown;
            Debug.Log("[LaraCroft] Skill1 (Placeholder)");
        }

        public override void Skill2()
        {
            if (!IsAlive) return;
            if (_skill2Timer > 0f) return;

            _skill2Timer = skill2Cooldown;
            Debug.Log("[LaraCroft] Skill2 (Placeholder)");
        }

        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[LaraCroft] {previous} → {next}");
        }

        protected override void OnDeath()
        {
            base.OnDeath();
        }
    }
}
