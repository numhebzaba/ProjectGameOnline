using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class Bomb : NetworkBehaviour
{
    public GameObject exp;
    public float expForce, radius;

     
    public AudioSource audioSource;

    public Renderer rend;

    public BombSpawner bombspawner;

    private void Start()
    {
        
        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }
    private void OnCollisionEnter(Collision other)
    {
        
        //if (!IsOwner) return;
        
        ulong networkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
        SpawnEffect();
        knockBack();
        audioSource.Play();
        RenderMeshtoFalseClientRpc();
        bombspawner.DestroyServerRpc(networkObjectId);




    }
    [ClientRpc]
    void RenderMeshtoFalseClientRpc()
    {
        rend.enabled = false;
    }
    void SpawnEffect()
    {
        GameObject _exp = Instantiate(exp, transform.position, transform.rotation);
        _exp.GetComponent<NetworkObject>().Spawn();
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
            else if (nearyby.CompareTag("Player"))
            {
                
                //characterImpact = GameObject.FindWithTag("Player").GetComponent<CharacterImpact>();
                CharacterImpact characterImpact = nearyby.transform.GetComponent<CharacterImpact>();
                Debug.Log(characterImpact);
                Debug.Log(nearyby.GetComponent<NetworkObject>().NetworkObjectId);
                
                var dir = exp.transform.position - nearyby.transform.position;
                Debug.Log("-----------------------------");
                Debug.Log(dir);

                characterImpact.AddImpact(dir, expForce);
            }
        }

    }
}
