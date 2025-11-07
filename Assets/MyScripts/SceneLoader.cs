using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Manual Load (Button)")]
    [Tooltip("Elle sahne yüklemek için çağrılacak fonksiyon. Örn: Button OnClick → LoadSceneByNumber(1)")]
    public bool useButtonLoad = true;

    [Header("Auto Load (Countdown)")]
    public bool useAutoLoad = false;
    public float countdownTime = 5f; // Geri sayım süresi
    public int sceneToLoadAfterCountdown = 1;

    [Header("UI References")]
    public Text countdownText; // Geri sayımı göstereceğin UI Text (opsiyonel)

    private bool isCountingDown = false;

    void Start()
    {
        if (useAutoLoad)
        {
            StartCoroutine(StartCountdown());
        }
    }

    // 🎯 Butonla çağrılacak fonksiyon
    public void LoadSceneByNumber(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("Geçersiz sahne numarası!");
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    // ⏳ Geri sayım sistemi
    private IEnumerator StartCountdown()
    {
        if (isCountingDown) yield break;
        isCountingDown = true;

        float timeLeft = countdownTime;

        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = "Yeni sahne yükleniyor: " + Mathf.CeilToInt(timeLeft).ToString();

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (countdownText != null)
            countdownText.text = "Yükleniyor...";

        SceneManager.LoadScene(sceneToLoadAfterCountdown);
    }
}
