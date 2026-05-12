using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector3 pressedScale = new Vector3(0.95f, 0.95f, 1f);

    private Vector3 originalScale;

    private void Start() {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        transform.localScale = new Vector3(originalScale.x * pressedScale.x, originalScale.y * pressedScale.y, originalScale.z);
    }

    public void OnPointerUp(PointerEventData eventData) 
    {
        transform.localScale = originalScale;
    }
}
