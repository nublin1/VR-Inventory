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

        List<ItemData> ghostItems = new List<ItemData>();

        private GridXY grid;

        public GridXY Grid { set => grid = value; }

        void Start()
        {
            grid.OnInventoryCellIntersected += RenderGhostItem;
        }

        void Update()
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
                ItemData newData = new ItemData();
               
                newData.showGhostItem = true;
                newData.cellObject = e.cellObject;
                newData.ghostItemOnCell = e.ghostObject;

                ghostItems.Add(newData);           
        }

        private ItemData UpdateVisual(ItemData data)
        {
            if (data.ghostVisual == null)
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

                if ( !data.ghostVisual.Equals(data.ghostItemOnCell))
                    data.showGhostItem = false;
            }

            return data;
        }

        private void DestroyVisual(ItemData data)
        {
            GameObject.Destroy(data.ghostVisual.gameObject);
        }

    }
}