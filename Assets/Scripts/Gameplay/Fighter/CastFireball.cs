using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastFireball : StateMachineBehaviour
{
    public float offsetProjectile = 0.5f;
    public Projectile projectilePrefab;

    FighterController Fighter;
   

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Fighter == null)
        {
            Fighter = animator.gameObject.GetComponent<FighterController>();
        }

        Vector3 dirToEnemy = Fighter.GetDirToEnemy();
        //Make if flat
        dirToEnemy.y = 0;
        dirToEnemy.Normalize();
        dirToEnemy *= offsetProjectile;
        Vector3 pos = animator.transform.position + dirToEnemy;

        projectilePrefab.gameObject.layer = Fighter.gameObject.layer;
        var g= Instantiate(projectilePrefab, pos, animator.transform.rotation) as Projectile;
        g.AddToManager(animator.gameObject.transform.parent.gameObject);
        g.SetVelocity(dirToEnemy);
        Fighter.SetCastingFireball(false);
    }
}
