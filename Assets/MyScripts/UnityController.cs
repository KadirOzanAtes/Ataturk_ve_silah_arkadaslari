using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Renderer rend;
    private Color defaultColor;
    private AudioSource audioSource;

    [Header("Selection Settings")]
    public bool isSelected = false;
    public Transform selectionPoint;

    [Header("Rifle References")]
    public GameObject backRifle;   
    public GameObject handRifle;   
    public GameObject shootRifle;  

    [Header("Combat Settings")]
    public string enemyTag = "Enemy";
    public float attackRange = 8f;
    public float attackRate = 1.5f;
    public float shootRifleDuration = 0.8f;

    [Header("Audio Settings")]
    public AudioClip shootSound;  // 🔊 Ateş sesi efekti
    [Range(0f, 1f)] public float shootVolume = 0.7f;

    private float nextAttackTime = 0f;
    private float lastShotTime = -Mathf.Infinity;
    private Transform currentTarget = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        rend = GetComponentInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();

        if (rend != null)
            defaultColor = rend.material.color;

        if (agent != null)
        {
            agent.stoppingDistance = 1f;
            agent.updateRotation = true;
            agent.avoidancePriority = Random.Range(30, 70);
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>(); // otomatik ekle
    }

    void Start()
    {
        SetRifleState(idleState: true);
        if (shootRifle != null) shootRifle.SetActive(false);
    }

    void Update()
    {
        if (agent == null || !agent.enabled) return;

        float speed = agent.velocity.magnitude;
        anim.SetFloat("Speed", speed);

        if (speed < 0.1f && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.velocity = Vector3.zero;
            speed = 0f;
        }

        if (speed < 0.1f)
            SetRifleState(idleState: true);
        else
            SetRifleState(idleState: false);

        if (isSelected)
        {
            if (rend != null) rend.material.color = Color.yellow;
            if (selectionPoint != null) selectionPoint.gameObject.SetActive(true);
        }
        else
        {
            if (rend != null) rend.material.color = defaultColor;
            if (selectionPoint != null) selectionPoint.gameObject.SetActive(false);
        }

        if (isSelected && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    MoveTo(hit.point);
                }
            }
        }

        ScanAndMaybeShoot();

        if (shootRifle != null && shootRifle.activeSelf)
        {
            bool targetStillInRange = (currentTarget != null &&
                currentTarget.gameObject.activeInHierarchy &&
                Vector3.Distance(transform.position, currentTarget.position) <= attackRange);

            if (!targetStillInRange && Time.time - lastShotTime >= shootRifleDuration)
            {
                shootRifle.SetActive(false);
                anim.SetBool("IsShooting", false);

                if (speed < 0.1f)
                    SetRifleState(idleState: true);
                else
                    SetRifleState(idleState: false);

                if (agent.enabled)
                    agent.isStopped = false;
            }
        }
    }

    private void ScanAndMaybeShoot()
    {
        if (agent == null || !agent.enabled) return;

        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy ||
                Vector3.Distance(transform.position, currentTarget.position) > attackRange)
            {
                currentTarget = null;
            }
        }

        if (currentTarget == null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            float minDist = Mathf.Infinity;
            Transform nearest = null;

            foreach (var e in enemies)
            {
                if (e == null) continue;
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d <= attackRange && d < minDist)
                {
                    minDist = d;
                    nearest = e.transform;
                }
            }

            currentTarget = nearest;
        }

        if (currentTarget != null)
        {
            Vector3 lookPos = currentTarget.position - transform.position;
            lookPos.y = 0;
            if (lookPos.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 6f);
            }

            if (agent.enabled)
                agent.isStopped = true;

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackRate;
                lastShotTime = Time.time;

                anim.SetBool("IsShooting", true);

                if (shootRifle != null && !shootRifle.activeSelf)
                    shootRifle.SetActive(true);

                // 🔫 Ateş sesi çal
                if (shootSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(shootSound, shootVolume);
                }

                // Hasar verme
                SoldierHealth targetHealth = currentTarget.GetComponent<SoldierHealth>();
                SoldierHealth myHealth = GetComponent<SoldierHealth>();

                if (targetHealth != null && myHealth != null)
                {
                    targetHealth.TakeDamage(myHealth.damagePerShot);
                }
            }
        }
        else
        {
            anim.SetBool("IsShooting", false);
            if (agent.enabled)
                agent.isStopped = false;
        }
    }

    private void SetRifleState(bool idleState)
    {
        if (shootRifle != null && shootRifle.activeSelf)
        {
            if (backRifle != null) backRifle.SetActive(false);
            if (handRifle != null) handRifle.SetActive(false);
            return;
        }

        if (backRifle != null) backRifle.SetActive(idleState);
        if (handRifle != null) handRifle.SetActive(!idleState);
    }

    public void Select() => isSelected = true;
    public void Deselect() => isSelected = false;

    public void MoveTo(Vector3 destination)
    {
        if (agent == null || !agent.enabled) return;
        agent.isStopped = false;
        agent.SetDestination(destination);
        currentTarget = null;
    }
}







