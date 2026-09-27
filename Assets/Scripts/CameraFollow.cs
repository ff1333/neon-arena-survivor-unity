using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;

    [Header("Shake")]
    [SerializeField, Min(0.01f)] private float shakeDuration = 0.08f;
    [SerializeField, Min(0f)] private float shakeStrength = 0.08f;

    private Camera attachedCamera;
    private Vector3 followPosition;
    private Vector3 velocity;
    private float shakeTimeRemaining;
    private float currentShakeStrength;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
        followPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null || arenaBounds == null || !attachedCamera.orthographic)
        {
            return;
        }

        Vector3 desired = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z);

        followPosition = Vector3.SmoothDamp(
            followPosition,
            desired,
            ref velocity,
            smoothTime);

        float halfHeight = attachedCamera.orthographicSize;
        float halfWidth = halfHeight * attachedCamera.aspect;
        Vector2 min = arenaBounds.Minimum;
        Vector2 max = arenaBounds.Maximum;

        followPosition.x = ClampCameraAxis(
            followPosition.x,
            min.x + halfWidth,
            max.x - halfWidth);
        followPosition.y = ClampCameraAxis(
            followPosition.y,
            min.y + halfHeight,
            max.y - halfHeight);

        Vector3 finalPosition = followPosition;

        if (Time.timeScale <= 0f)
        {
            shakeTimeRemaining = 0f;
            currentShakeStrength = 0f;
        }
        else if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining = Mathf.Max(0f, shakeTimeRemaining - Time.deltaTime);
            float strengthRatio = shakeTimeRemaining / shakeDuration;
            Vector2 offset = Random.insideUnitCircle * currentShakeStrength * strengthRatio;
            finalPosition.x += offset.x;
            finalPosition.y += offset.y;

            if (shakeTimeRemaining <= 0f)
            {
                currentShakeStrength = 0f;
            }
        }

        finalPosition.x = ClampCameraAxis(
            finalPosition.x,
            min.x + halfWidth,
            max.x - halfWidth);
        finalPosition.y = ClampCameraAxis(
            finalPosition.y,
            min.y + halfHeight,
            max.y - halfHeight);

        transform.position = finalPosition;
    }

    public void Shake(float strengthMultiplier)
    {
        if (strengthMultiplier <= 0f)
        {
            return;
        }

        shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, shakeDuration);
        currentShakeStrength = Mathf.Max(
            currentShakeStrength,
            shakeStrength * strengthMultiplier);
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