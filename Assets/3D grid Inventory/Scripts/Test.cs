using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class Test : MonoBehaviour
{
    [SerializeField]
    UIInputModule inputModule;

    private Vector2 pointerOffset;
    private RectTransform canvasRectTransform;
    private RectTransform panelRectTransform;

    void Awake()
    {


    }

    private void OnEnable()
    {
        if (inputModule != null)
        {
            inputModule.pointerEnter += OnDevicePoiterEnter;
            //inputModule.pointerExit += OnDevicePoiterExit;
            //
            inputModule.pointerClick += OnDevicePointerClick;
            inputModule.beginDrag += OnBeginDrag;
            inputModule.drag += OnDrag;
            //inputModule.endDrag += EndDrag;
           
        }
    }


    private void OnDisable()
    {
        if (inputModule != null)
        {
            inputModule.pointerEnter -= OnDevicePoiterEnter;
            //inputModule.pointerExit -= OnDevicePoiterExit;
            inputModule.pointerClick -= OnDevicePointerClick;
            inputModule.beginDrag -= OnBeginDrag;
            inputModule.drag -= OnDrag;
            //inputModule.endDrag -= EndDrag;
        }
    }

    private void OnDevicePoiterEnter(GameObject entered, PointerEventData pointerData)
    {
        //if (EventSystem.current.IsPointerOverGameObject(pointerData.pointerId))
        //    Debug.Log($"PointerEnter on {entered.name}", this);
    }

    private void OnDevicePoiterExit(GameObject exited, PointerEventData pointerData)
    {
        Debug.Log($"PointerExit from {exited.name}", this);
    }

    private void OnDevicePointerClick(GameObject selected, PointerEventData pointerData)
    {

        //Canvas canvas = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Canvas>();
        //if (canvas != null)
        //{
        //    canvasRectTransform = canvas.transform as RectTransform;
        //    panelRectTransform = transform.parent as RectTransform;
        //}

        if (EventSystem.current.IsPointerOverGameObject(pointerData.pointerId))
            Debug.Log($"Click on {selected.name}", this);
    }

    private void OnBeginDrag(GameObject selected, PointerEventData pointerData)
    {
        Debug.Log("BeginDrag ");
    }

    private void OnDrag(GameObject selected, PointerEventData pointerData)
    {
        Debug.Log("OnDrag ");
    }

    private void EndDrag(GameObject selected, PointerEventData pointerData)
    {
        Debug.Log($"EndDrag {EventSystem.current.currentSelectedGameObject}", this);
    }
}