using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Inventory
{    
    public class CellIntersectedEventArgs : EventArgs
    {
        public InventoryCellObject cellObject;
        public Transform item;
    }

    public class GridXY
    {
        public event EventHandler<CellIntersectedEventArgs> OnCellIntersected;
        public event EventHandler<CellIntersectedEventArgs> OnStopCellIntersected;

        public delegate void AddItemDelegate(Transform ghostItem, InventoryCellObject cell);
        public event AddItemDelegate OnAddItem;

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize = 1;
        private Vector3 _originalPosition = Vector3.zero;
        private GameObject[,] gridArray;

        private Shader cellGhostVisibleShader;
        private Shader itemInCellShader;

        // Internal variables
        Vector3 cellBoundSize = new Vector3(0.1f, 0.1f, 0.1f);

        public int Width { get => _width; }
        public int Height { get => _height; }
        public float CellSize { get => _cellSize; }
        public Vector3 OriginalPosition { get => _originalPosition; set => _originalPosition = value; }
        public GameObject[,] GridArray { get => gridArray; }
        public Shader CellGhostVisibleShader { get => cellGhostVisibleShader; }
        public Shader ItemInCellShader { get => itemInCellShader; }
        public Vector3 CellLossyScale { get => cellBoundSize; set => cellBoundSize = value; }

        public GridXY(int width, int height, float cellSize, Vector3 originalPosition, Shader cellGhostVisibleShader, Shader itemInCellShader,
            GameObject cellPrefab, Transform cellsContainer)
        {

            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originalPosition = originalPosition;

            this.cellGhostVisibleShader = cellGhostVisibleShader;
            this.itemInCellShader = itemInCellShader;

            gridArray = new GameObject[width, height];

            // generate cells
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    GameObject cellVisual = GameObject.Instantiate(cellPrefab);
                    cellVisual.transform.name = "Cell";
                    cellVisual.transform.SetParent(cellsContainer.transform, true);
                    cellVisual.transform.localPosition = new Vector3(0f, 0f, 0f);
                    cellVisual.transform.localRotation = Quaternion.identity;
                    cellVisual.transform.localScale = new Vector3(1f, 1f, 1f);

                    Transform cell2D = cellVisual.transform.Find("Cell2D");
                    Vector2 sizeOfCell = new Vector2(cellSize, cellSize);
                    cell2D.GetComponent<RectTransform>().sizeDelta = sizeOfCell;
                    cell2D.Find("CellFrame").GetComponent<RectTransform>().sizeDelta = sizeOfCell;
                    cell2D.Find("FrameText").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    cell2D.Find("FrameText").GetComponent<RectTransform>().sizeDelta = new Vector2(sizeOfCell.x, sizeOfCell.y / 5);
                    cell2D.Find("FrameText").GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
                    cell2D.Find("FrameText").gameObject.SetActive(false);
                    cell2D.Find("Outline").GetComponent<RectTransform>().sizeDelta = sizeOfCell;
                    cell2D.Find("CounterField").GetComponent<RectTransform>().anchoredPosition = new Vector2(-3.5f, 0);

                    Vector2 SizeOfCounterField = new Vector2(sizeOfCell.x / 3f, sizeOfCell.y / 4);
                    cell2D.Find("CounterField/CountBackground").GetComponent<RectTransform>().sizeDelta = SizeOfCounterField;
                    cell2D.Find("CounterField/Number").GetComponent<RectTransform>().sizeDelta = SizeOfCounterField;
                    cell2D.Find("CounterField/Number").GetComponent<TextMeshProUGUI>().enableAutoSizing = true;

                    Transform Cell3D = cellVisual.transform.Find("Cell3D");
                    Cell3D.transform.localScale = new Vector3(cellSize - 0.01f, cellSize - 0.01f, cellSize - 0.01f);
                    Cell3D.transform.localPosition = new Vector3(0, 0, cellSize / 2);

                    cellVisual.AddComponent<InventoryCellObject>().InitInventoryCellObject(this, x, y, cellVisual.transform);
                    gridArray[x, y] = cellVisual;
                }
            }

            CellLossyScale = gridArray[0, 0].transform.Find("Cell3D").transform.lossyScale;
            this.itemInCellShader = itemInCellShader;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * _cellSize + _originalPosition;
        }

        public void GetXY(Vector3 worldPosition, Quaternion rotation, out int x, out int y)
        {
            Vector3 localPosition = worldPosition - _originalPosition;
            localPosition = Quaternion.Inverse(rotation) * localPosition;

            x = Mathf.FloorToInt(localPosition.x / _cellSize);
            y = Mathf.FloorToInt(localPosition.y / _cellSize);
        }

        public InventoryCellObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return gridArray[x, y].GetComponent<InventoryCellObject>();
            }
            else
            {
                return default;
            }
        }

        public void PlaceItem(Transform _item, InventoryCellObject cell)
        {
            ItemData itemdata = new ItemData();
            itemdata.item = _item.gameObject;
            itemdata.originalScale = _item.transform.lossyScale;
            itemdata.item.transform.SetParent(cell.SpawnPoint.transform);
            itemdata.item.transform.position = cell.SpawnPoint.position;
            itemdata.item.transform.localRotation = Quaternion.identity;

            InventoryUtilities.SameSize(itemdata.item, CellLossyScale); // same with cell size           
            itemdata.item.transform.localRotation = Quaternion.Euler(cell.SpawnPoint.rotation.x, 90f, cell.SpawnPoint.rotation.z);

            // Change shaders
            Renderer[] renderers = itemdata.item.GetComponentsInChildren<Renderer>();
            itemdata.originalShaders = new List<Shader>();
            foreach (Renderer renderer in renderers)
            {
                itemdata.originalShaders.Add(renderer.material.shader);
                renderer.material.shader = ItemInCellShader;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Color color = renderer.material.color;
                renderer.material.color = new Color(color.r, color.g, color.b, .65f);
                renderer.material.renderQueue = CellGhostVisibleShader.renderQueue - 2;
            }

            // Item must not interact with anything
            itemdata.item.GetComponent<Rigidbody>().isKinematic = true;
            itemdata.item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Collider[] colliders = itemdata.item.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            cell.StoreItem(itemdata);
        }

        // find 1st cell where can be placed item 
        public void AddItemToInventoryManually(Transform item)
        {
            bool canPlace = true;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    canPlace = GetGridObject(x, y).IsCellEmpty() || GetGridObject(x, y).IsPlacedItemEqual(item);
                    if (canPlace)
                    {
                        Transform placedObj = GameObject.Instantiate(item);
                        placedObj.name = item.name;
                        PlaceItem(placedObj, GetGridObject(x, y));
                        canPlace = false;
                        return;
                    }
                    else if (x == _width && y == _height && !canPlace)
                        Debug.Log("Inventory is full");
                }
            }
        }

        public void Trigger_CellIntersected(InventoryCellObject inventoryCell, Transform ghostObject)
        {
            OnCellIntersected?.Invoke(this, new CellIntersectedEventArgs { cellObject = inventoryCell, item = ghostObject });
        }

        public void Trigger_StopCellIntersected(InventoryCellObject inventoryCell)
        {
            OnStopCellIntersected?.Invoke(this, new CellIntersectedEventArgs { cellObject = inventoryCell, item = null });
        }
    }
}