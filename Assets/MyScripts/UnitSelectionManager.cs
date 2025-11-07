using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    [Header("Selection Settings")]
    public RectTransform selectionBox; // UI kutusu
    private Vector2 startPos;
    private Camera cam;
    private Canvas canvas;

    [HideInInspector] public List<UnitController> selectedUnits = new List<UnitController>();

    void Start()
    {
        cam = Camera.main;
        canvas = selectionBox.GetComponentInParent<Canvas>();
        selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {
        // Sol tık başlama
        if (Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out startPos
            );
            selectionBox.gameObject.SetActive(true);
        }

        // Basılı tutuluyorsa kutuyu güncelle
        if (Input.GetMouseButton(0))
        {
            Vector2 currentMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out currentMousePos
            );

            UpdateSelectionBox(startPos, currentMousePos);
        }

        // Bırakıldığında seçim yap
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            selectionBox.gameObject.SetActive(false);
        }
    }

    void UpdateSelectionBox(Vector2 start, Vector2 end)
    {
        Vector2 center = (start + end) / 2;
        selectionBox.anchoredPosition = center;

        Vector2 size = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
        selectionBox.sizeDelta = size;
    }

    void SelectUnits()
    {
        // Önceki seçimleri temizle
        foreach (UnitController unit in selectedUnits)
            unit.Deselect();
        selectedUnits.Clear();

        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        // Tüm birimleri kontrol et
        foreach (UnitController unit in FindObjectsOfType<UnitController>())
        {
            Vector3 screenPos = cam.WorldToScreenPoint(
                unit.selectionPoint != null
                    ? unit.selectionPoint.position
                    : unit.transform.position + Vector3.up * 1.5f
            );

            if (screenPos.z < 0) continue;

            // Ekran konumunu Canvas lokaline dönüştür
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos
            );

            if (localPos.x > min.x && localPos.x < max.x &&
                localPos.y > min.y && localPos.y < max.y)
            {
                unit.Select();
                selectedUnits.Add(unit);
            }
        }
    }
}



