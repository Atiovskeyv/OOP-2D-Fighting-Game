using UnityEngine;
using System.Collections;

public class BatRandomSpawner : MonoBehaviour
{
    private ParticleSystem _batSystem;
    public float minWait = 10f; // Minimum bekleme
    public float maxWait = 25f; // Maksimum bekleme

    void Start()
    {
        _batSystem = GetComponent<ParticleSystem>();
        
        // İlk başta durduğundan emin olalım
        _batSystem.Stop();
        
        // Döngüyü başlat
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Rastgele süre bekle
            float randomWait = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(randomWait);

            // Sürüyü başlat (Sistem Duration süresi kadar çalışıp duracak)
            _batSystem.Play();

            // Debug için Console'a yazdıralım (Çalıştığını görmek için)
            Debug.Log("Yarasa sürüsü fırlatıldı!");

            // Sistemin süresi (Duration) bitene kadar burada bekleyelim ki 
            // üst üste binmesin (Opsiyonel)
            yield return new WaitForSeconds(_batSystem.main.duration);
        }
    }
}