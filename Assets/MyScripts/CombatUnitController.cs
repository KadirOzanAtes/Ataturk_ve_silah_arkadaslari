using UnityEngine;
using UnityEngine.AI;

public class CombatUnitController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    [Header("Team Settings")]
    public bool isFriendly = true; // ✅ Dost mu düşman mı?

    [Header("Selection Settings")]
    public bool isSelected = false;
    private Renderer rend;
    private Color defaultColor;

    [Header("Rifle References")]
    public GameObject backRifle;   // Arkadaki tüfek objesi
    public GameObject handRifle;   // Elde taşınan tüfek objesi

    [Header("Selection Point")]
    public Transform selectionPoint;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        rend = GetComponentInChildren<Renderer>();

        if (rend != null)
            defaultColor = rend.material.color;

        agent.stoppingDistance = 1f;
        agent.avoidancePriority = Random.Range(30, 70);
    }

    void Start()
    {
        // Başlangıçta Idle durumunda
        SetRifleState(idleState: true);
    }

    void Update()
    {
        float speed = agent.velocity.magnitude;

        anim.SetFloat("Speed", speed);

        if (speed < 0.1f && agent.remainingDistance <= agent.stoppingDistance)
        {
            speed = 0f;
            agent.velocity = Vector3.zero;
        }

        // Silah geçişi
        if (speed < 0.1f)
            SetRifleState(true);
        else
            SetRifleState(false);

        // Eğer düşman ise seçim, hareket vs yapılmaz
        if (!isFriendly) return;

        // Seçilmemişse renk sıfırla
        if (!isSelected)
        {
            rend.material.color = defaultColor;
            return;
        }

        // Sağ tıkla hareket et
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    agent.SetDestination(hit.point);
                }
            }
        }
    }

    private void SetRifleState(bool idleState)
    {
        if (backRifle != null)
            backRifle.SetActive(idleState);
        if (handRifle != null)
            handRifle.SetActive(!idleState);
    }

    public void Select()
    {
        if (!isFriendly) return; // ❌ Düşman seçilmez
        isSelected = true;
        if (rend != null)
            rend.material.color = Color.yellow;
    }

    public void Deselect()
    {
        isSelected = false;
        if (rend != null)
            rend.material.color = defaultColor;
    }

    public void MoveTo(Vector3 destination)
    {
        if (!isFriendly) return;
        agent.SetDestination(destination);
    }
}



