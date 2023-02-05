using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 centeredPosition = transform.position;
        Vector2 objectSize = new Vector2(1,1);
        
        // Vector2 point, Vector2 size, float angle, ContactFilter2D contactFilter, Collider2D[] results
        Collider2D[] collision = Physics2D.OverlapBoxAll(centeredPosition, objectSize, 0);
        if (collision == null || collision.Length == 0)
        {
            Debug.Log("Collision was null");
        }
        else
        {
            for (int i = 0; i < collision.Length; i++)
            {
                if (collision[i].gameObject.layer == LayerMask.NameToLayer("Unit") && !collision[i].isTrigger)
                {
                    Debug.Log("Collision: " + collision[i].gameObject.name);
                }
                
            }
        }
    }
}
