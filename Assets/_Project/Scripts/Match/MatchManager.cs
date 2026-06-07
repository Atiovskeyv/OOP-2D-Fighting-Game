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

namespace FightingGame.Match
{
    /// <summary>
    /// Oyun orkestratörü — Karakterleri dinamik spawn eder, binding atar.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
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

            Debug.Log("[MatchManager] Maç başladı!");
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

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
