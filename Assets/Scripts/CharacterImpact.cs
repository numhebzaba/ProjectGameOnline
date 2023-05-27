using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System;

public class CharacterImpact : NetworkBehaviour
{
    float mass = 3.0F; // defines the character mass
    Vector3 impact = Vector3.zero;
    public CharacterController character;
    // Use this for initialization
    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //// apply the impact force:
        //if (impact.magnitude > 0.2F) character.Move(impact * Time.deltaTime);
        //// consumes the impact energy each cycle:
        //impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
    }
    // call this function to add an impact force:

    public void AddImpact(Vector3 dir, float force)
    {
        Debug.Log(dir);
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;
        Debug.Log(impact);
        // apply the impact force:
        //if (impact.magnitude > 0.2F) 
        

        try
        {
            character.Move(impact * Time.deltaTime);
            Debug.Log(character + "Moveeeeeeeeeeeeeeeeee");
            Debug.Log(character.GetComponent<NetworkObject>().NetworkObjectId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        //if(IsOwner)
        //    AddImpactServerRpc(dir, force);

    }
    [ServerRpc]
    public void AddImpactServerRpc(Vector3 dir, float force)
    {
        AddImpactClientRpc(dir,force);
    }
    [ClientRpc]
    public void AddImpactClientRpc(Vector3 dir, float force)
    {
        Debug.Log(dir);
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;

        // apply the impact force:
        if (impact.magnitude > 0.2F) character.Move(impact * Time.deltaTime);
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
    }


}
