using UnityEngine;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthBehaviour))]
    public class SkeletonAI : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.5f;
        
        [Header("Pursue")]
        [Tooltip("How far the skeleton can 'see' the player before it starts chasing.")]
        [SerializeField] private float detectionRadius = 12f;
        [SerializeField] private float pursueWeight = 1f;
        
        [Header("Separation")]
        [Tooltip("Radius within which this skeleton pushes away from others.")]
        [SerializeField] private float separationRadius = 1.2f;
        [SerializeField] private float separationWeight = 1.8f;
        
        [Header("Cohesion")]
        [Tooltip("Radius within which this skeleton weakly groups with others.")]
        [SerializeField] private float cohesionRadius = 4f;
        [SerializeField] private float cohesionWeight = 0.3f;
        
        private Rigidbody2D rb;
        private HealthBehaviour health;
        
        private Transform playerTransform;
        
        private readonly Collider2D[] neighbourBuffer = new Collider2D[32];
        
        private int skeletonLayerMask;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthBehaviour>();
            skeletonLayerMask = 1 << gameObject.layer;
        }

        private void Start()
        {
            if (GameLoop.GameManager.Instance != null)
                playerTransform = GameLoop.GameManager.Instance.PlayerTransform;
        }

        private void FixedUpdate()
        {
            if (playerTransform == null && GameLoop.GameManager.Instance != null)
                playerTransform = GameLoop.GameManager.Instance.PlayerTransform;
            
            if (health.CurrentHealth <= 0)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            var steering = Vector2.zero;
            steering += Pursue() * pursueWeight;
            steering += Separation() * separationWeight;
            steering += Cohesion() * cohesionWeight;
            
            if (steering.sqrMagnitude > 1f) steering.Normalize();
            rb.linearVelocity = steering * moveSpeed;
        }
        
        private Vector2 Pursue()
        {
            if (playerTransform == null) return Vector2.zero;

            var toPlayer = (Vector2)(playerTransform.position - transform.position);
            if (toPlayer.sqrMagnitude > detectionRadius * detectionRadius) return Vector2.zero;

            return toPlayer.normalized;
        }
        
        private Vector2 Separation()
        {
            var force = Vector2.zero;
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, separationRadius, neighbourBuffer, skeletonLayerMask);

            for (int i = 0; i < count; i++)
            {
                var col = neighbourBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;

                var away = (Vector2)(transform.position - col.transform.position);
                var dist = away.magnitude;
                if (dist < 0.001f) dist = 0.001f;  
                
                force += away.normalized / dist;
            }

            return force;
        }
        
        private Vector2 Cohesion()
        {
            var centre = Vector2.zero;
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, cohesionRadius, neighbourBuffer, skeletonLayerMask);

            int neighbours = 0;
            for (int i = 0; i < count; i++)
            {
                var col = neighbourBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;
                centre += (Vector2)col.transform.position;
                neighbours++;
            }

            if (neighbours == 0) return Vector2.zero;

            centre /= neighbours;
            return ((Vector2)(centre - (Vector2)transform.position)).normalized;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, separationRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, cohesionRadius);
        }
    }
}