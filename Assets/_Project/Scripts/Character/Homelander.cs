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

        /// <summary>
        /// Lazer Bakış (Skill 1 - Enhanced) — Uzun menzilli ışın saldırısı.
        /// </summary>
        protected override void OnExecuteSkill1()
        {
            // TODO: Raycast-based lazer saldırısı
            // TODO: Özel animasyon + lazer VFX
            Debug.Log("[Homelander] Lazer Bakış! (Enhanced Skill 1)");
        }

        /// <summary>
        /// Combo Breaker (Skill 2 - Breaker) — Darbe alırken rakibi püskürtür.
        /// </summary>
        protected override void OnExecuteSkill2()
        {
            Debug.Log("[Homelander] Süt Sağanağı! (Breaker Skill 2)");
        }

        /// <summary>
        /// Süper İniş (Skill 3 - Ultimate) — Havadan süzülerek yere çakılma + AoE.
        /// </summary>
        protected override void OnExecuteSkill3()
        {
            // TODO: Fly up + slam down + shockwave AoE
            // TODO: Özel animasyon + VFX + kamera sarsıntısı
            Debug.Log("[Homelander] Süper İniş Ultimate! (Ultimate Skill 3)");
        }

#if UNITY_EDITOR
        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Homelander] {previous} → {next}");
        }
#endif

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Homelander'a özel ölüm efekti
        }
    }
}
