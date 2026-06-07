// ============================================================
//  KeyboardBindingArrows.cs
//  Namespace : FightingGame.Input
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  DESIGN PATTERN: Strategy Pattern (Concrete Strategy)
//  ---------------------------------------------------
//  Bu sınıf IKeyboardBinding interface'ini Ok tuşları + Numpad
//  şemasıyla implement eder. Player 2 için kullanılır.
//
//  Tuş Haritası:
//    Hareket   → Sol Ok / Sağ Ok
//    Çömelme   → Alt Ok
//    Zıplama   → Üst Ok
//    Saldırı   → Numpad 1
//    Blok      → Numpad 2
//    Dash      → Numpad 3
//    Q Skill   → Numpad 4
//    Ultimate  → Numpad 5
//
//  NOT: Bu sınıf MonoBehaviour DEĞİLDİR. Bir GameObject'e
//  eklenmez. Player tarafından "new" ile oluşturulur veya
//  MatchManager tarafından enjekte edilir.
//
// ============================================================

using UnityEngine.InputSystem;
using FightingGame.Core.Interfaces;

namespace FightingGame.Input
{
    /// <summary>
    /// Ok tuşları + Numpad tuş şeması ile IKeyboardBinding implementasyonu.
    /// Player 2 için varsayılan input kaynağıdır.
    /// </summary>
    public sealed class KeyboardBindingArrows : IKeyboardBinding
    {
        // ── InputAction Tanımları ────────────────────────────────

        private readonly InputAction _moveAction;
        private readonly InputAction _jumpAction;
        private readonly InputAction _punchAction;
        private readonly InputAction _kickAction;
        private readonly InputAction _shootAction;
        private readonly InputAction _dashAction;
        private readonly InputAction _blockAction;
        private readonly InputAction _crouchAction;
        private readonly InputAction _skill1Action;
        private readonly InputAction _skill2Action;

        // ── Input Buffer'ları ────────────────────────────────────

        private float _horizontalValue;
        private bool  _jumpBuffered;
        private bool  _punchBuffered;
        private bool  _kickBuffered;
        private bool  _shootBuffered;
        private bool  _dashBuffered;
        private bool  _skill1Buffered;
        private bool  _skill2Buffered;

        // ── Constructor ─────────────────────────────────────────

        public KeyboardBindingArrows()
        {
            // --- Hareket (1DAxis composite) ---
            _moveAction = new InputAction(name: "P2_Move", type: InputActionType.Value);
            _moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");

            // --- Anlık basım aksiyonları ---
            _jumpAction = new InputAction(
                name: "P2_Jump", type: InputActionType.Button,
                binding: "<Keyboard>/upArrow", interactions: "press");

            _punchAction = new InputAction(
                name: "P2_Punch", type: InputActionType.Button,
                binding: "<Keyboard>/numpad1", interactions: "press");

            _kickAction = new InputAction(
                name: "P2_Kick", type: InputActionType.Button,
                binding: "<Keyboard>/numpad2", interactions: "press");

            _shootAction = new InputAction(
                name: "P2_Shoot", type: InputActionType.Button,
                binding: "<Keyboard>/numpad3", interactions: "press");

            _dashAction = new InputAction(
                name: "P2_Dash", type: InputActionType.Button,
                binding: "<Keyboard>/numpad0", interactions: "press");

            _skill1Action = new InputAction(
                name: "P2_Skill1", type: InputActionType.Button,
                binding: "<Keyboard>/numpad5", interactions: "press");

            _skill2Action = new InputAction(
                name: "P2_Skill2", type: InputActionType.Button,
                binding: "<Keyboard>/numpad6", interactions: "press");

            // --- Sürekli basılı aksiyonlar ---
            _blockAction = new InputAction(
                name: "P2_Block", type: InputActionType.Button,
                binding: "<Keyboard>/numpad4");

            _crouchAction = new InputAction(
                name: "P2_Crouch", type: InputActionType.Button,
                binding: "<Keyboard>/downArrow");

            // --- Callback kayıtları ---
            _moveAction.performed   += ctx => _horizontalValue = ctx.ReadValue<float>();
            _moveAction.canceled    += _   => _horizontalValue = 0f;
            _jumpAction.performed   += _   => _jumpBuffered    = true;
            _punchAction.performed  += _   => _punchBuffered   = true;
            _kickAction.performed   += _   => _kickBuffered    = true;
            _shootAction.performed  += _   => _shootBuffered   = true;
            _dashAction.performed   += _   => _dashBuffered     = true;
            _skill1Action.performed += _   => _skill1Buffered   = true;
            _skill2Action.performed += _   => _skill2Buffered   = true;
        }

        // ── IKeyboardBinding — Continuous ───────────────────────

        public float GetHorizontal() => _horizontalValue;

        // ── IKeyboardBinding — Buffered (tek frame) ─────────────

        public bool GetJump()
        {
            bool v = _jumpBuffered;
            _jumpBuffered = false;
            return v;
        }

        public bool GetPunch()
        {
            bool v = _punchBuffered;
            _punchBuffered = false;
            return v;
        }

        public bool GetKick()
        {
            bool v = _kickBuffered;
            _kickBuffered = false;
            return v;
        }

        public bool GetShoot()
        {
            bool v = _shootBuffered;
            _shootBuffered = false;
            return v;
        }

        public bool GetDash()
        {
            bool v = _dashBuffered;
            _dashBuffered = false;
            return v;
        }

        // ── IKeyboardBinding — Sürekli basılı ───────────────────

        public bool GetBlock()  => _blockAction.IsPressed();
        public bool GetCrouch() => _crouchAction.IsPressed();

        // ── IKeyboardBinding — Özel Yetenekler ──────────────────

        public bool GetSkill1()
        {
            if (_skill1Buffered && _skill2Buffered) return false;
            bool v = _skill1Buffered;
            _skill1Buffered = false;
            return v;
        }

        public bool GetSkill2()
        {
            if (_skill1Buffered && _skill2Buffered) return false;
            bool v = _skill2Buffered;
            _skill2Buffered = false;
            return v;
        }

        public bool GetSkill3()
        {
            if (_skill1Buffered && _skill2Buffered)
            {
                _skill1Buffered = false;
                _skill2Buffered = false;
                return true;
            }
            return false;
        }

        // ── IKeyboardBinding — Yaşam Döngüsü ───────────────────

        public void Enable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _punchAction.Enable();
            _kickAction.Enable();
            _shootAction.Enable();
            _dashAction.Enable();
            _blockAction.Enable();
            _crouchAction.Enable();
            _skill1Action.Enable();
            _skill2Action.Enable();
        }

        public void Disable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
            _punchAction.Disable();
            _kickAction.Disable();
            _shootAction.Disable();
            _dashAction.Disable();
            _blockAction.Disable();
            _crouchAction.Disable();
            _skill1Action.Disable();
            _skill2Action.Disable();
        }

        public void Dispose()
        {
            _moveAction?.Dispose();
            _jumpAction?.Dispose();
            _punchAction?.Dispose();
            _kickAction?.Dispose();
            _shootAction?.Dispose();
            _dashAction?.Dispose();
            _blockAction?.Dispose();
            _crouchAction?.Dispose();
            _skill1Action?.Dispose();
            _skill2Action?.Dispose();
        }
    }
}
