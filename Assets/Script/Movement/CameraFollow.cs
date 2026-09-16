using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Batas Peta (Camera Clamp)")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target != null)
        {
            // Hitung posisi yang diinginkan
            Vector3 desiredPosition = target.position + offset;

            // Batasi nilai X dan Y agar tidak melewati batas min & max
            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

            Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

            // Gerakkan kamera secara halus ke posisi yang sudah dibatasi
            transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed * Time.deltaTime);
        }
    }
}