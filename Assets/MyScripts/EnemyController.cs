using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    [Header("Rifle References")]
    public GameObject backRifle;
    public GameObject handRifle;
    public GameObject shootRifle;

    [Header("Combat Settings")]
    public string friendlyTag = "Friendly";
    public float attackRange = 8f;
    public float attackRate = 1.5f;
    [Tooltip("ShootRifle en son atıştan sonra en az bu kadar süre açık kalır")]
    public float shootRifleDuration = 0.8f;

    [Header("Audio Settings")]
    public AudioClip shootSound; // 🔊 Ateş sesi efekti
    [Range(0f, 1f)] public float shootVolume = 0.7f;

    private float nextAttackTime = 0f;
    private float lastShotTime = -Mathf.Infinity;
    private Transform currentTarget = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (agent != null)
        {
            agent.stoppingDistance = 1f;
            agent.updateRotation = true;
            agent.avoidancePriority = Random.Range(30, 70);
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>(); // Otomatik ekle
    }

    void Start()
    {
        SetRifleState(idleState: true);
        if (shootRifle != null) shootRifle.SetActive(false);
    }

    void Update()
    {
        ScanAndMaybeShoot();

        // shootRifle kapatma yönetimi
        if (shootRifle != null && shootRifle.activeSelf)
        {
            bool targetStillInRange = (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange);

            if (!targetStillInRange && Time.time - lastShotTime >= shootRifleDuration)
            {
                shootRifle.SetActive(false);
                anim.SetBool("IsShooting", false);
                SetRifleState(idleState: true);
            }
        }
    }

    private void ScanAndMaybeShoot()
    {
        // Eğer ajan yoksa ya da devre dışıysa geri çık
        if (agent == null || !agent.enabled) return;

        // Mevcut hedef geçerli mi kontrol et
        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy || Vector3.Distance(transform.position, currentTarget.position) > attackRange)
            {
                currentTarget = null;
            }
        }

        // Hedef yoksa en yakın dost birimi bul
        if (currentTarget == null)
        {
            GameObject[] friendlies = GameObject.FindGameObjectsWithTag(friendlyTag);
            float minDist = Mathf.Infinity;
            Transform nearest = null;

            foreach (var f in friendlies)
            {
                if (f == null) continue;
                float d = Vector3.Distance(transform.position, f.transform.position);
                if (d <= attackRange && d < minDist)
                {
                    minDist = d;
                    nearest = f.transform;
                }
            }

            currentTarget = nearest;
        }

        // Hedef varsa: dur, dön ve ateş animasyonu tetikle
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
                var targetHealth = currentTarget.GetComponent<SoldierHealth>();
                var myHealth = GetComponent<SoldierHealth>();
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
}



