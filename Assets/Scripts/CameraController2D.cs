using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController2D : MonoBehaviour
{

    [SerializeField] private Camera followCam;
    private Vector2 viewportHalfSize;
    private float leftBoundLimit;
    private float rightBoundLimit;
    private float bottomBoundLimit;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothing = 5f;

    private Vector3 shakeOffset = Vector3.zero;
    private Coroutine shaking;

    void Start()
    {
        tilemap.CompressBounds();
        CalculateCameraBounds();
    }

    private void CalculateCameraBounds()
    {
        viewportHalfSize = new Vector2(followCam.orthographicSize * followCam.aspect, followCam.orthographicSize);

        leftBoundLimit = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewportHalfSize.x;
        rightBoundLimit = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewportHalfSize.x;
        bottomBoundLimit = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewportHalfSize.y;

    }

    public void LateUpdate()
    {
        // Testing only
        if (Input.GetKey(KeyCode.L)) Shake(10f, 2f);

        Vector3 desiredPos = target.position + new Vector3(offset.x, offset.y, transform.position.z) + shakeOffset;
        Vector3 smoothPos = Vector3.Lerp(transform.position, desiredPos, 1 - Mathf.Exp(-smoothing * Time.deltaTime));
        smoothPos.x = Mathf.Clamp(smoothPos.x, leftBoundLimit, rightBoundLimit);
        smoothPos.y = Mathf.Clamp(smoothPos.y, bottomBoundLimit, smoothPos.y);

        transform.position = smoothPos;
    }
    public void Shake(float intensity, float duration)
    {
        shaking = StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            shakeOffset = Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
