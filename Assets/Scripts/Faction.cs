using UnityEngine;

public enum Faction
{
    Neutral,
    Friendly,
    Enemy
}

public class FactionProvider : MonoBehaviour
{
    public Faction faction = Faction.Neutral;
    public Faction GetFaction() => faction;
}
