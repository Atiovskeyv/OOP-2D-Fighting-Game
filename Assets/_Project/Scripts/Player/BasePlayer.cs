// ============================================================
//  BasePlayer.cs
//  Namespace : FightingGame.Core.Player
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
//
//  Bu versiyonda eklenenler:
//    [1] Animator entegrasyonu  — OnStateChanged içinde parametre güncelleme
//    [2] Physical Crouch        — CharacterController.height dinamik ayarı
//    [3] Attack Sync            — Animation Event ile tetiklenebilir hitbox
// ============================================================

using System;
using UnityEngine;
using FightingGame.Core.Data;
using FightingGame.Core.Interfaces;

namespace FightingGame.Core.Player
{
    [Flags]
    public enum PlayerState
    {
        None    = 0,
        Idle    = 1 << 0,
        Move    = 1 << 1,
        Crouch  = 1 << 2,
        Jump    = 1 << 3,
        Attack  = 1 << 4,
        Block   = 1 << 5,
        Dash    = 1 << 6,
        Hit     = 1 << 7,
        Dead    = 1 << 8
    }

    // ── Animator Parametre Sabitleri ───────────────────────────────────────────────
    // string yerine hash kullanmak, Animator.SetBool/SetFloat çağrıları
    // ~%30 daha hızlı yapar. Her frame çağrıldığı için fark önemlidir.
    internal static class AnimParam
    {
        public static readonly int Speed        = Animator.StringToHash("Speed");
        public static readonly int IsJumping    = Animator.StringToHash("isJumping");
        public static readonly int IsCrouching  = Animator.StringToHash("isCrouching");
        public static readonly int IsBlocking   = Animator.StringToHash("isBlocking");
        public static readonly int IsHit        = Animator.StringToHash("isHit");
        public static readonly int IsDead       = Animator.StringToHash("isDead");
        public static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");  // Animator'daki gerçek isim
    }

    [RequireComponent(typeof(CharacterController))]
    //[RequireComponent(typeof(Animator))]            // [1] Animator zorunlu bileşen
    public abstract class BasePlayer : MonoBehaviour, IDamageable
    {
        // ── Inspector Alanları ───────────────────────────────────────────────────────

        [Header("Data")]
        [SerializeField] protected CharacterData data;

        [Header("Combat")]
        [Tooltip("Saldırı hitbox merkezi (child GameObject).")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float     groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        // ── [2] Crouch Ayarları ─────────────────────────────────────────────────────

        [Header("Crouch")]
        [Tooltip("Ayakta CharacterController yüksekliği.")]
        [SerializeField] private float standingHeight = 2f;

        [Tooltip("Çömeldikten sonraki yükseklik. Genellikle standingHeight * 0.5f.")]
        [SerializeField] private float crouchHeight   = 1f;

        [Tooltip("Crouch geçişinin yumuşaklığı (Lerp hızı).")]
        [SerializeField] private float crouchLerpSpeed = 12f;

        // ── [3] Attack Sync ─────────────────────────────────────────────────────────

        // true iken Animation Event'ten OnAttackHitFrame() çağrısı beklenir.
        // false iken eski davranış: saldırı başladığında anında hasar verir.
        [Header("Attack Sync")]
        [Tooltip("true → hitbox yalnızca Animation Event ile tetiklenir.\n"+
                 "false → saldırı başlar başlamaz hasar uygulanır (geliştirme modu).")]
        [SerializeField] private bool useAnimationEventSync = true;

        // ── Visual Settings ─────────────────────────────────────────────────────────
        
        [Header("Visual Settings")]
        [SerializeField] protected Transform visualMesh; // New addition for visual synchronization

        // ── IDamageable ─────────────────────────────────────────────────────────────

        private int _currentHealth;
        public int  CurrentHealth => _currentHealth;
        public int  MaxHealth     => data != null ? data.maxHealth : 0;
        public bool IsAlive       => _currentHealth > 0;

        // ── State ───────────────────────────────────────────────────────────────────

        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        // ── Bileşen Referansları ───────────────────────────────────────────────────

        private CharacterController _cc;
        protected Animator          _animator;          // alt sınıflar erişebilir

        // ── Fizik ───────────────────────────────────────────────────────────────────

        private Vector3 _velocity;
        private bool    _isGrounded;
        private float   _dashTimer;
        private float   _attackCooldownTimer;
        private float _targetHeight;
        private const float Gravity = -20f;

        // ── Unity Lifecycle ────────────────────────────────────────────────────────

        protected virtual void Awake()
        {
            _cc       = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>(); // Animator çocuk objede (dvl_mdl_guts)

            if (data == null)
            {
                Debug.LogError($"[BasePlayer] '{name}': CharacterData atanmamış!", this);
                return;
            }

            _currentHealth         = data.maxHealth;
            _cc.height             = standingHeight;        // [2] başlangıç yüksekliği
            _cc.center = new Vector3(0, standingHeight / 2f, 0);
            _targetHeight = standingHeight;
        }

        protected virtual void Update()
        {
            if (!IsAlive) return;

            TickCooldowns();
            CheckGround();
            HandleGravity();
            HandleMovement();
            HandleCrouch();
            HandleJump();
            HandleDash();
            HandleAttack();
            HandleBlock();
            ApplyMovement();
            UpdateAnimatorLocomotion();                     // [1] Speed her frame güncellenir
        }

        protected virtual void LateUpdate()
        {

        }


        // ── IDamageable ─────────────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            if (HasState(PlayerState.Block)) return;

            int finalDamage = Mathf.Max(0, amount - data.armor);
            _currentHealth  = Mathf.Max(0, _currentHealth - finalDamage);

            if (_currentHealth <= 0) Die();
            else                     TransitionTo(PlayerState.Hit);
        }

