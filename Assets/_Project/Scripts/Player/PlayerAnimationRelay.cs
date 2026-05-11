// ============================================================
//  PlayerAnimationRelay.cs
//  Namespace : FightingGame.Core.Player
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
//
//  Amaç:
//    Unity'nin Animation Event sistemi, çağıracağı fonksiyonu
//    Animator'ın bağlı olduğu GameObject üzerinde arar.
//    BasePlayer ise CharacterController ile birlikte kök
//    GameObject'tedir; Animator çocuk objededir.
//
//    Bu sınıf, Animator'ın olduğu child'a eklenir ve
//    Animation Event'leri parent'taki BasePlayer'a iletir
//    (Adapter pattern).
//
//  Kullanım:
//    1) Animator'ın bulunduğu child GameObject'e bu component'i ekle.
//    2) Animation Clip üzerinde event oluştur.
//    3) Function alanına bu sınıftaki metot adını yaz
//       (örn. "OnAttackHitFrame").
//
//  Polymorphism notu:
//    GetComponentInParent<BasePlayer>() çağrısı, parent'ta
//    BasePlayer'dan türemiş herhangi bir alt sınıfı (PlayerOne,
//    PlayerTwo, ...) bulur. Relay alt sınıfların ismini bilmez;
//    sadece soyut taban contract'ına güvenir.
// ============================================================

using UnityEngine;

namespace FightingGame.Core.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAnimationRelay : MonoBehaviour
    {
        private BasePlayer _player;

        private void Awake()
        {
            _player = GetComponentInParent<BasePlayer>();

            if (_player == null)
            {
                Debug.LogError(
                    $"[PlayerAnimationRelay] '{name}': parent hiyerarşisinde " +
                    "BasePlayer'dan türemiş bir component bulunamadı. " +
                    "Bu component, Animator'ın olduğu child GameObject'e eklenmeli.",
                    this);
            }
        }

        // ── Animation Event giriş noktaları ─────────────────────────────
        //
        // Animation Event'in "Function" alanına aşağıdaki metot
        // adlarından birini yaz. Yeni bir event türü eklemek için
        // önce BasePlayer'a public metot ekle, sonra burada bir
        // wrapper aç. Tek dosya değişikliğiyle tüm karakterler için
        // çalışır (PlayerOne, PlayerTwo, ...).

        /// <summary>
        /// Saldırı animasyonunun vuruş karesinde çağrılır.
        /// Hitbox kontrolünü tetikler.
        /// </summary>
        public void OnAttackHitFrame() => _player?.OnAttackHitFrame();

        /// <summary>
        /// Saldırı animasyonunun SON karesinde çağrılır.
        /// Karakteri Attack durumundan Idle'a geri döndürür.
        /// </summary>
        public void OnAttackEndFrame() => _player?.OnAttackEndFrame();

        // İleride eklenebilecek event örnekleri:
        // public void OnFootstep()           => _player?.OnFootstep();
        // public void OnVoiceLine(string id) => _player?.OnVoiceLine(id);
    }
}
