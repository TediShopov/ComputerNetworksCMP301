using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rigidbody;
    public int Damage = 15;
    // Start is called before the first frame update
    

    public void SetVelocity(Vector2 vel) 
    {
        rigidbody.velocity = vel * speed;
    }
    //void Start()
    //{
    //    rigidbody.velocity = transform.right * speed;
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogError("Projectile Collider");
        collision.gameObject.GetComponent<HealthScript>().TakeDamage(Damage);
        Destroy(gameObject);
    }

  
}
