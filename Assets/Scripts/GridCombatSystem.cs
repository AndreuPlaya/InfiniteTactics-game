using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCombatSystem : MonoBehaviour
{

    [SerializeField] private UnitGridCombat[] unitGridCombatArray;
    
    private State state;
    private List<UnitGridCombat> enemyTeamList;
    private List<UnitGridCombat> playerTeamList;
    private int enemyTeamActiveUnitIndex;
    private int playerTeamActiveUnitIndex;
    private UnitGridCombat unitGridCombat;
    private bool canMoveThisTurn;
    private bool canAttackThisTurn;
    private enum State
    {
        Normal,
        Waiting
    }
    private void Awake()
    {
        state = State.Normal;
    }
    private void Start()
    {
        enemyTeamList = new List<UnitGridCombat>();
        playerTeamList = new List<UnitGridCombat>();
        enemyTeamActiveUnitIndex = -1;
        playerTeamActiveUnitIndex = -1;
        //set all unitGridCombat on their GridPosition
        foreach(UnitGridCombat unitGridCombat in unitGridCombatArray)
        {
            GameHandler_GridCombatSystem.Instance.GetGrid().GetGridObject(unitGridCombat.GetPosition())
                .SetUnitGridCombat(unitGridCombat);
            if (UnitGridCombat.GetTeam() == UnitGridCombat.Team.Enemy)
            {
                enemyTeamList.Add(unitGridCombat);
            }
            else
            {
                playerTeamList.Add(unitGridCombat);
            }
        }
        SelectNextActiveUnit();
        UpdateValidMovePositions();
    }
    
    private void SelectNextActiveUnit()
    {
        if (unitGridCombat == null || unitGridCombat.GetTeam() == UnitGridCombat.Team.Player)
        {
            unitGridCombat = GetNextActiveUnit(UnitGridCombat.Team.Enemy);
        }
        else
        {
            unitGridCombat = GetNextActiveUnit(UnitGridCombat.Team.Player);
        }
        GameHandler_GridCombatSystem.Instance.SetCameraFollowPosition(unitGridCombat.GetPosition());
        canMoveThisTurn = true;
        canAttackThisTurn = true;
    }
    private UnitGridCombat GetNextActiveUnit(UnitGridCombat.Team team)
    {
        if (team == UnitGridCombat.Team.Player)
        {
            playerTeamActiveUnitIndex = (playerTeamActiveUnitIndex + 1) % playerTeamList.Count;
            return playerTeamList[playerTeamActiveUnitIndex];
        }
        else
        {
            enemyTeamActiveUnitIndex = (enemyTeamActiveUnitIndex + 1) % enemyTeamList.Count;
            return enemyTeamList[enemyTeamActiveUnitIndex];
        }
    }
    private void UpdateValidMovePositions()
    {
        Grid<GridObject> grid = GameHandler_GridCombatSystem.Instance.GetGrid();
        GridPathfinding gridPathfinding = GameHandler_GridCombatSystem.Instance.gridPathfinding;
        //Get unit grid X, Y
        grid.GetXY(unitGridCombat.GetPosition(), out int unitX, out int unitY);
        //Set entire map invisible
        GameHandler_GridCombatSystem.Instance.GetMovementTilemap().SetAllTilemapSprite(MovementTilemap.TilemapObject.TilemapSprite.None);
        for(int x=0; x< grid.GetWidth(); x++)
        {
            for(int y = 0; y< grid.GetHeight(); y++)
            {
                grid.GetGridObject(x, y).SetIsValidMovePosition(false);
            }
        }
        int maxMoveDistance = 5;
        for (int x = unitX - maxMoveDistance; x < unitX +maxMoveDistance; x++)
        {
            for (int y = unitY - maxMoveDistance; y < unitY + maxMoveDistance; y++)
            {
                if(gridPathfinding.IsWalkable(x, y))
                {
                    //Position is walkable
                    if (gridPathfinding.HasPath(unitX, unitY, x, y))
                    {
                        //there is a path
                       if( gridPathfinding.GetPath(unitX, unitY, x, y).Count <= maxMoveDistance)
                        {
                            //path within move distance

                            //set tilemap to move
                            GameHandler_GridCombatSystem.Instance.GetMovementTilemap().SetTilemapSprite(x, y, MovementTilemap.TilemapObject.TilemapSprite.Move);

                            grid.GetGridObject(x, y).SetIsValidMovePosition(true);
                        }
                        else
                        {
                            //path outside move distance
                            
                        }
                    }
                    else
                    {
                        //no valid path
                    }
                }
                else
                {
                    //psition is not walkable
                }
            }
        }
    }

    private void Update()
    {
        switch (state) {
            case State.Normal:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mouseWordPosition = Utility.GetMouseWorldPosition();
                    Grid<GridObject> grid = GameHandler_GridCombatSystem.Instance.GetGrid();
                    GridObject gridObject = grid.GetGridObject(mouseWordPosition);
                    
                    //Check if clickcing on a unit
                    if (gridObject.GetUnitGridCombat() != null)
                    {
                        //clicked on top of a unit
                        state = State.Waiting;
                        if (unitGridCombat.IsEnemy(gridObject.GetUnitGridCombat()))
                        {
                        
                            //clicked on an enemy of the current unit
                            if (unitGridCombat.CanAttackUnit(gridObject.GetUnitGridCombat()))
                            {
                                //can attack enemy
                                if (canAttackThisTurn)
                                {
                                    canAttackThisTurn = false;
                                    //attack enemy
                                    unitGridCombat.AttackUnit(gridObject.GetUnitGridCombat(), () =>
                                    {
                                        state = State.Normal;
                                        TestTurnIsOver();

                                    });
                                    break;
                                }
                            }
                            else
                            {
                                //cannot attack enemy
                           
                            }
                        }
                        else
                        {
                            //not an enemy
                        }

                    }
                    else
                    {
                        //no unit here
                    }
                    

                    if (gridObject.GetIsValidMovePosition())
                    {
                        if (canMoveThisTurn)
                        {
                            canMoveThisTurn = false;
                            state = State.Waiting;
                            //remove unit from current grid combat
                            grid.GetGridObject(unitGridCombat.GetPosition()).ClearUnitGridCombat();
                            //set unit on target grid object
                            gridObject.SetUnitGridCombat(unitGridCombat);
                            unitGridCombat.MoveTo(mouseWordPosition, () =>
                            {
                                state = State.Normal;

                                TestTurnIsOver();
                            });
                        }
                    }

                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ForceTurnOver();
                }
                break;
            case State.Waiting:
                break;

        }

    }
    private void TestTurnIsOver()
    {
        if (!canAttackThisTurn && !canMoveThisTurn)
        {
            ForceTurnOver();
        }
    }

    private void ForceTurnOver()
    {
        SelectNextActiveUnit();
        UpdateValidMovePositions();
    }
    public class GridObject
    {
        private Grid<GridObject> grid;
        private int x;
        private int y;
        private bool isValidMovePosition;
        private UnitGridCombat unitGridCombat;
        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public void SetIsValidMovePosition(bool isValidMovePosition)
        {
            this.isValidMovePosition = isValidMovePosition;
        }

        public bool GetIsValidMovePosition()
        {
            return isValidMovePosition;
        }

        public void SetUnitGridCombat(UnitGridCombat unitGridCombat)
        {
            this.unitGridCombat = unitGridCombat;
        }
        public void ClearUnitGridCombat()
        {
            SetUnitGridCombat(null);
        }
        public UnitGridCombat GetUnitGridCombat()
        {
            return unitGridCombat;
        }
    }

}
