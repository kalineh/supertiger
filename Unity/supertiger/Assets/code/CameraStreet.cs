using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStreet
    : MonoBehaviour
{
    public GameObject FollowTarget;
    public GameObject CameraX0;
    public GameObject CameraX1;

    private Camera _camera;

    public void OnEnable()
    {
        _camera = GetComponent<Camera>();
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        var worldLeft = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelRect.xMin, 0.0f, 0.0f));
        var worldRight = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelRect.xMax, 0.0f, 0.0f));

        var range = worldRight - worldLeft;
        var follow = range * 0.30f;
        var offset = FollowTarget.transform.position.x - transform.position.x;
        var distance = Mathf.Abs(offset);
        var adjust = Mathf.Max(distance - follow.x, 0.0f);
        var direction = Mathf.Sign(offset);

        transform.position += new Vector3(adjust * direction, 0.0f, 0.0f);

        worldLeft = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelRect.xMin, 0.0f, 0.0f));
        worldRight = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelRect.xMax, 0.0f, 0.0f));

        var overflowLeft = CameraX0.transform.position.x - worldLeft.x;
        var overflowRight = worldRight.x - CameraX1.transform.position.x;
        var shiftLeftWorld = Mathf.Max(overflowLeft, 0.0f);
        var shiftRightWorld = Mathf.Max(overflowRight, 0.0f);

        transform.position += new Vector3(shiftLeftWorld, 0.0f, 0.0f);
        transform.position -= new Vector3(shiftRightWorld, 0.0f, 0.0f);
    }
}
