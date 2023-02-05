using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunInstantiateMagicCallback
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

    public int amountWood{get; set;}
    public int amountWater{get; set;}
    public int amountRock{get; set;}

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        self = GetComponent<BaseUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.networkingManager.IsDebuggingMode || base.photonView.IsMine)
        {
            inputHorizontal = Input.GetAxisRaw("Horizontal");
            inputVertical = Input.GetAxisRaw("Vertical");
            inputBasicAttack = Input.GetButtonDown("Fire1") || inputBasicAttack;
            HandleInteractableInput();
        }
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
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
        {
            TiledGameObject interactObject = interactionController.GetInteractObject();
            if (interactObject != null)
            {
                interactObject.Interact(this);
            }
        }

        // Cheats REMOVE ME
        if (Input.GetKeyDown(KeyCode.O))
        {
            Give(ResourceType.Wood, 99);
            Give(ResourceType.Water, 99);
            Give(ResourceType.Rock, 99);
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
        }

        GameManager.Instance.gameController.uiController.UpdateResourceText();
    }

    public void Give(ShopItemInstance shopItem)
    {
        // TODO:
        Debug.Log("Given item: " + shopItem.description);
    }

    public void Take(int wood, int water, int rock)
    {
        amountWood -= wood;
        amountWater -= water;
        amountRock -= rock;
        GameManager.Instance.gameController.uiController.UpdateResourceText();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // TODO make sure that self is/not included in list
        if (info.photonView.gameObject == null)
        {
            return;
        }

        GameManager.Instance.gameController.AddPlayer(this, info.Sender);

        /*

        PlayerController controller = info.photonView.gameObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            players.Add(controller);
        }*/
    }

}
