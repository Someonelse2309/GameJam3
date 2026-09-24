using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    [Header("Sprite Settings")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites; // Drag frame foto idle NPC ke sini
    public float frameRate = 0.12f; // Kecepatan pergantian frame

    private int currentFrame;
    private float timer;

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (idleSprites == null || idleSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame = (currentFrame + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[currentFrame];
        }
    }
}