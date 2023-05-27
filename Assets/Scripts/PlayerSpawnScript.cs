using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
public class PlayerSpawnScript : NetworkBehaviour
{
    StarterAssets.ThirdPersonController mainPlayer;
    public Behaviour[] scripts;
    private Renderer[] renderers;

    [SerializeField] 
    PlayerInput _playerInput;

    GameLoop gameLoop;
    void Start()
    {
        //mainPlayer = GetComponent<StarterAssets.ThirdPersonController>();
        renderers = GetComponentsInChildren<Renderer>();
        gameLoop = GameObject.Find("GameLoop").GetComponent<GameLoop>();

    }
    private void Update()
    {
        if (IsClient)
        {
            if (gameLoop.IsphaseOne)
            {
                _playerInput.actions.Disable();
            }
            else
            {
                _playerInput.actions.Enable();
            }
        }
        
    }

    private void SetPlayerState(bool state)
    {
        foreach (var script in scripts) { script.enabled = state; }
        foreach (var renderer in renderers) { renderer.enabled = state; }
    }
    private Vector3 GetRandPos()
    {
        Vector3 randPos = new Vector3(Random.Range(-1f, 3f), 0.7f, Random.Range(0f, 8f));
        Debug.Log(randPos);

        return randPos;
    }
    public void Respawn()
    {
        Debug.Log("Respawn");
        RespawnServerRpc();
    }
    [ServerRpc]
    private void RespawnServerRpc()
    {
        Debug.Log("RespaRespawnServerRpcwn");
        RespawnClientRpc(GetRandPos());
    }
    [ClientRpc]
    private void RespawnClientRpc(Vector3 spawnPos)
    {
        Debug.Log("RespawnClientRpc");

        //mainPlayer.enabled = false; transform.position = spawnPos; mainPlayer.enabled = true;
        StartCoroutine(RespawnCoroutine(spawnPos));
    }

    IEnumerator RespawnCoroutine(Vector3 spawnPos)
    {
        //Debug.Log("RespawnCoroutine");

        SetPlayerState(false);
        transform.position = spawnPos;
        //Debug.Log(spawnPos);

        //transform.position = Vector3.zero;
        //transform.position = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(3);
        SetPlayerState(true);
    }
    public void DisableCharacter()
    {
        SetPlayerState(false);
    }
}
