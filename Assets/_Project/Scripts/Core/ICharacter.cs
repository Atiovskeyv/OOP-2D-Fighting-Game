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
        /// <param name="direction">
        /// -1f (sol) ile +1f (sağ) arasında yatay hareket değeri.
        /// </param>
        void Walk(float direction);

        /// <summary>
        /// Karakteri zıplatır. Yerdeyken çağrılmalıdır.
        /// </summary>
        void Jump();

        /// <summary>
        /// Temel saldırıyı tetikler. Hitbox kontrolü ve
        /// animator trigger'ı bu metodun içinde yönetilir.
        /// </summary>
        void Attack();

        /// <summary>
        /// Blok durumunu açar/kapatır.
        /// </summary>
        /// <param name="active">true = blok başla, false = blok bırak</param>
        void Block(bool active);

        /// <summary>
        /// Çömelme durumunu açar/kapatır.
        /// </summary>
        /// <param name="active">true = çömel, false = kalk</param>
        void Crouch(bool active);

        /// <summary>
        /// Belirtilen yöne dash yapar.
        /// </summary>
        /// <param name="direction">
        /// Dash yönü. 0 ise karakterin baktığı yöne dash yapılır.
        /// </param>
        void Dash(float direction);

        // ── Karaktere Özel Yetenekler ───────────────────────────

        /// <summary>
        /// Q Skill — Her karakterin kendine özgü birinci yeteneği.
        /// </summary>
        void Skill1();

        /// <summary>
        /// Ultimate — Her karakterin kendine özgü ikinci (ulti) yeteneği.
        /// </summary>
        void Skill2();

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
        /// Karakter hayatta mı?
        /// </summary>
        bool IsAlive { get; }
    }
}
