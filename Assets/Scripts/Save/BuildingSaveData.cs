using System;
using UnityEngine;

namespace Havengard.Save
{
    /// <summary>
    /// Serializable data for a placed building
    /// </summary>
    [Serializable]
    public class BuildingSaveData
    {
        // CHANGED: Make fields public for serialization
        public string buildingPrefabName;  // Name of building prefab
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;            // Y-axis rotation (for 2D, usually 0)
        
        // Future: building upgrade level, etc.
        public int buildingLevel;
        
        public BuildingSaveData() { }
        
        public BuildingSaveData(string prefabName, Vector3 position, float rotation)
        {
            this.buildingPrefabName = prefabName;
            this.positionX = position.x;
            this.positionY = position.y;
            this.positionZ = position.z;
            this.rotationY = rotation;
            this.buildingLevel = 1;
        }
        
        public Vector3 GetPosition() => new Vector3(positionX, positionY, positionZ);
        public Quaternion GetRotation() => Quaternion.Euler(0, rotationY, 0);
    }
}
