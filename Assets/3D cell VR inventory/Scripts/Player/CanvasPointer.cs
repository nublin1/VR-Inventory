using Inventory;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class CanvasPointer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] XRRayInteractor xRRayInteractor;
    [SerializeField] LineRenderer lineRenderer;

    [Header("Ray settings")]
    [SerializeField] Material defaultEmptyMaterial;
    [SerializeField] Material defaultTargetedMaterial;

    // Internal variables      
    RaycastResult raycastResult;
    RaycastResult lastRaycastResult;
    private bool hover = false;

    void Start()
    {
        if (lineRenderer == null)
            gameObject.TryGetComponent<LineRenderer>(out lineRenderer);
        if (xRRayInteractor == null)
            gameObject.TryGetComponent<XRRayInteractor>(out xRRayInteractor);

        if (defaultEmptyMaterial == null)
            Resources.Load<Material>("Assets/Materials/Line/AHRed.mat");
        if (defaultEmptyMaterial == null)
            Resources.Load<Material>("Assets/Materials/Line/AHPointer 2.mat");
    }


    void Update()
    {
        xRRayInteractor.TryGetCurrentUIRaycastResult(out raycastResult);

        if (raycastResult.gameObject == null || Vector3.Dot(transform.forward, raycastResult.worldNormal) > 0)
            StopHoveringUI();
        else
            StartHoveringUI();

        if (hover)
        {
            Hovering();

            if (lastRaycastResult.gameObject == null)
            {
                lastRaycastResult = raycastResult;
                return;
            }

            if (lastRaycastResult.gameObject.GetInstanceID() != raycastResult.gameObject.GetInstanceID())
                StopHovering();

            lastRaycastResult = raycastResult;
        }
    }

    private void StartHoveringUI()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { transform.position, raycastResult.worldPosition });
        lineRenderer.material = defaultTargetedMaterial;

        hover = true;
    }

    void StopHoveringUI()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);
        }
        lineRenderer.material = defaultEmptyMaterial;

        hover = false;
        StopHovering();
        lastRaycastResult = raycastResult;
    }

    void Hovering()
    {
        if (raycastResult.gameObject.CompareTag("Inventory"))
        {
            raycastResult.gameObject.GetComponentInParent<InventorySystem>().InventoryIntersected(
                raycastResult.gameObject.GetComponentInParent<InventoryCellObject>().CellCoord,
                transform.GetComponentInParent<Input.Hand>());
        }
    }

    void StopHovering()
    {
        if (lastRaycastResult.gameObject != null && lastRaycastResult.gameObject.CompareTag("Inventory"))
        {
            lastRaycastResult.gameObject.GetComponentInParent<InventorySystem>().StopIntersected(
               lastRaycastResult.gameObject.GetComponentInParent<InventoryCellObject>().CellCoord);
        }
    }
}