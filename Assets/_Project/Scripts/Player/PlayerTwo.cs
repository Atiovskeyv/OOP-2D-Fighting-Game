// ============================================================
//  PlayerTwo.cs
//  Namespace : FightingGame.Core.Player
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
//
//  Tuş Haritası:
//    Hareket   → Sol Ok / Sağ Ok
//    Çömelme   → Alt Ok
//    Zıplama   → Üst Ok
//    Saldırı   → Numpad 1
//    Blok      → Numpad 2
//    Dash      → Numpad 3
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace FightingGame.Core.Player
{
    public sealed class PlayerTwo : BasePlayer
    {
        // ── InputAction Tanımları ────────────────────────────────

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;
        private InputAction _dashAction;
        private InputAction _blockAction;
        private InputAction _crouchAction;

        // ── Input Buffer'ları ────────────────────────────────────

        private float _horizontalValue;
        private bool  _jumpBuffered;
        private bool  _attackBuffered;
        private bool  _dashBuffered;

        // ── Lifecycle ────────────────────────────────────────────

        protected override void Awake()
        {
            BuildActions();
            base.Awake();
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _attackAction.Enable();
            _dashAction.Enable();
            _blockAction.Enable();
            _crouchAction.Enable();

            _moveAction.performed   += OnMove;
            _moveAction.canceled    += OnMoveCanceled;
            _jumpAction.performed   += OnJump;
            _attackAction.performed += OnAttack;
            _dashAction.performed   += OnDash;
        }

        private void OnDisable()
        {
            _moveAction.performed   -= OnMove;
            _moveAction.canceled    -= OnMoveCanceled;
            _jumpAction.performed   -= OnJump;
            _attackAction.performed -= OnAttack;
            _dashAction.performed   -= OnDash;

            _moveAction.Disable();
            _jumpAction.Disable();
            _attackAction.Disable();
            _dashAction.Disable();
            _blockAction.Disable();
            _crouchAction.Disable();
        }

        private void OnDestroy()
        {
            _moveAction.Dispose();
            _jumpAction.Dispose();
            _attackAction.Dispose();
            _dashAction.Dispose();
            _blockAction.Dispose();
            _crouchAction.Dispose();
        }

        // ── Action İnşası ────────────────────────────────────────

        private void BuildActions()
        {
            _moveAction = new InputAction(name: "Move", type: InputActionType.Value);
            _moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");

            _jumpAction = new InputAction(
                name: "Jump", type: InputActionType.Button,
                binding: "<Keyboard>/upArrow", interactions: "press");

            _attackAction = new InputAction(
                name: "Attack", type: InputActionType.Button,
                binding: "<Keyboard>/numpad1", interactions: "press");

            _dashAction = new InputAction(
                name: "Dash", type: InputActionType.Button,
                binding: "<Keyboard>/numpad3", interactions: "press");

            _blockAction  = new InputAction(name: "Block",
                type: InputActionType.Button, binding: "<Keyboard>/numpad2");

            _crouchAction = new InputAction(name: "Crouch",
                type: InputActionType.Button, binding: "<Keyboard>/downArrow");
        }

        // ── Event Callback'leri ──────────────────────────────────

        private void OnMove(InputAction.CallbackContext ctx)
            => _horizontalValue = ctx.ReadValue<float>();

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
            => _horizontalValue = 0f;

        private void OnJump(InputAction.CallbackContext ctx)    => _jumpBuffered   = true;
        private void OnAttack(InputAction.CallbackContext ctx)  => _attackBuffered = true;
        private void OnDash(InputAction.CallbackContext ctx)    => _dashBuffered   = true;

        // ── BasePlayer Soyut Metotları ───────────────────────────

        protected override float GetHorizontalInput() => _horizontalValue;

        protected override bool GetJumpInput()
        {
            bool v = _jumpBuffered; _jumpBuffered = false; return v;
        }

        protected override bool GetAttackInput()
        {
            bool v = _attackBuffered; _attackBuffered = false; return v;
        }

        protected override bool GetDashInput()
        {
            bool v = _dashBuffered; _dashBuffered = false; return v;
        }

        protected override bool GetBlockInput()  => _blockAction.IsPressed();
        protected override bool GetCrouchInput() => _crouchAction.IsPressed();

        // ── Override'lar ─────────────────────────────────────────

        protected override void OnDeath()
        {
            base.OnDeath();
            // TODO: Game Over ekranını tetikle
        }

        /// <summary>
        /// BasePlayer → SyncAnimator çağrıldıktan SONRA buraya gelir.
        /// Animator zaten güncellenmiş durumda; burada ek ses/efekt tetiklenebilir.
        /// </summary>
        protected override void OnStateChanged(PlayerState previous, PlayerState next)
        {
            // Örnek: saldırı sesini tetikle
            // if (next == PlayerState.Attack) AudioManager.Play("swing");

            // Örnek: zıplama partikülleri
            // if (next == PlayerState.Jump) vfxJump.Play();

            Debug.Log($"[PlayerTwo] {previous} → {next}");
        }
    }
}
