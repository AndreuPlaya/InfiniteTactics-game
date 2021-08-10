using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;
    public Coord startingTile;
    public bool isMyTurn;

    public enum State { Idle, Attacking, Chasing}
    protected State currentState;

    protected float health;
    protected bool dead;
    private float moveSpeed = 3f;

    protected Coord currentTile;
    protected Coord moveTile;
    


    public LayerMask whatStopsMovement;
    public event System.Action OnDeath;
    public event System.Action OnActionFinish;

    protected virtual void Start()
    {
        health = startingHealth;
        currentTile = startingTile;
        moveTile = startingTile;
    }
    private void Update()
    {
        if (MapGenerator.Instance.mapIsReady)
        {
            transform.position = Vector3.MoveTowards(transform.position, MapGenerator.Instance.GetGrid().GetGridObject(moveTile.x, moveTile.y).GetWorldPos(), moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, MapGenerator.Instance.GetGrid().GetGridObject(moveTile.x, moveTile.y).GetWorldPos()) == 0.0)
            {
                currentTile = moveTile;
                isMyTurn = true;
            }
        }
            
    }
    public void TakeHit(float damage, RaycastHit2D hit)
    {
        TakeDamage(damage);
    }
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }
    public void MoveTowards(Vector3 moveDirection)
    {
        Coord targetTile = MapGenerator.Instance.GetGrid().GetGridObject(currentTile.x + (int)moveDirection.x, currentTile.y + (int)moveDirection.y);
        Collider2D nextTileObject = CheckTile(targetTile);
        if (nextTileObject == null)
        {
            isMyTurn = false;
            moveTile = targetTile;

            Debug.Log(gameObject.name + " moved to : " + targetTile.ToString());
        }
        else
        {
            Debug.Log(gameObject.name + " cannot move to : " + targetTile.ToString() + " tile is occupied by: " + nextTileObject.GetComponent<Transform>().name);
        }
    }
    

    private Collider2D CheckTile(Coord targetTile)
    {
        float worldPosX = MapGenerator.Instance.GetGrid().GetGridObject(targetTile.x, targetTile.y).GetWorldPos().x;
        float worldPosY = MapGenerator.Instance.GetGrid().GetGridObject(targetTile.x, targetTile.y).GetWorldPos().y;
        Vector3 worldPos = new Vector3(worldPosX, worldPosY);
        Debug.DrawLine(transform.position, worldPos, Color.green);
        return Physics2D.OverlapPoint(worldPos);

    }
    public Coord GetCurrentGridPosition()
    {
        return currentTile;
    }
    
    [ContextMenu("Self Destruct")]
    protected void Die()
    {
        dead = true;
        if (OnDeath != null)
        {
            Debug.Log(this.name + " has died");
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
