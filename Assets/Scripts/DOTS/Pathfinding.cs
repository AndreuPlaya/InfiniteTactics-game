using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;

public class Pathfinding : ComponentSystem
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private enum movementType { manhattan, euclidean }
    private const movementType selectedMovementType = movementType.manhattan;
    protected override void OnUpdate()
    {
        if (MapGenerator.Instance.mapIsReady)
        {
            int gridWidth = MapGenerator.Instance.GetGrid().GetWidth();
            int gridHeight = MapGenerator.Instance.GetGrid().GetHeight();
            int2 gridSize = new int2(gridWidth, gridHeight);
            List<FindPathJob> findPathJobList = new List<FindPathJob>();
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref PathfindingParams pathfindingParams) =>
            {
                FindPathJob findPathJob = new FindPathJob
                {
                    gridSize = gridSize,
                    pathNodeArray = GetPathNodeArray(),
                    startPosition = pathfindingParams.startPosition,
                    endPosition = pathfindingParams.endPosition,
                    entity = entity,
                    pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
                };
                findPathJobList.Add(findPathJob);
                jobHandleList.Add(findPathJob.Schedule());
                PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);
            });

            JobHandle.CompleteAll(jobHandleList);

            foreach (FindPathJob findPathJob in findPathJobList)
            {
                new SetBufferPathJob
                {
                    entity = findPathJob.entity,
                    gridSize = findPathJob.gridSize,
                    pathNodeArray = findPathJob.pathNodeArray,
                    pathfindingParamsComponentDataFromEntity = GetComponentDataFromEntity<PathfindingParams>(),
                    pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
                    pathPositionBufferFromEntity = GetBufferFromEntity<PathPosition>()
                }.Run();
            }
        }
    }

    private NativeArray<PathNode> GetPathNodeArray()
    {
        Grid<Coord> grid = MapGenerator.Instance.GetGrid();

        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());
        NativeArray<PathNode> pathNodeAray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);


        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNode pathNode = new PathNode();
                pathNode.x = x;
                pathNode.y = y;
                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;

                pathNode.isWalkable = grid.GetGridObject(x, y).GetIsWalkable();
                pathNode.cameFromNodeIndex = -1;

                pathNodeAray[pathNode.index] = pathNode;
            }
        }
        return pathNodeAray;
    }

    [BurstCompile]
    private struct SetBufferPathJob : IJob
    {
        public int2 gridSize;
        [DeallocateOnJobCompletion]
        public NativeArray<PathNode> pathNodeArray;

        public Entity entity;

        public ComponentDataFromEntity<PathfindingParams> pathfindingParamsComponentDataFromEntity;
        public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;
        public BufferFromEntity<PathPosition> pathPositionBufferFromEntity;

        public void Execute()
        {
            DynamicBuffer<PathPosition> pathPositionBuffer = pathPositionBufferFromEntity[entity];
            pathPositionBuffer.Clear();

            PathfindingParams pathfindingParams = pathfindingParamsComponentDataFromEntity[entity];
            int endNodeIndex = CalculateIndex(pathfindingParams.endPosition.x, pathfindingParams.endPosition.y, gridSize.x);
            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                //did not find a path
                pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = -1 };
            }
            else
            {
                //found a path
                CalculatePath(pathNodeArray, endNode, pathPositionBuffer);
                pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = pathPositionBuffer.Length - 1 };
            }

        }

    }


    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 gridSize;
        public NativeArray<PathNode> pathNodeArray;

        public int2 startPosition;
        public int2 endPosition;

        public Entity entity;
        [NativeDisableContainerSafetyRestriction]
        public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;

        public void Execute()
        {
           
            for (int i = 0; i< pathNodeArray.Length; i++)
            {
                PathNode pathNode = pathNodeArray[i];
                pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPosition);
                pathNode.cameFromNodeIndex = -1;
                pathNodeArray[i] = pathNode;
            }
            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Unity.Collections.Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); //left
            neighbourOffsetArray[1] = new int2(+1, 0);//right
            neighbourOffsetArray[2] = new int2(0, -1);//down
            neighbourOffsetArray[3] = new int2(0, +1);//up
            neighbourOffsetArray[4] = new int2(-1, -1);//left down
            neighbourOffsetArray[5] = new int2(-1, +1);//left up
            neighbourOffsetArray[6] = new int2(+1, -1);//right down
            neighbourOffsetArray[7] = new int2(+1, +1);//right up

           

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)] = startNode;


            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    //reached end destination!
                    break;
                }

                //remove current node from open list
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }
                closedList.Add(currentNodeIndex);
                int maxScan = neighbourOffsetArray.Length;
                if (selectedMovementType == movementType.euclidean)
                {
                    maxScan = 8;
                }
                else if (selectedMovementType == movementType.manhattan)
                {
                    maxScan = 4;
                }
                for (int i = 0; i < maxScan; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourNodePosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);
                    if (!IsPositionInsideGrid(neighbourNodePosition, gridSize))
                    {
                        //neighbour not valid position
                        continue;
                    }
                    int neighbourNodeIndex = CalculateIndex(neighbourNodePosition.x, neighbourNodePosition.y, gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        //already searched this node
                        continue;
                    }
                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable)
                    {
                        //not walkable
                        continue;
                    }
                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourNodePosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNodePosition, endPosition);
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode; // update array
                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }
                }


            }       
            openList.Dispose();
            closedList.Dispose();
            neighbourOffsetArray.Dispose();
        }
    }
        
    private static void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPosition> pathPositionBuffer)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            //could not find a path
        }
        else
        {
            //found a path
            pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) } );
            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                pathPositionBuffer.Add(new PathPosition { position = new int2(cameFromNode.x, cameFromNode.y) });
                currentNode = cameFromNode;
            }
        }
    }
    private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y >= 0 &&
            gridPosition.y < gridSize.y;
    }
    private static int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++)
        {
            PathNode testPathNode = pathNodeArray[openList[i]];
            if (testPathNode.fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = testPathNode;
            }
        }
        return lowestCostPathNode.index;
    }
    private static int CalculateDistanceCost(int2 aPosition, int2 bPosition)
    {
        if (selectedMovementType == movementType.euclidean)
        {
            int xDistance = Mathf.Abs(aPosition.x - bPosition.x);
            int yDistance = Mathf.Abs(aPosition.y - bPosition.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
        else if (selectedMovementType == movementType.manhattan)
        {
            return MOVE_STRAIGHT_COST * Mathf.Abs(aPosition.x - bPosition.x) + Mathf.Abs(aPosition.y - bPosition.y);
        }
        else
        {
            return -1;
        }
    }
    private static int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }
    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }
    }

}
