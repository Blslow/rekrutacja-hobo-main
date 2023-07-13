using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class WorkerController : MonoBehaviour
{
    public static List<GameObject> ExtractionBuildings = new();
    public static List<GameObject> ProductionBuildings = new();
    public static List<GameObject> Warehouses = new();

    [SerializeField]
    private bool isCarryingResource = false;
    [SerializeField]
    private GameResourceSO currentlyCarriedResourceSO;

    private GameResourcesList workerResources;

    [SerializeField]
    private GameResourceSO wood;
    [SerializeField]
    private GameResourceSO chairs;

    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField]
    private List<GameObject> targetsQueue = new();

    [SerializeField]
    private GameObject currentTarget;

    private void Awake()
    {
        workerResources = GetComponent<GameResourcesList>();
    }

    private void OnEnable()
    {
        ExtractionBuilding.OnExtract += QueueTask;
    }

    private void OnDisable()
    {
        ExtractionBuilding.OnExtract -= QueueTask;
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(TryCompleteTask(other.gameObject));
    }

    private void QueueTask(GameObject target)
    {
        if (targetsQueue.Contains(target))
            return;
        else if (targetsQueue.Count == 0)
        {
            targetsQueue.Add(target);
            MoveToTarget();
        }
        else
        {
            targetsQueue.Add(target);
        }
    }
    private void DequeueTask()
    {
        if (targetsQueue.Count > 0)
        {
            targetsQueue.RemoveAt(0);
        }
    }

    private void MoveToTarget()
    {
        if (targetsQueue.Count <= 0)
        {
            Debug.Log("There's no more tasks in queue");
            return;
        }

        agent.SetDestination(targetsQueue[0].transform.position);
    }

    private void StartCarryingResource(GameResourceSO resourceSO)
    {
        if (isCarryingResource)
        {
            Debug.Log("error - worker is already carrying resource");
            return;
        }
        else
        {
            isCarryingResource = true;
            workerResources.Add(resourceSO, 1);
            currentlyCarriedResourceSO = resourceSO;
        }
    }
    private void StopCarryingResource()
    {
        if (!isCarryingResource)
        {
            Debug.Log("error - worker isn't carrying any resource");
            return;
        }
        else
        {
            isCarryingResource = false;
            workerResources.TryUse(currentlyCarriedResourceSO, 1);
        }
    }

    IEnumerator TryCompleteTask(GameObject target)
    {
        if (targetsQueue.Count <= 0)
            yield break;

        if (target != targetsQueue[0])
            yield break;

        if (target.GetComponent<Building>() == null)
            yield break;

        if (target.GetComponent<GameResourcesList>() == null)
            yield break;

        BuildingType buildingType = target.GetComponent<Building>().buildingType;
        GameResourcesList gameResourcesList = target.GetComponent<GameResourcesList>();

        switch (buildingType)
        {
            case BuildingType.OTHER:
                yield break;
            case BuildingType.EXTRACTION:
                ExtractionBuilding extractionBuilding = target.GetComponent<ExtractionBuilding>();
                StartCarryingResource(extractionBuilding.resourceSO);
                gameResourcesList.TryUse(extractionBuilding.resourceSO, 1);

                while (ProductionBuildings.Count <= 0)
                {
                    Debug.Log("I can't find any production building");
                    yield return new WaitForSeconds(.5f);
                }
                targetsQueue[0] = ProductionBuildings[0]; // .. todo: change this lane so it looks for closest building instead of first building in list

                break;
            case BuildingType.PRODUCTION:
                ProductionBuilding productionBuilding = target.GetComponent<ProductionBuilding>();
                if (isCarryingResource)
                {
                    gameResourcesList.Add(currentlyCarriedResourceSO, 1);
                    StopCarryingResource();
                }
                if (gameResourcesList.TryUse(productionBuilding.outputResourceSO, 1))
                {
                    StartCarryingResource(productionBuilding.outputResourceSO);
                    while (Warehouses.Count <= 0)
                    {
                        Debug.Log("I can't find any warehouse");
                        yield return new WaitForSeconds(.5f);
                    }
                    targetsQueue[0] = Warehouses[0]; // .. todo: change this lane so it looks for closest building instead of first building in list
                }
                else
                {
                    DequeueTask();
                }

                break;
            case BuildingType.WAREHOUSE:
                gameResourcesList.Add(currentlyCarriedResourceSO, 1);
                StopCarryingResource();
                DequeueTask();

                break;
            default:
                break;
        }

        MoveToTarget();
    }
}
