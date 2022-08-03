using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class InventoryCellObject : MonoBehaviour
    {
        private GridXY _grid;
        private int x;
        private int y;
        private Transform spawnPoint;
        private Transform frameText;
        private Transform borderImage;
        private Transform amountText;

        private List<GameObject> items = new List<GameObject>();
        private int itemAmount = 0;

        private readonly List<Shader> originalShaders = new List<Shader>();
        private Vector3 originalScale = Vector3.zero;

        public GridXY Grid { get => _grid; }
        public Vector2Int CellCoord { get => new Vector2Int(x, y); }
        public Transform SpawnPoint { get => spawnPoint; }
        public Transform FrameText { get => frameText; }
        public Transform BorderImage { get => borderImage; }

        public void InitInventoryCellObject(GridXY grid, int _x, int _y, Transform cell)
        {
            _grid = grid;
            x = _x;
            y = _y;

            this.spawnPoint = cell.transform.Find("Cell3D/SpawnPoint");
            frameText = cell.transform.Find("Cell2D/FrameText");
            borderImage = cell.transform.Find("Cell2D/BorderImage");
            amountText = cell.transform.Find("Cell2D/CounterField/Number");
        }

        public void CellIntersected()
        {
            borderImage.gameObject.SetActive(true);
        }

        public void StopIntersected()
        {
            borderImage.gameObject.SetActive(false);
        }

        public void PlaceItem(Transform _visual)
        {            
            GameObject item = _visual.gameObject;
            originalScale = item.transform.lossyScale;

            item.transform.SetParent(spawnPoint.transform);
            item.transform.position = spawnPoint.position;
            item.transform.localRotation = Quaternion.identity;

            InventoryUtilities.SameSize(item, _grid.CellLossyScale); // same with cell size           
            item.transform.localRotation = Quaternion.Euler(SpawnPoint.rotation.x, 90f, SpawnPoint.rotation.z);

            // Change shaders
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                originalShaders.Add(renderer.material.shader);
                renderer.material.shader = _grid.ItemInCellShader;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Color color = renderer.material.color;
                renderer.material.color = new Color(color.r, color.g, color.b, .65f);
                renderer.material.renderQueue = _grid.CellGhostVisibleShader.renderQueue - 2;
            }

            // Item must not interact with anything
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Collider[] colliders = item.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }            

            items.Add(item);
            itemAmount++;

            // set name and amount
            if (itemAmount > 0)
            {
                frameText.GetComponent<TextMeshProUGUI>().text = item.name;                
                frameText.gameObject.SetActive(true);
                amountText.GetComponent<TextMeshProUGUI>().text = itemAmount.ToString();
                amountText.gameObject.SetActive(true);
            }            
        }

        public Transform GetItem()
        {
            GameObject item = items[itemAmount - 1];
            // Back all shaders
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
            int i = 0;
            foreach (Renderer renderer in renderers)
            {
                renderer.material.shader = originalShaders[i];
                i++;
            }

            item.transform.parent = null;
            item.transform.localScale = originalScale; 

            item.GetComponent<Rigidbody>().isKinematic = false;
            item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Collider[] colliders = item.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }

            // Editing amount
            itemAmount--;
            amountText.GetComponent<TextMeshProUGUI>().text = itemAmount.ToString();           
            items.Remove(item);
            return item.transform;
        }

        public void ClearCell()
        {
            if(items.Count != 0) { return; }              

            frameText.GetComponent<TextMeshProUGUI>().text = "";
            frameText.gameObject.SetActive(false);
            amountText.GetComponent<TextMeshProUGUI>().text = itemAmount.ToString();
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
            if (comparedItem.name.Equals( items[0].name))                            
                return true;            
            else                            
                return false;
            
        }
    }
}