using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class GhostItem : MonoBehaviour
    {
        struct ItemData
        {
            public Transform ghostVisual;
            public Transform ghostItemOnCell;
            public InventoryCellObject cellObject;
            public bool showGhostItem;
        }

        readonly List<ItemData> ghostItems = new List<ItemData>();

        private GridXY grid;

        public GridXY Grid { set => grid = value; }

        private void Start()
        {
            grid.OnInventoryCellIntersected += RenderGhostItem;
            grid.OnStopInventoryCellIntersected += StopRenderGhostItem;
        }

        private void OnEnable()
        {
            if (grid == null)
                return;
            grid.OnInventoryCellIntersected += RenderGhostItem;
            grid.OnStopInventoryCellIntersected += StopRenderGhostItem;
        }

        private void OnDisable()
        {
            if (grid == null)
                return;
            grid.OnInventoryCellIntersected -= RenderGhostItem;
            grid.OnStopInventoryCellIntersected -= StopRenderGhostItem;
        }

        void LateUpdate()
        {
            for (int i = 0; i < ghostItems.Count; ++i)
            {
                if (ghostItems[i].showGhostItem == true)
                {
                    ghostItems[i] = UpdateVisual(ghostItems[i]);
                }
                else
                {
                    DestroyVisual(ghostItems[i]);
                    ghostItems.RemoveAt(i);
                }
            }
        }

        private void RenderGhostItem(object sender, OnInventoryCellIntersectedEventArgs e)
        {
            ItemData newData = new ItemData
            {
                showGhostItem = true,
                cellObject = e.cellObject,
                ghostItemOnCell = e.ghostObject
            };

            ghostItems.Add(newData);
        }

        private void StopRenderGhostItem(object sender, OnInventoryCellIntersectedEventArgs e)
        {
            for (int i = 0; i < ghostItems.Count; ++i)
            {
                if (ghostItems[i].cellObject == e.cellObject)
                {
                    ItemData newData = ghostItems[i];
                    newData.showGhostItem = false;
                    ghostItems[i] = newData;
                }
            }
        }

        private ItemData UpdateVisual(ItemData data)
        {
            if (data.ghostVisual == null && data.cellObject.SpawnPoint.childCount == 0 && data.cellObject.IsCellEmpty())
            {
                data.ghostVisual = Instantiate(data.ghostItemOnCell);
                data.ghostVisual.transform.SetParent(data.cellObject.SpawnPoint);
                data.ghostVisual.transform.position = data.cellObject.SpawnPoint.position;
                data.ghostVisual.transform.localRotation = Quaternion.Euler(data.cellObject.SpawnPoint.rotation.x, 90f, data.cellObject.SpawnPoint.rotation.z);

                InventoryUtilities.SameSize(data.ghostVisual.gameObject, grid.CellLossyScale);

                Renderer[] renderers = data.ghostVisual.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material.shader = grid.CellGhostVisibleShader;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    Color color = renderer.material.color;
                    renderer.material.color = new Color(color.r, color.g, color.b, .65f);
                    renderer.material.renderQueue = grid.CellGhostVisibleShader.renderQueue - 2;
                }

                data.ghostVisual.GetComponent<Rigidbody>().isKinematic = true;
                data.ghostVisual.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                Collider[] colliders = data.ghostVisual.gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }
            }
            else if (data.cellObject.IsCellEmpty() == false)
                data.showGhostItem = false;

            return data;
        }

        private void DestroyVisual(ItemData data)
        {
            if (data.ghostVisual != null)
                GameObject.Destroy(data.ghostVisual.gameObject);
        }
    }
}