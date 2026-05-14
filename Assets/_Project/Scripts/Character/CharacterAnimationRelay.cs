// ============================================================
//  CharacterAnimationRelay.cs
//  Namespace : FightingGame.Character
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Mevcut PlayerAnimationRelay.cs'nin güncellenmiş versiyonu.
//
//  Amaç:
//    Unity Animation Event'leri Animator'ın bağlı olduğu
//    child GameObject üzerinde aranır. AbstractCharacter ise
//    kök GameObject'tedir. Bu sınıf, child'dan parent'a
//    event iletir (Adapter pattern).
//
//  Değişiklik:
//    • BasePlayer → AbstractCharacter referansı
//    • Sınıf adı: PlayerAnimationRelay → CharacterAnimationRelay
//
//  Kullanım:
//    1) Animator'ın bulunduğu child GameObject'e ekle.
//    2) Animation Clip'te event oluştur.
//    3) Function: "OnAttackHitFrame" veya "OnAttackEndFrame"
//
// ============================================================

using UnityEngine;

namespace FightingGame.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterAnimationRelay : MonoBehaviour
    {
        private AbstractCharacter _character;

        private void Awake()
        {
            _character = GetComponentInParent<AbstractCharacter>();

            if (_character == null)
            {
                Debug.LogError(
                    $"[CharacterAnimationRelay] '{name}': parent hiyerarşisinde " +
                    "AbstractCharacter'dan türemiş bir component bulunamadı. " +
                    "Bu component, Animator'ın olduğu child GameObject'e eklenmeli.",
                    this);
            }
        }

        // ── Animation Event giriş noktaları ─────────────────────

        /// <summary>
        /// Saldırı animasyonunun vuruş karesinde çağrılır.
        /// </summary>
        public void OnAttackHitFrame() => _character?.OnAttackHitFrame();

        /// <summary>
        /// Saldırı animasyonunun SON karesinde çağrılır.
        /// </summary>
        public void OnAttackEndFrame() => _character?.OnAttackEndFrame();

        // İleride eklenebilecek event örnekleri:
        // public void OnFootstep()           => _character?.OnFootstep();
        // public void OnVoiceLine(string id) => _character?.OnVoiceLine(id);
    }
}
