using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace Inventory
{
    public class OnInventoryCellIntersectedEventArgs : EventArgs
    {
        public InventoryCellObject cellObject;
        public Transform ghostObject;
    }

    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory settings")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 4;
        [SerializeField] private float cellSize;
        [Tooltip("Number of cells")]
        [SerializeField] private int panelHeight;
        
        [Header("Items inside at the starting")]
        [SerializeField] private List<Transform> startingItems = new List<Transform>();

        [Header("Inventory Prefabs")]
        [SerializeField] private GameObject panelPefab;
        [SerializeField] private GameObject cellPrefab;

        [Header("Shaders used")]
        [SerializeField] private bool setupShaders;
        [EnableIf("setupShaders")]
        [SerializeField] Shader cellGhostVisibleShader;
        [EnableIf("setupShaders")]
        [SerializeField] Shader itemInCellShader;

        // Internal variables
        Vector3 halfCellSize;
        private static readonly float scaleFactor = 1000f;

        private GameObject panel;
        private Transform cellsContainer;        

        private GameObject[,] cellsArray;
        private GridXY grid;
        private GameObject ghostItem;

        void Awake()
        {
            cellsArray = new GameObject[gridWidth, gridHeight];
            halfCellSize = new Vector3(cellSize / 2 / scaleFactor, cellSize / 2 / scaleFactor, 0);
            Initialized();
        }

        private void Start()
        {
            grid = new GridXY(gridWidth, gridHeight, cellSize, transform.position, cellGhostVisibleShader, itemInCellShader, cellsArray)
            {
                CellLossyScale = cellsArray[0, 0].transform.Find("Cell3D").transform.lossyScale
            };

            ghostItem = new GameObject("GhostItem");
            ghostItem.transform.parent = transform.parent;
            ghostItem.AddComponent<GhostItem>();
            ghostItem.GetComponent<GhostItem>().Grid = grid;

            foreach (var item in startingItems)
            {
                AddItemToInventoryManually(item);
            }
        }

        private void Update()
        {
            grid.OriginalPosition = cellsArray[0, 0].transform.position - halfCellSize;            
        }

        void Initialized()
        {
            panel = Instantiate(panelPefab);
            panel.transform.name = panelPefab.name;
            panel.transform.SetParent(transform, true);
            panel.transform.localPosition = new Vector3(0, 0, 0);
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = new Vector3(1f / scaleFactor, 1f / scaleFactor, 1f / scaleFactor);

            Transform CellsArea = panel.transform.Find("CellsArea");
            Vector2 cellsAreaSize = new Vector2(cellSize * gridWidth, cellSize * gridHeight);

            cellsContainer = panel.transform.Find("CellsArea/Viewport/cellsContainer");
            cellsContainer.localScale = new Vector3(1f, 1f, 1f);
            cellsContainer.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
            cellsContainer.GetComponent<GridLayoutGroup>().spacing = Vector2.zero;
            cellsContainer.GetComponent<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.LowerLeft;
            cellsContainer.GetComponent<GridLayoutGroup>().startAxis = GridLayoutGroup.Axis.Vertical;
            cellsContainer.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            cellsContainer.GetComponent<RectTransform>().sizeDelta = cellsAreaSize;
            CellsArea.GetComponent<RectTransform>().sizeDelta = cellsAreaSize;

            Transform viewport = panel.transform.Find("CellsArea/Viewport");
            Vector2 viewportSize = new Vector2(cellsAreaSize.x, cellSize * panelHeight);
            viewport.GetComponent<RectTransform>().sizeDelta = viewportSize;
            panel.GetComponent<RectTransform>().sizeDelta = viewportSize + new Vector2(viewportSize.x / 100 * 5, 0);

            Transform Scrollbar = panel.transform.Find("Scrollbar");
            Vector3 pos = panel.GetComponent<RectTransform>().GetComponent<RectTransform>().localPosition;
            pos.x = panel.GetComponent<RectTransform>().sizeDelta.x / 2 - 10f;
            pos.z = -0.01f;
            Scrollbar.transform.localPosition = pos;
            Scrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(50, panel.GetComponent<RectTransform>().sizeDelta.y * 2);
            Scrollbar.GetComponent<Scrollbar>().value = 1;

            Transform frameMask = panel.transform.Find("FrameMask");
            frameMask.GetComponent<RectTransform>().sizeDelta = panel.GetComponent<RectTransform>().sizeDelta;

            Vector2 headerSize = new Vector2(frameMask.GetComponent<RectTransform>().sizeDelta.x, 250);
            Transform invHeader = panel.transform.Find("Header");
            invHeader.localPosition = new Vector2(0, panel.GetComponent<RectTransform>().sizeDelta.y / 2 + 25);
            invHeader.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = headerSize;
            invHeader.transform.Find("Text").localScale = new Vector3(1f, panel.transform.Find("Header/Text").localScale.y, 1f);
            invHeader.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new Vector2(headerSize.x * 0.90f, headerSize.y);            

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GameObject cellVisual = Instantiate(cellPrefab);
                    cellVisual.transform.name = "Cell";
                    cellVisual.transform.SetParent(cellsContainer.transform, true);
                    cellVisual.transform.localPosition = new Vector3(0f, 0f, 0f);
                    cellVisual.transform.localRotation = Quaternion.identity;
                    cellVisual.transform.localScale = new Vector3(1f, 1f, 1f);

                    cellVisual.transform.Find("Cell2D/CellFrame").GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
                    cellVisual.transform.Find("Cell2D/FrameText").GetComponent<RectTransform>().localPosition = new Vector2(0, cellSize / 2 - cellSize / 10);
                    cellVisual.transform.Find("Cell2D/BorderImage").GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);

                    cellVisual.transform.Find("Cell3D").transform.localScale = new Vector3(cellSize - 0.01f, cellSize - 0.01f, cellSize - 0.01f);
                    cellVisual.transform.Find("Cell3D").transform.localPosition = new Vector3(0, 0, cellSize / 2);
                    cellsArray[x, y] = cellVisual;
                }
            }
        }


        public void InventoryIntersected(Vector2Int cell, Input.Hand hand)
        {
            if (OutOfBoundsCheck(cell) == false)
                return;

            InventoryCellObject inventoryCell = GetCellObject(cell);
            inventoryCell.CellIntersected(); // Draw cell border            

            if (hand.ObjectInHand != null && inventoryCell.IsCellEmpty() == true)
            {
                grid.Trigger_CellIntersected(inventoryCell, hand.ObjectInHand); // Draw ghost item    
            }
            else if (hand.ObjectInHand == null && inventoryCell.IsCellEmpty() == false)
            {
                if (hand.Controller.selectAction.action.IsPressed())
                {
                    hand.XRInteractionManager.SelectEnter(hand.XRRayInteractor, inventoryCell.GetVisual().GetComponent<IXRSelectInteractable>());
                    inventoryCell.ClearCell();
                }
            }

            if (hand.Controller.selectAction.action.WasReleasedThisFrame()
                && hand.LastObjectInHand != null
                && inventoryCell.IsCellEmpty() == true
                )
            {               
                inventoryCell.PlaceVisual(hand.LastObjectInHand);
                hand.LastObjectInHand = null;
            }
        }

        public void StopIntersected(Vector2Int cell)
        {      
            if (OutOfBoundsCheck(cell) == false)
                return;

            InventoryCellObject inventoryCell = GetCellObject(cell);
            inventoryCell.StopIntersected(); // Stop Draw cell border   

            grid.Trigger_StopCellIntersected(inventoryCell); // Stop Draw ghost item   
        }

        private Vector2Int GetCellUnderRay(Vector3 raycastHitPoint)
        {
            Vector2Int gridCoord = InventoryUtilities.CalculateInventorySlotCoordinateVR(raycastHitPoint, transform.rotation, grid);
            return gridCoord;
        }

        private InventoryCellObject GetCellObject(Vector2Int cell)
        {
            return grid.GetGridObject(cell.x, cell.y);
        }

        private void AddItemToInventoryManually(Transform item)
        {
            bool canPlace = true;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    canPlace = cellsArray[x, y].GetComponent<InventoryCellObject>().IsCellEmpty();
                    if (canPlace)
                    {
                        Transform placedObj = Instantiate(item);
                        placedObj.name = item.name;
                        GetCellObject(new Vector2Int(x, y)).PlaceVisual(placedObj);
                        canPlace = false;
                        return;
                    }
                    else if (x == grid.Width && y == grid.Height && !canPlace)
                        Debug.Log("Inventory is full");
                }
            }
        }
        // Return true if cell is OutOfBounds
        private bool OutOfBoundsCheck(Vector2Int cellPos)
        {
            if (cellPos.x >= grid.Width || cellPos.y >= grid.Height || cellPos.x < 0 || cellPos.y < 0)
                return false;

            return true;
        }
    }
}