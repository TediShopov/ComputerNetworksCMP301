using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{

    private const int _HIGH_DMG_THRESHOLD = 15;

    public int MaxHealth = 100;
    public int MinHealth = 0;


    public bool IsEnemy;
    public int CurrentHealth { get;  private set; }

    private FighterController Fighter;
    private void Awake()
    {
        CurrentHealth = MaxHealth;
        Fighter = this.gameObject.GetComponent<FighterController>();
    }

    public void SetValues(HealthScript healthScript) 
    {
        this.MaxHealth = healthScript.MaxHealth;
        this.MinHealth = healthScript.MinHealth;
        this.CurrentHealth = healthScript.CurrentHealth;
        this.IsEnemy = healthScript.IsEnemy;

    }

    public void TakeDamage(int dmgAmount) 
    {

        if (Fighter.GetBlocking())
        {
            dmgAmount = 0;
            Debug.LogError("Blocked");
            return;
        }
        this.CurrentHealth -= dmgAmount;
        if (this.CurrentHealth<0)
        {
            this.CurrentHealth = 0;
            Fighter.SetDying(true);
        }

        if (dmgAmount > _HIGH_DMG_THRESHOLD)
        {
            Fighter.setDamaged(false);
        }
        else 
        {
            Fighter.setDamaged(true);
        }
    }

    
  
    // Update is called once per frame
    void Update()
    {
      

        float fillValue = (float)CurrentHealth / (float)MaxHealth;
    }
}
