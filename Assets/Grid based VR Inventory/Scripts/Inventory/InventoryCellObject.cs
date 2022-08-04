using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Inventory
{
    public class InventoryCellObject : MonoBehaviour
    {
        private GridXY _grid;
        private int x;
        private int y;
        private Transform spawnPoint;
        private Transform frameText;
        private Transform outlineImage;
        private Transform amountText;

        private readonly List<ItemData> items = new List<ItemData>();

        public GridXY Grid { get => _grid; }
        public Vector2Int CellCoord { get => new Vector2Int(x, y); }
        public Transform SpawnPoint { get => spawnPoint; }   

        public void InitInventoryCellObject(GridXY grid, int _x, int _y, Transform cell)
        {
            _grid = grid;
            x = _x;
            y = _y;

            spawnPoint = cell.transform.Find("Cell3D/SpawnPoint");
            frameText = cell.transform.Find("Cell2D/FrameText");
            outlineImage = cell.transform.Find("Cell2D/Outline");
            amountText = cell.transform.Find("Cell2D/CounterField/Number");
        }

        public void CellIntersected()
        {
            outlineImage.gameObject.SetActive(true);
        }

        public void StopIntersected()
        {
            outlineImage.gameObject.SetActive(false);
        }

        public void StoreItem(ItemData data)
        {
            if (items.Count > 0)
                data.item.SetActive(false);

            items.Add(data);

            // set name and amount
            if (items.Count > 0)
            {
                frameText.GetComponent<TextMeshProUGUI>().text = data.item.name;
                frameText.gameObject.SetActive(true);
                amountText.GetComponent<TextMeshProUGUI>().text = items.Count.ToString();
                amountText.gameObject.SetActive(true);
            }
        }

        public Transform GetItem()
        {
            GameObject item = items[items.Count - 1].item;
            // Back all shaders
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
            int i = 0;
            foreach (Renderer renderer in renderers)
            {
                renderer.material.shader = items[items.Count - 1].originalShaders[i];
                i++;
            }

            item.transform.parent = null;
            item.transform.localScale = items[items.Count - 1].originalScale;

            item.GetComponent<Rigidbody>().isKinematic = false;
            item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Collider[] colliders = item.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }

            item.SetActive(true);

            // Editing amount            
            amountText.GetComponent<TextMeshProUGUI>().text = items.Count.ToString();
            items.RemoveAt(items.Count - 1);
            return item.transform;
        }

        public void ClearCell()
        {
            if (items.Count != 0) { return; }

            frameText.GetComponent<TextMeshProUGUI>().text = "";
            frameText.gameObject.SetActive(false);
            amountText.GetComponent<TextMeshProUGUI>().text = items.Count.ToString();
            amountText.gameObject.SetActive(false);
        }

        public bool IsCellEmpty()
        {
            if (items.Count == 0)
                return true;
            else
                return false;
        }

        public bool IsPlacedItemEqual(Transform comparedItem)
        {
            if (comparedItem.name.Equals(items[0].item.name))
                return true;
            else
                return false;

        }
    }
}