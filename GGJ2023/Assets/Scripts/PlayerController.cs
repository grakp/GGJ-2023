using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Components
    public Rigidbody2D rb;

    // Player
    public float walkSpeed = 4f;
    float inputHorizontal;
    float inputVertical;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate() 
    {
        if (inputHorizontal != 0 || inputVertical != 0)
        {
            Vector2 moveDir = new Vector2(inputHorizontal, inputVertical).normalized;
            rb.velocity = moveDir * walkSpeed;
        }
        else
        {
          rb.velocity = new Vector2(0f, 0f);
        }
    }
}
