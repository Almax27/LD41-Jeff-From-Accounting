using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCaster : MonoBehaviour {

    public float heightOffFloor = 0.1f;
    public LayerMask floorMask = new LayerMask();

    public float scaleOffDistance = 1.0f;
    public float minScaleSize = 0.3f;
    public float maxScaleSize = 1.0f;

    Vector3 rayOffset = Vector3.up;

	void LateUpdate ()
    {
        RaycastHit hit;

        Vector3 position = transform.position;
        position.y -= transform.localPosition.y - 0.1f;

        if (Physics.Raycast(position, Vector3.down, out hit, float.PositiveInfinity, floorMask.value))
        {
            position.y = hit.point.y + heightOffFloor;
        }
        else
        {
            position.y = -10000;
        }

        transform.position = position;

        float relativeHeight = -transform.localPosition.y;
        float scale = 1.0f - Mathf.InverseLerp(heightOffFloor, scaleOffDistance, relativeHeight);
        scale = Mathf.Lerp(minScaleSize, maxScaleSize, scale);
        transform.localScale = new Vector3(scale, scale, scale);
	}
}
