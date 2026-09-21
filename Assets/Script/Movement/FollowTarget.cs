using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;

    private Vector3 initialOffset;
    private Rigidbody2D rb;
    private SpriteRenderer mySR;
    private SpriteRenderer targetSR;
    private PlayerAnimateEP1 myAnimate;

    private Vector3 lastPosition;
    private bool wasMoving = false;
    private bool isFollowing = true;

    [Header("Control")]
    public bool disableControl = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mySR = GetComponent<SpriteRenderer>();
        myAnimate = GetComponent<PlayerAnimateEP1>();

        if (target != null)
        {
            initialOffset = transform.position - target.position;
            targetSR = target.GetComponent<SpriteRenderer>();
        }

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        // Jangan follow kalau isFollowing = false
        if (target == null || !isFollowing) return;

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
            float moveThreshold = 0.001f; // Threshold untuk deteksi movement
            bool isMoving = (transform.position - lastPosition).sqrMagnitude > moveThreshold * moveThreshold;

            // Trigger animation kalau state berubah
            if (isMoving != wasMoving)
            {
                wasMoving = isMoving;
                // Pakai SetMoving() untuk sync dengan PlayerAnimateEP1
                myAnimate.SetMoving(isMoving);
            }
        }

        lastPosition = transform.position;
    }

    public void StopFollowing()
    {
        isFollowing = false;
        // Set idle saat berhenti follow
        if (myAnimate != null)
        {
            myAnimate.SetMoving(false);
        }
        Debug.Log("FollowTarget: Stopped following");
    }

    public void StartFollowing()
    {
        isFollowing = true;
        Debug.Log("FollowTarget: Started following");
    }

    public bool IsFollowing()
    {
        return isFollowing;
    }
}
