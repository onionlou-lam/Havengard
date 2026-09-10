using UnityEngine;

namespace Havengard.Player
{
    /// <summary>
    /// Tracks whether this unit is currently standing on the wall-top walkway.
    /// Used to determine whether outgoing projectiles should ignore wall collision
    /// (shooting down from the wall) vs. respect it (shooting at the wall from outside/below).
    /// </summary>
    public class PlayerWallState : MonoBehaviour
    {
        public bool IsOnWall { get; private set; }

        public void SetOnWall(bool onWall)
        {
            IsOnWall = onWall;
        }
    }
}