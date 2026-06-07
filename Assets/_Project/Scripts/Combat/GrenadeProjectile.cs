// ============================================================
//  GrenadeProjectile.cs
//  Namespace : FightingGame.Combat
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Projectile'dan türeyen el bombası. Farkları:
//    • Yerçekimi etkisinde yay çizerek uçar
//    • Yere düşünce veya rakibe çarpınca patlayarak AoE hasar verir
//    • Physics.OverlapSphere ile patlama alanındaki herkesi vurur
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Interfaces;

namespace FightingGame.Combat
{
    public class GrenadeProjectile : Projectile
    {
        // ── Inspector Alanları ───────────────────────────────────
        [Header("Grenade Settings")]
        [Tooltip("Patlama alanı yarıçapı (birim).")]
        [SerializeField] private float explosionRadius = 3f;

        [Tooltip("Fırlatma açısı (derece). Yüksek değer = daha yüksek yay.")]
        [SerializeField] private float launchAngle = 45f;

        // ══════════════════════════════════════════════════════════
        //  Başlatma — Yerçekimli fırlatma
        // ══════════════════════════════════════════════════════════

        public override void Initialize(GameObject owner, Vector3 direction, int damage)
        {
            _owner = owner;
            _direction = direction.normalized;
            _damage = damage;

            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = true;              // Bomba yerçekimine tabi
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Yay çizen fırlatma vektörü hesapla
            float angleRad = launchAngle * Mathf.Deg2Rad;
            Vector3 launchVelocity = new Vector3(
                _direction.x * speed * Mathf.Cos(angleRad),
                speed * Mathf.Sin(angleRad),
                0f
            );

            _rb.linearVelocity = launchVelocity;

            Destroy(gameObject, lifetime);
        }

        // ══════════════════════════════════════════════════════════
        //  Çarpışma — AoE Patlama
        // ══════════════════════════════════════════════════════════

        protected override void OnTriggerEnter(Collider other)
        {
            // Sahibine çarpma
            if (other.gameObject == _owner) return;
            if (_owner != null && other.transform.IsChildOf(_owner.transform)) return;

            // Herhangi bir şeye çarptığında patlat
            Explode();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Trigger olmayan (yani zemine) çarptığında da patlat
            if (collision.gameObject == _owner) return;
            Explode();
        }

        private void Explode()
        {
            Debug.Log($"<color=#FF4500>💥 Bomba patladı! (Yarıçap: {explosionRadius})</color>");

            // Patlama alanındaki tüm collider'ları bul
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider hit in hits)
            {
                // Sahibine hasar verme
                if (hit.gameObject == _owner) continue;
                if (_owner != null && hit.transform.IsChildOf(_owner.transform)) continue;

                if (hit.TryGetComponent<IDamageable>(out var target) && target.IsAlive)
                {
                    // Mesafeye göre hasar azaltması (merkezde tam hasar, kenarda yarım)
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float falloff = Mathf.Clamp01(1f - (distance / explosionRadius));
                    int finalDamage = Mathf.RoundToInt(_damage * falloff);

                    if (finalDamage > 0)
                    {
                        target.TakeDamage(finalDamage);
                        Debug.Log($"<color=#FF6347>💥 Bomba hasarı → {hit.name} ({finalDamage} Damage, mesafe: {distance:F1})</color>");
                    }
                }
            }

            // TODO: Patlama VFX (Instantiate particle system)
            // TODO: Patlama SFX

            Destroy(gameObject);
        }
    }
}
