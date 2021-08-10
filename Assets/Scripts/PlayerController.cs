using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    Rigidbody2D rigidbody;
    Player player;

    private bool isAxisInUse;
    public void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        player = gameObject.GetComponent<Player>();
       
    }

    public void Update()
    {
        
        if (player.isMyTurn)
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 && !isAxisInUse)
            {
                isAxisInUse = true;
                Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0);
                
                player.MoveTowards(moveInput);

            }else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0 && !isAxisInUse)
            {
                isAxisInUse = true;
                Vector3 moveInput = new Vector3(0, Input.GetAxisRaw("Vertical"));
                player.MoveTowards(moveInput);
            }
            
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 0 && Mathf.Abs(Input.GetAxisRaw("Vertical")) == 0)
            {
                isAxisInUse = false;
            }

            
        }

        
    }
}
   
