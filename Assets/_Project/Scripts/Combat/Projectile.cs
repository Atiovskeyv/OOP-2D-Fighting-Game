// ============================================================
//  Projectile.cs
//  Namespace : FightingGame.Combat
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Temel mermi sınıfı. Spawn olunca ileri doğru uçar,
//  ilk IDamageable objeye çarptığında hasar verip yok olur.
//
//  Kullanım:
//    1) Boş bir GameObject oluştur.
//    2) Rigidbody (Use Gravity = false, Is Kinematic = false) ekle.
//    3) Collider (Is Trigger = true) ekle.
//    4) Bu script'i ekle.
//    5) Prefab olarak kaydet.
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Interfaces;

namespace FightingGame.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        // ── Inspector Alanları ───────────────────────────────────
        [Header("Projectile Settings")]
        [Tooltip("Mermi uçuş hızı (birim/saniye).")]
        [SerializeField] protected float speed = 20f;

        [Tooltip("Merminin yaşam süresi (saniye). Bu süre sonunda yok olur.")]
        [SerializeField] protected float lifetime = 3f;

        // ── Runtime Değişkenleri ─────────────────────────────────
        protected int _damage;
        protected GameObject _owner;       // Mermiyi atan karakter (kendine vurmayı önler)
        protected Vector3 _direction;      // Uçuş yönü
        protected Rigidbody _rb;

        // ══════════════════════════════════════════════════════════
        //  Başlatma — SpawnProjectile tarafından çağrılır
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Mermiyi başlatır. AbstractCharacter.SpawnProjectile() tarafından çağrılır.
        /// </summary>
        /// <param name="owner">Mermiyi atan GameObject (self-damage önleme).</param>
        /// <param name="direction">Uçuş yönü (normalize edilmiş).</param>
        /// <param name="damage">Verilecek hasar miktarı.</param>
        public virtual void Initialize(GameObject owner, Vector3 direction, int damage)
        {
            _owner = owner;
            _direction = direction.normalized;
            _damage = damage;

            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.linearVelocity = _direction * speed;

            // Mermiyi uçuş yönüne döndür
            if (_direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
            }

            // Ömür süresi dolunca yok et
            Destroy(gameObject, lifetime);
        }

        // ══════════════════════════════════════════════════════════
        //  Çarpışma — Hasar Verme
        // ══════════════════════════════════════════════════════════

        protected virtual void OnTriggerEnter(Collider other)
        {
            // Sahibine çarpma
            if (other.gameObject == _owner) return;
            // Sahibinin child objelerine çarpma
            if (_owner != null && other.transform.IsChildOf(_owner.transform)) return;

            // IDamageable olan her şeye hasar ver
            if (other.TryGetComponent<IDamageable>(out var target) && target.IsAlive)
            {
                target.TakeDamage(_damage);
                Debug.Log($"<color=#FFA500>🔫 Mermi isabet! → {other.name} ({_damage} Damage)</color>");
                Destroy(gameObject);
                return;
            }

            // Zemin veya duvara çarpınca da yok ol (IDamageable olmayan katı cisimler)
            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
