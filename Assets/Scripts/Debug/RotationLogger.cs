using UnityEngine;
using System.Diagnostics;

public class RotationLogger : MonoBehaviour
{
    private Vector3 lastEuler;

    void Start()
    {
        lastEuler = transform.eulerAngles;
    }

    void Update()
    {
        Vector3 cur = transform.eulerAngles;
        if (!Mathf.Approximately(cur.x, lastEuler.x) ||
            !Mathf.Approximately(cur.y, lastEuler.y) ||
            !Mathf.Approximately(cur.z, lastEuler.z))
        {
            UnityEngine.Debug.LogWarning($"[RotationLogger] {name} rotation changed: {lastEuler} -> {cur}\nStack trace:\n{new StackTrace(true)}");
            lastEuler = cur;

            // stop after first change so log is readable
            enabled = false;
        }
    }
}