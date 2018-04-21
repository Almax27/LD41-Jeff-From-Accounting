using UnityEngine;
using System.Collections;

public class FollowCamera2D : MonoBehaviour
{
	public enum DeadZoneMode
	{
		None,
		Drag, //player will drag camera around when they reach the deadzone extents
		Center //center the camera on the player
	}

	public Transform initialTarget = null;
    [Tooltip("world rect in world space")]
    public Rect worldRect = new Rect(-500, -500, 500, 500);
    [Tooltip("Approximately the time it will take to reach the target ")]
	public float smoothTime = 0.1f;
	[Tooltip("Maximum camera speed")]
	public float maxSpeed = 150.0f;
	[Tooltip("Camera offset from the target")]
	public Vector2 offset = new Vector2(0, 0);
	[Tooltip("Normalised deadzone in screenspace")]
	public Vector2 deadZone = new Vector2(0.1f, 0.1f);
	public DeadZoneMode deadZoneMode = DeadZoneMode.None;

    //target
    [ReadOnly]
    public Transform target = null;

    //movement
    [ReadOnly]
    public Vector2 followVelocity = Vector2.zero;

    [ReadOnly]
    public Vector2 desiredPosition = Vector2.zero;

    //flags
    [ReadOnly]
    public bool pendingSnap = true;

    [ReadOnly]
    public bool deadZoneCenterLock = false;

    //camera cache
    [ReadOnly]
    public Camera cam = null;

	//set the target transform to follow
	public void SetTarget(Transform newTarget, bool snapTo = true)
	{
		target = newTarget;
		if (snapTo)
		{
			RequestSnapToTarget();
		}
	}

	//attempt to snap to target
	public void RequestSnapToTarget()
	{
		pendingSnap = true;
	}

	// Use this for initialization
	void OnEnable()
	{
		cam = GetComponent<Camera>();
	}

	// Update is called once per frame
	void LateUpdate()
	{
		Vector2 pos = transform.position;
		float zPos = transform.position.z;

		Vector2 viewHalfSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
		Vector2 deadZoneHalfSize = Vector2.Scale(viewHalfSize, deadZone);

		if (target != null)
		{
			if (pendingSnap)
			{
				transform.position = target.position + (Vector3)offset;
				followVelocity = Vector2.zero;
				pendingSnap = false;
			}
			else
			{
				if (deadZoneMode != DeadZoneMode.None && deadZoneHalfSize != Vector2.zero)
				{
					var fromCenter = (Vector2)target.position - pos;
					bool isOutsideDeadZone = Mathf.Abs(fromCenter.x) > Mathf.Abs(deadZoneHalfSize.x) || Mathf.Abs(fromCenter.y) > Mathf.Abs(deadZoneHalfSize.y);

					switch (deadZoneMode)
					{
						case DeadZoneMode.Drag:
							if (isOutsideDeadZone)
							{
								var fromDeadZone = fromCenter - Vector2.Scale(fromCenter.normalized, deadZoneHalfSize);
								desiredPosition = pos + fromDeadZone;
							}
							break;
						case DeadZoneMode.Center:
							if (isOutsideDeadZone || (deadZoneCenterLock && Vector2.Distance(pos, target.position) > 0.01f))
							{
								desiredPosition = target.position;
								deadZoneCenterLock = true;
							}
							else
							{
								deadZoneCenterLock = false;
							}
							break;
					}
				}
				else
				{
					desiredPosition = target.position;
				}

				if (smoothTime > 0)
				{
					float maxSmoothSpeed = maxSpeed > 0 ? maxSpeed : float.MaxValue;
					pos = Vector2.SmoothDamp(transform.position, desiredPosition + offset, ref followVelocity, smoothTime, maxSmoothSpeed, Time.smoothDeltaTime);					
				}
				else if(maxSpeed > 0)
				{
					var deltaPos = Vector2.ClampMagnitude(desiredPosition - pos, maxSpeed) * Time.deltaTime;
					pos += deltaPos + offset;
				}
				else
				{
					pos = desiredPosition + offset;
				}
			}
		}
		else
		{
			pendingSnap = true;
		}

        //clamp to world bounds
        { 
            Rect clampRect = new Rect(worldRect);

            //adjust size by view
            clampRect.width = Mathf.Max(0, clampRect.width * 0.5f - viewHalfSize.x);
            clampRect.height = Mathf.Max(0, clampRect.height * 0.5f - viewHalfSize.y);

            //clamp
			pos.x = Mathf.Clamp(pos.x, clampRect.xMin, clampRect.xMax);
			pos.y = Mathf.Clamp(pos.y, clampRect.yMin, clampRect.yMax);
		}

        //update position
        transform.position = new Vector3(pos.x, pos.y, zPos);
    }

	void OnDrawGizmosSelected()
	{
		//grab cam component as this may be called from the editor
		cam = GetComponent<Camera>();

		Vector2 viewSize = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize) * 2.0f;

        //draw world bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldRect.position, worldRect.size);

		//draw camera view area
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(transform.position, viewSize);

		//draw deadzone area
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, Vector2.Scale(viewSize, deadZone));
	}
}