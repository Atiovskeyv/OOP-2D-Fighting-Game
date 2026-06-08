// ============================================================
//  Player.cs
//  Namespace : FightingGame.Player
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Mevcut PlayerOne.cs + PlayerTwo.cs → tek bir Player.cs
//
//  Bu sınıf sadece 2 şey bilir:
//    1. Input okumak (IKeyboardBinding üzerinden)
//    2. Karakter üzerindeki aksiyonları tetiklemek (ICharacter)
//
//  Ne YAPMAZ:
//    • Hareket etmez, hasar vermez, animasyon çalmaz.
//    • Bunlar Character'ın (AbstractCharacter) işi.
//
//  Kritik nokta:
//    Player, _input ve _character değişkenlerini interface tipi
//    olarak tutuyor. "WASD mi Arrow mu?" veya "Omniman mı
//    Guts mu?" bilmiyor. Sadece "bir input kaynağı" ve
//    "bir karakter" biliyor.
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Interfaces;
using FightingGame.Character;

namespace FightingGame.Player
{
    /// <summary>
    /// Oyuncu controller'ı — input okur, karakter aksiyonlarını tetikler.
    /// <para>
    /// Sahne'de 2 adet GameObject üzerinde bulunur: biri Player 1, biri Player 2.
    /// Aynı script, farklı binding ve farklı character enjekte edilir.
    /// </para>
    /// </summary>
    public class Player : MonoBehaviour
    {
        // ── Interface Referansları ───────────────────────────────
        // Player asla somut tipe bağımlı değil (DIP).

        private IKeyboardBinding _input;
        private ICharacter       _character;

        // ── Durum ───────────────────────────────────────────────

        private bool _initialized;

        // ══════════════════════════════════════════════════════════
        //  Initialization — MatchManager tarafından çağrılır
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Player'ı hazırlar. MatchManager tarafından Start()'ta çağrılır.
        /// </summary>
        /// <param name="input">Input kaynağı (WASD, Arrows, Gamepad vs.)</param>
        /// <param name="character">Kontrol edilecek karakter (Omniman, Guts vs.)</param>
        public void Initialize(IKeyboardBinding input, ICharacter character)
        {
            _input     = input;
            _character = character;

            _input.Enable();
            _initialized = true;

            Debug.Log($"[Player] Initialized — Character: {_character.Data.characterName}");
        }

        /// <summary>
        /// Rakibin transform'unu set eder. MatchManager tarafından çağrılır.
        /// </summary>
        public void SetOpponent(Transform opponentTransform)
        {
            _character.SetOpponent(opponentTransform);
        }

        // ══════════════════════════════════════════════════════════
        //  Unity Lifecycle
        // ══════════════════════════════════════════════════════════

        private void Update()
        {
            if (!_initialized) return;
            if (!_character.IsAlive) return;

            HandleWalk();
            HandleJump();
            HandleSkill3(); // Ulti öncelikli taranır (Q+E)
            HandleSkill1(); // Breaker (Q)
            HandleSkill2(); // Enhanced (E)
            HandlePunch();  // J
            HandleKick();   // K
            HandleShoot();  // U
            HandleBlock();  // L (Sürekli)
            HandleCrouch(); // S (Sürekli)
            HandleDash();   // Shift / Numpad 0
        }

        private void OnDisable()
        {
            _input?.Disable();
        }

        private void OnDestroy()
        {
            _input?.Disable();
            _input?.Dispose();
        }

        // ══════════════════════════════════════════════════════════
        //  Input → Character Köprüsü
        //  Her metot: input'tan oku → character'a ilet
        // ══════════════════════════════════════════════════════════

        private void HandleWalk()
        {
            float horizontal = _input.GetHorizontal();
            _character.Walk(horizontal);
        }

        private void HandleJump()
        {
            if (_input.GetJump())
                _character.Jump();
        }

        private void HandlePunch()
        {
            if (_input.GetPunch())
                _character.Punch();
        }

        private void HandleKick()
        {
            if (_input.GetKick())
                _character.Kick();
        }

        private void HandleShoot()
        {
            if (_input.GetShoot())
                _character.Shoot();
        }

        private void HandleBlock()
        {
            _character.Block(_input.GetBlock());
        }

        private void HandleCrouch()
        {
            _character.Crouch(_input.GetCrouch());
        }

        private void HandleDash()
        {
            if (_input.GetDash())
            {
                float horizontal = _input.GetHorizontal();
                _character.Dash(horizontal);
            }
        }

        private void HandleSkill1()
        {
            if (_input.GetSkill1())
                _character.ExecuteSkill1();
        }

        private void HandleSkill2()
        {
            if (_input.GetSkill2())
                _character.ExecuteSkill2();
        }

        private void HandleSkill3()
        {
            if (_input.GetSkill3())
                _character.ExecuteSkill3();
        }
    }
}
