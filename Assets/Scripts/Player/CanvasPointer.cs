
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasPointer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] XRRayInteractor xRRayInteractor;
    [SerializeField] LineRenderer lineRenderer;

    [Header("Ray settings")]
    public float raycastLength = 8.0f;
    [Space]
    [SerializeField] Material defaultEmptyMaterial;
    [SerializeField] Material defaultTargetedMaterial;

    [Header("Events")]
    public UnityEvent StartPoint;
    public UnityEvent StopPoint;


    // Internal variables      
    RaycastResult raycastResult;

    GameObject lastHoveringObject;
    private bool hover = false;

    void Start()
    {
        if (lineRenderer == null)
            gameObject.TryGetComponent<LineRenderer>(out lineRenderer);
        if (xRRayInteractor == null)
            gameObject.TryGetComponent<XRRayInteractor>(out xRRayInteractor);        
    }


    void Update()
    {
        xRRayInteractor.TryGetCurrentUIRaycastResult(out raycastResult);        

        if (raycastResult.gameObject == null)
        {
            StopHoveringUI();
            StopPoint?.Invoke();
            hover = false;
            lastHoveringObject = raycastResult.gameObject;
            return;
        }

        else 
        {
            
            StartHoveringUI();
            StartPoint?.Invoke();
            hover = true;
        }        

        if (hover)
        {
            Hovering();
        }

        lastHoveringObject = raycastResult.gameObject;
    }

    private void StartHoveringUI()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { transform.position, raycastResult.worldPosition });
        lineRenderer.material = defaultTargetedMaterial;
    }

    void StopHoveringUI()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);
        }
        lineRenderer.material = defaultEmptyMaterial;
    }

    void Hovering()
    {
        if (raycastResult.gameObject.tag == "Inventory")
        {
            raycastResult.gameObject.GetComponentInParent<InventorySystem>().InventoryIntersected(
                raycastResult.worldPosition,
                transform.GetComponentInParent<Input.Hand>());            
        }
    }
}