        // ── Durum Makinesi ─────────────────────────────────────────────────────────────

        protected void TransitionTo(PlayerState newState)
        {
            if (CurrentState == newState) return;

            PlayerState previous = CurrentState;
            CurrentState         = newState;

            SyncAnimator(previous, newState);               // [1] önce animator
            OnStateChanged(previous, newState);             // sonra alt sınıf hook'u
        }

        protected bool HasState(PlayerState flag) => (CurrentState & flag) != 0;

        protected virtual void OnStateChanged(PlayerState previous, PlayerState next) { }

        // ── [1] Animator Senkronizasyonu ───────────────────────────────────────────────
        //
        // Tüm bool parametreleri her geçişte sıfırlanıp yalnızca aktif durum
        // true yapılır. Bu "resetle, sonra set et" yaklaşımı, yanlış kalan
        // bool'lardan kaynaklanan animasyon takılmalarını önler.

        private void SyncAnimator(PlayerState previous, PlayerState next)
        {
            if (_animator == null) return;

            // Tüm bool'ları sıfırla
            _animator.SetBool(AnimParam.IsJumping,   false);
            _animator.SetBool(AnimParam.IsCrouching, false);
            _animator.SetBool(AnimParam.IsBlocking,  false);
            _animator.SetBool(AnimParam.IsHit,       false);
            _animator.SetBool(AnimParam.IsDead,      false);

            // Yeni duruma göre ilgili parametreyi aç
            if      (HasState(PlayerState.Jump))   _animator.SetBool(AnimParam.IsJumping,   true);
            else if (HasState(PlayerState.Crouch)) _animator.SetBool(AnimParam.IsCrouching, true);
            else if (HasState(PlayerState.Block))  _animator.SetBool(AnimParam.IsBlocking,  true);
            else if (HasState(PlayerState.Hit))    _animator.SetBool(AnimParam.IsHit,       true);
            else if (HasState(PlayerState.Dead))   _animator.SetBool(AnimParam.IsDead,      true);

            // Attack: bool yerine Trigger kullan — animasyon bir kez oynar,
            // otomatik reset edilir; bool gibi "takılı kalma" riski yoktur.
            if (HasState(PlayerState.Attack))
                _animator.SetTrigger(AnimParam.AttackTrigger);
        }

        // Speed her frame güncellenmeli; durum geçişine bağlı değil
        private void UpdateAnimatorLocomotion()
        {
            if (_animator == null) return;
            //_animator.SetFloat(AnimParam.Speed, Mathf.Abs(_velocity.x));
            Vector3 horizontalVelocity = new Vector3(_cc.velocity.x, 0, _cc.velocity.z);
            float currentSpeed = horizontalVelocity.magnitude;
            _animator.SetFloat(AnimParam.Speed, currentSpeed);
        }

        // ── Hareket Sistemleri ──────────────────────────────────────────────────────────

        private void TickCooldowns()
        {
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;

            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                    TransitionTo(_isGrounded ? PlayerState.Idle : PlayerState.Jump);
            }
        }

