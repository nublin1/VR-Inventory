using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class GridXY 
    {       
        public event EventHandler<OnInventoryCellIntersectedEventArgs> OnInventoryCellIntersected;
        

        private int _width;
        private int _height;
        private float _cellSize = 1;
        private Vector3 _originalPosition = Vector3.zero;
        private InventoryCellObject[,] gridArray;

        private Shader cellGhostVisibleShader;
        private Shader itemInCellShader;

        // Internal variables
        Vector3 cellBoundSize = new Vector3(0.1f, 0.1f, 0.1f);

        public int Width { get => _width; }
        public int Height { get => _height; }     
        public float CellSize { get => _cellSize; }
        public Vector3 OriginalPosition         { get => _originalPosition; set => _originalPosition = value;  }
        public InventoryCellObject[,] GridArray { get => gridArray; }
        public Shader CellGhostVisibleShader    { get => cellGhostVisibleShader;  }
        public Shader ItemInCellShader          { get => itemInCellShader;}
        public Vector3 CellLossyScale            { get => cellBoundSize; set => cellBoundSize = value; }

        //public int Width { get => _width; }
        //public int Height { get => _height; }


        public GridXY(int width, int height, float cellSize, Vector3 originalPosition, Shader cellGhostVisibleShader, Shader itemInCellShader,
            GameObject[,] cellArrayUI, Func<GridXY, int, int, Transform, InventoryCellObject> createGridCellObject)
        {
            
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originalPosition = originalPosition;

            this.cellGhostVisibleShader = cellGhostVisibleShader;
            this.itemInCellShader = itemInCellShader;

            gridArray = new InventoryCellObject[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = createGridCellObject(this, x, y, cellArrayUI[x, y].transform);
                }
            }

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
                return gridArray[x, y];
            }
            else
            {
                return default(InventoryCellObject);
            }
        }
        
        public void TriggerCellIntersected(InventoryCellObject inventoryCell, Transform ghostObject)
        {
            OnInventoryCellIntersected?.Invoke(this, new OnInventoryCellIntersectedEventArgs { cellObject = inventoryCell, ghostObject = ghostObject });
        }
    }
}