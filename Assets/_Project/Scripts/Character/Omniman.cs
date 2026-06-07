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


        /// <summary>
        /// Hız Darbesi (Skill 1 - Enhanced) — Süper hızla düşmana uçar, yakın mesafe hasar verir.
        /// </summary>
        protected override void OnExecuteSkill1()
        {
            // TODO: Dash-like movement toward opponent + damage
            // TODO: Özel animasyon trigger'ı
            Debug.Log($"[Omniman] Hız Darbesi! (Enhanced Skill 1)");
        }

        /// <summary>
        /// Combo Breaker (Skill 2 - Breaker) — Darbe alırken rakibi püskürtür.
        /// </summary>
        protected override void OnExecuteSkill2()
        {
            Debug.Log("[Omniman] Metro Savunması! (Breaker Skill 2)");
        }

        /// <summary>
        /// Yere Çakma (Skill 3 - Ultimate) — Havadan yere vurarak alan hasarı verir.
        /// </summary>
        protected override void OnExecuteSkill3()
        {
            // TODO: Jump up + slam down + AoE damage
            // TODO: Özel animasyon trigger'ı + VFX
            Debug.Log($"[Omniman] Yere Çakma Ultimate! (Ultimate Skill 3)");
        }

#if UNITY_EDITOR
        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Omniman] {previous} → {next}");
        }
#endif

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Omniman'a özel ölüm efekti
        }
    }
}
