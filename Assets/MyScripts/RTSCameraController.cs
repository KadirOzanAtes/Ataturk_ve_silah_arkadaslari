using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("RTS/RTS Camera (Edge Scroll + Zoom)")]
public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;             // kenar hareket hızı
    public float moveSmoothTime = 0.08f;      // hareketin yumuşaklığı
    public float screenEdgeBorder = 20f;      // kenardan kaç px'de harekete başlasın
    public bool useKeyboard = true;           // WASD ile de hareket etsin mi

    [Header("Zoom")]
    public float zoomSpeed = 250f;            // tekerlek duyarlılığı
    public float minY = 15f;                  // en yakın zoom (kamera yüksekliği)
    public float maxY = 80f;                  // en uzak zoom
    public float zoomSmoothTime = 0.08f;

    [Header("Map Bounds (optional)")]
    public bool useBounds = true;
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    // iç durum
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private float targetY;
    private float currentYVelocity;

    void Start()
    {
        targetPosition = transform.position;
        targetY = transform.position.y;
    }

    void Update()
    {
        // UI üzerine gelindiyse kamera hareketini engelle
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 inputMove = Vector3.zero;

        // 1) Kenar hareketi (mouse position)
        Vector3 mouse = Input.mousePosition;
        if (mouse.x >= Screen.width - screenEdgeBorder)
            inputMove += Vector3.right;
        else if (mouse.x <= screenEdgeBorder)
            inputMove += Vector3.left;

        if (mouse.y >= Screen.height - screenEdgeBorder)
            inputMove += Vector3.forward;
        else if (mouse.y <= screenEdgeBorder)
            inputMove += Vector3.back;

        // 2) Klavye hareketi (opsiyonel)
        if (useKeyboard)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputMove += Vector3.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputMove += Vector3.back;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputMove += Vector3.right;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputMove += Vector3.left;
        }

        // Hareket yönünü kameranın yatay düzlemine göre ayarla
        Vector3 camRight = transform.right;
        Vector3 camForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 worldMove = (camRight * inputMove.x + camForward * inputMove.z).normalized;

        // Hedef pozisyonu güncelle
        if (worldMove.sqrMagnitude > 0.0001f)
            targetPosition += worldMove * moveSpeed * Time.deltaTime;

        // 3) Zoom (mouse wheel)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            // Kamera yüksekliğini değiştiriyoruz (y ekseni)
            targetY -= scroll * zoomSpeed * Time.deltaTime;
            targetY = Mathf.Clamp(targetY, minY, maxY);
        }

        // Sınır kontrolleri (opsiyonel)
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        }
    }

    void LateUpdate()
    {
        // Smooth pozisyon güncellemesi (X,Z)
        Vector3 smoothPos = Vector3.SmoothDamp(transform.position, 
                                               new Vector3(targetPosition.x, transform.position.y, targetPosition.z), 
                                               ref currentVelocity, 
                                               moveSmoothTime);
        // Smooth zoom (Y)
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetY, ref currentYVelocity, zoomSmoothTime);

        transform.position = new Vector3(smoothPos.x, smoothY, smoothPos.z);
    }

    // Inspector için yardımcı: harita sınırlarını scene view'da görsel göstermek istersen
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.cyan;
        Vector3 a = new Vector3(minX, transform.position.y, minZ);
        Vector3 b = new Vector3(maxX, transform.position.y, minZ);
        Vector3 c = new Vector3(maxX, transform.position.y, maxZ);
        Vector3 d = new Vector3(minX, transform.position.y, maxZ);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }
}


