// ============================================================
//  AbstractCharacter.cs
//  Namespace : FightingGame.Character
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  BasePlayer.cs'nin refactored versiyonu.
//  Input okuma kaldırıldı — artık Player'ın işi.
//  ICharacter ve IDamageable implement ediliyor.
//  Skill1/Skill2 abstract olarak alt sınıflara bırakıldı.
//
// ============================================================

using System;
using UnityEngine;
using FightingGame.Core.Data;
using FightingGame.Core.Interfaces;

namespace FightingGame.Character
{
    [Flags]
    public enum CharacterState
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

    internal static class AnimParam
    {
        public static readonly int Speed         = Animator.StringToHash("Speed");
        public static readonly int IsJumping     = Animator.StringToHash("isJumping");
        public static readonly int IsCrouching   = Animator.StringToHash("isCrouching");
        public static readonly int IsBlocking    = Animator.StringToHash("isBlocking");
        public static readonly int IsHit         = Animator.StringToHash("isHit");
        public static readonly int IsDead        = Animator.StringToHash("isDead");
        public static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");
    }

    [RequireComponent(typeof(CharacterController))]
    public abstract class AbstractCharacter : MonoBehaviour, ICharacter, IDamageable
    {
        // ── Inspector Alanları ───────────────────────────────────
        [Header("Data")]
        [SerializeField] protected CharacterData data;

