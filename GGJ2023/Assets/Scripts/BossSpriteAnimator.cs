using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpriteAnimator : MonoBehaviour
{
    public List<Sprite> sprites;
    public SpriteRenderer spriteRenderer;

    public float animInterval = 0.1f;

    private float currentInterval = 0.0f;
    private int currentIndex = 0;

    // Update is called once per frame
    void Update()
    {
        currentInterval += Time.deltaTime;
        if (currentInterval >= animInterval)
        {
            currentInterval = 0;
            currentIndex = (currentIndex + 1) % sprites.Count;
            spriteRenderer.sprite = sprites[currentIndex];
        }
    }
}
