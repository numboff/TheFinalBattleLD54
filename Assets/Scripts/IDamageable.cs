using UnityEngine;

public interface IDamageable
{
    // Start is called before the first frame update
    void Damage(float damage, Vector2 AttackDirection);
}
