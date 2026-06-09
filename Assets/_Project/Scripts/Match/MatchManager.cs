// ============================================================
//  MatchManager.cs
//  Namespace : FightingGame.Match
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Oyunun orkestratörü. GameManager'dan seçilen harita ve
//  karakter bilgilerini alır, prefab'ları dinamik olarak
//  spawn eder, Player + Binding + Character üçlüsünü bağlar.
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Interfaces;
using FightingGame.Input;
using FightingGame.Character;
using FightingGame.UI;

namespace FightingGame.Match
{
    /// <summary>
    /// Oyun orkestratörü — Karakterleri dinamik spawn eder, binding atar.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        // ── Inspector Alanları ───────────────────────────────────

        [Header("Health Bars")]
        [Tooltip("Player 1'in health bar UI'ı (sahnede Canvas altında olmalı).")]
        [SerializeField] private HealthBarUI healthBar1;

        [Tooltip("Player 2'nin health bar UI'ı (sahnede Canvas altında olmalı).")]
        [SerializeField] private HealthBarUI healthBar2;
        // ══════════════════════════════════════════════════════════
        //  Unity Lifecycle
        // ══════════════════════════════════════════════════════════

        private void Start()
        {
            if (GameManager.instance == null)
            {
                Debug.LogError("[MatchManager] GameManager bulunamadı! MainMenu sahnesinden başlatın.");
                return;
            }

            // Fallback: Sahnede atanmamış can barlarını otomatik bulmaya çalış
            if (healthBar1 == null || healthBar2 == null)
            {
                var foundHealthBars = FindObjectsOfType<HealthBarUI>();
                foreach (var hb in foundHealthBars)
                {
                    string nameLower = hb.name.ToLower();
                    if (nameLower.Contains("p1") || nameLower.Contains("player1") || nameLower.Contains("player 1") || nameLower.EndsWith("1"))
                    {
                        if (healthBar1 == null) healthBar1 = hb;
                    }
                    else if (nameLower.Contains("p2") || nameLower.Contains("player2") || nameLower.Contains("player 2") || nameLower.EndsWith("2"))
                    {
                        if (healthBar2 == null) healthBar2 = hb;
                    }
                }

                // Hala atanmamış varsa ve en az 2 tane bulunduysa sırayla eşleştir
                if (foundHealthBars.Length >= 2)
                {
                    if (healthBar1 == null) healthBar1 = foundHealthBars[0];
                    if (healthBar2 == null) healthBar2 = foundHealthBars[1];
                }
            }

            int p1Id = GameManager.instance.player1Character;
            int p2Id = GameManager.instance.player2Character;

            if (p1Id < 0 || p2Id < 0)
            {
                Debug.LogError("[MatchManager] Karakter seçimi yapılmamış!");
                return;
            }

            GameObject[] prefabs = GameManager.instance.characterPrefabs;
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError("[MatchManager] GameManager'da characterPrefabs atanmamış!");
                return;
            }

            if (p1Id >= prefabs.Length || prefabs[p1Id] == null)
            {
                Debug.LogError($"[MatchManager] Player 1 prefab bulunamadı! (id={p1Id})");
                return;
            }

            if (p2Id >= prefabs.Length || prefabs[p2Id] == null)
            {
                Debug.LogError($"[MatchManager] Player 2 prefab bulunamadı! (id={p2Id})");
                return;
            }

            // Spawn pozisyonlarını belirle
            GetSpawnPositions(out Vector3 p1Pos, out Quaternion p1Rot,
                              out Vector3 p2Pos, out Quaternion p2Rot);

            // Karakterleri spawn et
            GameObject p1Obj = Instantiate(prefabs[p1Id], p1Pos, p1Rot);
            GameObject p2Obj = Instantiate(prefabs[p2Id], p2Pos, p2Rot);

            p1Obj.name = "Player1_" + prefabs[p1Id].name;
            p2Obj.name = "Player2_" + prefabs[p2Id].name;

