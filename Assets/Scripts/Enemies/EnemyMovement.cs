using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float m_speed = 5.0f;

    private Animator m_animator;
    private Sensor_HeroKnight m_groundSensor;
    private Rigidbody2D m_body2d;
    private Transform m_attackPoint;
    private bool m_grounded = false;
    public bool m_startFighting = false;
    private float m_delayToIdle = 0.0f;
    private float m_attackPointDistance;

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
        if (m_animator.GetBool("Death") == false)
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

            MoveToPlayer();
        }
        else
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
        }

        else if (player.transform.position.x < transform.position.x)
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                m_attackPoint.position = new Vector2(gameObject.transform.position.x - m_attackPointDistance, m_attackPoint.position.y);
            }
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    private void MoveToPlayer()
    {
        Vector2 target = new Vector2(player.transform.position.x, m_body2d.position.y);
        Vector2 newPos = Vector2.MoveTowards(m_body2d.position, target, m_speed * Time.fixedDeltaTime);

        m_body2d.MovePosition(newPos);
    }
}
