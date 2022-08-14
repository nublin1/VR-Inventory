using System.Collections;
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

            xRInteractionManager = FindObjectOfType<XRInteractionManager>();
        }

        private void OnEnable()
        {
            xRRayInteractor.selectEntered.AddListener(OnSelectEntering);
            xRRayInteractor.selectExited.AddListener(OnSelectExiting);
        }

        private void OnDisable()
        {
            xRRayInteractor.selectEntered.RemoveListener(OnSelectEntering);
            xRRayInteractor.selectExited.RemoveListener(OnSelectExiting);
        }

        private void OnSelectEntering(SelectEnterEventArgs args)
        {
            objectInHand = args.interactableObject.transform;
        }

        private void OnSelectExiting(SelectExitEventArgs args)
        {
            objectInHand = null;
            lastobjectInHand = args.interactableObject.transform;
            StartCoroutine(DelayBeforeClear());            
        }

        IEnumerator DelayBeforeClear()
        {
            yield return new WaitForSeconds(0.15f); 
            lastobjectInHand = null;
        }

    }
}