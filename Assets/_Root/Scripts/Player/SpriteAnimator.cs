using UnityEngine;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Player
{
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] idleFrames;

        [SerializeField]
        private Sprite[] runFrames;

        [SerializeField]
        private Sprite[] deathFrames;

        [SerializeField]
        private Sprite[] attackFrames;

        [SerializeField]
        private float moveSpeedThreshold = 0.08f;

        [SerializeField]
        private float idleFramesPerSecond = 6f;

        [SerializeField]
        private float runFramesPerSecond = 10f;

        [SerializeField]
        private float deathFramesPerSecond = 8f;

        [SerializeField]
        private float attackFramesPerSecond = 12f;

        private SpriteRenderer sr;
        private Rigidbody2D rb;
        private HealthBehaviour health;
        private Movement movement;

        private float frameTime;
        private int frameIndex;
        private bool dead;
        private bool deathSequenceComplete;
        private bool attacking;

        public bool IsAttacking => attacking;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthBehaviour>();
            movement = GetComponent<Movement>();

            if (idleFrames != null && idleFrames.Length > 0) sr.sprite = idleFrames[0];
        }

        private void OnEnable()
        {
            health.OnDied += OnDied;
        }

        private void OnDisable()
        {
            health.OnDied -= OnDied;
        }

        private void OnDied()
        {
            Debug.Log("Player died");
            dead = true;
            frameIndex = 0;
            frameTime = 0f;
            rb.linearVelocity = Vector2.zero;
            if (movement != null) movement.enabled = false;
        }

        public void TriggerAttack()
        {
            if (dead) return;

            attacking = true;
            frameIndex = 0;
            frameTime = 0f;
        }

        private void Update()
        {
            if (dead)
            {
                UpdateDeath();
                return;
            }

            if (attacking)
            {
                UpdateAttack();
                return;
            }

            UpdateLiving();
        }

        private void UpdateLiving()
        {
            var v = rb.linearVelocity;
            var moving = v.sqrMagnitude > moveSpeedThreshold * moveSpeedThreshold;
            if (Mathf.Abs(v.x) > moveSpeedThreshold) sr.flipX = v.x < 0f;

            var frames = moving ? runFrames : idleFrames;
            var fps = moving ? runFramesPerSecond : idleFramesPerSecond;
            if (frames == null || frames.Length == 0) return;
            if (frameIndex < 0 || frameIndex >= frames.Length) frameIndex = 0;

            frameTime += Time.deltaTime;
            var step = 1f / Mathf.Max(0.01f, fps);
            while (frameTime >= step)
            {
                frameTime -= step;
                frameIndex = (frameIndex + 1) % frames.Length;
            }

            sr.sprite = frames[frameIndex];
        }

        private void UpdateAttack()
        {
            if (attackFrames == null || attackFrames.Length == 0)
            {
                attacking = false;
                return;
            }

            if (frameIndex < 0 || frameIndex >= attackFrames.Length) frameIndex = 0;

            frameTime += Time.deltaTime;
            var step = 1f / Mathf.Max(0.01f, attackFramesPerSecond);
            while (frameTime >= step)
            {
                frameTime -= step;
                frameIndex++;
                if (frameIndex >= attackFrames.Length)
                {
                    frameIndex = 0;
                    attacking = false;
                    break;
                }
            }

            if (frameIndex < attackFrames.Length) sr.sprite = attackFrames[frameIndex];
        }

        private void UpdateDeath()
        {
            if (deathFrames == null || deathFrames.Length == 0)
            {
                deathSequenceComplete = true;
                return;
            }

            if (deathSequenceComplete) return;

            frameTime += Time.deltaTime;
            var step = 1f / Mathf.Max(0.01f, deathFramesPerSecond);
            while (frameTime >= step)
            {
                frameTime -= step;
                frameIndex++;
                if (frameIndex >= deathFrames.Length)
                {
                    frameIndex = deathFrames.Length - 1;
                    deathSequenceComplete = true;
                    break;
                }
            }

            sr.sprite = deathFrames[frameIndex];
        }
    }
}
