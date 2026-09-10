using UnityEngine;
using System.Diagnostics;

[RequireComponent(typeof(Rigidbody2D))]
public class Rigidbody2DMonitor : MonoBehaviour
{
    private Rigidbody2D rb;
    private RigidbodyType2D lastBodyType;
    private float lastGravity;
    private RigidbodyConstraints2D lastConstraints;
    private bool logged = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastBodyType = rb.bodyType;
        lastGravity = rb.gravityScale;
        lastConstraints = rb.constraints;
    }

    // Update is called once per frame
    void Update()
    {
        if (logged) return;

        if (rb.bodyType != lastBodyType || !Mathf.Approximately(rb.gravityScale, lastGravity) || rb.constraints != lastConstraints)
        {
            UnityEngine.Debug.LogWarning($"[Rigidbody2DMonitor] Detected Rigidbody2D change on {name}: bodyType {lastBodyType} -> {rb.bodyType}, gravity {lastGravity} -> {rb.gravityScale}, constraints {lastConstraints} -> {rb.constraints}\nStack:\n{new StackTrace(true)}");
            logged = true;
        }
    }
}
