using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{

    public HashSet<TiledGameObject> interactableGameObjects{get; set;}

    void Start()
    {
        interactableGameObjects = new HashSet<TiledGameObject>();
    }

    void OnTriggerEnter2D(Collider2D other) {

        TiledGameObject tiledGameObject = other.GetComponent<TiledGameObject>();
        if (tiledGameObject == null)
        {
            return;
        }

        if (other.tag == "Interactable")
        {
            interactableGameObjects.Add(tiledGameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        TiledGameObject tiledGameObject = other.GetComponent<TiledGameObject>();
        if (tiledGameObject == null)
        {
            return;
        }

        if (interactableGameObjects.Contains(tiledGameObject))
        {
            tiledGameObject.UnInteract();
            interactableGameObjects.Remove(tiledGameObject);
        }
    }

    public TiledGameObject GetInteractObject()
    {
        if (interactableGameObjects.Count == 0)
        {
            return null;
        }

        HashSet<TiledGameObject>.Enumerator enumerator = interactableGameObjects.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.Current;
    }
}
