using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject mapPanel;
    public GameObject characterPanel;
    public GameObject settingsPanel;

    [Header("Character Selection Panels")]
    public GameObject character1Panel;
    public GameObject character2Panel;

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

        character1Panel.SetActive(true);
        character2Panel.SetActive(true);

        player1Selected = false;
        player2Selected = false;

        GameManager.instance.player1Character = -1;
        GameManager.instance.player2Character = -1;
    }

    // =========================
    // CHARACTER SELECTION
    // =========================

    // PLAYER 1
    public void SelectCharacterP1(int id)
    {
        GameManager.instance.player1Character = id;

        player1Selected = true;

        Debug.Log("P1 seçti: " + id);

        CheckBothPlayersReady();
    }

    // PLAYER 2
    public void SelectCharacterP2(int id)
    {
        GameManager.instance.player2Character = id;

        player2Selected = true;

        Debug.Log("P2 seçti: " + id);

        CheckBothPlayersReady();
    }

    void CheckBothPlayersReady()
    {
        if (player1Selected && player2Selected)
        {
            Debug.Log("İki oyuncu da karakter seçti!");
        }
    }

    public void StartMatch()
    {
        if (!player1Selected || !player2Selected)
        {
            Debug.Log("İki oyuncu da karakter seçmeli!");
            return;
        }

        SceneManager.LoadScene(GameManager.instance.selectedMap);
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