// ============================================================
//  MatchManager.cs
//  Namespace : FightingGame.Match
//  Unity 2022.3 | URP | 2.5D Fighting Game Architecture
// ============================================================
//
//  Oyunun orkestratörü. Player'ları oluşturur, her birine
//  binding ve character atar, rakipleri birbirine tanıtır.
//
//  Sorumluluklar:
//    • Player + Binding + Character üçlüsünü bağlamak
//    • Rakipleri birbirine tanıtmak (SetOpponent)
//    • Maç akışı yönetimi (başlat/bitir)
//
// ============================================================

using UnityEngine;
using FightingGame.Core.Interfaces;
using FightingGame.Input;
using FightingGame.Character;

namespace FightingGame.Match
{
    /// <summary>
    /// Oyun orkestratörü — Player'ları hazırlar, binding ve character atar.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        // ── Inspector Alanları ───────────────────────────────────

        [Header("Players")]
        [Tooltip("Player 1 GameObject'i (üzerinde Player.cs component'i olmalı).")]
        [SerializeField] private Player.Player player1;

        [Tooltip("Player 2 GameObject'i (üzerinde Player.cs component'i olmalı).")]
        [SerializeField] private Player.Player player2;

        // ══════════════════════════════════════════════════════════
        //  Unity Lifecycle
        // ══════════════════════════════════════════════════════════

        private void Start()
        {
            InitializePlayers();
            SetOpponents();

            Debug.Log("[MatchManager] Maç başladı!");
        }

        // ══════════════════════════════════════════════════════════
        //  Initialization
        // ══════════════════════════════════════════════════════════

        private void InitializePlayers()
        {
            // Player 1 — WASD binding + prefab'daki karakter (Omniman/Guts/vs.)
            if (player1 != null)
            {
                ICharacter character1 = player1.GetComponentInChildren<ICharacter>();
                if (character1 != null)
                {
                    player1.Initialize(new KeyboardBindingWasd(), character1);
                }
                else
                {
                    Debug.LogError("[MatchManager] Player 1 üzerinde ICharacter bulunamadı!", player1);
                }
            }

            // Player 2 — Arrow binding + prefab'daki karakter
            if (player2 != null)
            {
                ICharacter character2 = player2.GetComponentInChildren<ICharacter>();
                if (character2 != null)
                {
                    player2.Initialize(new KeyboardBindingArrows(), character2);
                }
                else
                {
                    Debug.LogError("[MatchManager] Player 2 üzerinde ICharacter bulunamadı!", player2);
                }
            }
        }

        private void SetOpponents()
        {
            // Rakipleri birbirine tanıt
            if (player1 != null && player2 != null)
            {
                player1.SetOpponent(player2.transform);
                player2.SetOpponent(player1.transform);
            }
        }
    }
}
