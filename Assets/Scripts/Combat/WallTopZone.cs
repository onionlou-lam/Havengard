using UnityEngine;
using Havengard.Player;

namespace Havengard.Combat
{
    /// <summary>
    /// Place on a trigger collider covering the walkable wall-top area
    /// (the orange-outlined walkway in the level, where towers are built).
    /// Flags PlayerWallState so projectiles fired while standing here can bypass wall collision.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WallTopZone : MonoBehaviour
    {
        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var wallState = other.GetComponent<PlayerWallState>();
            if (wallState != null)
                wallState.SetOnWall(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var wallState = other.GetComponent<PlayerWallState>();
            if (wallState != null)
                wallState.SetOnWall(false);
        }
    }
}