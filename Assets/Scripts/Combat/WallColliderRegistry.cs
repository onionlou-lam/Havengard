using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Havengard.Combat
{
    /// <summary>
    /// Central registry of all wall colliders (the tagged "Wall" TilemapCollider2D
    /// plus individual sprite-based wall objects like gates).
    /// Used so projectiles can selectively ignore wall collision on a per-instance basis
    /// (e.g. when fired from atop the wall) without affecting global layer collision.
    /// </summary>
    public static class WallColliderRegistry
    {
        private static readonly List<Collider2D> wallColliders = new List<Collider2D>();
        private static bool initialized;

        public static void Register(Collider2D col)
        {
            if (col != null && !wallColliders.Contains(col))
                wallColliders.Add(col);
        }

        public static void Unregister(Collider2D col)
        {
            wallColliders.Remove(col);
        }

        /// <summary>
        /// Lazily gathers all wall colliders in the scene by tag, in case some
        /// weren't registered explicitly (fallback safety net).
        /// </summary>
        public static IReadOnlyList<Collider2D> GetAll()
        {
            if (!initialized)
            {
                initialized = true;

                var tagged = GameObject.FindGameObjectsWithTag("Wall");
                foreach (var go in tagged)
                {
                    var cols = go.GetComponents<Collider2D>();
                    foreach (var c in cols)
                        Register(c);

                    // TilemapCollider2D may live on a child/composite setup
                    var tilemapCol = go.GetComponent<TilemapCollider2D>();
                    if (tilemapCol != null)
                        Register(tilemapCol);
                }
            }

            return wallColliders;
        }

        /// <summary>
        /// Clears the cache - call on scene unload if you reload scenes without domain reload.
        /// </summary>
        public static void Clear()
        {
            wallColliders.Clear();
            initialized = false;
        }
    }
}