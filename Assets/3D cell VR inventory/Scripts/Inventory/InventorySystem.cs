using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory settings")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 4;
        [SerializeField] private float cellSize = 100f;
        [Tooltip("Number of cells")]
        [SerializeField] private int viewportHeight;
        [SerializeField] private AxisDirections DefaultDirectionAxis;       

        [Header("Items inside at the starting")]
        [SerializeField] private List<Transform> startingItems = new List<Transform>();

        [Header("Inventory Prefabs")]
        [SerializeField] private bool setupPrefabs;
        [ShowIf("setupPrefabs")]
        [SerializeField] private GameObject panelPrefab;
        [ShowIf("setupPrefabs")]
        [SerializeField] private GameObject cellPrefab;

        [Header("Shaders used")]
        [SerializeField] private bool setupShaders;
        [ShowIf("setupShaders")]
        [SerializeField] Shader cellGhostVisibleShader;
        [ShowIf("setupShaders")]
        [SerializeField] Shader itemInCellShader;

        // Internal variables
        GameObject invPanel;
        Vector3 halfCellSize;
        private static readonly float scaleFactor = 1000f;

        private GridXY grid;
        private GameObject ghostItem;

        void Awake()
        {            
            InventoryGeneration();
        }

        private void Start()
        {          
            ghostItem = new GameObject("GhostItem");
            ghostItem.transform.parent = transform;
            ghostItem.AddComponent<GhostItem>();
            ghostItem.GetComponent<GhostItem>().Grid = grid;

            foreach (var item in startingItems)
            {
                grid.AddItemToInventoryManually(item);
            }
        }

        private void Update()
        {
            grid.OriginalPosition = grid.GridArray[0, 0].transform.position - halfCellSize;
        }

        [Button("Generate inventory")]
        private void InventoryGeneration()
        {
            // Delete all cells that already exits
            for (int i = 0; i < transform.childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            
            if (panelPrefab == null || cellPrefab == null)
            {
                Debug.LogError("One of the prefabs is empty");
                return;
            }

            halfCellSize = new Vector3(cellSize / 2 / scaleFactor, cellSize / 2 / scaleFactor, 0);

            if (viewportHeight > gridHeight)
                viewportHeight = gridHeight;

            Vector2 cellsAreaSize = new Vector2(cellSize * gridWidth, cellSize * gridHeight);
            Vector2 viewportSize = new Vector2(cellsAreaSize.x, cellSize * viewportHeight);

            // root of inventory gui
            invPanel = Instantiate(panelPrefab);
            invPanel.transform.name = panelPrefab.name;
            invPanel.transform.SetParent(transform, true);
            invPanel.transform.localPosition = new Vector3(0, 0, 0);
            invPanel.transform.localRotation = Quaternion.identity;
            invPanel.transform.localScale = new Vector3(1f / scaleFactor, 1f / scaleFactor, 1f / scaleFactor);

            // contains array of cells
            Transform cellsContainer = invPanel.transform.Find("CellsArea/Viewport/CellsContainer");
            cellsContainer.localScale = new Vector3(1f, 1f, 1f);
            cellsContainer.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
            cellsContainer.GetComponent<GridLayoutGroup>().spacing = Vector2.zero;
            cellsContainer.GetComponent<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.UpperLeft;
            cellsContainer.GetComponent<GridLayoutGroup>().startAxis = GridLayoutGroup.Axis.Vertical;
            cellsContainer.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            Transform CellsArea = invPanel.transform.Find("CellsArea");
            cellsContainer.GetComponent<RectTransform>().sizeDelta = cellsAreaSize;
            CellsArea.GetComponent<RectTransform>().sizeDelta = cellsAreaSize;

            // quad that blocking visibility
            Transform viewport = invPanel.transform.Find("CellsArea/Viewport");
            viewport.GetComponent<RectTransform>().sizeDelta = viewportSize;
            invPanel.GetComponent<RectTransform>().sizeDelta = viewportSize + new Vector2(viewportSize.x / 100 * 5, 0);

            Vector2 panelSize = invPanel.GetComponent<RectTransform>().GetComponent<RectTransform>().sizeDelta;
            Vector3 scrollbarPos = new Vector3((panelSize.x - viewportSize.x) / 2 * -1, 0, -0.01f);

            Transform Scrollbar = invPanel.transform.Find("Scrollbar");
            //pos.x = invPanel.GetComponent<RectTransform>().sizeDelta.x / 2 - 10f;            
            Scrollbar.GetComponent<RectTransform>().anchoredPosition = scrollbarPos;
            Scrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(50, invPanel.GetComponent<RectTransform>().sizeDelta.y * 2);
            Scrollbar.GetComponent<Scrollbar>().value = 1;

            // outer frame
            Transform frameMask = invPanel.transform.Find("FrameMask");
            frameMask.GetComponent<RectTransform>().sizeDelta = invPanel.GetComponent<RectTransform>().sizeDelta;

            Vector2 headerSize = new Vector2(frameMask.GetComponent<RectTransform>().sizeDelta.x, 200);
            Transform invHeader = invPanel.transform.Find("Header");
            invHeader.localPosition = new Vector2(0, invPanel.GetComponent<RectTransform>().sizeDelta.y / 2 + 20);
            invHeader.GetComponent<RectTransform>().sizeDelta = headerSize;
            invHeader.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = headerSize;
            invHeader.transform.Find("Text").localScale = new Vector3(1f, invPanel.transform.Find("Header/Text").localScale.y, 1f);
            invHeader.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new Vector2(headerSize.x * 0.95f, headerSize.y / invHeader.transform.Find("Text").localScale.y);

            InitializationGrid();
        }

        private void InitializationGrid()
        {
            grid = new GridXY(gridWidth, gridHeight, cellSize, transform.position, cellGhostVisibleShader, itemInCellShader, cellPrefab,
               invPanel.transform.Find("CellsArea/Viewport/CellsContainer"));
        } 

        public void InventoryIntersected(Vector2Int cell, Input.Hand hand)
        {
            if (OutOfBoundsCheck(cell) == false)
                return;

            InventoryCellObject inventoryCell = grid.GetGridObject(cell.x, cell.y); // get cell
            inventoryCell.CellOutlineSetActive(true); // Draw cell Outline            

            if (hand.ObjectInHand != null && inventoryCell.IsCellEmpty() == true)
            {
                grid.Trigger_CellIntersected(inventoryCell, hand.ObjectInHand, DefaultDirectionAxis); // Draw ghost item    
            }
            else if (hand.ObjectInHand == null && inventoryCell.IsCellEmpty() == false)
            {
                if (hand.Controller.selectAction.action.IsPressed())
                {
                    // take item from cell anf put it to hand
                    hand.XRInteractionManager.SelectEnter(hand.XRRayInteractor, grid.GetPlacedItem(inventoryCell).GetComponent<IXRSelectInteractable>());
                    inventoryCell.ClearCell();
                }
            }

            if (hand.Controller.selectAction.action.WasReleasedThisFrame())
            {
                if (hand.LastObjectInHand != null && (inventoryCell.IsCellEmpty() == true || inventoryCell.IsPlacedItemEqual(hand.LastObjectInHand))) // compared by name
                {
                    // take item from hand and put it to cell
                    grid.PlaceItem(hand.LastObjectInHand, grid.GetGridObject(cell.x, cell.y), DefaultDirectionAxis);
                    hand.LastObjectInHand = null;
                }
            }
        }

        public void StopIntersected(Vector2Int cell)
        {
            if (OutOfBoundsCheck(cell) == false)
                return;

            InventoryCellObject inventoryCell = grid.GetGridObject(cell.x, cell.y);
            inventoryCell.CellOutlineSetActive(false); // Stop Draw Outline  

            grid.Trigger_StopCellIntersected(inventoryCell); // Stop Draw ghost item   
        }

        private Vector2Int GetCellUnderWorldPosition(Vector3 raycastHitPoint)
        {
            Vector2Int gridCoord = InventoryUtilities.CalculateInventorySlotCoordinateVR(raycastHitPoint, transform.rotation, grid);
            return gridCoord;
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