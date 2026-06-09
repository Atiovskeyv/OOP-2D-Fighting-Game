using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using FightingGame.UI;

public class SetupSpecialMeterTool
{
    [MenuItem("FightingGame/Setup Special Meter")]
    public static void SetupSpecialMeter()
    {
        HealthBarUI[] healthBars = Object.FindObjectsOfType<HealthBarUI>(true);
        if (healthBars.Length == 0)
        {
            Debug.LogWarning("Sahne'de HealthBarUI bulunamadı.");
            return;
        }

        // Find Sprites from exact paths and force them to be Sprites
        Sprite filledSprite = GetOrFixSprite("Assets/Bar/Filled_Yellow.png");
        Sprite emptySprite = GetOrFixSprite("Assets/Bar/Empty.png");

        if (filledSprite == null || emptySprite == null)
        {
            Debug.LogError("Filled veya Empty sprite'ları bulunamadı! 'Assets/Bar/Filled_Yellow.png' veya 'Assets/Bar/Empty.png' mevcut mu kontrol edin.");
            return;
        }

        foreach (var hb in healthBars)
        {
            SetupForHealthBar(hb, filledSprite, emptySprite);
        }

        Debug.Log($"[SetupSpecialMeterTool] {healthBars.Length} adet HealthBarUI başarıyla güncellendi!");
    }

    private static Sprite GetOrFixSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                Debug.Log($"[SetupSpecialMeter] '{path}' Sprite olarak ayarlandı.");
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SetupForHealthBar(HealthBarUI hb, Sprite filled, Sprite empty)
    {
        bool isP2 = hb.gameObject.name.Contains("P2");
        Transform container = hb.transform.Find("SpecialMeterContainer");
        if (container == null)
        {
            GameObject go = new GameObject("SpecialMeterContainer", typeof(RectTransform));
            go.transform.SetParent(hb.transform, false);
            container = go.transform;

            RectTransform rt = go.GetComponent<RectTransform>();
            
            rt.anchorMin = isP2 ? new Vector2(1, 0) : new Vector2(0, 0);
            rt.anchorMax = isP2 ? new Vector2(1, 0) : new Vector2(0, 0);
            rt.pivot = isP2 ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(0, -10f); // Healthbar'ın hemen altına
            rt.sizeDelta = new Vector2(150f, 40f); // Elle sürüklenebilecek bir alan
        }

        float spacing = 60f; // Yuvarlaklar arası mesafe (Araları daha açık)

        Image[] images = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            Transform segment = container.Find("Segment_" + i);
            if (segment == null)
            {
                GameObject segGo = new GameObject("Segment_" + i, typeof(RectTransform), typeof(Image));
                segGo.transform.SetParent(container, false);
                segment = segGo.transform;

                RectTransform srt = segGo.GetComponent<RectTransform>();
                srt.anchorMin = isP2 ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
                srt.anchorMax = isP2 ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
                srt.pivot = isP2 ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
                
                // P1 ise sağa doğru, P2 ise sola doğru diz
                float posX = isP2 ? (-i * spacing) : (i * spacing);
                srt.anchoredPosition = new Vector2(posX, 0f);
                
                srt.sizeDelta = new Vector2(40f, 40f); // Yuvarlakların boyutu biraz büyütüldü
            }
            Image img = segment.GetComponent<Image>();
            img.sprite = empty;
            img.preserveAspect = true; // Sıkışmayı engeller
            images[i] = img;
        }

        SerializedObject so = new SerializedObject(hb);
        so.FindProperty("filledSegmentSprite").objectReferenceValue = filled;
        so.FindProperty("emptySegmentSprite").objectReferenceValue = empty;
        so.FindProperty("filledSegmentColor").colorValue = Color.yellow;
        so.FindProperty("emptySegmentColor").colorValue = Color.white;
        
        SerializedProperty arrayProp = so.FindProperty("specialMeterImages");
        arrayProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(hb);
    }
}
