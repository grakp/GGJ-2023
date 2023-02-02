using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Components
    public Rigidbody2D rb;
    public BaseUnit self;

    public PlayerInteractionController interactionController;

    // Player
    public float walkSpeed = 4f;
    float inputHorizontal;
    float inputVertical;
    bool inputBasicAttack;

    int amountWood = 0;
    int amountWater = 0;
    int amountRock = 0;
    int amountLeaf = 0;

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
        HandleInteractableInput();
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

    void HandleInteractableInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TiledGameObject interactObject = interactionController.GetInteractObject();
            if (interactObject != null)
            {
                interactObject.Interact(this);
            }
        }
    }

    public void Give(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                amountWood += amount;
                break;
            case ResourceType.Water:
                amountWater += amount;
                break;
            case ResourceType.Rock:
                amountRock += amount;
                break;
            case ResourceType.Leaf:
                amountLeaf += amount;
                break;
        }


        Debug.Log("Amount wood: " + amountWood);
        Debug.Log("Amount water: " + amountWater);
        Debug.Log("Amount rock: " + amountRock);
        Debug.Log("Amount leaf: " + amountLeaf);
    }
}
