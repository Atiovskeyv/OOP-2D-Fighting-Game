using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject mapPanel;
    public GameObject characterPanel;
    public GameObject settingsPanel;

    private bool player1Selected = false;
    private bool player2Selected = false;

    void Start()
    {
        CloseAllPanels();

        mainMenuPanel.SetActive(true);

        GameManager.instance.player1Character = -1;
        GameManager.instance.player2Character = -1;

    }

    // =========================
    // PANEL MANAGEMENT
    // =========================

    void CloseAllPanels()
    {
        mainMenuPanel.SetActive(false);
        mapPanel.SetActive(false);
        characterPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }


    // =========================
    // MAIN MENU
    // =========================

    public void StartGame()
    {
        CloseAllPanels();
        mapPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        settingsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // =========================
    // MAP SELECTION
    // =========================

    public void SelectMap1()
    {
        GameManager.instance.selectedMap = "Mezarlık";

        OpenCharacterSelection();

        Debug.Log("Map1 seçildi");
    }

    public void SelectMap2()
    {
        GameManager.instance.selectedMap = "MoodyNight";

        OpenCharacterSelection();

        Debug.Log("Map2 seçildi");
    }

    void OpenCharacterSelection()
    {
        CloseAllPanels();

        characterPanel.SetActive(true);

        player1Selected = false;
        player2Selected = false;

        GameManager.instance.player1Character = -1;
        GameManager.instance.player2Character = -1;
    }

    // =========================
    // CHARACTER SELECTION
    // =========================


    // PLAYER 1 CHARACTER

    public void SelectCharacterP1(int id)
    {
        GameManager.instance.player1Character = id;

        player1Selected = true;

        Debug.Log("P1 seçti: " + GetCharacterName(id));

        CheckBothPlayersReady();
    }

    // PLAYER 2 CHARACTER

    public void SelectCharacterP2(int id)
    {
        GameManager.instance.player2Character = id;

        player2Selected = true;

        Debug.Log("P2 seçti: " + GetCharacterName(id));

        CheckBothPlayersReady();
    }

    // CHECK READY

    void CheckBothPlayersReady()
    {
        if (player1Selected && player2Selected)
        {
            Debug.Log("İki oyuncu da karakter seçti!");
        }
    }

    // START MATCH

    public void StartMatch()
    {
        if (!player1Selected || !player2Selected)
        {
            Debug.Log("İki oyuncu da karakter seçmeli!");
            return;
        }

        Debug.Log("Maç Başlıyor!");

        Debug.Log("P1: " + GetCharacterName(GameManager.instance.player1Character));
        Debug.Log("P2: " + GetCharacterName(GameManager.instance.player2Character));

        SceneManager.LoadScene(GameManager.instance.selectedMap);
    }

    // =========================
    // CHARACTER NAME SYSTEM
    // =========================

    string GetCharacterName(int id)
    {
        switch (id)
        {
            case 0:
                return "Omni-man";

            case 1:
                return "Soldier Boy";

            case 2:
                return "Alber Wesker";

            case 3:
                return "Lara Croft";

            default:
                return "Unknown";
        }
    }

    // =========================
    // SETTINGS
    // =========================

    public void SetVolume(float value)
    {
        AudioManager.instance.SetVolume(value);
    }

    public void BackToMenu()
    {
        CloseAllPanels();

        mainMenuPanel.SetActive(true);
    }
}