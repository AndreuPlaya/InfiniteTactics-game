using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UnitGridCombat : MonoBehaviour
{
    [SerializeField] private Team team;

    private Character_Base characterBase;
    private HealthSystem healthSystem;
    private GameObject selectedGameObject;
    private State state;
    private World_Bar worldBar;
    public enum Team { Player, Enemy};

    private enum State { Iddle, Chasing, Attacking}

    private void Awake()
    {
        
    }

    private void HealthSystem_OnHealthChanged(Object sender, EventArgs e)
    {

    }

    private void Update()
    {
        
    }

    public void SetSelectedVisible(bool visible)
    {

    }

    public void MoveTo(Vector3 targetPosition, Action onReachedPosition) 
    {
        
    }

    public bool CanAttackUnit(UnitGridCombat unitGridCombat)
    {
        return Vector3.Distance(GetPosition(), unitGridCombat.GetPosition()) < 60f;
    }
    public void AttackUnit(UnitGridCombat unitGridCombat,Action onAttackComplete)
    {

    }
    public void Damage(UnitGridCombat attacker, int damageAmmount)
    {

    }

    public bool IsDead()
    {
        return false;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
