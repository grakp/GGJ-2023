using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Components
    public Rigidbody2D rb;
    public BaseUnit self;

    // Player
    public float walkSpeed = 4f;
    float inputHorizontal;
    float inputVertical;
    bool inputBasicAttack;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        self = GetComponent<BaseUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
        inputBasicAttack = Input.GetButtonDown("Fire1") || inputBasicAttack;
    }

    void FixedUpdate() 
    {
        handleMovementInput();
        handleCursorMovementInput();
        handleAttackInput();
    }

    // Helpers //
    void handleMovementInput() {
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

    void handleCursorMovementInput() {        
        // Keep track of facing direction
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        self.SetDirection((new Vector3(mousePoint.x, mousePoint.y, transform.position.z) - transform.position).normalized);
    }

    void handleAttackInput() {
        if (inputBasicAttack) {
            // (Benji): Setting inputBasicAttack to false first due to concurrency issues mixing Update() and FixedUpdate()
            inputBasicAttack = false;
            self.UseAbility("Swing");         
        }
    }
}
