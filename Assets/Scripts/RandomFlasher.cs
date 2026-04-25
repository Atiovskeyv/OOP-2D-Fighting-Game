using System.Collections;
using UnityEngine;

public class RandomFlasher : MonoBehaviour
{
    private Light targetLight;
    
    [Header("Bekleme Süreleri (Saniye)")]
    [Tooltip("Işığın yanmadan önce bekleyeceği en kısa süre")]
    public float minWaitTime = 2f;
    [Tooltip("Işığın yanmadan önce bekleyeceği en uzun süre")]
    public float maxWaitTime = 8f;

    [Header("Parlama Süreleri (Saniye)")]
    [Tooltip("Işığın açık kalacağı en kısa süre")]
    public float minFlashDuration = 0.05f;
    [Tooltip("Işığın açık kalacağı en uzun süre")]
    public float maxFlashDuration = 0.2f;

    void Start()
    {
        // Objede bulunan Light bileşenini al
        targetLight = GetComponent<Light>();
        
        if (targetLight != null)
        {
            // Başlangıçta ışığı kapat ve rastgele yanıp sönme döngüsünü başlat
            targetLight.enabled = false;
            StartCoroutine(FlashRoutine());
        }
        else
        {
            Debug.LogError("RandomFlasher scripti çalışamadı! Lütfen bu scripti bir 'Light' objesinin üzerine attığınızdan emin olun.");
        }
    }

    IEnumerator FlashRoutine()
    {
        while (true)
        {
            // Belirlenen aralıkta rastgele bir süre bekle
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Işığı aç (Parlama)
            targetLight.enabled = true;

            // Flaşın ne kadar süreceğini belirle (çok kısa bir süre)
            float flashTime = Random.Range(minFlashDuration, maxFlashDuration);
            yield return new WaitForSeconds(flashTime);

            // Işığı geri kapat
            targetLight.enabled = false;
        }
    }
}
