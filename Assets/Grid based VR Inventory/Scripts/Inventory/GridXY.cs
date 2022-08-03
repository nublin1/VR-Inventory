using System;
using TMPro;
using UnityEngine;

namespace Inventory
{
    public class GridXY 
    {       
        public event EventHandler<OnInventoryCellIntersectedEventArgs> OnInventoryCellIntersected;
        public event EventHandler<OnInventoryCellIntersectedEventArgs> OnStopInventoryCellIntersected;

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize = 1;
        private Vector3 _originalPosition = Vector3.zero;
        private GameObject[,] gridArray;

        private Shader cellGhostVisibleShader;
        private Shader itemInCellShader;

        // Internal variables
        Vector3 cellBoundSize = new Vector3(0.1f, 0.1f, 0.1f);

        public int Width        { get => _width; }
        public int Height       { get => _height; }     
        public float CellSize   { get => _cellSize; }
        public Vector3 OriginalPosition         { get => _originalPosition; set => _originalPosition = value;  }
        public GameObject[,] GridArray          { get => gridArray; }
        public Shader CellGhostVisibleShader    { get => cellGhostVisibleShader;  }
        public Shader ItemInCellShader          { get => itemInCellShader;}
        public Vector3 CellLossyScale           { get => cellBoundSize; set => cellBoundSize = value; }        

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

            //for (int x = 0; x < gridArray.GetLength(0); x++)
            //{
            //    for (int y = 0; y < gridArray.GetLength(1); y++)
            //    {      
            //        cellArrayUI[x, y].AddComponent<InventoryCellObject>().InitInventoryCellObject(this, x, y, cellArrayUI[x, y].transform);
            //        gridArray[x, y] = cellArrayUI[x, y].GetComponent<InventoryCellObject>();                    
            //    }
            //}

            this.itemInCellShader = itemInCellShader;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * _cellSize + _originalPosition;
        }

        public void GetXY(Vector3 worldPosition, Quaternion rotation, out int x, out int y)
        {   
            Vector3 localPosition = worldPosition - _originalPosition;
            localPosition =  Quaternion.Inverse(rotation) * localPosition;            

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

        // find 1st cell where can be placed item 
        public void AddItemToInventoryManually(Transform item)
        {
            bool canPlace = true;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    canPlace = GetGridObject(x,y).IsCellEmpty() || GetGridObject(x, y).IsPlacedItemEqual(item);
                    if (canPlace)
                    {
                        Transform placedObj = GameObject.Instantiate(item);
                        placedObj.name = item.name;
                        GetGridObject(x, y).PlaceItem(placedObj);
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
            OnInventoryCellIntersected?.Invoke(this, new OnInventoryCellIntersectedEventArgs { cellObject = inventoryCell, ghostObject = ghostObject });
        }

        public void Trigger_StopCellIntersected(InventoryCellObject inventoryCell)
        {
            OnStopInventoryCellIntersected?.Invoke(this, new OnInventoryCellIntersectedEventArgs { cellObject = inventoryCell, ghostObject = null });
        }
    }
}