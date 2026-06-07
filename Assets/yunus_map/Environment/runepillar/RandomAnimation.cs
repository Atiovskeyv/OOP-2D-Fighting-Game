using UnityEngine;
using System.Collections;

public partial class RandomAnimation : MonoBehaviour
{
    private Animator animator;
    public string animationName = "Armature|ArmatureAction"; // Animator'daki turuncu kutunun adı
    public float minWaitTime = 3f; // En az kaç saniye beklesin?
    public float maxWaitTime = 10f; // En fazla kaç saniye beklesin?

    void Start()
    {
        animator = GetComponent<Animator>();
        // Döngüyü başlat
        StartCoroutine(PlayRandomly());
    }

    IEnumerator PlayRandomly()
    {
        while (true)
        {
            // Belirlenen aralıkta rastgele bir süre bekle
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Animasyonu oynat
            if (animator != null)
            {
                animator.Play(animationName, 0, 0f);
            }
        }
    }
}