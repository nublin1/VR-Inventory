using Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class HeaderGrab : XRBaseInteractable
{
    private Vector3 pointerOffset;
    private RectTransform canvasRectTransform;
    private float distance;

    private Canvas canvas;
    private Hand hand;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.transform as RectTransform;            
            
            hand = args.interactorObject.transform.GetComponentInParent<Hand>();
            args.interactorObject.transform.GetComponent<XRRayInteractor>().TryGetCurrentUIRaycastResult(out RaycastResult raycastResult);

            pointerOffset = raycastResult.worldPosition - canvasRectTransform.position;
            distance = raycastResult.distance;           
        }
       
        base.OnSelectEntered(args);
    }

    private void Update()
    {
        if (canvas != null)
        {
            if (hand.Controller.selectAction.action.IsPressed())
            {
                canvasRectTransform.position = hand.transform.position - pointerOffset + hand.transform.forward * distance;
                Vector3 rotateDir = (canvasRectTransform.position - hand.transform.position).normalized;
                canvasRectTransform.forward = rotateDir;                
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        ClearData();
        base.OnSelectExited(args);
    }   

    private void ClearData()
    {
        canvas = null;
        hand = null;
    }
}
