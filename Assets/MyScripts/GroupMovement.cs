using UnityEngine;
using System.Collections.Generic;

public class GroupMovement : MonoBehaviour
{
    public UnitSelectionManager selectionManager;
    public float spacing = 3.5f; // askerler arası mesafe

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveUnitsInFormation(hit.point);
            }
        }
    }

    void MoveUnitsInFormation(Vector3 center)
    {
        List<UnitController> units = selectionManager.selectedUnits;
        int count = units.Count;
        if (count == 0) return;

        int formationSize = Mathf.CeilToInt(Mathf.Sqrt(count)); // kare formasyon
        int index = 0;

        for (int row = 0; row < formationSize; row++)
        {
            for (int col = 0; col < formationSize; col++)
            {
                if (index >= count) return;

                Vector3 offset = new Vector3(
                    (col - formationSize / 2f) * spacing,
                    0,
                    (row - formationSize / 2f) * spacing
                );

                Vector3 target = center + offset;
                units[index].MoveTo(target);
                index++;
            }
        }
    }
}



