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
    /// İki karakteri ortalar, mesafeye göre yaklaşır/uzaklaşır (Z ekseninde zoom yapar)
    /// ve harita sınırlarına göre kendini kısıtlar.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Eğer boş bırakılırsa sahnedeki oyuncuları otomatik olarak bulur.")]
        [SerializeField] private Transform player1;
        [SerializeField] private Transform player2;

        [Header("Movement Tuning")]
        [Tooltip("Kameranın karakterleri takip etme yumuşaklığı (Düşük değer = daha hızlı takip).")]
        [SerializeField] private float smoothTime = 0.15f;
        [Tooltip("Oyuncu merkez noktasına göre kameranın sahip olacağı sabit yükseklik ve başlangıç derinlik ofseti.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.8f, -5.5f);

        [Header("Zoom (Z Depth) Settings")]
        [Tooltip("Kameranın karakterlere yaklaşabileceği en yakın mesafe (Z derinliği).")]
        [SerializeField] private float minDistanceZ = 4f;
        [Tooltip("Kameranın karakterlerden uzaklaşabileceği en uzak mesafe (Z derinliği).")]
        [SerializeField] private float maxDistanceZ = 10f;
        [Tooltip("Karakterler arasındaki mesafenin kameranın derinliğine (Zoom) olan etki çarpanı.")]
        [SerializeField] private float zoomMultiplier = 0.4f;

        [Header("Camera Bounds (Harita Sınırları)")]
        [SerializeField] private float minX = -15f;
        [SerializeField] private float maxX = 15f;
        [SerializeField] private float minY = 0.5f;
        [SerializeField] private float maxY = 8f;

        // Dahili fizik değişkenleri
        private Vector3 _smoothVelocity;
        private bool _isInitialized;
        private UnityEngine.Camera _cam;
        private float _orthoZoomVelocity;

        private void Awake()
        {
            // Değerleri daha da yakın olacak şekilde güncelliyoruz (Yükseklik 2.0f'e çıkarıldı)
            offset = new Vector3(0f, 2.0f, -3.2f);
            minDistanceZ = 2.2f;
            maxDistanceZ = 12.0f; // Limit artırıldı (6.0f -> 12.0f)
            zoomMultiplier = 0.4f; // Oyuncular açıldıkça daha hızlı uzaklaşması için çarpan artırıldı (0.25f -> 0.4f)
        }

        private void Start()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            FindPlayers();
            ConfigureBoundsForCurrentScene();
        }

        private void ConfigureBoundsForCurrentScene()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Sahne ismine göre harita sınırlarını (kamera kelepçelerini) otomatik ayarlıyoruz
            if (sceneName == "Mezarlık" || sceneName == "Mezarlik" || sceneName == "SampleScene")
            {
                minX = -15f;
                maxX = 15f;
                minY = 0.5f;
                maxY = 8f;
            }
            else if (sceneName == "MoodyNight" || sceneName == "Büyülü Orman")
            {
                // Büyülü Orman (MoodyNight) haritasında oyuncular x=50 civarında spawn olmaktadır.
                minX = 35f;
                maxX = 70f;
                minY = 0.5f;
                maxY = 10f;
            }
        }

        private void LateUpdate()
        {
            // Eğer oyuncular henüz atanmadıysa veya sahneden silindiyse tekrar bulmayı dene
            if (player1 == null || player2 == null)
            {
                FindPlayers();
                if (player1 == null || player2 == null) return;
            }

            // 1. İki oyuncunun merkez noktasını bul
            Vector3 centerPoint = GetCenterPoint();

            // 2. Oyuncular arasındaki yatay mesafeyi hesapla
            float playerDistance = Mathf.Abs(player1.position.x - player2.position.x);

            // 3. Hedef X ve Y pozisyonlarını hesapla ve sınırlar içinde sınırla (Clamp)
            float targetX = Mathf.Clamp(centerPoint.x + offset.x, minX, maxX);
            float targetY = Mathf.Clamp(centerPoint.y + offset.y, minY, maxY);

            // 4. Oyuncuların mesafesine göre dinamik derinlik (Z ekseninde zoom) hesapla
            // offset.z negatif olduğu için (-12f gibi), mesafeyi çıkartarak daha da geriye gitmesini sağlıyoruz.
            float targetDepth = offset.z - (playerDistance * zoomMultiplier);
            targetDepth = Mathf.Clamp(-targetDepth, minDistanceZ, maxDistanceZ); // Derinliği sınırlar içinde tut

            // Oyuncuların ortalama Z pozisyonunu temel alarak hedef Z'yi ata (Map fark etmeksizin çalışması için)
            float targetZ = centerPoint.z - targetDepth;

            // 5. Hedef pozisyonu birleştir ve SmoothDamp ile kamerayı yumuşakça hareket ettir
            Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _smoothVelocity, smoothTime);

            // 6. Eğer kamera Orthographic (Ortografik) ise, sadece konumu değiştirmek yakınlaştırmaz.
            // Bu yüzden Size değerini oyuncuların mesafesine göre dinamik olarak değiştiriyoruz.
            if (_cam != null && _cam.orthographic)
            {
                // Uzaklaştıkça daha fazla genişlemesi için katsayıyı 0.35'e, limiti de 6.5'e çıkardım
                float targetOrthoSize = 1.6f + (playerDistance * 0.35f);
                targetOrthoSize = Mathf.Clamp(targetOrthoSize, 1.4f, 6.5f);
                _cam.orthographicSize = Mathf.SmoothDamp(_cam.orthographicSize, targetOrthoSize, ref _orthoZoomVelocity, smoothTime);
            }
        }

        /// <summary>
        /// İki oyuncunun dünya alanındaki merkez pozisyonunu döndürür.
        /// </summary>
        private Vector3 GetCenterPoint()
        {
            if (player1 == null || player2 == null) return Vector3.zero;
            return (player1.position + player2.position) / 2f;
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