        [Header("Combat")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Opponent")]
        [SerializeField] private Transform opponent;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float     groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Crouch")]
        [SerializeField] private float standingHeight  = 2f;
        [SerializeField] private float crouchHeight    = 1f;
        [SerializeField] private float crouchLerpSpeed = 12f;

        [Header("Attack Sync")]
        [SerializeField] private bool useAnimationEventSync = true;

        [Header("Visual Settings")]
        [SerializeField] protected Transform visualMesh;

        // ── IDamageable ─────────────────────────────────────────
        private int _currentHealth;
        public  int  CurrentHealth => _currentHealth;
        public  int  MaxHealth     => data != null ? data.maxHealth : 0;
        public  bool IsAlive       => _currentHealth > 0;

        // ── ICharacter — Data ───────────────────────────────────
        public CharacterData Data => data;

        // ── State ───────────────────────────────────────────────
        public CharacterState CurrentState { get; private set; } = CharacterState.Idle;

        // ── Bileşen Referansları ────────────────────────────────
        private CharacterController _cc;
        protected Animator          _animator;

        // ── Fizik ───────────────────────────────────────────────
        private Vector3 _velocity;
        private bool    _isGrounded;
        private float   _dashTimer;
        private float   _dashCooldownTimer;
        private float   _attackCooldownTimer;
        private float   _hitStunTimer;
        private const float Gravity = -20f;
        private float _defaultYRotation;

        // Player tarafından set edilen pending input değerleri
        private float _pendingHorizontal;
        private bool  _pendingCrouch;
        private bool  _pendingBlock;

        // ══════════════════════════════════════════════════════════
        //  Unity Lifecycle
        // ══════════════════════════════════════════════════════════
        protected virtual void Awake()
        {
            _cc       = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();

            if (data == null)
            {
                Debug.LogError($"[AbstractCharacter] '{name}': CharacterData atanmamış!", this);
                return;
            }

            _currentHealth    = data.maxHealth;
            _cc.height        = standingHeight;
            _cc.center        = new Vector3(0, standingHeight / 2f, 0);
            _defaultYRotation = transform.eulerAngles.y;
        }

        protected virtual void Update()
        {
            if (!IsAlive) return;

            TickCooldowns();
            CheckGround();
            HandleGravity();
            ProcessMovement();
            ProcessCrouch();
            ProcessBlock();
            ApplyMovement();
            UpdateFacing();
            UpdateAnimatorLocomotion();
        }

        protected virtual void LateUpdate() { }

        // ══════════════════════════════════════════════════════════
        //  ICharacter — Dışarıdan Tetiklenen Aksiyonlar
        // ══════════════════════════════════════════════════════════
        public virtual void Walk(float direction) => _pendingHorizontal = direction;

        public virtual void Jump()
        {
            if (!_isGrounded) return;
            if (HasState(CharacterState.Dead | CharacterState.Crouch | CharacterState.Dash)) return;
            _velocity.y = data.jumpForce;
            TransitionTo(CharacterState.Jump);
        }

        public virtual void Attack()
        {
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;
            if (_attackCooldownTimer > 0f) return;

            _attackCooldownTimer = data.attackCooldown;
            TransitionTo(CharacterState.Attack);

            if (_animator != null)
                _animator.SetTrigger(AnimParam.AttackTrigger);

            if (!useAnimationEventSync)
            {
                PerformHitboxCheck();
                Invoke(nameof(ResetAfterAttack), 0.3f);
            }
        }

        public virtual void Block(bool active) => _pendingBlock = active;
        public virtual void Crouch(bool active) => _pendingCrouch = active;

        public virtual void Dash(float direction)
        {
            if (HasState(CharacterState.Dead | CharacterState.Dash)) return;
            if (_dashCooldownTimer > 0f) return;

            float dirSign = direction != 0f ? Mathf.Sign(direction) : GetFacingSign();
            _dashTimer         = 0.2f;
            _dashCooldownTimer = data.dashCooldown;
            _velocity          = new Vector3(dirSign, 0f, 0f) * data.moveSpeed * 2.5f;
            TransitionTo(CharacterState.Dash);
        }

        public abstract void Skill1();
        public abstract void Skill2();

        // ══════════════════════════════════════════════════════════
        //  IDamageable
        // ══════════════════════════════════════════════════════════
        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;
            if (!IsAlive) return;
            if (HasState(CharacterState.Block)) return;

            int finalDamage = Mathf.Max(0, amount - data.armor);
            _currentHealth  = Mathf.Max(0, _currentHealth - finalDamage);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                _hitStunTimer = data.hitStunDuration;
                TransitionTo(CharacterState.Hit);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Rakip Yönetimi
        // ══════════════════════════════════════════════════════════
        public void SetOpponent(Transform opponentTransform) => opponent = opponentTransform;

        // ══════════════════════════════════════════════════════════
        //  Durum Makinesi
        // ══════════════════════════════════════════════════════════
        protected void TransitionTo(CharacterState newState)
        {
            if (CurrentState == newState) return;
            CharacterState previous = CurrentState;
            CurrentState = newState;
            SyncAnimator(previous, newState);
            OnStateChanged(previous, newState);
        }

        protected bool HasState(CharacterState flag) => (CurrentState & flag) != 0;
        protected virtual void OnStateChanged(CharacterState previous, CharacterState next) { }

        // ══════════════════════════════════════════════════════════
        //  Animator Senkronizasyonu
        // ══════════════════════════════════════════════════════════
        private void SyncAnimator(CharacterState previous, CharacterState next)
        {
            if (_animator == null) return;

            _animator.SetBool(AnimParam.IsJumping,   false);
            _animator.SetBool(AnimParam.IsCrouching, false);
            _animator.SetBool(AnimParam.IsBlocking,  false);
            _animator.SetBool(AnimParam.IsHit,       false);
            _animator.SetBool(AnimParam.IsDead,      false);

            if      (HasState(CharacterState.Jump))   _animator.SetBool(AnimParam.IsJumping,   true);
            else if (HasState(CharacterState.Crouch)) _animator.SetBool(AnimParam.IsCrouching, true);
            else if (HasState(CharacterState.Block))  _animator.SetBool(AnimParam.IsBlocking,  true);
            else if (HasState(CharacterState.Hit))    _animator.SetBool(AnimParam.IsHit,       true);
            else if (HasState(CharacterState.Dead))   _animator.SetBool(AnimParam.IsDead,      true);
        }

        private void UpdateAnimatorLocomotion()
        {
            if (_animator == null) return;
            Vector3 hVel = new Vector3(_cc.velocity.x, 0, _cc.velocity.z);
            _animator.SetFloat(AnimParam.Speed, hVel.magnitude);
        }

        // ══════════════════════════════════════════════════════════
        //  İç Fizik & Hareket
        // ══════════════════════════════════════════════════════════
        private void TickCooldowns()
        {
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;
            if (_dashCooldownTimer  > 0f) _dashCooldownTimer  -= Time.deltaTime;

            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                    TransitionTo(_isGrounded ? CharacterState.Idle : CharacterState.Jump);
            }

            // Hit stun süresi dolunca otomatik recover.
            // Olmadığında karakter ilk darbeden sonra sonsuza dek Hit state'inde sıkışırdı.
            if (_hitStunTimer > 0f)
            {
                _hitStunTimer -= Time.deltaTime;
                if (_hitStunTimer <= 0f && HasState(CharacterState.Hit))
                    TransitionTo(_isGrounded ? CharacterState.Idle : CharacterState.Jump);
            }
        }

