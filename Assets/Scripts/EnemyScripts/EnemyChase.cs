using System.Net.NetworkInformation;
using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [SerializeField] private Transform player;

    // line of sight details
    [SerializeField] private float visionDistance = 10f;
    [SerializeField] private float visionAngle = 45f;
    [SerializeField] private LayerMask obstacleMask; // wall

    // chase
    [SerializeField] private float chaseSpeed = 5f;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
        
    //}

    //ENEMY STATES
    public enum EnemyStates
    {
        Idle,
        Chasing,
        Searching,
        Inactive
    }

    private EnemyStates state = EnemyStates.Idle; // DEFAULT ENEMY STATE



    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 target = player.position + Vector3.up;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        // check distance and angle
        if (distance > visionDistance) return false;

        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > visionAngle / 2f) return false;

        // check for obstacles
        if (Physics.Raycast(origin, direction.normalized, distance, obstacleMask)) return false;

        // else return true -> saw player
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (CanSeePlayer())
        {
            Debug.Log("spotted the player, now chasing");
        }
    }
}
