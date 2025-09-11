using UnityEngine;

namespace Havengard.NPCs
{
    public interface IAlly
    {
        void FollowPlayer(GameObject player);
        void AssistAttack(GameObject target);
    }
}
