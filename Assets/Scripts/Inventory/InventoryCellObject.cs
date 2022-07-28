using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class InventoryCellObject
    {
        private GridXY _grid;
        private readonly int x;
        private readonly int y;
        private Transform spawnPoint;
        private Transform frameText;
        private Transform borderImage;

        private GameObject visual;

        private List<Shader> originalShaders = new List<Shader>();
        private Vector3 originalScale = Vector3.zero;

        public GridXY Grid { get => _grid; }
        public Transform SpawnPoint { get => spawnPoint; set => spawnPoint = value; }
        public Transform FrameText { get => frameText; set => frameText = value; }
        public Transform BorderImage { get => borderImage; set => borderImage = value; }

        public InventoryCellObject(GridXY grid, int _x, int _y, Transform cell)
        {
            _grid = grid;
            x = _x;
            y = _y;

            this.spawnPoint = cell.transform.Find("Cell3D/SpawnPoint");
            frameText = cell.transform.Find("Cell2D/FrameText");
            borderImage = cell.transform.Find("Cell2D/BorderImage");
        }

        public void FixedUpdate()
        {
            if (borderImage.gameObject.activeSelf == true)
                borderImage.gameObject.SetActive(false);
        }

        public void CellIntersected()
        {
            borderImage.gameObject.SetActive(true);
        }

        public void PlaceVisual(Transform _visual)
        {
            visual = _visual.gameObject;
            originalScale = visual.transform.lossyScale;

            visual.transform.SetParent(spawnPoint.transform);
            visual.transform.position = spawnPoint.position;
            visual.transform.localRotation = Quaternion.Euler(SpawnPoint.rotation.x, 90f, SpawnPoint.rotation.z);

            InventoryUtilities.SameSize(visual, _grid.CellLossyScale);
           
            Renderer[] renderers = visual.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                originalShaders.Add(renderer.material.shader);
                renderer.material.shader = _grid.CellGhostVisibleShader;
                Color color = renderer.material.color;
                renderer.material.color = new Color(color.r, color.g, color.b, .65f);
                renderer.material.renderQueue = _grid.CellGhostVisibleShader.renderQueue - 2;
            }

            visual.GetComponent<Rigidbody>().isKinematic = true;
            visual.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            Collider[] colliders = visual.gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            // set Text
            frameText.GetComponent<Text>().text = visual.name;            
            frameText.gameObject.SetActive(true);
        }
        public Transform GetVisual()
        {
            Renderer[] renderers = visual.gameObject.GetComponentsInChildren<Renderer>();
            int i= 0;
            foreach (Renderer renderer in renderers)
            {
                renderer.material.shader = originalShaders[i];
                i++;
            }               

            visual.transform.parent = null;
            visual.transform.localScale = originalScale;

            visual.GetComponent<Rigidbody>().isKinematic = false;
            visual.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Collider[] colliders = visual.gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }

            return visual.transform;
        }

        public void ClearCell()
        {
            if (visual != null)
                visual = null;

            frameText.GetComponent<Text>().text = "";
            frameText.gameObject.SetActive(false);
        }

        public bool isCellEmpty()
        {
            if (visual == null)
                return true;
            else
                return false;
        }
    }
}