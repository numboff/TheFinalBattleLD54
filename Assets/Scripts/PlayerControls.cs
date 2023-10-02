using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerControls : MonoBehaviour, IDamageable {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    public PlayerInput controls = null;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Transform           m_attackPoint;
    private float               m_attackPointDistance;
    private bool                m_isWallSliding = false;
    private bool                m_grounded = false;
    private bool                m_rolling = false;
    private bool                m_blocking = false;
    public bool                isDead = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 1.0f;
    private float               m_rollCurrentTime;

    public LayerMask enemyLayer;
    public float attackRange = 0.5f;

    public float health = 100f;
    public float damage = 15f;

    private void Awake()
    {
        controls = new PlayerInput();
    }

    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_attackPoint = transform.Find("AttackPoint");
        m_attackPointDistance = m_attackPoint.localPosition.x + 1.0f;
    }

    // Update is called once per frame
    void Update ()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if(m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if(m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

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

        // -- Handle input and movement --
        float inputX = controls.Land.Movement.ReadValue<float>();

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            if (GetComponent<SpriteRenderer>().flipX)
            {
                m_attackPoint.position = new Vector2(gameObject.transform.position.x + m_attackPointDistance, m_attackPoint.position.y);
            }
            GetComponent<SpriteRenderer>().flipX = false;

            m_facingDirection = 1;
        }
            
        else if (inputX < 0)
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                m_attackPoint.position = new Vector2(gameObject.transform.position.x - m_attackPointDistance, m_attackPoint.position.y);
            }
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        float inputAttack = controls.Land.Fighting.ReadValue<float>();
        //Attack
        if ((inputAttack < 0) && (m_timeSinceAttack > 0.25f) && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(m_attackPoint.position, attackRange, enemyLayer);

            foreach(Collider2D enemie in hitEnemies)
            {
                IDamageable damageable = enemie.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Debug.Log("I hit someone!");
                    damageable.Damage(damage);
                }
            }
        }

        // Block
        else if ((inputAttack > 0) && !m_rolling)
        {
            m_animator.SetBool("IdleBlock", true);

            if (!m_blocking)
                m_blocking = true;
        }

        else if ((inputAttack <= 0) && m_blocking)
        {
            m_animator.SetBool("IdleBlock", false);

            m_blocking = false;
        }

        // Roll
        else if ((controls.Land.Roll.ReadValue<float>() >= 0.5f) && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }


        //Jump
        else if ((controls.Land.Jump.ReadValue<float>() >= 0.5f) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }

        if (transform.position.y < -8)
        {
            Death();
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(m_attackPoint.position, attackRange);
    }

    public void Damage(float d)
    {
        if (!isDead)
        {
            if (!m_rolling && !m_blocking)
            {
                m_animator.SetTrigger("Hurt");
                health -= d;
            }
            else if (m_blocking)
            {
                m_animator.SetTrigger("Block");
                health -= (d / 2);
                Debug.Log("Current HP: "+health);
            }
            if (health <= 0)
                Death();
        }
    }

    private void Death()
    {
        m_animator.SetBool("IdleBlock", false);
        m_animator.SetTrigger("Death");
        isDead = true;

        this.enabled = false;
    }
}
