using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;

    private Vector3 initialOffset;
    private Rigidbody2D rb;
    private SpriteRenderer mySR;
    private SpriteRenderer targetSR;
    private PlayerAnimate myAnimate;

    private Vector3 lastPosition;
    private bool wasMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mySR = GetComponent<SpriteRenderer>();
        myAnimate = GetComponent<PlayerAnimate>();

        if (target != null)
        {
            initialOffset = transform.position - target.position;
            targetSR = target.GetComponent<SpriteRenderer>();
        }

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Update posisi
        Vector3 newPos = target.position + initialOffset;
        newPos.z = transform.position.z;

        if (rb != null)
        {
            rb.MovePosition(newPos);
        }
        else
        {
            transform.position = newPos;
        }

        // Sync flip sprite
        if (targetSR != null && mySR != null)
        {
            mySR.flipX = targetSR.flipX;
        }

        // Sync animation - cek apakah bergerak
        if (myAnimate != null)
        {
            bool isMoving = (transform.position - lastPosition).sqrMagnitude > 0.0001f;

            // Trigger animation kalau state berubah
            if (isMoving != wasMoving)
            {
                wasMoving = isMoving;
                // Pause/Resume animation berdasarkan movement
                if (isMoving)
                {
                    myAnimate.ResumeAnimation();
                }
                else
                {
                    myAnimate.PauseAnimation();
                    // Set ke frame 0 (idle)
                    if (myAnimate.normalSprites != null && myAnimate.normalSprites.Length > 0)
                    {
                        mySR.sprite = myAnimate.normalSprites[0];
                    }
                }
            }
        }

        lastPosition = transform.position;
    }
}
