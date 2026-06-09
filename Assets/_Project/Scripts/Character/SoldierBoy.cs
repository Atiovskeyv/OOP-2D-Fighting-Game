// ============================================================
//  SoldierBoy.cs
//  Namespace : FightingGame.Character
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  AbstractCharacter'dan türeyen somut karakter sınıfı.
//
//  Soldier Boy — Kalkan kullanan ve göğsünden radyasyon
//  patlaması salan eski süper kahraman (The Boys dizisi).
//
//  Özel Mekanikler:
//    • Tabancayla uzaktan ateş etme (Projectile sistemi)
//    • Skill 1 (1 Bar): Seri Ateş — Coroutine ile 6 mermi
//    • Skill 2 (2 Bar): El Bombası — GrenadeProjectile (AoE)
//    • Skill 3 (3 Bar): Göğüs Patlaması — Bloklanamaz enerji dalgası
//
// ============================================================

using System.Collections;
using UnityEngine;

namespace FightingGame.Character
{
    /// <summary>
    /// Soldier Boy — Kalkan + tabanca + göğüs patlaması savaşçısı.
    /// </summary>
    public class SoldierBoy : AbstractCharacter
    {
        // ── Mermi Prefab'ları ────────────────────────────────────
        [Header("Soldier Boy — Projectiles")]
        [Tooltip("Tabanca mermisi prefab'ı (Projectile.cs).")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("El bombası prefab'ı (GrenadeProjectile.cs).")]
        [SerializeField] private GameObject grenadePrefab;

        [Tooltip("Göğüs patlaması enerji mermisi prefab'ı (Projectile.cs, büyük hitbox).")]
        [SerializeField] private GameObject chestBlastPrefab;

        // ── Skill Ayarları ──────────────────────────────────────
        [Header("Soldier Boy — Skill Settings")]
        [Tooltip("Seri Ateş: Mermi sayısı.")]
        [SerializeField] private int rapidFireCount = 6;

        [Tooltip("Seri Ateş: Mermiler arası gecikme (saniye).")]
        [SerializeField] private float rapidFireInterval = 0.1f;

        [Tooltip("Seri Ateş: Mermi başına hasar çarpanı.")]
        [SerializeField] private float rapidFireDamageMultiplier = 1.5f;

        [Tooltip("El Bombası: Hasar çarpanı.")]
        [SerializeField] private float grenadeDamageMultiplier = 4f;

        [Tooltip("Göğüs Patlaması: Hasar çarpanı.")]
        [SerializeField] private float chestBlastDamageMultiplier = 8f;

        [Header("Soldier Boy — Combo Forces")]
        [Tooltip("PPP Kombosunda rakibin ne kadar havaya/geriye uçacağı (X: İleri/Geri, Y: Yukarı)")]
        [SerializeField] private Vector3 pppLaunchForce = new Vector3(3f, 8f, 0f);

        [Tooltip("PPK Kombosunda rakibin ne kadar havaya/geriye uçacağı")]
        [SerializeField] private Vector3 ppkLaunchForce = new Vector3(3f, 9f, 0f);

        // Seri ateş sırasında hareket engeli
        private bool _isRapidFiring;

        // ══════════════════════════════════════════════════════════
        //  Kombo Tanımları — Soldier Boy'a Özel
        // ══════════════════════════════════════════════════════════

        protected override void InitializeCombos()
        {
            // Uzun kombolar önce tanımlanır (en uzun eşleşme öncelikli)

            // P + P + P (Launcher — Havaya fırlatma)
            _combos.Add(new FightingCombo
            {
                name = "PPP_Launcher",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch, AttackInputType.Punch },
                damageMultiplier = 2.5f,
                isLauncher = true,
                animTrigger = "PPP_Combo",
                launchForce = pppLaunchForce
            });

            // P + P + K (Launcher — İki yumruk + tekme, havaya fırlatma)
            _combos.Add(new FightingCombo
            {
                name = "PPK_Launcher",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch, AttackInputType.Kick },
                damageMultiplier = 3.0f,
                isLauncher = true,
                animTrigger = "PPK_Combo",
                launchForce = ppkLaunchForce
            });

