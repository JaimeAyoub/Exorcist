using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrulla")] public Transform pointA;

    public Transform pointB;
    public float patrolSpeed = 2f;
    public float waitTime = 0.5f;

    [Header("Visión y persecución")] public Transform player;

    public float chaseSpeed = 4f;
    public float visionRange = 5f;
    [Range(0, 360)] public float visionAngle = 120f;
    public float maxChaseDistance = 8f; // distancia máxima desde donde empezó a perseguir
    public float attackRange = 0.8f; // al llegar aquí "alcanzó" al jugador

    [Header("General")] public bool is2D = true; // true = plano XY, false = plano XZ

    public float arriveThreshold = 0.1f;
    private Vector3 chaseStartPos;
    private Transform currentTarget;
    private Vector3 facing;

    private State state = State.Patrol;
    private float waitTimer;

    private void Awake()
    {
        facing = is2D ? Vector3.right : transform.forward;
        currentTarget = pointB;
    }

    private void Update()
    {
        if (CombatManager.Instance.isCombat) return;
        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Return: DoReturn(); break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA && pointB)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying && state == State.Chase ? chaseStartPos : transform.position,
            maxChaseDistance);
    }

    // ---------- PATRULLA ----------
    private void DoPatrol()
    {
        if (CanSeePlayer())
        {
            StartChase();
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        MoveTo(currentTarget.position, patrolSpeed);

        if (FlatDistance(currentTarget.position) <= arriveThreshold)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
            waitTimer = waitTime;
        }
    }

    // ---------- PERSECUCIÓN ----------
    private void StartChase()
    {
        state = State.Chase;
        chaseStartPos = transform.position;
    }

    private void DoChase()
    {
        // Se alejó demasiado del punto donde empezó: se rinde
        if (Vector3.Distance(transform.position, chaseStartPos) > maxChaseDistance)
        {
            state = State.Return;
            currentTarget = ClosestPatrolPoint();
            return;
        }

        if (FlatDistance(player.position) <= attackRange)
        {
            OnReachedPlayer();
            return;
        }

        MoveTo(player.position, chaseSpeed);
    }

    // ---------- REGRESO ----------
    private void DoReturn()
    {
        MoveTo(currentTarget.position, patrolSpeed);

        if (FlatDistance(currentTarget.position) <= arriveThreshold)
        {
            state = State.Patrol;
            currentTarget = currentTarget == pointA ? pointB : pointA;
            waitTimer = waitTime;
        }
    }

    // ---------- UTILIDADES ----------
    // Aquí pones tu daño, game over, iniciar combate, etc.
    private void OnReachedPlayer()
    {
        Debug.Log($"{name} alcanzó al jugador");
    }

    private bool CanSeePlayer()
    {
        var toPlayer = Flatten(player.position) - transform.position;
        if (toPlayer.magnitude > visionRange) return false;
        return Vector3.Angle(facing, toPlayer) <= visionAngle * 0.5f;
    }

    private Transform ClosestPatrolPoint()
    {
        return FlatDistance(pointA.position) <= FlatDistance(pointB.position) ? pointA : pointB;
    }

    private void MoveTo(Vector3 target, float speed)
    {
        target = Flatten(target);
        var dir = target - transform.position;
        if (dir.sqrMagnitude > 0.0001f) facing = dir.normalized;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        if (is2D)
        {
            // Asume que el sprite mira a la derecha por defecto
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (facing.x >= 0 ? 1 : -1);
            transform.localScale = s;
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
        }
    }

    // Ignora el eje que no cuenta (Z en 2D, Y en 3D)
    private Vector3 Flatten(Vector3 p)
    {
        if (is2D) p.z = transform.position.z;
        else p.y = transform.position.y;
        return p;
    }

    private float FlatDistance(Vector3 p)
    {
        return Vector3.Distance(transform.position, Flatten(p));
    }

    private enum State
    {
        Patrol,
        Chase,
        Return
    }
}