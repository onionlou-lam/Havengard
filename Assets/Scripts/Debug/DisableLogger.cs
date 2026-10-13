using UnityEngine;

public class DisableLogger : MonoBehaviour
{
    private void OnDisable()
    {
        Debug.Log($"[DisableLogger] {name} disabled\n{System.Environment.StackTrace}", this);
    }
}