            // P + P + S (İki yumruk + güçlü ateş — Uzaktan)
            _combos.Add(new FightingCombo
            {
                name = "PPS_GunCombo",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch, AttackInputType.Shoot },
                damageMultiplier = 2.0f,
                isLauncher = false,
                animTrigger = "PPS_Combo",
                isRanged = true,
                projectilePrefab = bulletPrefab
            });

            // P + P (İkili Yumruk)
            _combos.Add(new FightingCombo
            {
                name = "PP_DoublePunch",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch },
                damageMultiplier = 1.5f,
                isLauncher = false,
                animTrigger = "PP_Combo",
                pushForce = 2f
            });

            // P (Tekli Yumruk)
            _combos.Add(new FightingCombo
            {
                name = "Punch",
                sequence = new AttackInputType[] { AttackInputType.Punch },
                damageMultiplier = 1.0f,
                isLauncher = false,
                animTrigger = "Punch"
            });

            // K (Tekli Tekme)
            _combos.Add(new FightingCombo
            {
                name = "Kick",
                sequence = new AttackInputType[] { AttackInputType.Kick },
                damageMultiplier = 1.2f,
                isLauncher = false,
                animTrigger = "Kick",
                pushForce = 1.5f
            });

            // S (Tabanca — Uzaktan, düşük hasar, bloklanabilir)
            _combos.Add(new FightingCombo
            {
                name = "Shoot",
                sequence = new AttackInputType[] { AttackInputType.Shoot },
                damageMultiplier = 0.3f,
                isLauncher = false,
                animTrigger = "Shoot",
                isRanged = true,
                projectilePrefab = bulletPrefab
            });
        }

        // ══════════════════════════════════════════════════════════
        //  Skill 1 — Seri Ateş (1 Bar = 100 Meter)
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Seri Ateş — Coroutine ile art arda mermi fırlatır.
        /// Seri ateş sırasında karakter yerinde kalır (hareket edemez).
        /// </summary>
        protected override void OnExecuteSkill1()
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[Soldier Boy] Bullet prefab atanmamış! Seri ateş iptal.");
                return;
            }

            Debug.Log($"<color=#FFD700>🔫 [Soldier Boy] Seri Ateş Başladı! ({rapidFireCount} mermi)</color>");
            StartCoroutine(RapidFireRoutine());
        }

        private IEnumerator RapidFireRoutine()
        {
            _isRapidFiring = true;
            TransitionTo(CharacterState.Attack);

            int damage = Mathf.RoundToInt(data.attackPower * rapidFireDamageMultiplier);

            for (int i = 0; i < rapidFireCount; i++)
            {
                SpawnProjectile(bulletPrefab, damage);
                yield return new WaitForSeconds(rapidFireInterval);
            }

            _isRapidFiring = false;

            // Seri ateş bittikten sonra idle'a dön
            if (HasState(CharacterState.Attack))
                TransitionTo(_isGrounded ? CharacterState.Idle : CharacterState.Jump);
        }

        // ══════════════════════════════════════════════════════════
        //  Skill 2 — El Bombası (2 Bar = 200 Meter)
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// El Bombası — Soldier Boy için Skill 2 saldırı skill'i olarak çalışır.
        /// Base class'taki Combo Breaker (Hit state gerekliliğini) bypass eder.
        /// </summary>
        public override void ExecuteSkill2()
        {
            if (!IsAlive) return;
            if (SpecialMeter < 200f)
            {
                Debug.Log($"[{Data.characterName}] El Bombası için yeterli bar yok! (Gerekli: 2 Bar, Mevcut: {SpecialMeter:F0})");
                return;
            }
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;

            AddSpecialMeter(-200f);
            OnExecuteSkill2();
        }

        /// <summary>
        /// El Bombası — GrenadeProjectile fırlatır (yay çizer, AoE patlama).
        /// </summary>
        protected override void OnExecuteSkill2()
        {
            if (grenadePrefab == null)
            {
                Debug.LogWarning("[Soldier Boy] Grenade prefab atanmamış! El bombası iptal.");
                return;
            }

            int damage = Mathf.RoundToInt(data.attackPower * grenadeDamageMultiplier);
            Debug.Log($"<color=#FF4500>💣 [Soldier Boy] El Bombası! ({damage} Damage)</color>");
            SpawnProjectile(grenadePrefab, damage);
        }

        // ══════════════════════════════════════════════════════════
        //  Skill 3 — Göğüs Patlaması / Ultimate (3 Bar = 300 Meter)
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Göğüs Patlaması (Radyoaktif Enerji) — Büyük enerji mermisi.
        /// Bloklanamaz, çok yüksek hasar. Dizideki ikonik gücü.
        /// </summary>
        protected override void OnExecuteSkill3()
        {
            if (chestBlastPrefab == null)
            {
                Debug.LogWarning("[Soldier Boy] Chest Blast prefab atanmamış! Göğüs patlaması iptal.");
                return;
            }

            int damage = Mathf.RoundToInt(data.attackPower * chestBlastDamageMultiplier);
            Debug.Log($"<color=#FF0000>☢️ [Soldier Boy] GÖĞÜS PATLAMASI! ({damage} Damage — BLOKLANAMAZ)</color>");
            SpawnProjectile(chestBlastPrefab, damage);
        }

        // ══════════════════════════════════════════════════════════
        //  Update Override — Seri ateş sırasında hareketi engelle
        // ══════════════════════════════════════════════════════════

        protected override void Update()
        {
            if (_isRapidFiring) return; // Seri ateş sırasında hareket etme
            base.Update();
        }

        // ══════════════════════════════════════════════════════════
        //  Debug / State Logging
        // ══════════════════════════════════════════════════════════

#if UNITY_EDITOR
        protected override void OnStateChanged(CharacterState previous, CharacterState next)
        {
            Debug.Log($"[Soldier Boy] {previous} → {next}");
        }
#endif
    }
}
