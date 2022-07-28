using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace Inventory
{
    public class OnInventoryCellIntersectedEventArgs : EventArgs
    {
        public InventoryCellObject cellObject;
        public Transform ghostObject;
    }

    public class InventoryUtilities
    {
        public static Vector2Int CalculateInventorySlotCoordinateVR(Vector3 raycastHitPoint, Quaternion rotation, GridXY grid)
        {
            int x, y;
            grid.GetXY(raycastHitPoint, rotation, out x, out y);
            return new Vector2Int(x, y);
        }

        public static void SameSize(GameObject obj, Vector3 cellBoundSize)
        {
            // If true, the objects are scaled uniformly. If false, scale is per-component
            bool preserveDimensions = true;

            MeshRenderer[] meshRen = obj.GetComponentsInChildren<MeshRenderer>();
            List<Bounds> bounds = new List<Bounds>();
            foreach (MeshRenderer mr in meshRen)
                bounds.Add(mr.bounds);

            Bounds maxBound = FindMaxBound(bounds);            

            Vector3 obj_size = maxBound.max - maxBound.min;
            if (preserveDimensions)
            {
                obj.transform.localScale = obj.transform.localScale * (componentMax(cellBoundSize) / componentMax(obj_size)) * 0.95f ;
                
            }
            else
            {
                obj.transform.localScale = Vector3.Scale(obj.transform.localScale, div(cellBoundSize, obj_size));
            }
        }

        private static Bounds FindMaxBound(List<Bounds> bounds)
        {
            Bounds bound = new Bounds();
            for (int i = 0; i < bounds.Count; i++)
            {
                float a = componentMax(bounds[i].max);
                float b = componentMax(bound.max);
                if (a > b)
                    bound = bounds[i];
            }

            return bound;
        }

        private static float componentMax(Vector3 a)
        {
            return Mathf.Max(Mathf.Max(a.x, a.y), a.z);
        }

        private static Vector3 div(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }


    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory settings")]
        [SerializeField] int gridWidth = 6;
        [SerializeField] int gridHeight = 4;
        [SerializeField] float cellSize;
        [Tooltip("Number of cells")]
        [SerializeField] int panelHeight;

        [Header("Inventory Open Button")]
        [SerializeField] InputActionReference openAction;

        [SerializeField] Shader cellGhostVisibleShader;
        [SerializeField] Shader itemInCellShader;

        [Header("Items inside at the starting")]
        [SerializeField] List<Transform> startingItems = new List<Transform>();

        [Header("Inventory Prefabs")]
        [SerializeField] GameObject panelPefab;
        [SerializeField] GameObject cellPrefab;

        // Internal variables
        Vector3 halfCellSize;
        private float scaleFactor = 1000f;

        bool showInventory = true;

        private GameObject panel;
        private Transform cellsContainer;
        private Transform raycastingBackPlate;  

        private GameObject[,] cellsArray;

        private GridXY grid;
        private GhostItem ghostItem;


        void Awake()
        {            
            cellsArray = new GameObject[gridWidth, gridHeight];
            halfCellSize = new Vector3(cellSize / 2 / scaleFactor, cellSize / 2 / scaleFactor, 0);
            Initialized();
        }

        private void Start()
        {
            grid = new GridXY(gridWidth, gridHeight, cellSize / scaleFactor, transform.position, cellGhostVisibleShader, itemInCellShader, cellsArray,
                (GridXY g, int x, int y, Transform t) => new InventoryCellObject(g, x, y, t));   
            grid.CellLossyScale = cellsArray[0, 0].transform.Find("Cell3D").transform.lossyScale;            

            GameObject ghost = new GameObject("GhostItem");
            ghost.transform.parent = transform.parent;
            ghost.AddComponent<GhostItem>();
            ghost.GetComponent<GhostItem>().Grid = grid;

            foreach (var item in startingItems)
            {
                AddItemToInventoryManually(item);
            }
        }

        private void Update()
        {
            
            grid.OriginalPosition = cellsArray[0, 0].transform.position - halfCellSize;
            raycastingBackPlate.transform.position = cellsContainer.position;

            if (openAction.action.WasPerformedThisFrame())
                showInventory = !showInventory;

            // if (showInventory)
            //     InventoryOpen();
            // else
            //     InventoryClose();
        }

        private void FixedUpdate()
        {
            foreach (var cell in grid.GridArray)
                cell.FixedUpdate();

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

            raycastingBackPlate = panel.transform.Find("RaycastingBackPlate");            
            raycastingBackPlate.gameObject.layer = LayerMask.NameToLayer("UI"); //UI Layer            
            raycastingBackPlate.position = cellsContainer.transform.position;
            raycastingBackPlate.localScale = new Vector3(1f, 1f, 1f);
            raycastingBackPlate.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * gridWidth, cellSize * gridHeight);
            raycastingBackPlate.SetAsFirstSibling();            

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
                    cellsArray[x, y] = cellVisual;

                    cellVisual.transform.Find("Cell2D/CellFrame").GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
                    cellVisual.transform.Find("Cell2D/FrameText").GetComponent<RectTransform>().localPosition = new Vector2(0, cellSize / 2 - cellSize / 10);
                    cellVisual.transform.Find("Cell2D/BorderImage").GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);

                    cellVisual.transform.Find("Cell3D").transform.localScale = new Vector3(cellSize - 0.01f, cellSize - 0.01f, cellSize - 0.01f);
                    cellVisual.transform.Find("Cell3D").transform.localPosition = new Vector3(0, 0, cellSize / 2);
                }
            }
        }

        private void InventoryClose()
        {
            panel.gameObject.SetActive(false);
            ghostItem.gameObject.SetActive(false);
        }

        private void InventoryOpen()
        {
            panel.gameObject.SetActive(true);
            ghostItem.gameObject.SetActive(true);
        }

        public void InventoryIntersected(Vector3 worldPosition, Input.Hand hand)
        {
            Vector2Int cell = getCellUnderRay(worldPosition);

            if (OutOfBoundsCheck(cell) == false)
                return;            

            InventoryCellObject inventoryCell = GetCellObject(cell);
            inventoryCell.CellIntersected(); // Draw cell border            

            if (hand.ObjectInHand != null && inventoryCell.isCellEmpty() == true)
            {
                grid.TriggerCellIntersected(inventoryCell, hand.ObjectInHand); // Draw ghost item    
            }
            else if (hand.ObjectInHand == null && inventoryCell.isCellEmpty() == false)
            {
                if (hand.Controller.selectAction.action.IsPressed())
                {
                    hand.XRInteractionManager.SelectEnter(hand.XRRayInteractor, inventoryCell.GetVisual().GetComponent<IXRSelectInteractable>());
                    inventoryCell.ClearCell();
                }
            }


            if (hand.Controller.selectAction.action.WasReleasedThisFrame()
                && hand.LastObjectInHand != null
                && inventoryCell.isCellEmpty() == true)
            {
                inventoryCell.PlaceVisual(hand.LastObjectInHand);
                hand.LastObjectInHand = null;
            }
        }


        private Vector2Int getCellUnderRay(Vector3 raycastHitPoint)
        {
            Vector2Int gridCoord = InventoryUtilities.CalculateInventorySlotCoordinateVR(raycastHitPoint, transform.rotation, grid);
            return gridCoord;
        }

        private InventoryCellObject GetCellObject(Vector2Int cell)
        {
            return grid.GetGridObject(cell.x, cell.y);
        }

        private bool isCellEmpty(Vector2Int cell)
        {
            if (grid.GetGridObject(cell.x, cell.y).isCellEmpty())
                return true;
            else
                return false;
        }

        private void AddItemToInventoryManually(Transform item)
        {
            bool canPlace = true;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    canPlace = GetCellObject(new Vector2Int(x, y)).isCellEmpty();
                    if (canPlace)
                    {
                        Transform placedObj =  Instantiate(item);
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