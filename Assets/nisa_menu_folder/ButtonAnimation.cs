using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.localScale = originalScale * 0.9f;

        Invoke("ResetScale", 0.1f);
    }

    void ResetScale()
    {
        transform.localScale = originalScale;
    }
}
