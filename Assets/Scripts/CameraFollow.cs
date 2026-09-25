using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;

    private Camera attachedCamera;
    private Vector3 velocity;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null || arenaBounds == null || !attachedCamera.orthographic)
            return;
        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        float halfHeight = attachedCamera.orthographicSize;
        float halfWidth = halfHeight * attachedCamera.aspect;
        Vector2 min = arenaBounds.Minimum;
        Vector2 max = arenaBounds.Maximum;

        smoothed.x = ClampCameraAxis(smoothed.x, min.x + halfWidth, max.x - halfWidth);
        smoothed.y = ClampCameraAxis(smoothed.y, min.y + halfHeight, max.y - halfHeight);
        transform.position = smoothed;
    }

    private static float ClampCameraAxis(float value, float minimum, float maximum)
    {
        if (minimum > maximum)
        {
            return (minimum + maximum) * 0.5f;
        }
        return Mathf.Clamp(value, minimum, maximum);
    }
}
