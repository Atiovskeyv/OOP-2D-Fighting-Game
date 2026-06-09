using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private Transform currentTarget;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null) return;

        Transform target = eventData.pointerEnter.transform;
        
        if (!originalScales.ContainsKey(target))
        {
            originalScales[target] = target.localScale;
        }

        target.localScale = originalScales[target] * 1.1f;
        currentTarget = target;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentTarget != null && originalScales.ContainsKey(currentTarget))
        {
            currentTarget.localScale = originalScales[currentTarget];
            currentTarget = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentTarget != null && originalScales.ContainsKey(currentTarget))
        {
            currentTarget.localScale = originalScales[currentTarget] * 0.9f;
            Invoke("ResetScale", 0.1f);
        }
    }

    void ResetScale()
    {
        if (currentTarget != null && originalScales.ContainsKey(currentTarget))
        {
            currentTarget.localScale = originalScales[currentTarget];
        }
    }
}
