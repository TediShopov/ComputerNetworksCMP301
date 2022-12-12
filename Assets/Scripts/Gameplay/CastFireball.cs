using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastFireball : StateMachineBehaviour
{
    public float offsetProjectile = 0.5f;
    public Projectile projectilePrefab;

    FighterController FighterController;
   

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (FighterController==null)
        {
            FighterController = animator.gameObject.GetComponent<FighterController>();
        }

        Vector3 dirToEnemy = FighterController.GetDirToEnemy();
        //Make if flat
        dirToEnemy.y = 0;
        dirToEnemy.Normalize();
        dirToEnemy *= offsetProjectile;
        Vector3 pos = animator.transform.position + dirToEnemy;

        var g= Instantiate(projectilePrefab, pos, animator.transform.rotation) as Projectile;
        g.SetVelocity(dirToEnemy);
    }
}
