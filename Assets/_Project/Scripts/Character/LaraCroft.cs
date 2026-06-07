// ============================================================
//  LaraCroft.cs
//  Namespace : FightingGame.Character
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  AbstractCharacter'dan türeyen somut karakter sınıfı.
//  Bu script karakter prefab'ının üzerine eklenir.
//
// ============================================================

using UnityEngine;

namespace FightingGame.Character
{
    /// <summary>
    /// Lara Croft — Çevik ve çok yönlü maceracı savaşçı.
    /// <para>
    /// Skill1: Çift Tabanca Saldırısı (Enhanced) — Hızlı ateş.
    /// Skill2: Combo Breaker — Darbe alırken rakibi püskürtür.
    /// Skill3: Dinamit Atışı (Ultimate) — Güçlü patlayıcı saldırı.
    /// </para>
    /// </summary>
    public class LaraCroft : AbstractCharacter
    {

        /// <summary>
        /// Çift Tabanca Saldırısı (Skill 1 - Enhanced) — Hızlı ateş.
        /// </summary>
        protected override void OnExecuteSkill1()
        {
            // TODO: Çift tabanca ateşi + özel animasyon
            Debug.Log("[LaraCroft] Çift Tabanca Saldırısı! (Enhanced Skill 1)");
        }

        /// <summary>
        /// Combo Breaker (Skill 2 - Breaker) — Darbe alırken rakibi püskürtür.
        /// </summary>
        protected override void OnExecuteSkill2()
        {
            Debug.Log("[LaraCroft] Kaçış Manevras?! (Breaker Skill 2)");
        }

        /// <summary>
        /// Dinamit Atışı (Skill 3 - Ultimate) — Güçlü patlayıcı saldırı.
        /// </summary>
        protected override void OnExecuteSkill3()
        {
            // TODO: Dinamit fırlatma + AoE hasar + VFX
            Debug.Log("[LaraCroft] Dinamit Atışı! (Ultimate Skill 3)");
        }

#if UNITY_EDITOR
        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[LaraCroft] {previous} → {next}");
        }
#endif

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: LaraCroft'a özel ölüm efekti
        }
    }
}
