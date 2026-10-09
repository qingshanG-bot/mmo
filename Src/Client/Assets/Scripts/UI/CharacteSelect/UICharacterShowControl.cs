using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICharacterShowControl : MonoBehaviour, IDragHandler , IBeginDragHandler, IEndDragHandler
{

    public Transform target;

    private bool m_IsDragging = false;

    private float lastX;
    private float rotationSpeed = 150f;

   
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_IsDragging = true;
        lastX = eventData.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_IsDragging)
        {
            return;
        }

        float deltaX = eventData.position.x - lastX;
        target.Rotate(Vector3.up, -deltaX * rotationSpeed * Time.deltaTime, Space.World);
        lastX = eventData.position.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_IsDragging = false;
    }
}