        private void CheckGround()
        {
            Vector3 capsuleBase = transform.position
                                + _cc.center
                                - new Vector3(0f, _cc.height * 0.5f, 0f);
            _isGrounded = Physics.CheckSphere(capsuleBase, groundCheckRadius, groundLayer);
            if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;
        }

        private void HandleGravity()
        {
            if (HasState(CharacterState.Dash)) return;
            if (_isGrounded) return;
            _velocity.y += Gravity * Time.deltaTime;
        }

        private void ProcessMovement()
        {
            if (HasState(CharacterState.Dead | CharacterState.Hit | CharacterState.Dash)) return;
            _velocity.x = _pendingHorizontal * data.moveSpeed;

            if (_isGrounded && !HasState(CharacterState.Attack | CharacterState.Block | CharacterState.Crouch))
                TransitionTo(Mathf.Abs(_pendingHorizontal) > 0.01f ? CharacterState.Move : CharacterState.Idle);

            if (HasState(CharacterState.Jump) && _isGrounded && _velocity.y <= 0f)
                TransitionTo(CharacterState.Idle);
        }

        private void ProcessCrouch()
        {
            if (!_isGrounded) return;
            if (HasState(CharacterState.Dead | CharacterState.Dash)) return;

            if (_pendingCrouch) TransitionTo(CharacterState.Crouch);
            else if (HasState(CharacterState.Crouch)) TransitionTo(CharacterState.Idle);

            float targetH = HasState(CharacterState.Crouch) ? crouchHeight : standingHeight;
            float newH     = Mathf.Lerp(_cc.height, targetH, crouchLerpSpeed * Time.deltaTime);
            _cc.height = newH;
            _cc.center = new Vector3(0f, newH * 0.5f, 0f);
        }

        private void ProcessBlock()
        {
            if (HasState(CharacterState.Dead | CharacterState.Dash | CharacterState.Attack)) return;
            if (_pendingBlock) TransitionTo(CharacterState.Block);
            else if (HasState(CharacterState.Block)) TransitionTo(CharacterState.Idle);
        }

        private void UpdateFacing()
        {
            if (opponent == null) return;
            if (HasState(CharacterState.Attack | CharacterState.Hit | CharacterState.Dead | CharacterState.Dash)) return;

            float dx = opponent.position.x - transform.position.x;
            if (Mathf.Abs(dx) < 0.01f) return;

            bool faceRight = dx > 0f;
            transform.rotation = Quaternion.Euler(0f,
                faceRight ? _defaultYRotation : _defaultYRotation + 180f, 0f);
        }

        private void ApplyMovement() => _cc.Move(_velocity * Time.deltaTime);

        private float GetFacingSign()
        {
            float deltaY = Mathf.DeltaAngle(_defaultYRotation, transform.eulerAngles.y);
            return Mathf.Abs(deltaY) < 90f ? 1f : -1f;
        }

        // ══════════════════════════════════════════════════════════
        //  Attack — Animation Event Sync
        // ══════════════════════════════════════════════════════════
        public void OnAttackHitFrame()
        {
            if (!HasState(CharacterState.Attack)) return;
            PerformHitboxCheck();
        }

        public void OnAttackEndFrame() => ResetAfterAttack();

        private void ResetAfterAttack()
        {
            if (HasState(CharacterState.Attack))
                TransitionTo(_isGrounded ? CharacterState.Idle : CharacterState.Jump);
        }

        private void PerformHitboxCheck()
        {
            if (attackPoint == null) return;
            Collider[] hits = Physics.OverlapSphere(attackPoint.position, data.attackRange, enemyLayer);
            foreach (Collider hit in hits)
                if (hit.TryGetComponent<IDamageable>(out var target) && target.IsAlive)
                    target.TakeDamage(data.attackPower);
        }

        // ── Ölüm ───────────────────────────────────────────────
        private void Die()
        {
            TransitionTo(CharacterState.Dead);
            _velocity = Vector3.zero;
            OnDeath();
        }

        protected virtual void OnDeath() => Debug.Log($"[{data.characterName}] öldü.");

        // ── Editor Gizmos ───────────────────────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, data != null ? data.attackRange : 1f);
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
