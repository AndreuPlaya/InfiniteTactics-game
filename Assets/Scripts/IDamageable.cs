
using UnityEngine;

public interface IDamageable
{
    void TakeHit(float damage, RaycastHit2D hit);

    void TakeDamage(float damage);
}
