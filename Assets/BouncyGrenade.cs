using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class BouncyGrenade : MonoBehaviour {

    public static void Create(Transform pfBouncyGrenade, Vector3 spawnPosition, Vector3 targetPosition, Action<Vector3> onExplodeAction) {
        BouncyGrenade bouncyGrenade = Instantiate(pfBouncyGrenade, spawnPosition, Quaternion.identity).GetComponent<BouncyGrenade>();

        bouncyGrenade.Setup(targetPosition, onExplodeAction);
    }

    private Action<Vector3> onExplodeAction;
    private float timeToExplode;
    private int bounceState;

    private void Setup(Vector3 targetPosition, Action<Vector3> onExplodeAction) {
        this.onExplodeAction = onExplodeAction;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        float moveSpeed = 250f;
        gameObject.GetComponent<Rigidbody2D>().velocity = moveDirection * moveSpeed;
        transform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVector(moveDirection));

        timeToExplode = 2.5f;
        bounceState = 0;
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.GetComponent<ExplodeOnContact>() != null) {
            ExplodeGrenade();
        }
    }

    private void Update() {
        switch (bounceState) {
        default:
        case 0:
            transform.localScale += Vector3.one * 7f * Time.deltaTime;
            if (transform.localScale.x >= 2.5f) bounceState = 1;
            break;
        case 1:
            transform.localScale -= Vector3.one * 7f * Time.deltaTime;
            if (transform.localScale.x <= 1f) bounceState = 2;
            break;
        case 2:
            break;
        }

        timeToExplode -= Time.deltaTime;
        if (timeToExplode <= 0f) {
            ExplodeGrenade();
        }
    }

    private void ExplodeGrenade() {
        onExplodeAction(transform.position);
        Destroy(gameObject);
    }
}
