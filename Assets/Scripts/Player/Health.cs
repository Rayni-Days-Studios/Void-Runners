using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{

    public float MaxHitPoints = 100;
    private float currentHitPoints;
    
    void Start ()
	{
	    currentHitPoints = MaxHitPoints;
	}

    [RPC]
    void TakeDamage(float amount)
    {
        currentHitPoints -= amount;
        if (currentHitPoints <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
