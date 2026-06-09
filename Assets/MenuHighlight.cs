using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuHighlight : MonoBehaviour
{
    public TextMeshProUGUI[] menuItems;

    int selectedIndex = 0;

    void Start()
    {
        UpdateVisual();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0)
                selectedIndex = menuItems.Length - 1;

            UpdateVisual();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= menuItems.Length)
                selectedIndex = 0;

            UpdateVisual();
        }

        // 🔥 ENTER ile seçim yap
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectOption();
        }
    }

    void UpdateVisual()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == selectedIndex)
            {
                menuItems[i].color = Color.yellow;
                menuItems[i].transform.localScale = Vector3.one * 1.2f;
            }
            else
            {
                menuItems[i].color = Color.white;
                menuItems[i].transform.localScale = Vector3.one;
            }
        }
    }

    void SelectOption()
    {
        // 🎯 Örnek mapping (index'e göre)
        switch (selectedIndex)
        {
            // MAP SEÇİMİ
            case 0:
                PlayerPrefs.SetInt("Map", 0);
                Debug.Log("Map1 seçildi");
                break;

            case 1:
                PlayerPrefs.SetInt("Map", 1);
                Debug.Log("Map2 seçildi");
                break;

            // CHARACTER SEÇİMİ
            case 2:
                PlayerPrefs.SetInt("Character", 0);
                Debug.Log("Character1 seçildi");
                break;

            case 3:
                PlayerPrefs.SetInt("Character", 1);
                Debug.Log("Character2 seçildi");
                break;
        }

        // 🚀 OYUNU BAŞLAT (örnek)
        if (PlayerPrefs.HasKey("Map") && PlayerPrefs.HasKey("Character"))
        {
            int map = PlayerPrefs.GetInt("Map");

            if (map == 0)
                SceneManager.LoadScene("Map1");
            else
                SceneManager.LoadScene("Map2");
        }
    }
}