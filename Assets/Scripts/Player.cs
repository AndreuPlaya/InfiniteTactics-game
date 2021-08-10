using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : LivingEntity
{


    public event System.Action OnPlayerFinishedTurn;
    protected override void Start()
    {
        base.Start();
    }

   
}
