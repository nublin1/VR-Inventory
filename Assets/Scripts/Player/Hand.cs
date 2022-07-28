using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Input
{
    public class Hand : MonoBehaviour
    {        
        [Header("References")]
        [SerializeField] ActionBasedController controller;
        [SerializeField] XRRayInteractor xRRayInteractor;
        [SerializeField] XRDirectInteractor xRDirectInteractor;

        Transform objectInHand;
        Transform lastobjectInHand;

        XRInteractionManager xRInteractionManager;

        public ActionBasedController Controller { get => controller; }
        public XRRayInteractor XRRayInteractor { get => xRRayInteractor; }
        public XRDirectInteractor XRDirectInteractor { get => xRDirectInteractor; }
        public Transform ObjectInHand { get => objectInHand; }
        public Transform LastObjectInHand { get => lastobjectInHand; set => objectInHand = value; }

        public XRInteractionManager XRInteractionManager { get => xRInteractionManager; }

        private void Start()
        {
            if (controller == null)
                controller = GetComponent<ActionBasedController>();
            if (xRRayInteractor == null)
                gameObject.GetComponentInChildren<XRRayInteractor>();

            xRInteractionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();

        }

        private void Update()
        {
            updateObjectsInHand();
            
        }

        public void updateObjectsInHand()
        {
            if (xRRayInteractor.firstInteractableSelected != null)
            {
                objectInHand = xRRayInteractor.firstInteractableSelected.transform;

                if (lastobjectInHand != objectInHand)
                {
                    lastobjectInHand = objectInHand;
                }
            }
            else
            {
                if (objectInHand == null)
                    lastobjectInHand = objectInHand;
                objectInHand = null;
            }
        }
    }
}