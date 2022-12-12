using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rigidbody;
    public int Damage = 15;
    // Start is called before the first frame update

    public StateProjectileManager AttachToManager;

    public void AddToManager(GameObject stateObj) 
    {
        AttachToManager = stateObj.GetComponent<StateProjectileManager>();
        this.AttachToManager.ProjectilesInState.Add(this.gameObject);
        this.gameObject.layer = stateObj.layer;
    }

    public void RemoveFromManager() 
    {
       int index= AttachToManager.ProjectilesInState.FindIndex(x => x == gameObject);
        AttachToManager.ProjectilesInState.RemoveAt(index);
    }
    public void SetVelocity(Vector2 vel) 
    {
        rigidbody.velocity = vel * speed;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
         Debug.LogError("Projectile Collider");

        RemoveFromManager();
        Destroy(gameObject);
        
    }

  
}
