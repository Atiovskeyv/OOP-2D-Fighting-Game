// ============================================================
//  HealthBarUI.cs
//  Namespace : FightingGame.UI
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Bir oyuncunun health bar'ını yönetir.
//  MatchManager, karakter spawn ettikten sonra SetTarget() ile
//  IDamageable referansını atar. HealthBarUI her frame'de
//  CurrentHealth/MaxHealth okuyarak Fill nesnesinin anchor'ını günceller.
//
//  Yöntem: anchorMax.x  (Image.Type.Filled KULLANILMAZ — Sprite gerektirmez)
//  Fill (RectTransform) → anchorMax.x = fillRatio
//  FillMask üzerindeki RectMask2D zaten kırpar.
//
//  Hiyerarşi (iki seçenek — hangisi olursa çalışır):
//    HealthBarP1  ← Bu script burada
//      └─ FillMask  (RectMask2D)
//           └─ Fill  (Image, Simple) ← fillRect
//  VEYA:
//    HealthBarP1
//      └─ Fill  (Image, Simple) ← fillRect
//
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using FightingGame.Core.Interfaces;

namespace FightingGame.UI
{
    /// <summary>
    /// Health bar UI — IDamageable'dan can bilgisi okuyarak
    /// Fill nesnesinin RectTransform.anchorMax.x değerini günceller.
    /// Sprite gerekmez; RectMask2D ile kırpma yapılır.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        // ── Inspector Alanları ───────────────────────────────────

        [Header("UI References")]
        [Tooltip("Can çubuğunun dolu kısmını temsil eden Image bileşeni. " +
                 "FillMask/Fill veya direkt Fill nesnesi olabilir. " +
                 "Sprite atanmış olmasına gerek YOK.")]
        [SerializeField] private Image fillImage;

        [Header("Colors")]
        [Tooltip("Can yüksekken renk (yeşil).")]
        [SerializeField] private Color highHealthColor = new Color(0.18f, 0.78f, 0.18f, 1f);
        [Tooltip("Can düşükken renk (kırmızı).")]
        [SerializeField] private Color lowHealthColor  = new Color(0.85f, 0.08f, 0.08f, 1f);
        [Tooltip("Bu oranın altında bar tamamen kırmızı olur (0.4 = %40).")]
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.4f;

        [Header("Animation")]
        [Tooltip("Smooth lerp hızı. 0 = anlık.")]
        [SerializeField] private float smoothSpeed = 5f;

        [Header("Special Meter")]
        [Tooltip("Özel yetenek barının (Special Meter) segmentleri (yuvarlaklar). Soldan sağa sırayla atayın.")]
        [SerializeField] private Image[] specialMeterImages = new Image[3];
        [Tooltip("Dolu yuvarlak görseli (Filled.png)")]
        [SerializeField] private Sprite filledSegmentSprite;
        [Tooltip("Boş yuvarlak görseli (Empty.png)")]
        [SerializeField] private Sprite emptySegmentSprite;
        [Tooltip("Dolu yuvarlak rengi")]
        [SerializeField] private Color filledSegmentColor = Color.yellow;
        [Tooltip("Boş yuvarlak rengi")]
        [SerializeField] private Color emptySegmentColor = Color.white;

        // ── Dahili ──────────────────────────────────────────────

        private IDamageable  _damageable;
        private ICharacter   _character;
        private RectTransform _fillRect;
        private float         _targetFill  = 1f;
        private float         _currentFill = 1f;
        private int           _lastLoggedHealth = -1;

        // ── Public State — MatchManager / diğer sistemler okuyabilir ──

        /// <summary>
        /// Bağlı karakterin anlık can değeri. SetTarget() çağrılmadan önce 0 döner.
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Bağlı karakterin maksimum can değeri. SetTarget() çağrılmadan önce 0 döner.
        /// </summary>
        public int MaxHealth { get; private set; }

        // ══════════════════════════════════════════════════════════
        //  Unity Lifecycle
        // ══════════════════════════════════════════════════════════

        private void Awake()
        {
            // fillImage Inspector'dan atanmamışsa hiyerarşide ara
            if (fillImage == null)
            {
                Transform t = transform.Find("FillMask/Fill") ?? transform.Find("Fill");
                if (t != null) fillImage = t.GetComponent<Image>();
                if (fillImage == null) fillImage = GetComponentInChildren<Image>();

                if (fillImage == null)
                {
                    Debug.LogError("[HealthBarUI] Fill Image bulunamadı! " +
                                   "Inspector'dan 'fillImage' alanına Fill nesnesini atayın.", this);
                    return;
                }
            }

            // RectTransform referansını al
            _fillRect = fillImage.rectTransform;

            // Image Type → Simple (Sprite gerektirmez, RectMask2D kırpar)
            fillImage.type = Image.Type.Simple;

            // Tam dolu başlat
            SetFillAnchor(1f);
            fillImage.color = highHealthColor;

            Debug.Log($"[HealthBarUI] '{gameObject.name}' hazır. Fill: {fillImage.gameObject.name}", this);
        }

