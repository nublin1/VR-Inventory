using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Inventory
{
    public class InventoryWizard : EditorWindow
    {
        #region variables
        private int gridWidth, gridHeight;
        private int cellSize;
        private float panelHeight;

        //Inventory Prefabs
        GameObject panelPefab;
        GameObject cellPrefab;
        GameObject frameMaskPrefab;

        #endregion

        private int tabIndex = 0;
        string[] tabHeaders = new string[] { "Inventory" };

        [MenuItem("Inventory/New Inventory")]
        static void Init()
        {
            InventoryWizard _editor = (InventoryWizard)GetWindow(typeof(InventoryWizard));

            _editor.Show();
        }

        void OnGUI()
        {
            tabIndex = GUILayout.Toolbar(tabIndex, tabHeaders);

            if (tabIndex == 0)
            {
                EditorGUILayout.LabelField("Inventory & Cell size", EditorStyles.centeredGreyMiniLabel);

                gridWidth = EditorGUILayout.IntSlider("Inventory Horizontal size", gridWidth, 1, 100);
                gridHeight = EditorGUILayout.IntSlider("Inventory Vertical size", gridHeight, 1, 100);
                cellSize = EditorGUILayout.IntSlider("Cell size", cellSize, 32, 200);
                panelHeight = EditorGUILayout.IntSlider("panelHeight", (int)panelHeight, 1, 10);


                panelPefab = (GameObject)EditorGUILayout.ObjectField("PanelPrefab", obj: panelPefab, typeof(GameObject), false);
                cellPrefab = (GameObject)EditorGUILayout.ObjectField("cellPrefab", obj: cellPrefab, typeof(GameObject), false);
                frameMaskPrefab = (GameObject)EditorGUILayout.ObjectField("frameMaskPrefab", obj: frameMaskPrefab, typeof(GameObject), false);


                if (GUILayout.Button("Build inventory"))
                {
                    if (panelPefab == null || cellPrefab == null || frameMaskPrefab == null)
                    {
                        EditorUtility.DisplayDialog("Setup uncompleted", " Please select all prefabs", "Continue");
                        return;
                    }

                    initializeInventory();       
                }
            }
        }

        void initializeInventory()
        {
            GameObject panel;
            GameObject cellsContainer;
            GameObject frameMask;
            GameObject raycastingBackPlate;
            GameObject achor;

            var inventory = Instantiate(new GameObject());
            float scaleFactor = 1000f;

            panel = Instantiate(panelPefab);
            panel.transform.name = panelPefab.name;
            panel.AddComponent<Canvas>();
            panel.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            panel.AddComponent<TrackedDeviceGraphicRaycaster>();
            //panel.transform.SetParent(inventory.transform, false);
            panel.transform.localPosition = new Vector3(cellSize / scaleFactor * gridWidth / 2, cellSize / scaleFactor * gridHeight / 2, -.1f);
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = new Vector3(1f / scaleFactor, 1f / scaleFactor, 1f / scaleFactor);
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(scaleFactor * gridWidth + cellSize * gridWidth + cellSize / 2, cellSize * panelHeight);

            cellsContainer = new GameObject("cellsContainer");
            cellsContainer.AddComponent<GridLayoutGroup>();
            cellsContainer.transform.SetParent(panel.transform, true);
            cellsContainer.transform.localPosition = new Vector3(0, gridHeight / 4 * cellSize * -1);
            cellsContainer.transform.localRotation = Quaternion.identity;
            cellsContainer.transform.localScale = new Vector3(1f, 1f, 1f);

            cellsContainer.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
            //cellsContainer.GetComponent<GridLayoutGroup>().spacing = cellSpacing * scaleFactor;
            cellsContainer.GetComponent<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.LowerLeft;
            cellsContainer.GetComponent<GridLayoutGroup>().startAxis = GridLayoutGroup.Axis.Vertical;
            cellsContainer.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            cellsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(scaleFactor * (gridWidth - 1) + cellSize * gridWidth,
                                                                                 scaleFactor * (gridHeight - 1) + cellSize * gridHeight);

            panel.GetComponent<ScrollRect>().content = cellsContainer.GetComponent<RectTransform>();

            Transform slider = panel.transform.Find("Scrollbar");
            Vector3 pos = panel.GetComponent<RectTransform>().GetComponent<RectTransform>().localPosition;
            pos.x = panel.GetComponent<RectTransform>().sizeDelta.x / 2 - 15.0f;
            slider.transform.localPosition = pos;
            slider.transform.localRotation = Quaternion.identity;
            slider.GetComponent<RectTransform>().sizeDelta = new Vector2(50, panel.GetComponent<RectTransform>().sizeDelta.y * 2);

            frameMask = Instantiate(frameMaskPrefab);
            frameMask.transform.name = frameMaskPrefab.name;
            frameMask.transform.SetParent(panel.transform);
            frameMask.transform.localPosition = Vector3.zero;
            frameMask.transform.localRotation = Quaternion.identity;
            frameMask.transform.localScale = new Vector3(1f, 1f, 1f);
            frameMask.GetComponent<RectTransform>().sizeDelta = panel.GetComponent<RectTransform>().sizeDelta;
            GameObject vieport = new GameObject("Viewport");
            vieport.transform.SetParent(frameMask.transform);
            vieport.transform.localPosition = Vector3.zero;
            vieport.transform.localRotation = Quaternion.identity;
            vieport.transform.localScale = new Vector3(1f, 0.9f, 1f);
            vieport.AddComponent<RectTransform>();
            vieport.GetComponent<RectTransform>().sizeDelta = panel.GetComponent<RectTransform>().sizeDelta;
            panel.GetComponent<ScrollRect>().viewport = vieport.GetComponent<RectTransform>();

            raycastingBackPlate = new GameObject("raycastingBackPlate");
            raycastingBackPlate.tag = "Inventory";
            raycastingBackPlate.layer = 5; //UI Layer
            raycastingBackPlate.AddComponent<Image>();
            raycastingBackPlate.transform.SetParent(panel.transform, true);
            raycastingBackPlate.transform.position = cellsContainer.transform.position;
            raycastingBackPlate.transform.localRotation = Quaternion.identity;
            raycastingBackPlate.transform.localScale = new Vector3(1f, 1f, 1f);
            raycastingBackPlate.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * gridWidth, cellSize * gridHeight);
            raycastingBackPlate.transform.SetAsFirstSibling();

            achor = new GameObject("achor");
            achor.transform.SetParent(inventory.transform);
            achor.transform.localPosition = Vector3.zero;
            achor.transform.localRotation = Quaternion.identity;
            achor.transform.localScale = new Vector3(1f, 1f, 1f);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GameObject cellVisual = Instantiate(cellPrefab);
                    //cellVisual.AddComponent<RectTransform>();
                    cellVisual.transform.name = "Cell";
                    cellVisual.transform.SetParent(cellsContainer.transform, true);
                    cellVisual.transform.localPosition = new Vector3(0f, 0f, 0f);
                    cellVisual.transform.localScale = new Vector3(1f, 1f, 1f);
                    cellVisual.transform.localRotation = Quaternion.identity;
                    //cellsArray[x, y] = cellVisual;

                    //Quaternion rotation = transform.rotation * Quaternion.Euler(0f, -90f, 0f);
                    cellVisual.transform.Find("Cell3D").transform.localScale = new Vector3(cellSize - 0.01f, cellSize - 0.01f, cellSize - 0.01f);
                    cellVisual.transform.Find("Cell3D").transform.localPosition = new Vector3(0, 0, cellSize / 2);
                    //cell3D.GetComponent<Inv_3DCell>().spawnPoint = cell3D.transform.Find("SpawnPoint").transform;
                }
            }
        }

    }
}