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
using System.Collections.Generic;
using UnityEngine;
using FightingGame.Core.Data;
using FightingGame.Core.Interfaces;
using FightingGame.Combat;

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

        [Header("Ranged Combat")]
        [Tooltip("Mermi çıkış noktası. Karakterin silah ucu veya el pozisyonuna bağlanır.")]
        [SerializeField] protected Transform shootPoint;

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
        protected bool  _isGrounded;
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

        // Animatör parametre varlığı kontrolü
        private bool _hasSpeedParam;
        private bool _hasIsJumpingParam;
        private bool _hasIsCrouchingParam;
        private bool _hasIsBlockingParam;
        private bool _hasIsHitParam;
        private bool _hasIsDeadParam;
        private bool _hasAttackTriggerParam;

        // ── Dövüş ve Kombo Sistemi ──────────────────────────────
        public enum AttackInputType { Punch, Kick, Shoot }

        [System.Serializable]
        public struct BufferedAttackInput
        {
            public AttackInputType Type;
            public float TimeStamp;

            public BufferedAttackInput(AttackInputType type, float timeStamp)
            {
                Type = type;
                TimeStamp = timeStamp;
            }
        }

        [System.Serializable]
        public class FightingCombo
        {
            public string name;
            public AttackInputType[] sequence;
            public float damageMultiplier = 1f;
            public bool isLauncher;
            public string animTrigger;
            public float pushForce = 2f;
            public Vector3 launchForce = new Vector3(0f, 12f, 0f);

            // ── Uzaktan Saldırı ──
            public bool isRanged;                   // Bu kombo mermi fırlatıyor mu?
            public GameObject projectilePrefab;      // Fırlatılacak mermi prefab'ı
            public int projectileDamageOverride;     // 0 ise data.attackPower * damageMultiplier kullanılır
        }

        protected List<FightingCombo> _combos = new List<FightingCombo>();
        protected List<BufferedAttackInput> _inputBuffer = new List<BufferedAttackInput>();

        [Header("Combo System")]
        [Tooltip("Kombo girişleri arasındaki maksimum süre (saniye). Bu pencere içinde girilen tuşlar kombo olarak değerlendirilir.")]
        [SerializeField] private float _comboWindow = 0.5f;

        // Özel Bar (Special Meter)
        protected float _specialMeter;
        public float SpecialMeter => _specialMeter;
        public int SpecialMeterSegments => Mathf.FloorToInt(_specialMeter / 100f);

        // Juggle / Havaya Fırlatma Fiziği Durumu
        private bool _isJuggled;
        private float _juggleGravityMultiplier = 0.3f; // Havada süzülmesi için yerçekimi çarpanı

        private FightingCombo _currentCombo;


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

            InitializeAnimatorParameters();
            InitializeCombos();
        }

        protected virtual void Update()
        {
            if (!IsAlive) return;
            if (data == null) return;

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

        // ── ICharacter — Saldırılar ve Kombolar ──────────────────

        public virtual void Punch()
        {
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;
            AddInputToBuffer(AttackInputType.Punch);
        }

        public virtual void Kick()
        {
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;
            AddInputToBuffer(AttackInputType.Kick);
        }

        public virtual void Shoot()
        {
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;
            AddInputToBuffer(AttackInputType.Shoot);
        }

        private void AddInputToBuffer(AttackInputType type)
        {
            _inputBuffer.Add(new BufferedAttackInput(type, Time.time));

            FightingCombo matchedCombo = MatchCombo();
            if (matchedCombo != null)
            {
                ExecuteCombo(matchedCombo);
            }
        }

        private FightingCombo MatchCombo()
        {
            float now = Time.time;
            _inputBuffer.RemoveAll(i => now - i.TimeStamp > _comboWindow);

            if (_inputBuffer.Count == 0) return null;

            FightingCombo bestMatch = null;
            int bestLength = 0;

            foreach (var combo in _combos)
            {
                if (combo.sequence.Length > _inputBuffer.Count) continue;

                bool isMatch = true;
                for (int i = 0; i < combo.sequence.Length; i++)
                {
                    int bufferIdx = _inputBuffer.Count - combo.sequence.Length + i;
                    if (_inputBuffer[bufferIdx].Type != combo.sequence[i])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch && combo.sequence.Length > bestLength)
                {
                    bestMatch = combo;
                    bestLength = combo.sequence.Length;
                }
            }

            return bestMatch;
        }

        private void ExecuteCombo(FightingCombo combo)
        {
            if (_attackCooldownTimer > 0f && combo.sequence.Length == 1) return; // Düz saldırılarda cooldown koruması

            _currentCombo = combo;
            _attackCooldownTimer = data.attackCooldown;
            
            // Kombo başladığını bildiren temiz log
            if (combo.sequence.Length > 1)
            {
                Debug.Log($"<color=#00FFFF>🔥 [{gameObject.name}] Kombo Başladı: {combo.name} ({combo.damageMultiplier}x Hasar)</color>");
            }

            TransitionTo(CharacterState.Attack);

            SetAnimatorTrigger(combo.animTrigger);

            if (useAnimationEventSync)
            {
                CancelInvoke(nameof(SafetyResetAfterAttack));
                Invoke(nameof(SafetyResetAfterAttack), 1.0f);
            }
            else
            {
                CancelInvoke(nameof(PerformHitboxCheck));
                CancelInvoke(nameof(ResetAfterAttack));
                Invoke(nameof(PerformHitboxCheck), 0.15f);
                Invoke(nameof(ResetAfterAttack), 0.3f);
            }
        }

        private void SetAnimatorTrigger(string triggerName)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;

            bool triggerExists = false;
            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                {
                    triggerExists = true;
                    break;
                }
            }

            if (triggerExists)
            {
                _animator.SetTrigger(triggerName);
            }
            else if (_hasAttackTriggerParam)
            {
                _animator.SetTrigger(AnimParam.AttackTrigger);
            }
        }

        private void SafetyResetAfterAttack()
        {
            if (HasState(CharacterState.Attack))
            {
                Debug.LogWarning($"[{data.characterName}] Safety reset triggered. Animation event might be missing.");
                ResetAfterAttack();
            }
        }

        private void InitializeAnimatorParameters()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;

            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.nameHash == AnimParam.Speed) _hasSpeedParam = true;
                else if (param.nameHash == AnimParam.IsJumping) _hasIsJumpingParam = true;
                else if (param.nameHash == AnimParam.IsCrouching) _hasIsCrouchingParam = true;
                else if (param.nameHash == AnimParam.IsBlocking) _hasIsBlockingParam = true;
                else if (param.nameHash == AnimParam.IsHit) _hasIsHitParam = true;
                else if (param.nameHash == AnimParam.IsDead) _hasIsDeadParam = true;
                else if (param.nameHash == AnimParam.AttackTrigger) _hasAttackTriggerParam = true;
            }
        }

        protected virtual void InitializeCombos()
        {
            // P + P + P (Launcher 1)
            _combos.Add(new FightingCombo
            {
                name = "PPP_Launcher",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch, AttackInputType.Punch },
                damageMultiplier = 2.5f,
                isLauncher = true,
                animTrigger = "PPP_Combo",
                launchForce = new Vector3(3f, 12f, 0f)
            });

            // P + P + K (Launcher 2)
            _combos.Add(new FightingCombo
            {
                name = "PPK_Launcher",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch, AttackInputType.Kick },
                damageMultiplier = 3.0f,
                isLauncher = true,
                animTrigger = "PPK_Combo",
                launchForce = new Vector3(3f, 13f, 0f)
            });

            // P + P + S (Heavy Shoot Combo)
            _combos.Add(new FightingCombo
            {
                name = "PPS_HeavyShoot",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch, AttackInputType.Shoot },
                damageMultiplier = 2.0f,
                isLauncher = false,
                animTrigger = "PPS_Combo",
                pushForce = 5f
            });

            // P + P (Double Punch)
            _combos.Add(new FightingCombo
            {
                name = "PP_DoublePunch",
                sequence = new AttackInputType[] { AttackInputType.Punch, AttackInputType.Punch },
                damageMultiplier = 1.5f,
                isLauncher = false,
                animTrigger = "PP_Combo",
                pushForce = 2f
            });

            // P (Single Punch)
            _combos.Add(new FightingCombo
            {
                name = "Punch",
                sequence = new AttackInputType[] { AttackInputType.Punch },
                damageMultiplier = 1.0f,
                isLauncher = false,
                animTrigger = "Punch",
                pushForce = 1f
            });

            // K (Single Kick)
            _combos.Add(new FightingCombo
            {
                name = "Kick",
                sequence = new AttackInputType[] { AttackInputType.Kick },
                damageMultiplier = 1.2f,
                isLauncher = false,
                animTrigger = "Kick",
                pushForce = 1.5f
            });

            // S (Single Shoot)
            _combos.Add(new FightingCombo
            {
                name = "Shoot",
                sequence = new AttackInputType[] { AttackInputType.Shoot },
                damageMultiplier = 0.5f,
                isLauncher = false,
                animTrigger = "Shoot",
                pushForce = 0.5f
            });
        }

        // ── ICharacter — Özel Yetenekler (Bar Harcayan) ──────────

        public virtual void ExecuteSkill1()
        {
            if (!IsAlive) return;
            if (_specialMeter < 100f)
            {
                Debug.Log($"[{data.characterName}] Not enough energy for Skill 1 (Enhanced Shoot)! Current: {_specialMeter}");
                return;
            }
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;

            AddSpecialMeter(-100f);
            Debug.Log($"[{data.characterName}] Executing Skill 1 (Enhanced Shoot)!");
            OnExecuteSkill1();
        }

        public virtual void ExecuteSkill2()
        {
            if (!IsAlive) return;
            if (_specialMeter < 200f)
            {
                Debug.Log($"[{data.characterName}] Not enough energy for Skill 2 (Combo Breaker)! Current: {_specialMeter}");
                return;
            }
            if (!HasState(CharacterState.Hit))
            {
                Debug.Log($"[{data.characterName}] Combo Breaker can only be executed in HIT state!");
                return;
            }

            AddSpecialMeter(-200f);
            Debug.Log($"[{data.characterName}] Executing Skill 2 (Combo Breaker)!");
            
            BreakCombo();
            OnExecuteSkill2();
        }

        public virtual void ExecuteSkill3()
        {
            if (!IsAlive) return;
            if (_specialMeter < 300f)
            {
                Debug.Log($"[{data.characterName}] Not enough energy for Skill 3 (Ultimate)! Current: {_specialMeter}");
                return;
            }
            if (HasState(CharacterState.Dead | CharacterState.Hit)) return;

            AddSpecialMeter(-300f);
            Debug.Log($"[{data.characterName}] Executing Skill 3 (Ultimate)!");
            OnExecuteSkill3();
        }

        public void AddSpecialMeter(float amount)
        {
            _specialMeter = Mathf.Clamp(_specialMeter + amount, 0f, 300f);
        }

        private void BreakCombo()
        {
            _hitStunTimer = 0f;
            TransitionTo(CharacterState.Idle);
            _velocity = Vector3.zero;

            if (opponent != null)
            {
                float dist = Vector3.Distance(transform.position, opponent.position);
                if (dist < 4f && opponent.TryGetComponent<AbstractCharacter>(out var oppChar))
                {
                    float pushDir = Mathf.Sign(opponent.position.x - transform.position.x);
                    oppChar.TakeDamage(10, true, new Vector3(pushDir * 10f, 5f, 0f));
                    Debug.Log($"[{data.characterName}] Combo Breaker knocked back opponent [{oppChar.Data.characterName}]!");
                }
            }
        }

        protected abstract void OnExecuteSkill1();
        protected abstract void OnExecuteSkill2();
        protected abstract void OnExecuteSkill3();

        // ══════════════════════════════════════════════════════════
        //  IDamageable
        // ══════════════════════════════════════════════════════════
        public void TakeDamage(int amount)
        {
            TakeDamage(amount, false, Vector3.zero);
        }

        public void TakeDamage(int amount, bool launch, Vector3 launchForce)
        {
            if (amount <= 0) return;
            if (!IsAlive) return;

            if (HasState(CharacterState.Block))
            {
                int blockedDamage = Mathf.Max(0, Mathf.RoundToInt(amount * 0.2f) - data.armor);
                _currentHealth = Mathf.Max(0, _currentHealth - blockedDamage);
                AddSpecialMeter(blockedDamage * data.specialMeterFillMultiplier * 0.5f);
                if (_currentHealth <= 0)
                {
                    Die();
                }
                return;
            }

            int finalDamage = Mathf.Max(0, amount - data.armor);
            _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);

            AddSpecialMeter(finalDamage * data.specialMeterFillMultiplier);

            if (_currentHealth <= 0)
            {
                Die();
                return;
            }

            _hitStunTimer = data.hitStunDuration;
            TransitionTo(CharacterState.Hit);

            if (launch)
            {
                _isJuggled = true;
                _velocity = launchForce;
            }
            else
            {
                float pushDirection = -GetFacingSign();
                _velocity = new Vector3(pushDirection * 2f, 0f, 0f);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Rakip Yönetimi
        // ══════════════════════════════════════════════════════════
        public void SetOpponent(Transform opponentTransform) => opponent = opponentTransform;
        public void SetEnemyLayer(LayerMask mask) => enemyLayer = mask;

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
            if (_animator == null || _animator.runtimeAnimatorController == null) return;

            // Önce tüm parametreleri sıfırla (sadece varsa)
            if (_hasIsJumpingParam)   _animator.SetBool(AnimParam.IsJumping,   false);
            if (_hasIsCrouchingParam) _animator.SetBool(AnimParam.IsCrouching, false);
            if (_hasIsBlockingParam)  _animator.SetBool(AnimParam.IsBlocking,  false);
            if (_hasIsHitParam)       _animator.SetBool(AnimParam.IsHit,       false);
            if (_hasIsDeadParam)      _animator.SetBool(AnimParam.IsDead,      false);

            // Aktif state'e göre ilgili parametreyi aç
            if      (_hasIsJumpingParam   && HasState(CharacterState.Jump))   _animator.SetBool(AnimParam.IsJumping,   true);
            else if (_hasIsCrouchingParam && HasState(CharacterState.Crouch)) _animator.SetBool(AnimParam.IsCrouching, true);
            else if (_hasIsBlockingParam  && HasState(CharacterState.Block))  _animator.SetBool(AnimParam.IsBlocking,  true);
            else if (_hasIsHitParam       && HasState(CharacterState.Hit))    _animator.SetBool(AnimParam.IsHit,       true);
            else if (_hasIsDeadParam      && HasState(CharacterState.Dead))   _animator.SetBool(AnimParam.IsDead,      true);
        }

        private void UpdateAnimatorLocomotion()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            if (!_hasSpeedParam) return;
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
            if (_hitStunTimer > 0f)
            {
                _hitStunTimer -= Time.deltaTime;
            }

            if (HasState(CharacterState.Hit) && _hitStunTimer <= 0f)
            {
                if (!_isJuggled)
                {
                    TransitionTo(_isGrounded ? CharacterState.Idle : CharacterState.Jump);
                }
                else if (_isGrounded)
                {
                    TransitionTo(CharacterState.Idle);
                }
            }
        }

        private void CheckGround()
        {
            Vector3 checkPosition = groundCheck != null ? groundCheck.position : (transform.position + _cc.center - new Vector3(0f, _cc.height * 0.5f, 0f));
            _isGrounded = Physics.CheckSphere(checkPosition, groundCheckRadius, groundLayer);
            if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
                _isJuggled = false;
            }
        }

        private void HandleGravity()
        {
            if (HasState(CharacterState.Dash)) return;

            // Apply horizontal deceleration in hit stun or air
            if (HasState(CharacterState.Hit | CharacterState.Jump))
            {
                _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, 15f * Time.deltaTime);
            }

            if (_isGrounded) return;

            float currentGravity = Gravity;
            if (_isJuggled && _velocity.y < 0f)
            {
                currentGravity *= _juggleGravityMultiplier;
            }
            _velocity.y += currentGravity * Time.deltaTime;
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
            // ── Uzaktan saldırı kontrolü ──
            if (_currentCombo != null && _currentCombo.isRanged)
            {
                int rangedDamage = _currentCombo.projectileDamageOverride > 0
                    ? _currentCombo.projectileDamageOverride
                    : Mathf.RoundToInt(data.attackPower * _currentCombo.damageMultiplier);

                SpawnProjectile(_currentCombo.projectilePrefab, rangedDamage);
                return; // Yakın dövüş hitbox kontrolü yapma
            }

            // ── Yakın dövüş hitbox kontrolü ──
            if (attackPoint == null) return;
            Collider[] hits = Physics.OverlapSphere(attackPoint.position, data.attackRange, enemyLayer);

            float damageMult = _currentCombo != null ? _currentCombo.damageMultiplier : 1f;
            bool launch = _currentCombo != null ? _currentCombo.isLauncher : false;
            Vector3 launchForce = _currentCombo != null ? _currentCombo.launchForce : Vector3.zero;

            float facingSign = GetFacingSign();
            launchForce.x *= facingSign;

            int damage = Mathf.RoundToInt(data.attackPower * damageMult);

            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                if (hit.TryGetComponent<IDamageable>(out var target) && target.IsAlive)
                {
                    if (target is AbstractCharacter targetChar)
                    {
                        targetChar.TakeDamage(damage, launch, launchForce);
                        Debug.Log($"<color=#00FF00>⚔️ [{data.characterName}] hit [{targetChar.Data.characterName}] -> {_currentCombo?.name} ({damage} Damage!)</color>");
                    }
                    else
                    {
                        target.TakeDamage(damage);
                        Debug.Log($"<color=#00FF00>⚔️ [{data.characterName}] hit [{hit.name}] -> {_currentCombo?.name} ({damage} Damage!)</color>");
                    }
                }
            }
        }

        // ── Mermi Fırlatma ───────────────────────────────────────
        /// <summary>
        /// ShootPoint'ten verilen prefab'ı mermi olarak fırlatır.
        /// Alt sınıflar (SoldierBoy gibi) skill'lerden de çağırabilir.
        /// </summary>
        protected void SpawnProjectile(GameObject prefab, int damage)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Mermi prefab'ı atanmamış!");
                return;
            }

            Transform spawnPoint = shootPoint != null ? shootPoint : attackPoint;
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ShootPoint ve AttackPoint ikisi de boş, mermi fırlatılamaz!");
                return;
            }

            Vector3 direction = new Vector3(GetFacingSign(), 0f, 0f);
            GameObject projectileObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            if (projectileObj.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Initialize(gameObject, direction, damage);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Mermi prefab'ında Projectile script'i yok!", prefab);
                Destroy(projectileObj);
            }
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
            if (shootPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(shootPoint.position, 0.15f);
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
