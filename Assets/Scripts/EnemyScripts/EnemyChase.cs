using System.Net.NetworkInformation;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.Cinemachine;

public class EnemyChase : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float angleThreshold = 0.75f; // angle threshold for player looking at enemy
    private Transform enemyPosition;
    private NavMeshAgent agent;

    // line of sight details
    [Header("Line of Sight")]
    [SerializeField] private float visionDistance = 10f;
    [SerializeField] private float visionAngle = 360f;
    [SerializeField] private LayerMask obstacleMask; // idk why i have this but we might need this, loadbearing layermask

    // chase
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stoppingDistance = 1.5f;


    // patrol points
    [Header("Patrol")]
    private bool isPatrolling = false;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float waitTime = 0f;
    private float patrolStoppingDistance;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool waitingAtPoint = false;

    // stun
    // idk we can tweak this
    [Header("Stun (durations)")]
    [SerializeField] private float minStunDuration = 0f;
    [SerializeField] private float maxStunDuration = 5f;
    [SerializeField] private float maxStunDistance = 10f;

    // cinemachine shake source
    [Header("Camera Shake")]
    [SerializeField] private float shakeInterval = 0.5f; // seconds
    private float lastShakeTime = 0f; // track last shake time
    
    private CinemachineImpulseSource impulseSource;
    // PUT THIS WHEREVER YOU CALL THE SOUND: CameraShakeManager.Instance.CameraShake(impulseSource, 1f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get enemy position
        enemyPosition = GetComponent<Transform>();

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    //ENEMY STATES
    public enum EnemyStates
    {
        Idle,
        Chasing,
        Searching,
        Inactive
    }

    private EnemyStates state = EnemyStates.Idle; // DEFAULT ENEMY STATE

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolStoppingDistance = agent.stoppingDistance;
    }

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
        
        return true; // else return true -> saw player
    }

    // Update is called once per frame
    void Update()
    {
        //if (CanSeePlayer())
        //{
        //    Debug.Log("spotted the player, now chasing");
        //}
        if (!agent.isOnNavMesh) return;

        switch (state)
        {
            case EnemyStates.Idle:
                if (CanSeePlayer())
                {
                    StartChase();
                }
                else
                {
                    Patrol();
                }
                break;

            case EnemyStates.Chasing:
                Chase();
                break;

            case EnemyStates.Inactive:
                // no need to write anything here -> gotta check player side
                break;
        }
    }

    // PATROLLING THE MAZE - IDLE
    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        //AudioManager.Instance.PlayMusic(AudioManager.Instance.backgroundMusic); // force ambiance music

        agent.speed = patrolSpeed;

        // waiting at patrol point
        if (waitingAtPoint)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                //Debug.Log("finished waiting at point" + currentPatrolIndex);
                waitTimer = 0f;
                waitingAtPoint = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                isPatrolling = false;
            }
            return;
        }

        // start moving to patrol point
        if (!isPatrolling)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            isPatrolling = true;
            //Debug.Log("moving towards " + currentPatrolIndex);
        }

        // check if dest has been reached
        if (isPatrolling && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                // DEBUGGING: FUCKING STOOOOOOP MOVINGGGG
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                {
                    //Debug.Log("reached point" + currentPatrolIndex);
                    agent.ResetPath();
                    isPatrolling = false;
                    waitingAtPoint = true;
                    waitTimer = 0f;
                }
            }
        }
    }

    // CHASING
    private void StartChase()
    {
        // stop patrolling behaviour and start dashing towards player :sob:
        state = EnemyStates.Chasing;
        
        isPatrolling = false;
        waitingAtPoint = false ;
        waitTimer = 0f;

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.screamSFX); // monster screeches when sees player

        // set dest to player
        agent.SetDestination(player.position);
        Debug.Log("started chasing player");
    }

    private void Chase()
    {
        if (player == null) return;
        agent.SetDestination(player.position); // keep updating player's position
        
        // shake camera on an interval

        if (Time.time - lastShakeTime >= shakeInterval)
        {
            CameraShakeManager.Instance.CameraShake(impulseSource, 1f);
            lastShakeTime = Time.time;
        }

        
        //AudioManager.Instance.PlayMusic(AudioManager.Instance.chaseMusic); // chase music on a longer timer

    }

    // STUN

    public void Stun()
    {
        if (state == EnemyStates.Inactive) return; // ignore if enemy alr stunned
        if (player == null) return;
        // ignore if player not looking at enemy
        Vector3 directionToEnemy = (enemyPosition.position - playerCamera.position).normalized;
        float dotProduct = Vector3.Dot(playerCamera.forward, directionToEnemy);

        if (dotProduct < angleThreshold)
        {
            Debug.Log("player not looking at enemy, stun ignored");
            return;
        }
        

        float distance = Vector3.Distance(transform.position, player.position);
        float t = Mathf.Clamp01(distance / maxStunDistance); // normalized dist -> interpolation factor
        
        // find duration of stun
        // if enemy closer -> stun duration longer
        // LINEAR INTERPOLATION..............
        float duration = Mathf.Lerp(maxStunDuration, minStunDuration, t); 
        if (duration <= 0f)
        {
            Debug.Log("enemy too far away, stun ignored");
            return;
        }

        StartCoroutine(StunCoroutine(duration));
    }

    /*
     * 1. stop enemy immediately
     * 2. wait for stun to expire
     * 3. recover
     * 4. potentially reset patrol state
     */
    private IEnumerator StunCoroutine(float duration)
    {
        state = EnemyStates.Inactive; // stun state

        // stop enemy
        agent.isStopped = true;
        agent.ResetPath();
        Debug.Log("enemy stunned for " + duration);

        // wait for stun to be over
        yield return new WaitForSeconds(duration);

        // recover
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = patrolStoppingDistance;

        // reset
        isPatrolling = false;
        waitingAtPoint = false;
        waitTimer = 0f;
        state = EnemyStates.Idle;
        Debug.Log("enemy recovered and returning to patrol");
    }


}
