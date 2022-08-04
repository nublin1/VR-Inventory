using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public struct ItemData
    {
        public GameObject item;
        public Vector3 originalScale;
        public List<Shader> originalShaders;
    }
}