        private void CheckGround()
        {
            Vector3 capsuleBase = transform.position
                                + _cc.center
                                - new Vector3(0f, _cc.height * 0.5f, 0f);

            _isGrounded = Physics.CheckSphere(capsuleBase, groundCheckRadius, groundLayer);

            if (_isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;
        }

        private void HandleGravity()
        {
            if (HasState(PlayerState.Dash)) return;
            _velocity.y += Gravity * Time.deltaTime;
        }

        private void HandleMovement()
        {
            if (HasState(PlayerState.Dead | PlayerState.Hit | PlayerState.Dash)) return;

            float horizontal = GetHorizontalInput();
            _velocity.x      = horizontal * data.moveSpeed;

            if (horizontal != 0f)
                transform.localScale = new Vector3(Mathf.Sign(horizontal), 1f, 1f);

            if (_isGrounded && !HasState(PlayerState.Attack | PlayerState.Block | PlayerState.Crouch))
                TransitionTo(Mathf.Abs(horizontal) > 0.01f ? PlayerState.Move : PlayerState.Idle);
        }

        // ── [2] Physical Crouch ─────────────────────────────────────────────────────────
        //
        // CharacterController.height doğrudan değiştirilirse karakter aniden
        // yükselip alçalır. Lerp ile yumuşak geçiş sağlanır.
        // center.y da yarı oranda ayarlanmazsa collider zeminden yükselir.

        private void HandleCrouch()
        {
            if (!_isGrounded) return;
            if (HasState(PlayerState.Dead | PlayerState.Dash)) return;

            if (GetCrouchInput())
                TransitionTo(PlayerState.Crouch);
            else if (HasState(PlayerState.Crouch))
                TransitionTo(PlayerState.Idle);

            float targetHeight = HasState(PlayerState.Crouch) ? crouchHeight : standingHeight;
            float newHeight    = Mathf.Lerp(_cc.height, targetHeight, crouchLerpSpeed * Time.deltaTime);

            _cc.height   = newHeight;
            _cc.center   = new Vector3(0f, newHeight * 0.5f, 0f);
        }

        private void HandleJump()
        {
            if (!_isGrounded) return;
            if (HasState(PlayerState.Dead | PlayerState.Crouch | PlayerState.Dash)) return;

            if (GetJumpInput())
            {
                _velocity.y = data.jumpForce;
                TransitionTo(PlayerState.Jump);
            }
            else if (HasState(PlayerState.Jump) && _isGrounded)
            {
                TransitionTo(PlayerState.Idle);
            }
        }

        private void HandleDash()
        {
            if (HasState(PlayerState.Dead | PlayerState.Dash)) return;
            if (!GetDashInput()) return;

            float horizontal = GetHorizontalInput();
            Vector3 dir      = new Vector3(
                horizontal != 0f ? Mathf.Sign(horizontal) : transform.localScale.x,
                0f, 0f);

            _dashTimer = 0.2f;
            _velocity  = dir * data.moveSpeed * 2.5f;
            TransitionTo(PlayerState.Dash);
        }

        // ── [3] Attack Sync ─────────────────────────────────────────────────────────
        //
        // useAnimationEventSync == true  → Trigger animator'ı ateşler,
        //   animasyon vuruş karesine geldiğinde Animation Event üzerinden
        //   OnAttackHitFrame() çağrılır; hasar o anda uygulanır.

        // useAnimationEventSync == false → eski davranış; anlık hasar.
        //   Animator henüz kurulmamışken geliştirme aşamasında kullanışlıdır.

        private void HandleAttack()
        {
            if (HasState(PlayerState.Dead | PlayerState.Hit)) return;
            if (_attackCooldownTimer > 0f) return;
            if (!GetAttackInput()) return;

            _attackCooldownTimer = data.attackCooldown;
            TransitionTo(PlayerState.Attack);

            if (_animator != null)
                {
                    _animator.SetTrigger("AttackTrigger");
                }

            if (!useAnimationEventSync)
            {
                PerformHitboxCheck();
                Invoke(nameof(ResetAfterAttack), 0.3f);
            }
        }

        /// <summary>
        /// Animation Event ile çağrılır.
        /// Animator Controller'daki saldırı animasyonunun vuruş karesine
        /// bu metodu bir "Animation Event" olarak ekle:
        /// Function : OnAttackHitFrame
        /// (parametre yok)
        /// </summary>
        public void OnAttackHitFrame()
        {
            if (!HasState(PlayerState.Attack)) return;
            PerformHitboxCheck();
            Invoke(nameof(ResetAfterAttack), 0.05f);
        }

        private void ResetAfterAttack()
        {
            if (HasState(PlayerState.Attack))
                TransitionTo(_isGrounded ? PlayerState.Idle : PlayerState.Jump);
        }

        private void HandleBlock()
        {
            if (HasState(PlayerState.Dead | PlayerState.Dash | PlayerState.Attack)) return;

            if (GetBlockInput())      TransitionTo(PlayerState.Block);
            else if (HasState(PlayerState.Block)) TransitionTo(PlayerState.Idle);
        }

        private void ApplyMovement() => _cc.Move(_velocity * Time.deltaTime);

        // ── Hitbox ──────────────────────────────────────────────────────────────────────

        private void PerformHitboxCheck()
        {
            if (attackPoint == null) return;

            Collider[] hits = Physics.OverlapSphere(
                attackPoint.position, data.attackRange, enemyLayer);
            
            foreach (Collider hit in hits)
                if (hit.TryGetComponent<IDamageable>(out var target) && target.IsAlive)
                    target.TakeDamage(data.attackPower);
        }

        // ── Ölüm ────────────────────────────────────────────────────────────────────────

        private void Die()
        {
            TransitionTo(PlayerState.Dead);
            _velocity = Vector3.zero;
            OnDeath();
        }

        protected virtual void OnDeath() =>
            Debug.Log($"[{data.characterName}] öldü.");

        // ── Soyut Input Metotları ──────────────────────────────────────────────────────

        protected abstract float GetHorizontalInput();
        protected abstract bool  GetJumpInput();
        protected abstract bool  GetAttackInput();
        protected abstract bool  GetBlockInput();
        protected abstract bool  GetDashInput();
        protected abstract bool  GetCrouchInput();

        // ── Editor Gizmos ──────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position,
                    data != null ? data.attackRange : 1f);
            }
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
#endif
    }
}