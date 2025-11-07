using UnityEngine;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject soldierPrefab;
    public Transform spawnCenter;
    public float spawnRadius = 3f;
    public int soldierCost = 100;

    [Header("Spawn Distance Settings")]
    public float minDistanceBetweenUnits = 1.2f;
    public int maxSpawnAttempts = 15;

    [Header("Spawn Timer")]
    public float spawnCooldown = 5f; // asker üretme süresi (saniye)
    private float currentCooldown = 0f;
    public Text cooldownText;

    private void Update()
    {
        // Zamanlayıcı geri sayımı
        if (currentCooldown > 0f)
            currentCooldown -= Time.deltaTime;
    
    
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            cooldownText.text = $"Bekleme: {Mathf.Ceil(currentCooldown)} sn";
        }
        else
        {
            cooldownText.text = "Hazır!";
        }
    
    
    }

    public void TrySpawnSoldier()
    {
        if (currentCooldown > 0f)
        {
            Debug.Log($"Bekleme süresi: {currentCooldown:F1} sn");
            return;
        }

        if (soldierPrefab == null || spawnCenter == null)
        {
            Debug.LogWarning("Spawner ayarları eksik!");
            return;
        }

        // Parayı kontrol et
        if (!EconomyManager.Instance.TrySpendMoney(soldierCost))
        {
            Debug.Log("Yetersiz para!");
            return;
        }

        // Geçerli bir konum bul
        Vector3 spawnPos = FindValidSpawnPosition();
        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

        GameObject newSoldier = Instantiate(soldierPrefab, spawnPos, rot);
        newSoldier.tag = "Friendly";

        // Süreyi sıfırla
        currentCooldown = spawnCooldown;
    }

    private Vector3 FindValidSpawnPosition()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPos = spawnCenter.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            // Sadece askerlerle çakışmayı kontrol et
            Collider[] colliders = Physics.OverlapSphere(randomPos, minDistanceBetweenUnits);
            bool valid = true;

            foreach (var col in colliders)
            {
                if (col.CompareTag("Friendly") || col.CompareTag("Enemy"))
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                return randomPos;
        }

        Debug.LogWarning("Uygun spawn pozisyonu bulunamadı, merkezden spawn ediliyor.");
        return spawnCenter.position; // son çare
    }
}


