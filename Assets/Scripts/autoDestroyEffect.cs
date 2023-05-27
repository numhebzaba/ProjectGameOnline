using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class autoDestroyEffect : NetworkBehaviour
{
    public float delayBeforeDestroy = 3f;
    private ParticleSystem ps;
    public void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if(ps && !ps.IsAlive())
        {
            DestroyObject();
        }
    }
    void DestroyObject()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject, delayBeforeDestroy);
    }
}
