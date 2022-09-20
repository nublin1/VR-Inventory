using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public struct ItemData
    {
        public GameObject item;
        public Vector3 originalScale;
        public List<Shader> originalShaders;

        public static Vector3 CalculateDirection(AxisDirections direction)
        {
            switch (direction)
            {
                case AxisDirections.positive_X: return new Vector3(0, 90, 0);
                case AxisDirections.positive_Y: return new Vector3(-90, 0, 0);
                case AxisDirections.positive_Z: return new Vector3(0, 0, 0);
                case AxisDirections.negative_X: return new Vector3(0, -90, 0);
                case AxisDirections.negative_Y: return new Vector3(90, 0, 0);
                case AxisDirections.negative_Z: return new Vector3(0, -180, 0);
                default: return Vector3.zero;

            }
        }
    }

    public enum AxisDirections
    {
        positive_X,
        positive_Y,
        positive_Z,
        negative_X,
        negative_Y,
        negative_Z,
    }
}