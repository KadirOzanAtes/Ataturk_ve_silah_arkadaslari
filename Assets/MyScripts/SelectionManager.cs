using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private UnitController selectedUnit;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                UnitController unit = hit.collider.GetComponent<UnitController>();

                if (unit != null)
                {
                    // Önceki seçimi kaldır
                    if (selectedUnit != null)
                        selectedUnit.Deselect();

                    // Yeni birimi seç
                    selectedUnit = unit;
                    selectedUnit.Select();
                }
                else
                {
                    // Boşa tıklanırsa seçimi kaldır
                    if (selectedUnit != null)
                    {
                        selectedUnit.Deselect();
                        selectedUnit = null;
                    }
                }
            }
        }
    }
}

