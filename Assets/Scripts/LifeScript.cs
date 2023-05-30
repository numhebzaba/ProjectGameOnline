using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Cinemachine;

public class LifeScript : NetworkBehaviour
{
    public TMP_Text p1Text;
    public TMP_Text p2Text;
    public TMP_Text p3Text;
    public TMP_Text p4Text;
    PlayerName PlayerName;
    private bool alreadyHit;

    public NetworkVariable<int> lifeP1 = new NetworkVariable<int>(2,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> lifeP2 = new NetworkVariable<int>(2,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> lifeP3 = new NetworkVariable<int>(2,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> lifeP4 = new NetworkVariable<int>(2,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Start is called before the first frame update

    GameLoop gameloop;
    public GameObject cinemachineVirtualCamera;

    void Start()
    {
        p1Text = GameObject.Find("P1LifeText").GetComponent<TMP_Text>();
        p2Text = GameObject.Find("P2LifeText").GetComponent<TMP_Text>();
        p3Text = GameObject.Find("P3LifeText").GetComponent<TMP_Text>();
        p4Text = GameObject.Find("P4LifeText").GetComponent<TMP_Text>();
        PlayerName = GetComponent<PlayerName>();
        gameloop = GameObject.Find("GameLoop").GetComponent<GameLoop>();
        cinemachineVirtualCamera = GameObject.Find("PlayerFollowCamera");

    }
    
    private void updateLife()
    {
        if (IsOwnedByServer)
        { p1Text.text = $"{"Player 1"} : {lifeP1.Value}"; }
        else if (OwnerClientId == 1) { p2Text.text = $"{"Player 2"} : {lifeP2.Value}"; }
        else if (OwnerClientId == 2) { p3Text.text = $"{"Player 3"} : {lifeP3.Value}"; }
        else if (OwnerClientId == 3) { p4Text.text = $"{"Player 4"} : {lifeP4.Value}"; }
      
    }
    // Update is called once per frame
    void Update()
    {
        updateLife();
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsLocalPlayer) return;
        if (alreadyHit) return;

        if (hit.gameObject.tag == "DeathZone")
        {
            Debug.Log("Dead");
            alreadyHit = true;
            if (IsOwnedByServer) 
            {
                if (lifeP1.Value > 0)
                {
                    lifeP1.Value--; Debug.Log(" lifeP1.Value--");
                    GetComponent<PlayerSpawnScript>().Respawn();
                }
                else if (lifeP1.Value == 0)
                {
                    cinemachineVirtualCamera.SetActive(false);
                    GetComponent<PlayerSpawnScript>().DisableCharacter();
                    gameloop.IsPlayer_1CanWin = false;
                    updateCurreentPlayerServerRpc(1);

                }

            }
            else if (OwnerClientId == 1) 
            {
                if (lifeP2.Value > 0)
                {
                    lifeP2.Value--; Debug.Log(" lifeP2.Value--");
                    GetComponent<PlayerSpawnScript>().Respawn();
                }
                else if (lifeP2.Value == 0)
                {
                    cinemachineVirtualCamera.SetActive(false);
                    GetComponent<PlayerSpawnScript>().DisableCharacter();
                    gameloop.IsPlayer_2CanWin = false;
                    updateCurreentPlayerServerRpc(2);

                }
            }
            else if (OwnerClientId == 2) 
            {
                if (lifeP3.Value > 0)
                {
                    lifeP3.Value--; Debug.Log(" lifeP3.Value--");
                    GetComponent<PlayerSpawnScript>().Respawn();
                }
                else if (lifeP3.Value == 0)
                {
                    cinemachineVirtualCamera.SetActive(false);

                    GetComponent<PlayerSpawnScript>().DisableCharacter();
                    gameloop.IsPlayer_3CanWin = false;
                    updateCurreentPlayerServerRpc(3);

                }
            }
            else if (OwnerClientId == 3) 
            {
                if (lifeP4.Value > 0)
                {
                    lifeP4.Value--; Debug.Log(" lifeP4.Value--");
                    GetComponent<PlayerSpawnScript>().Respawn();
                }else if(lifeP4.Value == 0)
                {
                    cinemachineVirtualCamera.SetActive(false);

                    GetComponent<PlayerSpawnScript>().DisableCharacter();
                    gameloop.IsPlayer_4CanWin = false;
                    updateCurreentPlayerServerRpc(4);
                }
            }
                
            StartCoroutine(waitHitReset());
        }
    }
    IEnumerator waitHitReset()
    {
        yield return new WaitForSeconds(3);
        alreadyHit = false;

    }
    [ServerRpc]
    public void updateCurreentPlayerServerRpc(int indexPlayer)
    {
        updateCurreentPlayerClientRpc(indexPlayer);

    }
    [ClientRpc]
    public void updateCurreentPlayerClientRpc(int indexPlayer)
    {
        gameloop.currentPlayerDead++;
        Debug.Log("LifeScript----------------currentPlayerDead : " + (gameloop.currentPlayerDead));
        if (indexPlayer == 1)
            gameloop.IsPlayer_1CanWin = false;
        else if (indexPlayer == 2)
            gameloop.IsPlayer_2CanWin = false;
        else if (indexPlayer == 3)
            gameloop.IsPlayer_3CanWin = false;
        else if (indexPlayer == 4)
            gameloop.IsPlayer_4CanWin = false;


    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //cinemachineVirtualCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();


    }


}