        private void Update()
        {
            if (_damageable == null || _fillRect == null) return;

            int maxHp     = _damageable.MaxHealth;
            int currentHp = _damageable.CurrentHealth;

            // Public property'leri her frame güncelle
            CurrentHealth = currentHp;
            MaxHealth     = maxHp;

            // Değişiklik logu
            if (currentHp != _lastLoggedHealth)
            {
                float ratio = maxHp > 0 ? (float)currentHp / maxHp : 0f;
                Debug.Log($"[HealthBarUI] '{gameObject.name}' Can: {currentHp}/{maxHp}  ({ratio:P0})", this);
                _lastLoggedHealth = currentHp;
            }

            _targetFill = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;

            // Smooth lerp — sadece bar genişliği animasyonu için
            _currentFill = smoothSpeed > 0f
                ? Mathf.Lerp(_currentFill, _targetFill, smoothSpeed * Time.deltaTime)
                : _targetFill;

            // Anchor güncelle — animasyonlu genişlik
            SetFillAnchor(_currentFill);

            // ── Renk geçişi ─────────────────────────────────────
            // ÖNEMLI: _currentFill değil, GERÇEK oran (_targetFill) kullanılır.
            // Smooth değer eşiği geçmeden önce renk değişmemesi sorununu çözer.
            // %40 üstü → yeşil (anında)
            // %0 – %40 → kırmızıya geçiş (anında, can düştüğü anda)
            if (_targetFill >= lowHealthThreshold)
            {
                fillImage.color = highHealthColor;
            }
            else if (_targetFill <= 0f)
            {
                fillImage.color = lowHealthColor;
            }
            else
            {
                // 0 → lowHealthColor (kırmızı), threshold → highHealthColor (yeşil)
                float t = _targetFill / lowHealthThreshold;
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, t);
            }

            if (_character != null)
            {
                UpdateSpecialMeterUI(_character.SpecialMeterSegments);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Public API — MatchManager tarafından çağrılır
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Spawn edilen karakterin IDamageable referansını atar.
        /// MatchManager, karakter spawn ettikten sonra bunu çağırır.
        /// </summary>
        public void SetTarget(IDamageable target)
        {
            _damageable  = target;
            _character   = target as ICharacter;
            _currentFill = 1f;
            _targetFill  = 1f;

            if (target != null)
            {
                _lastLoggedHealth = target.CurrentHealth;
                CurrentHealth     = target.CurrentHealth;
                MaxHealth         = target.MaxHealth;
            }
            else
            {
                CurrentHealth = 0;
                MaxHealth     = 0;
            }

            // UI'yı anında dolu konuma getir
            if (_fillRect != null)
            {
                SetFillAnchor(1f);
                if (fillImage != null) fillImage.color = highHealthColor;
            }

            Debug.Log($"[HealthBarUI] Hedef atandı: {(target as MonoBehaviour)?.name ?? "null"} | " +
                      $"Can: {CurrentHealth}/{MaxHealth}", this);
        }

        // ══════════════════════════════════════════════════════════
        //  Yardımcı
        // ══════════════════════════════════════════════════════════

        private int _lastLoggedSpecialSegments = -1;

        private void UpdateSpecialMeterUI(int segments)
        {
            if (specialMeterImages == null || specialMeterImages.Length == 0) return;
            if (segments == _lastLoggedSpecialSegments) return;

            _lastLoggedSpecialSegments = segments;

            for (int i = 0; i < specialMeterImages.Length; i++)
            {
                if (specialMeterImages[i] != null)
                {
                    bool isFilled = i < segments;
                    specialMeterImages[i].sprite = isFilled ? filledSegmentSprite : emptySegmentSprite;
                    specialMeterImages[i].color = isFilled ? filledSegmentColor : emptySegmentColor;
                }
            }
        }

        /// <summary>
        /// Fill nesnesinin anchorMax.x değerini ayarlayarak görsel genişliği değiştirir.
        /// anchorMin.x = 0 (sol kenar sabit), anchorMax.x = ratio (sağ kenar kayar).
        /// RectMask2D fazlayı otomatik kırpar.
        /// </summary>
        private void SetFillAnchor(float ratio)
        {
            if (_fillRect == null) return;
            Vector2 anchorMin = _fillRect.anchorMin;
            Vector2 anchorMax = _fillRect.anchorMax;
            anchorMin.x = 0f;
            anchorMax.x = Mathf.Clamp01(ratio);
            _fillRect.anchorMin = anchorMin;
            _fillRect.anchorMax = anchorMax;
        }
    }
}
