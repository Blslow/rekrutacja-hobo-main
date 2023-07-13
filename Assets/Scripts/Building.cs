using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingType buildingType;
    public Canvas buildingCanvas;

    private void OnEnable()
    {
        switch (buildingType)
        {
            case BuildingType.OTHER:
                break;
            case BuildingType.EXTRACTION:
                WorkerController.ExtractionBuildings.Add(gameObject);
                break;
            case BuildingType.PRODUCTION:
                WorkerController.ProductionBuildings.Add(gameObject);
                break;
            case BuildingType.WAREHOUSE:
                WorkerController.Warehouses.Add(gameObject);
                break;
            default:
                break;
        }
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject == gameObject)
            {
                buildingCanvas.gameObject.SetActive(true);
            }
            else
            {
                buildingCanvas.gameObject.SetActive(false);
            }
        }
        else
        {
            buildingCanvas.gameObject.SetActive(false);
        }
    }
}

public enum BuildingType
{
    OTHER,
    EXTRACTION,
    PRODUCTION,
    WAREHOUSE,
}
