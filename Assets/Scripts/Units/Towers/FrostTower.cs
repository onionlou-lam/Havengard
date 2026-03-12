using Havengard.Core.HealthSystem;
using Havengard.Units;
using UnityEngine;

public class FrostTower : UnitBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

    override protected GameObject FindTarget()
    {
        // find the closest enemy within aggro range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
         foreach (var hit in hits)
         {
             if (hit.TryGetComponent<Health>(out var h))
             {
                 if (h.GetFaction() == Faction.Enemy)
                 {
                    Debug.Log("FrostTower found target: " + hit.gameObject.name);
                    return hit.gameObject;
                 }
             }
        }
        return null;
    }

    protected override void PerformAttack(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
