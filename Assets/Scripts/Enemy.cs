using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : LivingEntity
{
    
    Transform target;
    Coord targetTile;
    LivingEntity targetEntity;
    bool hasTarget;

    int attackDistanceThreshold = 1;
    int chasingDistanceThreshold = 10;
    int attackDownTime = 2;
    float damage = 1;

    Material skinMaterial;
    Color originalColor;

    public event System.Action OnEnemyFinishedTurn;

    protected override void Start()
    {
        base.Start();
        skinMaterial = GetComponentInChildren<Renderer>().material;
        originalColor = skinMaterial.color;
        currentState = State.Idle;
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            targetEntity.OnDeath += OnTargetDeath;
        }

    }
    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }
    
    void Update()
    {
        if (isMyTurn && hasTarget)
        {
            targetTile = targetEntity.GetCurrentGridPosition();
            MapGenerator.Instance.GetGrid().GetGridObject(transform.position);
            int distanceToTarget = targetTile.ManhattanDistance(MapGenerator.Instance.GetGrid().GetGridObject(transform.position));
            if (distanceToTarget <= chasingDistanceThreshold)
            {
                if (distanceToTarget <= attackDistanceThreshold)
                {
                    currentState = State.Attacking;
                }
                else
                {
                    currentState = State.Chasing;
                }
            }
            else
            {
                currentState = State.Idle;
            }
        
        }
    }

    IEnumerator Attack()
    {
       
       

        float attackSpped = 3;
        float percent = 0;

        skinMaterial.color = Color.red;

        bool hasAppliedDamage = false;
        while (percent <= 1)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                targetEntity.TakeDamage(damage);
                hasAppliedDamage = true;
            }
            percent += Time.deltaTime * attackSpped;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
           // transform.position = Vector2.Lerp(originalPosition, targetPosition, interpolation);
        }
        isMyTurn = false;
        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        OnEnemyFinishedTurn();
        yield return null;

    }
}
