// ============================================================
//  IDamageable.cs
//  Namespace : FightingGame.Core.Interfaces
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  DESIGN PRINCIPLES
//  -----------------
//  • Interface Segregation Principle (ISP): Bu arayüz yalnızca
//    hasar alma sorumluluğunu kapsar. Sağlık yönetimi, ölüm
//    mantığı ya da animasyon tetiklemeleri ayrı katmanlara
//    bırakılmıştır; böylece her bileşen tek bir amaca hizmet eder.
//
//  • Dependency Inversion Principle (DIP): Saldırı/hasar veren
//    sistemler (Projectile, MeleeHitbox, AoE vb.) bu somut
//    sınıflara değil, IDamageable soyutlamasına bağımlıdır.
//    Bu sayede yeni düşman türleri eklemek mevcut kodu bozmaz.
//
//  • Open/Closed Principle (OCP): Yeni bir hedef türü (boss,
//    araç, yıkılabilir nesne) eklemek için yalnızca bu arayüzü
//    implemente etmek yeterlidir; saldırı sistemi kapatılmış
//    (closed) olduğu için değiştirilmesine gerek kalmaz.
//
// ============================================================

namespace FightingGame.Core.Interfaces
{
    /// <summary>
    /// Hasar alabilen her varlığın uygulaması gereken temel sözleşme.
    /// <para>
    /// Implementasyon örnekleri:
    /// <list type="bullet">
    ///   <item><description>PlayerHealth : MonoBehaviour, IDamageable</description></item>
    ///   <item><description>EnemyHealth   : MonoBehaviour, IDamageable</description></item>
    ///   <item><description>BreakableObject : MonoBehaviour, IDamageable</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public interface IDamageable
    {
        // ── Temel Özellikler ────────────────────────────────────

        /// <summary>
        /// Varlığın mevcut can puanı. Sadece okunabilir (get).
        /// Sağlık yönetimi implementasyon sınıfının sorumluluğundadır.
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Varlığın maksimum can puanı. Sadece okunabilir (get).
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// Varlığın hâlâ hayatta/aktif olup olmadığını döner.
        /// CurrentHealth > 0 kontrolünü kapsüller; dışarıdan ham
        /// veri karşılaştırması yapılmasına gerek kalmaz.
        /// </summary>
        bool IsAlive { get; }

        // ── Temel Metot ─────────────────────────────────────────

        /// <summary>
        /// Varlığa belirtilen miktarda ham hasar uygular.
        /// <para>
        /// Implementasyon notları:
        /// <list type="bullet">
        ///   <item><description>
        ///     Zırh/direnç hesaplaması bu metot <b>içinde</b> yapılmalıdır;
        ///     çağıran kod ham hasar değerini geçer, son değeri bilmez.
        ///   </description></item>
        ///   <item><description>
        ///     <c>amount</c> &lt;= 0 ise erken çıkış yapılmalı, negatif
        ///     hasar ile iyileştirme yapılmamalıdır (bunun için ayrı
        ///     <c>IHealable</c> arayüzü kullanın).
        ///   </description></item>
        ///   <item><description>
        ///     Ölüm kontrolü bu metodun sonunda yapılmalı; gerekirse
        ///     <c>OnDeath</c> olayı fırlatılmalıdır.
        ///   </description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="amount">
        /// Uygulanacak ham hasar miktarı. Pozitif tam sayı olmalıdır.
        /// </param>
        void TakeDamage(int amount);
    }
}