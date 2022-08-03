using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryUtilities
    {
        public static Vector2Int CalculateInventorySlotCoordinateVR(Vector3 raycastHitPoint, Quaternion rotation, GridXY grid)
        {            
            grid.GetXY(raycastHitPoint, rotation, out int x, out  int y);
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
                obj.transform.localScale = obj.transform.localScale * (ComponentMax(cellBoundSize) / ComponentMax(obj_size)) * 0.92f;

            }
            else
            {
                obj.transform.localScale = Vector3.Scale(obj.transform.localScale, Div(cellBoundSize, obj_size));
            }
        }

        private static Bounds FindMaxBound(List<Bounds> bounds)
        {
            Bounds bound = new Bounds();
            for (int i = 0; i < bounds.Count; i++)
            {
                float a = ComponentMax(bounds[i].max);
                float b = ComponentMax(bound.max);
                if (a > b)
                    bound = bounds[i];
            }

            return bound;
        }

        private static float ComponentMax(Vector3 a)
        {
            return Mathf.Max(Mathf.Max(a.x, a.y), a.z);
        }

        private static Vector3 Div(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}