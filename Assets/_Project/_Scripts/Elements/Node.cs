using UnityEngine;
using UnityEngine.Serialization;

namespace _Project._Scripts.Elements
{
    public class Node : MonoBehaviour {
        [FormerlySerializedAs("OccupiedBlock")] public Block _occupiedBlock;
        public Vector2 Pos => transform.position;
    }
}
