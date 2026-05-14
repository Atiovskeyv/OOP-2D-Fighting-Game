// ============================================================
//  IKeyboardBinding.cs
//  Namespace : FightingGame.Core.Interfaces
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  DESIGN PATTERN: Strategy Pattern
//  --------------------------------
//  Bu interface, input okuma davranışını soyutlar. Farklı tuş
//  şemaları (WASD, Arrow Keys, Gamepad) bu kontratı implement
//  ederek Player sınıfına enjekte edilir.
//
//  Player asla "WASD mi Arrow mu?" bilmez; sadece "bir input
//  kaynağı" bilir. Yeni input şeması eklemek için:
//    1. Bu interface'i implement eden yeni sınıf yaz
//    2. MatchManager'da atamayı değiştir
//    Player koduna dokunmaya gerek yok.
//
// ============================================================

using System;

namespace FightingGame.Core.Interfaces
{
    /// <summary>
    /// Input okuyan her şeyin sahip olması gereken metot sözleşmesi.
    /// <para>
    /// Strategy Pattern: Runtime'da farklı input kaynakları
    /// (WASD, Arrows, Gamepad) takılabilir.
    /// </para>
    /// </summary>
    public interface IKeyboardBinding : IDisposable
    {
        // ── Hareket ─────────────────────────────────────────────

        /// <summary>
        /// Yatay hareket değeri. -1 sol, +1 sağ.
        /// Continuous (her frame okunur).
        /// </summary>
        float GetHorizontal();

        // ── Anlık Aksiyonlar (WasPressedThisFrame) ──────────────

        /// <summary>Zıplama tuşuna basıldı mı? (tek frame)</summary>
        bool GetJump();

        /// <summary>Saldırı tuşuna basıldı mı? (tek frame)</summary>
        bool GetAttack();

        /// <summary>Dash tuşuna basıldı mı? (tek frame)</summary>
        bool GetDash();

        // ── Sürekli Basılı Aksiyonlar (IsPressed) ───────────────

        /// <summary>Blok tuşu basılı mı? (sürekli)</summary>
        bool GetBlock();

        /// <summary>Çömelme tuşu basılı mı? (sürekli)</summary>
        bool GetCrouch();

        // ── Skill Aksiyonları ───────────────────────────────────

        /// <summary>Q Skill tuşuna basıldı mı? (tek frame)</summary>
        bool GetSkill1();

        /// <summary>Ultimate tuşuna basıldı mı? (tek frame)</summary>
        bool GetSkill2();

        // ── Yaşam Döngüsü ──────────────────────────────────────

        /// <summary>Tüm InputAction'ları etkinleştirir.</summary>
        void Enable();

        /// <summary>Tüm InputAction'ları devre dışı bırakır.</summary>
        void Disable();

        // IDisposable.Dispose() zaten miras alınıyor.
        // InputAction nesnelerinin bellek temizliği için kullanılır.
    }
}