            Debug.Log($"[MatchManager] Player 1 spawn edildi: {p1Obj.name} @ {p1Pos}");
            Debug.Log($"[MatchManager] Player 2 spawn edildi: {p2Obj.name} @ {p2Pos}");

            // Player component'lerini bul ve initialize et
            InitializePlayer(p1Obj, p2Obj, isPlayer1: true);
            InitializePlayer(p2Obj, p1Obj, isPlayer1: false);

            // Health bar'ları spawn edilen karakterlere bağla
            BindHealthBars(p1Obj, p2Obj);

            // Kamera sınırlarını seçilen haritaya göre ayarla
            SetCameraBoundsForMap(GameManager.instance.selectedMap);

            // Sayacı ve oyun sonu mantığını başlat
            SetupTimer(p1Obj, p2Obj);

            Debug.Log("[MatchManager] Maç başladı!");
        }

        private void SetCameraBoundsForMap(string mapName)
        {
            if (UnityEngine.Camera.main != null)
            {
                var camFollow = UnityEngine.Camera.main.GetComponent<FightingGame.Core.CameraFollow>();
                if (camFollow != null)
                {
                    if (mapName == "Mezarlık")
                    {
                        camFollow.SetBounds(-15f, 15f, 0.5f, 8f);
                    }
                    else // MoodyNight (Büyülü Orman vb.)
                    {
                        // Karakterler X=50 civarında spawn olduğu için kameranın sınırlarını o bölgeye kaydır
                        camFollow.SetBounds(30f, 70f, 0.5f, 10f);
                    }
                }
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Spawn Pozisyonları
        // ══════════════════════════════════════════════════════════

        private void GetSpawnPositions(out Vector3 p1Pos, out Quaternion p1Rot,
                                       out Vector3 p2Pos, out Quaternion p2Rot)
        {
            string map = GameManager.instance.selectedMap;

            if (map == "Mezarlık")
            {
                p1Pos = new Vector3(-1f, 0f, -22f);
                p1Rot = Quaternion.Euler(0f, 90f, 0f);
                p2Pos = new Vector3(4f, 0f, -22f);
                p2Rot = Quaternion.Euler(0f, 90f, 0f);
            }
            else // MoodyNight (Büyülü Orman)
            {
                p1Pos = new Vector3(50f, 1.5f, 59f);
                p1Rot = Quaternion.Euler(0f, 90f, 0f);
                p2Pos = new Vector3(53f, 1.5f, 59f);
                p2Rot = Quaternion.Euler(0f, 90f, 0f);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Player Initialization
        // ══════════════════════════════════════════════════════════

        private void InitializePlayer(GameObject playerObj, GameObject opponentObj, bool isPlayer1)
        {
            // Set layers
            int myLayer = LayerMask.NameToLayer(isPlayer1 ? "Player" : "Enemy");
            int opponentLayer = LayerMask.NameToLayer(isPlayer1 ? "Enemy" : "Player");

            if (myLayer != -1)
            {
                SetLayerRecursively(playerObj, myLayer);
            }
            else
            {
                Debug.LogWarning($"[MatchManager] {(isPlayer1 ? "Player" : "Enemy")} layer is not defined in TagManager!");
            }

            // Set enemy layer mask on AbstractCharacter
            var absChar = playerObj.GetComponent<AbstractCharacter>();
            if (absChar != null && opponentLayer != -1)
            {
                absChar.SetEnemyLayer(1 << opponentLayer);
            }

            // Player component'ini bul
            var player = playerObj.GetComponent<Player.Player>();
            if (player == null)
            {
                Debug.LogWarning($"[MatchManager] {playerObj.name} üzerinde Player component'i bulunamadı. " +
                                 "Karakter sadece görsel olarak sahneye eklendi.");
                return;
            }

            // ICharacter component'ini bul
            ICharacter character = playerObj.GetComponent<ICharacter>();
            if (character == null)
            {
                Debug.LogWarning($"[MatchManager] {playerObj.name} üzerinde ICharacter bulunamadı. " +
                                 "Karakter sadece görsel olarak sahneye eklendi.");
                return;
            }

            // Binding ata: Player 1 → WASD, Player 2 → Arrow Keys
            IKeyboardBinding binding = isPlayer1
                ? (IKeyboardBinding)new KeyboardBindingWasd()
                : (IKeyboardBinding)new KeyboardBindingArrows();

            player.Initialize(binding, character);

            // Rakibi tanıt
            player.SetOpponent(opponentObj.transform);
        }

        // ══════════════════════════════════════════════════════════
        //  Health Bar Binding
        // ══════════════════════════════════════════════════════════

        private void BindHealthBars(GameObject p1Obj, GameObject p2Obj)
        {
            // Player 1 health bar
            if (healthBar1 != null)
            {
                var damageable1 = p1Obj.GetComponent<IDamageable>();
                if (damageable1 != null)
                    healthBar1.SetTarget(damageable1);
                else
                    Debug.LogWarning($"[MatchManager] {p1Obj.name} üzerinde IDamageable bulunamadı, health bar bağlanamadı.");
            }
            else
            {
                Debug.LogWarning("[MatchManager] healthBar1 atanmamış!");
            }

            // Player 2 health bar
            if (healthBar2 != null)
            {
                var damageable2 = p2Obj.GetComponent<IDamageable>();
                if (damageable2 != null)
                    healthBar2.SetTarget(damageable2);
                else
                    Debug.LogWarning($"[MatchManager] {p2Obj.name} üzerinde IDamageable bulunamadı, health bar bağlanamadı.");
            }
            else
            {
                Debug.LogWarning("[MatchManager] healthBar2 atanmamış!");
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        // ══════════════════════════════════════════════════════════
        //  Match Timer & Game End Logic
        // ══════════════════════════════════════════════════════════
        
        [Header("Match Settings")]
        [Tooltip("Maçın saniye cinsinden süresi.")]
        public float matchDuration = 90f;
        
        [Tooltip("Sayacın ve kazanan yazısının fontu (İsteğe bağlı). Maceracı/Büyülü fontunuzu buraya sürükleyin.")]
        public TMPro.TMP_FontAsset timerFont;
        
        private float _currentMatchTime;
        private bool _isMatchActive = false;
        private TMPro.TextMeshProUGUI _timerText;
        private RectTransform _timerRect;
        private AbstractCharacter _p1Character;
        private AbstractCharacter _p2Character;
        
        private void SetupTimer(GameObject p1Obj, GameObject p2Obj)
        {
            _p1Character = p1Obj.GetComponent<AbstractCharacter>();
            _p2Character = p2Obj.GetComponent<AbstractCharacter>();
            _currentMatchTime = matchDuration;
            
            // Sahnedeki Canvas'ı bul
            Canvas mainCanvas = null;
            if (healthBar1 != null) mainCanvas = healthBar1.GetComponentInParent<Canvas>();
            if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
            
            if (mainCanvas != null)
            {
                // Timer için dinamik TextMeshPro objesi oluştur
                GameObject timerObj = new GameObject("MatchTimerText");
                timerObj.transform.SetParent(mainCanvas.transform, false);
                
                _timerRect = timerObj.AddComponent<RectTransform>();
                _timerRect.anchorMin = new Vector2(0.5f, 1f);
                _timerRect.anchorMax = new Vector2(0.5f, 1f);
                _timerRect.pivot = new Vector2(0.5f, 1f);
                _timerRect.anchoredPosition = new Vector2(0f, -20f); // Üstten biraz boşluk
                _timerRect.sizeDelta = new Vector2(1200f, 150f); // Genişliği artırdık ki taşmasın
                
                _timerText = timerObj.AddComponent<TMPro.TextMeshProUGUI>();
                _timerText.alignment = TMPro.TextAlignmentOptions.Center;
                _timerText.enableAutoSizing = true;
                _timerText.fontSizeMin = 40f;
                _timerText.fontSizeMax = 80f; // Sayaç için maksimum 80
                _timerText.color = Color.white;
                _timerText.fontStyle = TMPro.FontStyles.Bold;
                _timerText.enableWordWrapping = true; // Taştığında alt satıra geçmesine izin ver
                _timerText.overflowMode = TMPro.TextOverflowModes.Truncate;
                
                if (timerFont != null)
                {
                    _timerText.font = timerFont;
                }
                
                _timerText.text = Mathf.CeilToInt(_currentMatchTime).ToString();
            }
            else
            {
                Debug.LogWarning("[MatchManager] Sahnede Canvas bulunamadığı için timer UI oluşturulamadı.");
            }
            
            _isMatchActive = true;
        }

        private void Update()
        {
            if (!_isMatchActive) return;

            // Süreyi azalt
            if (_currentMatchTime > 0)
            {
                _currentMatchTime -= Time.deltaTime;
                
                if (_timerText != null)
                {
                    int secondsLeft = Mathf.CeilToInt(_currentMatchTime);
                    _timerText.text = secondsLeft.ToString();
                    
                    if (secondsLeft <= 10)
                        _timerText.color = Color.red; // Son 10 saniye kırmızı yap
                }
                
                if (_currentMatchTime <= 0)
                {
                    _currentMatchTime = 0;
                    if (_timerText != null) _timerText.text = "0";
                    EndMatch(timeout: true);
                }
            }

            // Ölüm kontrolü
            if (_p1Character != null && !_p1Character.IsAlive)
            {
                EndMatch(timeout: false, winner: _p2Character);
            }
            else if (_p2Character != null && !_p2Character.IsAlive)
            {
                EndMatch(timeout: false, winner: _p1Character);
            }
        }

        private void EndMatch(bool timeout, AbstractCharacter winner = null)
        {
            _isMatchActive = false;

            if (timeout)
            {
                // Süre bitti, canı yüksek olan kazanır
                if (_p1Character != null && _p2Character != null)
                {
                    if (_p1Character.CurrentHealth > _p2Character.CurrentHealth)
                        winner = _p1Character;
                    else if (_p2Character.CurrentHealth > _p1Character.CurrentHealth)
                        winner = _p2Character;
                    // Eğer canlar eşitse winner null kalır (Berabere)
                }
            }

            if (_timerText != null)
            {
                _timerText.fontSizeMax = 120f; // Kazandı yazısı için maksimum boyutu artır
                
                if (_timerRect != null)
                {
                    // Yazıyı ekranın tam ortasına al ve alanını büyüt
                    _timerRect.anchorMin = new Vector2(0.5f, 0.5f);
                    _timerRect.anchorMax = new Vector2(0.5f, 0.5f);
                    _timerRect.pivot = new Vector2(0.5f, 0.5f);
                    _timerRect.anchoredPosition = Vector2.zero;
                    _timerRect.sizeDelta = new Vector2(1600f, 300f); // Ekrana sığması için devasa bir alan
                }

                if (winner != null)
                    _timerText.text = $"{winner.Data.characterName} KAZANDI!";
                else
                    _timerText.text = "BERABERE!";
            }

            Debug.Log($"[MatchManager] OYUN BİTTİ! " + (winner != null ? $"Kazanan: {winner.Data.characterName}" : "Berabere."));
            
            // Karakterlerin kontrolünü durdurmak için
            if (_p1Character != null && _p1Character.TryGetComponent<Player.Player>(out var p1Ctrl)) p1Ctrl.enabled = false;
            if (_p2Character != null && _p2Character.TryGetComponent<Player.Player>(out var p2Ctrl)) p2Ctrl.enabled = false;
            
            // TODO: Ana menüye dönüş veya rövanş paneli buraya eklenebilir.
            // Invoke(nameof(ReturnToMainMenu), 3f);
        }
    }
}
