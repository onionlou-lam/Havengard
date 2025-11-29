using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public int speed;

    private float dirx, diry;
    Rigidbody2D body;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        dirx = Input.GetAxisRaw("Horizontal");
        diry = Input.GetAxisRaw("Vertical");
        detectTile();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.DrawRay(new Vector3(mousePos.x, mousePos.y), new Vector3(0, 0, 0), Color.red);
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(speed * dirx, speed * diry);
    }

    Vector2 worldPoint;
    RaycastHit2D hit;

    // Update is called once per frame
    private void detectTile()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("test2");
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.DrawRay(new Vector3(mousePos.x, mousePos.y), new Vector3(0,0,0), Color.red);
            Debug.Log(mousePos);
            if (Physics2D.Raycast(mousePos, Vector2.zero))
            {
                Debug.Log("hit");
                Debug.Log(hit.point);
            };
            //worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.DrawLine(transform.position, transform.forward, Color.green);
            //Debug.Log("test3");
            ////hit = Physics2D.Raycast(worldPoint, Vector2.down);
            //Debug.Log(hit.point);
            
            
        }
    }
}
