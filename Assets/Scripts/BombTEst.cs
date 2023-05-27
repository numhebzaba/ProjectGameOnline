using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTEst : MonoBehaviour
{
    public GameObject exp;
    public float expForce, radius;
    private void OnCollisionEnter(Collision other)
    {
        GameObject _exp = Instantiate(exp, transform.position, transform.rotation);
        Destroy(_exp,3);
        knockBack();
        Destroy(gameObject);
       

    }
    void knockBack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider nearyby in colliders)
        {
            Rigidbody rigg = nearyby.GetComponent<Rigidbody>();
            if (rigg != null)
            {
                rigg.AddExplosionForce(expForce, transform.position, radius);
            }
            //else
            //{
            //    //characterImpact = GameObject.FindWithTag("Player").GetComponent<CharacterImpact>();
            //    CharacterImpact characterImpact = nearyby.transform.GetComponent<CharacterImpact>();

            //    var dir = exp.transform.position - nearyby.transform.position;
            //    characterImpact.AddImpact(dir, expForce);
            //}
        }

    }
}
