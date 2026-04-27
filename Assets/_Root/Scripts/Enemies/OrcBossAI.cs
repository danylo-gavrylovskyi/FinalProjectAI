using UnityEngine;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthBehaviour))]
    public class OrcBossAI : MonoBehaviour
    {
        [Header("Distances")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private float attackRadius    = 1.5f;
        [SerializeField] private float waypointReachedDist = 0.4f;
        
        [Header("Speeds")]
        [SerializeField] private float patrolSpeed  = 1.2f;
        [SerializeField] private float chaseSpeed   = 2.8f;
        [SerializeField] private float retreatSpeed = 3.5f;
        
        [Header("Patrol")]
        [Tooltip("Random waypoints are chosen within this radius of the spawn point.")]
        [SerializeField] private float patrolRadius = 4f;
        [SerializeField] private float patrolWaitMin = 0.5f;
        [SerializeField] private float patrolWaitMax = 1.5f;
        
        [Header("Retreat & Heal")]
        [Range(0f, 1f)]
        [SerializeField] private float retreatHpPercent = 0.3f;
        [SerializeField] private float healPerSecond = 15f;
        [SerializeField] private float bonfireHealRadius = 1.5f;
        
        private enum State { Patrol, Chase, Attack, Retreat }
        private State currentState = State.Patrol;

        private Rigidbody2D rb;
        private HealthBehaviour health;
        private Transform playerTransform;

        private Vector2 spawnPoint;
        private Vector2 patrolTarget;
        private float patrolWaitTimer;

        private Transform currentBonfire;
        private bool retreatTriggered;

        private void Awake()
        {
            rb     = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthBehaviour>();
        }

        private void Start()
        {
            spawnPoint   = transform.position;
            patrolTarget = PickPatrolPoint();

            if (GameLoop.GameManager.Instance != null)
                playerTransform = GameLoop.GameManager.Instance.PlayerTransform;
        }

        private void Update()
        {
            if (playerTransform == null && GameLoop.GameManager.Instance != null)
                playerTransform = GameLoop.GameManager.Instance.PlayerTransform;
        }

        private void FixedUpdate()
        {
            if (health.CurrentHealth <= 0)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            EvaluateTransitions();

            switch (currentState)
            {
                case State.Patrol:  TickPatrol();  break;
                case State.Chase:   TickChase();   break;
                case State.Attack:  TickAttack();  break;
                case State.Retreat: TickRetreat(); break;
            }
        }

        private void EvaluateTransitions()
        {
            if (!retreatTriggered && HpBelowThreshold())
            {
                retreatTriggered = true;
                currentBonfire   = FindNearestBonfire();
                if (currentBonfire != null)
                {
                    EnterState(State.Retreat);
                    return;
                }
            }

            if (currentState == State.Retreat) return;

            if (currentState == State.Patrol && PlayerInRange(detectionRadius))
            {
                EnterState(State.Chase);
                return;
            }

            if (currentState == State.Chase)
            {
                if (PlayerInRange(attackRadius))  { EnterState(State.Attack); return; }
                if (!PlayerInRange(detectionRadius * 1.3f)) { EnterState(State.Patrol); return; }
            }

            if (currentState == State.Attack && !PlayerInRange(attackRadius))
            {
                EnterState(State.Chase);
            }
        }

        private void EnterState(State next)
        {
            currentState = next;

            if (next == State.Patrol)
            {
                patrolWaitTimer = 0f;
                patrolTarget    = PickPatrolPoint();
            }
        }

        private void TickPatrol()
        {
            if (patrolWaitTimer > 0f)
            {
                patrolWaitTimer -= Time.fixedDeltaTime;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            var toTarget = patrolTarget - (Vector2)transform.position;
            if (toTarget.magnitude <= waypointReachedDist)
            {
                patrolTarget    = PickPatrolPoint();
                patrolWaitTimer = Random.Range(patrolWaitMin, patrolWaitMax);
                rb.linearVelocity = Vector2.zero;
                return;
            }

            MoveToward(patrolTarget, patrolSpeed);
        }

        private void TickChase()
        {
            if (playerTransform == null) { rb.linearVelocity = Vector2.zero; return; }
            MoveToward(playerTransform.position, chaseSpeed);
        }

        private void TickAttack()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void TickRetreat()
        {
            if (currentBonfire == null)
            {
                currentBonfire = FindNearestBonfire();
                if (currentBonfire == null) { EnterState(State.Chase); return; }
            }

            float distToBonfire = Vector2.Distance(transform.position, currentBonfire.position);

            if (distToBonfire <= bonfireHealRadius)
            {
                rb.linearVelocity = Vector2.zero;
                health.Heal(Mathf.RoundToInt(healPerSecond * Time.fixedDeltaTime));

                if (health.CurrentHealth >= health.MaxHealth)
                {
                    retreatTriggered = false;
                    EnterState(State.Chase);
                }
            }
            else
            {
                MoveToward(currentBonfire.position, retreatSpeed);
            }
        }

        private void MoveToward(Vector2 target, float speed)
        {
            var dir = (target - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * speed;
        }

        private bool PlayerInRange(float radius)
        {
            if (playerTransform == null) return false;
            return Vector2.Distance(transform.position, playerTransform.position) <= radius;
        }

        private bool HpBelowThreshold()
        {
            if (health.MaxHealth <= 0) return false;
            return (float)health.CurrentHealth / health.MaxHealth <= retreatHpPercent;
        }
        
        private Transform FindNearestBonfire()
        {
            var bonfires = GameObject.FindGameObjectsWithTag("Bonfire");
            Transform nearest = null;
            float bestDist = float.MaxValue;

            foreach (var b in bonfires)
            {
                float d = Vector2.Distance(transform.position, b.transform.position);
                if (d < bestDist) { bestDist = d; nearest = b.transform; }
            }

            return nearest;
        }

        private Vector2 PickPatrolPoint()
        {
            var offset = Random.insideUnitCircle * patrolRadius;
            return spawnPoint + offset;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, bonfireHealRadius);

#if UNITY_EDITOR
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                $"State: {currentState}", style);
#endif
        }
    }
}