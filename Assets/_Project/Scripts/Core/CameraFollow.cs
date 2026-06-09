// ============================================================
//  CameraFollow.cs
//  Namespace : FightingGame.Core
//  Unity 2022.3 | URP | 2.5D Fighting Game Camera System
// ============================================================

using UnityEngine;
using FightingGame.Character;

namespace FightingGame.Core
{
    /// <summary>
    /// 2.5D Dövüş oyunu kamera sistemi.
    /// İki karakteri merkeze alır, mesafeye göre FOV-tabanlı çerçeveleme yapar.
    /// Yakın dövüşte sabit zoom, uzakta ise oyuncuları ekran kenarlarına hizalar.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Eğer boş bırakılırsa sahnedeki oyuncuları otomatik olarak bulur.")]
        [SerializeField] private Transform player1;
        [SerializeField] private Transform player2;

        [Header("Movement Tuning")]
        [Tooltip("Kameranın karakterleri takip etme yumuşaklığı (Düşük = daha hızlı).")]
        [SerializeField] private float smoothTime = 0.15f;
        [Tooltip("Merkez noktasına göre kamera ofseti (X: yatay, Y: dikey). Z kullanılmaz, zoom otomatik hesaplanır.")]
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);

        [Header("Framing (Çerçeveleme)")]
        [Tooltip("Ekran kenar boşluğu oranı (0-1). Oyuncular ekranın ne kadar içinde kalacak. 0.12 = her kenardan %12 boşluk.")]
        [SerializeField][Range(0.05f, 0.4f)] private float screenEdgePadding = 0.12f;

        [Header("Close Combat (Yakın Dövüş)")]
        [Tooltip("Kameranın oyunculara yaklaşabileceği minimum Z mesafesi. Oyuncular çok yakınlaşınca aşırı zoom-in'i önler.")]
        [SerializeField] private float closeCombatDistance = 3.5f;

        [Header("Zoom Limits")]
        [Tooltip("Kameranın oyunculara olabileceği en yakın Z mesafesi.")]
        [SerializeField] private float minZoomDistance = 4f;
        [Tooltip("Kameranın oyunculardan olabileceği en uzak Z mesafesi.")]
        [SerializeField] private float maxZoomDistance = 12f;

        [Header("Height Tuning")]
        [Tooltip("Kamera uzaklaştıkça (zoom out) kazanacağı ekstra yükseklik çarpanı. Dövüş açısını korur.")]
        [SerializeField] private float heightMultiplier = 0.1f;

        [Header("Camera Bounds (Harita Sınırları)")]
        [SerializeField] private float minX = -15f;
        [SerializeField] private float maxX = 15f;
        [SerializeField] private float minY = 0.5f;
        [SerializeField] private float maxY = 8f;

        // Dahili değişkenler
        private Vector3 _smoothVelocity;
        private UnityEngine.Camera _cam;
        private float _orthoZoomVelocity;

        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        private void Start()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            FindPlayers();
        }

        private void LateUpdate()
        {
            if (player1 == null || player2 == null)
            {
                FindPlayers();
                if (player1 == null || player2 == null) return;
            }

            // 1. İki oyuncunun merkez noktası
            Vector3 centerPoint = (player1.position + player2.position) * 0.5f;

            // 2. Oyuncular arasındaki yatay mesafe
            float playerDistance = Mathf.Abs(player1.position.x - player2.position.x);

            // 3. Hedef Z mesafesini hesapla
            float targetDepth = CalculateTargetDepth(playerDistance);

            // 4. Hedef pozisyonu oluştur
            float extraHeight = (targetDepth - minZoomDistance) * heightMultiplier;
            float targetX = Mathf.Clamp(centerPoint.x + offset.x, minX, maxX);
            float targetY = Mathf.Clamp(centerPoint.y + offset.y + extraHeight, minY, maxY);
            float targetZ = centerPoint.z - targetDepth;

            Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);

            // 5. SmoothDamp ile yumuşak takip
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPosition, ref _smoothVelocity, smoothTime);

            // 6. Ortografik kamera desteği (opsiyonel)
            if (_cam.orthographic)
            {
                UpdateOrthographicZoom(playerDistance);
            }
        }

        /// <summary>
        /// Perspective kamera için FOV ve aspect ratio'ya göre her iki oyuncuyu
        /// ekrana sığdıracak Z mesafesini hesaplar.
        /// closeCombatDistance bir taban sınır görevi görür: kamera bundan daha yakına gidemez.
        /// </summary>
        private float CalculateTargetDepth(float playerDistance)
        {
            // Horizontal FOV = 2 * atan(tan(vFOV/2) * aspect)
            float vFovRad = _cam.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float hFovRad = Mathf.Atan(Mathf.Tan(vFovRad) * _cam.aspect);

            // Ekranın kullanılabilir genişliği (padding çıkarılmış)
            float usableFraction = 1f - 2f * screenEdgePadding;
            float requiredWidth = playerDistance / Mathf.Max(usableFraction, 0.1f);

            // Bu genişliği görebilmek için gereken Z mesafesi
            float framingDepth = requiredWidth / (2f * Mathf.Tan(hFovRad));

            // Taban sınır: oyuncular çok yakınlaşınca framingDepth çok küçülür,
            // closeCombatDistance ile aşırı zoom-in'i engelle
            float targetDepth = Mathf.Max(framingDepth, closeCombatDistance);

            return Mathf.Clamp(targetDepth, minZoomDistance, maxZoomDistance);
        }

        /// <summary>
        /// Ortografik kamera için orthographicSize'ı dinamik olarak ayarlar.
        /// </summary>
        private void UpdateOrthographicZoom(float playerDistance)
        {
            float minOrthoSize = 2.5f;
            float usableFraction = 1f - 2f * screenEdgePadding;
            float framingOrthoSize = (playerDistance / usableFraction) / (2f * _cam.aspect);

            // Taban sınır: aşırı zoom-in'i engelle
            float targetSize = Mathf.Max(framingOrthoSize, minOrthoSize);
            targetSize = Mathf.Clamp(targetSize, 2f, 6f);

            _cam.orthographicSize = Mathf.SmoothDamp(
                _cam.orthographicSize, targetSize, ref _orthoZoomVelocity, smoothTime);
        }

        /// <summary>
        /// Sahnedeki AbstractCharacter bileşenine sahip iki karakteri otomatik bulur.
        /// </summary>
        private void FindPlayers()
        {
            var characters = FindObjectsOfType<AbstractCharacter>();
            if (characters == null || characters.Length < 2) return;

            // İsim kontrolüne göre Player 1 ve Player 2'yi ayır
            foreach (var charComponent in characters)
            {
                if (charComponent.gameObject.name.Contains("Player1"))
                {
                    player1 = charComponent.transform;
                }
                else if (charComponent.gameObject.name.Contains("Player2"))
                {
                    player2 = charComponent.transform;
                }
            }

            // Eğer isimlendirmeden bulunamadıysa ilk iki karakteri ata
            if (player1 == null && characters.Length > 0) player1 = characters[0].transform;
            if (player2 == null && characters.Length > 1) player2 = characters[1].transform;
        }
    }
}
