using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFighting : MonoBehaviour, IDamageable
{
    private Animator m_animator;
    public Transform m_attackPoint;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_attackDelay = 1.0f;

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
    }

    // Update is called once per frame
    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        AttackPlayer();

        if (transform.position.y < -8)
        {
            Death();
        }
    }

    public void Damage(float d)
    {
        if (!isDead)
        {
            m_animator.SetTrigger("Hit");
            health -= d;

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

    private void AttackPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");

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
                        damageable.Damage(damage);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(m_attackPoint.position, attackRange);
    }
}
