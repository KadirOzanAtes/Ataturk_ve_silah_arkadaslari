using UnityEngine;
using System.Collections;

public class SoldierHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Combat Settings")]
    public float damagePerShot = 10f;     // Tek atışta verilen hasar
    public float fireRate = 1.5f;         // Atış sıklığı (saniye)
    public float attackRange = 8f;        // Ateş menzili

    [Header("Animation & References")]
    public Animator anim;
    //public GameObject ragdollPrefab;      // Ölürken spawn olacak ragdoll opsiyonel
    public string enemyTag;               // Bu askerin düşman tag’i (“Friendly” veya “Enemy”)

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    // 🔸 Hasar alma
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Ölme animasyonunu tetikle
        if (anim != null)
            anim.SetTrigger("Die");

        // AI ve fizik etkileşimlerini durdur
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Eğer ragdoll varsa oluştur
        //if (ragdollPrefab != null)
            //Instantiate(ragdollPrefab, transform.position, transform.rotation);

        // 3 saniye sonra karakteri sahneden kaldır
        Destroy(gameObject, 3f);
    }

    // 🔸 Düşmana hasar verme (örnek çağrı için)
    public void DealDamageTo(SoldierHealth target)
    {
        if (target == null || target.isDead) return;
        target.TakeDamage(damagePerShot);
    }

    // 🔸 Dışarıdan çağrılabilir (örnek: AI saldırı anında)
    public bool IsDead()
    {
        return isDead;
    }
}
