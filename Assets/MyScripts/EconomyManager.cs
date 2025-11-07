using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Economy Settings")]
    public int currentMoney = 0;
    public int incomePerSecond = 5;

    [Header("UI References")]
    public Text moneyText;

    private float timer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateMoneyUI();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            currentMoney += incomePerSecond;
            UpdateMoneyUI();
            timer = 0f;
        }
    }

    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = "💰 " + currentMoney.ToString();
    }
}

