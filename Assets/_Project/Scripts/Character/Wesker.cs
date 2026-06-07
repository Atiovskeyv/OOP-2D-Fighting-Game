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
    /// Skill1: Cobra Strike (Enhanced) — Hızlı bir ileri atılma ve avuç içi darbesi.
    /// Skill2: Combo Breaker — Darbe alırken rakibi püskürtür.
    /// Skill3: Phantom Move (Ultimate) — Çok yüksek hızda hareket ederek rakibin arkasından saldırı.
    /// </para>
    /// </summary>
    public class Wesker : AbstractCharacter
    {

        /// <summary>
        /// Cobra Strike (Skill 1 - Enhanced) — Hızlı bir ileri atılma ve avuç içi darbesi.
        /// </summary>
        protected override void OnExecuteSkill1()
        {
            // TODO: İleri atılma hareketi (Dash benzeri) + hasar
            // TODO: Özel animasyon tetikleyicisi
            Debug.Log("[Wesker] Cobra Strike! (Enhanced Skill 1)");
        }

        /// <summary>
        /// Combo Breaker (Skill 2 - Breaker) — Darbe alırken rakibi püskürtür.
        /// </summary>
        protected override void OnExecuteSkill2()
        {
            Debug.Log("[Wesker] Virüs Savuşması! (Breaker Skill 2)");
        }

        /// <summary>
        /// Phantom Move (Skill 3 - Ultimate) — Çok yüksek hızda hareket ederek rakibin arkasından saldırı.
        /// </summary>
        protected override void OnExecuteSkill3()
        {
            // TODO: Işınlanma/Göz kırpma efekti + yüksek hasar
            // TODO: Özel animasyon + VFX
            Debug.Log("[Wesker] Phantom Move Ultimate! (Ultimate Skill 3)");
        }

#if UNITY_EDITOR
        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Wesker] {previous} → {next}");
        }
#endif

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Wesker'a özel ölüm efekti
        }
    }
}
