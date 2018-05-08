using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyController : MonoBehaviour {

    [Header("Shooting Behaviour")]
    [Tooltip("Time to spend charging up the shot before firing (telegraph)")]
    public float chargeUpDuration = 2.0f;
    [Tooltip("Time to wait after shooting before taking next action")]
    public float cooldownAfterShooting = 2.0f;

    [Header("Movement Behaviour")]
    public NavMeshQueryFilter moveFilter;
    public float awakenRange = 50;
    public float movementSpeed = 10;

    private NavMeshAgent navMeshAgent = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    IEnumerator WaitForPlayer()
    {
        while(GameManager.Instance.Player)
        {
            Transform playerTransform = GameManager.Instance.Player.transform;
            if(Vector3.Distance(playerTransform.position, this.transform.position) < awakenRange)
            {
                StartCoroutine(MoveToPositionWithLineOfSight());
                break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator MoveToPositionWithLineOfSight()
    {
        if(navMeshAgent)
        {
            Vector3 targetPosition = Vector3.zero;
            NavMeshHit hit;
            if(NavMesh.Raycast(this.transform.position, targetPosition, out hit, moveFilter))
            {
                yield return null;
            }
        }
    }



    /* Range behaviour
     * Check for player periodically - become active if they are in range
     * Find a path with line of sight
     * Move to new position
     * Fire at player if they are in view
     * % chance of moving after fire if player is still in view
     * 
     * */

}
