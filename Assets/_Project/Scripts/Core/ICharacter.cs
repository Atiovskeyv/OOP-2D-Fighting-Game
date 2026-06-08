// ============================================================
//  ICharacter.cs
//  Namespace : FightingGame.Core.Interfaces
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  DESIGN PRINCIPLES
//  -----------------
//  • Interface Segregation Principle (ISP): ICharacter yalnızca
//    bir savaş karakterinin yapabileceği aksiyonları tanımlar.
//    Hasar alma sorumluluğu IDamageable'a bırakılmıştır.
//
//  • Dependency Inversion Principle (DIP): Player sınıfı somut
//    karakter tiplerine (Omniman, Guts, Homelander) değil, bu
//    interface'e bağımlıdır. Yeni karakter eklemek Player kodunu
//    değiştirmeyi gerektirmez.
//
//  • Open/Closed Principle (OCP): Yeni karakter eklemek için
//    sadece AbstractCharacter'dan türeyen yeni bir sınıf
//    oluşturmak ve Skill1/Skill2'yi doldurmak yeterlidir.
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Data;

namespace FightingGame.Core.Interfaces
{
    /// <summary>
    /// Savaşabilen her karakterin uygulaması gereken aksiyon sözleşmesi.
    /// <para>
    /// Player sınıfı bu interface üzerinden karakterle konuşur.
    /// "Omniman mı Guts mu?" bilmez; sadece "yürüyebilen, zıplayabilen,
    /// saldırabilen, skill kullanan bir şey" bilir.
    /// </para>
    /// </summary>
    public interface ICharacter
    {
        // ── Ortak Aksiyonlar ────────────────────────────────────

        /// <summary>
        /// Karakteri yatay eksende hareket ettirir.
        /// </summary>
        void Walk(float direction);

        /// <summary>
        /// Karakteri zıplatır.
        /// </summary>
        void Jump();

        /// <summary>
        /// Yumruk (Punch) saldırısını tetikler.
        /// </summary>
        void Punch();

        /// <summary>
        /// Tekme (Kick) saldırısını tetikler.
        /// </summary>
        void Kick();

        /// <summary>
        /// Ateş (Shoot) saldırısını tetikler.
        /// </summary>
        void Shoot();

        /// <summary>
        /// Blok durumunu açar/kapatır.
        /// </summary>
        void Block(bool active);

        /// <summary>
        /// Çömelme durumunu açar/kapatır.
        /// </summary>
        void Crouch(bool active);

        /// <summary>
        /// Belirtilen yöne dash yapar.
        /// </summary>
        void Dash(float direction);

        // ── Özel Yetenekler (Bar Harcayan) ──────────────────────

        /// <summary>
        /// Özel Yetenek 1 (Enhanced Shoot) - 1 Bar harcar.
        /// </summary>
        void ExecuteSkill1();

        /// <summary>
        /// Özel Yetenek 2 (Combo Breaker) - 2 Bar harcar.
        /// </summary>
        void ExecuteSkill2();

        /// <summary>
        /// Özel Yetenek 3 (Ultimate/X-Ray) - 3 Bar harcar.
        /// </summary>
        void ExecuteSkill3();

        // ── Rakip Yönetimi ──────────────────────────────────────

        /// <summary>
        /// Rakibin transform referansını set eder.
        /// </summary>
        void SetOpponent(Transform opponentTransform);

        // ── Veri Erişimi ────────────────────────────────────────

        /// <summary>
        /// Karakter stat verileri (HP, hız, hasar vs.)
        /// </summary>
        CharacterData Data { get; }

        /// <summary>
        /// Karakterin mevcut can puanı.
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Karakterin mevcut Özel Bar (Special Meter) değeri. 0 - 300.
        /// </summary>
        float SpecialMeter { get; }

        /// <summary>
        /// Özel Bar kademe sayısı. 0 - 3.
        /// </summary>
        int SpecialMeterSegments { get; }

        /// <summary>
        /// Karakter hayatta mı?
        /// </summary>
        bool IsAlive { get; }
    }
}
