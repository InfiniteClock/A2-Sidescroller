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
        Vector3 desiredPos = target.position + new Vector3(offset.x, offset.y, transform.position.z);
        Vector3 smoothPos = Vector3.Lerp(transform.position, desiredPos, 1 - Mathf.Exp(-smoothing * Time.deltaTime));
        smoothPos.x = Mathf.Clamp(smoothPos.x, leftBoundLimit, rightBoundLimit);
        smoothPos.y = Mathf.Clamp(smoothPos.y, bottomBoundLimit, smoothPos.y);

        transform.position = smoothPos;
    }
}
