using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    private MapGenerator map;
    private Pathfinding pathfinding;
  /*  private void Start()
    {
        
        map = FindObjectOfType<MapGenerator>();

        SetupPathfindingGrid();
    }


    private void SetupPathfindingGrid()
    {
        pathfinding = new Pathfinding(map.GetGrid().GetWidth(), map.GetGrid().GetHeight());
        for (int x= 0; x< map.GetGrid().GetWidth(); x++)
        {
            for (int y = 0; y < map.GetGrid().GetHeight(); y++)
            {
                pathfinding.GetNode(x, y).SetIsWalkable(map.GetGrid().GetGridObject(x, y).GetValue() == 0 && map.GetObstacleMap()[x,y] == false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = Utility.GetMouseWorldPosition();
            
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            
            List<PathNode> path = pathfinding.FindPath(10, 10, x, y);
            
            if (path != null)
            {
                Debug.Log(path.Count.ToString());
                for (int i=1; i< path.Count - 1; i++)
                {
                    Debug.Log(path[i].x + " " + path[i].y);
                    Debug.DrawLine(map.GetGrid().GetCenteredWorldPosition(path[i].x, path[i].y), map.GetGrid().GetCenteredWorldPosition(path[i + 1].x, path[i + 1].y), Color.green, 2f);
                }
            }
        }
    }*/

}
