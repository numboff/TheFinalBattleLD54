using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Enemy : MonoBehaviour, IDamageable
{
    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Transform m_attackPoint;
    private float m_attackPointDistance;
    private bool m_grounded = false;
    public bool m_startFighting = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_attackDelay = 1.0f;
    private float m_delayToIdle = 0.0f;

    public LayerMask enemyLayer;
    public float attackRange = 1.5f;

    public float health = 150f;
    public float damage = 20f;
    public bool isDead = false;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_attackPoint = transform.Find("AttackPoint");
        m_attackPointDistance = m_attackPoint.localPosition.x + 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //Run
        if (m_startFighting)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState1", 1);
        }
        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState1", 0);
        }

        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        LookAtPlayer();

        if ((player.transform.position.x > m_attackPoint.position.x - attackRange) && (player.transform.position.x < m_attackPoint.position.x + attackRange))
        {
            if (m_timeSinceAttack > m_attackDelay)
            {
                m_currentAttack++;

                // Loop back to one after third attack
                if (m_currentAttack > 2)
                    m_currentAttack = 1;

                // Reset Attack combo if time since last attack is too large
                if (m_timeSinceAttack > 3.0f)
                    m_currentAttack = 1;

                // Call one of three attack animations "Attack1", "Attack2", "Attack3"
                m_animator.SetTrigger("Attack" + m_currentAttack);

                // Reset timer
                m_timeSinceAttack = 0.0f;

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(m_attackPoint.position, attackRange, enemyLayer);
                foreach (Collider2D enemie in hitEnemies)
                {
                    IDamageable damageable = enemie.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        Debug.Log("I hit someone!");
                        damageable.Damage(damage, new Vector2(m_facingDirection, 0));
                    }
                }
            }
        }
    }

    public void Damage(float d, Vector2 direction)
    {
        if (!isDead)
        {
            m_animator.SetTrigger("Hit");
            health -= d;
            m_body2d.AddForce(direction * 1000, ForceMode2D.Impulse);

            if (health <= 0)
            {
                Death();
            }
        }
    }

    private void Death()
    {
        m_animator.SetBool("Death", true);

        isDead = true;
        
        this.enabled = false;
    }

    private void LookAtPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // Swap direction of sprite depending on walk direction
        if (player.transform.position.x > transform.position.x)
        {
            if (GetComponent<SpriteRenderer>().flipX)
            {
                m_attackPoint.position = new Vector2(gameObject.transform.position.x + m_attackPointDistance, m_attackPoint.position.y);
            }
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (player.transform.position.x < transform.position.x)
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                m_attackPoint.position = new Vector2(gameObject.transform.position.x - m_attackPointDistance, m_attackPoint.position.y);
            }
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(m_attackPoint.position, attackRange);
    }
}
