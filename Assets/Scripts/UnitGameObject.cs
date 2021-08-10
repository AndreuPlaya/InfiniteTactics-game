using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class UnitGameObject : MonoBehaviour
{
    [SerializeField] private ConvertedEntityHolder convertedEntityHolder;
    private Entity entity;
    private EntityManager entityManager;
    private PathFollow pathFollow;
    private DynamicBuffer<PathPosition> pathPositionBuffer;
    private float moveSpeed = 10f;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (MapGenerator.Instance.mapIsReady)
        {
            SetupEntityData();
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Utility.GetMouseWorldPosition();
                MapGenerator.Instance.GetGrid().GetXY(transform.position, out int startX, out int startY);
                MapGenerator.Instance.GetGrid().GetXY(mousePosition, out int endX, out int endY);
                SetPath(startX, startY, endX, endY);
            }
            if (pathFollow.pathIndex < 0)
            {
                MapGenerator.Instance.GetGrid().GetXY(transform.position, out int startX, out int startY);
                ObjectSpawner.Instance.GetRandomOpenXY(out int endX, out int endY);
                SetPath(startX, startY, endX, endY);
            }
            UpdatePathPosition();
        }
    }
   
    private void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, MapGenerator.Instance.GetGrid().GetWidth() - 1);
        y = math.clamp(y, 0, MapGenerator.Instance.GetGrid().GetHeight() - 1);
    }

    private void SetPath(int startX, int startY, int endX, int endY)
    {
        ValidateGridPosition(ref startX, ref startY);
        ValidateGridPosition(ref endX, ref endY);
        
        //Add pathfinding params
        entityManager.AddComponentData(entity, new PathfindingParams
        {
            startPosition = new int2(startX, startY),
            endPosition = new int2(endX, endY)
        });
        

    }

    private void UpdatePathPosition()
    {
        if (pathFollow.pathIndex >= 0)
        {
            int2 pathPosition = pathPositionBuffer[pathFollow.pathIndex].position;

            float3 targetPosition = new float3(MapGenerator.Instance.GetGrid().GetCenteredWorldPosition(pathPosition.x, pathPosition.y));
            float3 moveDir = math.normalizesafe(targetPosition - (float3)transform.position);


            transform.position = math.lerp((float3)transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (math.distance((float3)transform.position, targetPosition) < 0.05f)
            {
                //next waypoint
                pathFollow.pathIndex--;
                entityManager.SetComponentData(entity, pathFollow);
            }
        }

    }

    private void SetupEntityData()
    {
        entity = convertedEntityHolder.GetEntity();
        entityManager = convertedEntityHolder.GetEntityManager();
        pathFollow = entityManager.GetComponentData<PathFollow>(entity);
        pathPositionBuffer = entityManager.GetBuffer<PathPosition>(entity);
    }
}
