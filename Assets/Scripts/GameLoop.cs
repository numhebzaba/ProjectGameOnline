using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class GameLoop : NetworkBehaviour
{
    public int CountdownTimePhase_1 = 5;
    public int CountdownTimePhase_2 = 5;
    TMP_Text CountdownTimeText;
    //public GameObject PlatformButtons;
    public GameObject StartButtons;

    public bool IsphaseOne = false;

    public bool IsPlayer_1CanWin = true;
    public bool IsPlayer_2CanWin = true;
    public bool IsPlayer_3CanWin = true;
    public bool IsPlayer_4CanWin = true;


    //public NetworkVariable<int> PlatformButtons_1Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<int> PlatformButtons_2Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<int> PlatformButtons_3Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<int> PlatformButtons_4Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<int> PlatformButtons_5Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<int> PlatformButtons_6Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<int> PlatformButtons_7Point = new NetworkVariable<int>(0,
    //   NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public GameObject[] Platforms;

    int NumberOfPlayer = 0;
    public int currentPlayerDead  = 0;
    public GameObject EndGameTextBar;
    TMP_Text EndGameText;

    public WallRandomUpDown wallRandomUpDown;
    public BombSpawner bombSpawner;
    private void Start()
    {
        CountdownTimeText = GameObject.Find("CountdownTimeText").GetComponent<TMP_Text>();
        CountdownTimeText.enabled = false;
        //PlatformButtons.SetActive(false);

        EndGameText = GameObject.Find("EndGameText (TMP)").GetComponent<TMP_Text>();
        EndGameTextBar.SetActive(false);
        EndGameText.enabled = false;
    }

    private void Update()
    {
        NumberOfPlayer = GameObject.Find("LoginManager").GetComponent<LoginManager>().playerNameList.Count;


    }
    public void StartGameButton()
    {
        if (IsHost)
            startGameClientRpc();
    }
    [ClientRpc]
    public void startGameClientRpc()
    {
        CountdownTimeText.enabled = true;
        //PlatformButtons.SetActive(true);
        StartCoroutine(WalkPhaseCountDownCoroutine());
        StartButtons.SetActive(false);
        
        StartCoroutine(wallRandomUpDown.WaitWallCanUp(3.0f));
        StartCoroutine(bombSpawner.CoolDownBomb(3.0f));
    }
    IEnumerator SelectButtonCountDownCoroutine()
    {
        CheckWinner();

        int CountdownTimeTemp = CountdownTimePhase_1;
        //PlatformButtons.SetActive(true);

        for (int i = CountdownTimeTemp; i >= 0; i--)
        {
            CountdownTimeTemp = i;
            CountdownTimeText.text = i.ToString();
            if (CountdownTimeTemp == 0)
                CountdownTimeText.text = "Phase 1 : Time up";
            Debug.Log(CountdownTimeTemp);
            yield return new WaitForSeconds(1);
        }
        if (CountdownTimeTemp == 0)
        {
            Debug.Log("Phase 1 : Time up");
            StartCoroutine(WalkPhaseCountDownCoroutine());
            CountdownTimeTemp = CountdownTimePhase_1;
            //PlatformButtons.SetActive(false);

        }
    }

    IEnumerator WalkPhaseCountDownCoroutine()
    {
        CheckWinner();
        Debug.Log("Phase 2 : Start");
        CountdownTimeText.color = new Color(255/255f, 148/255f, 0);

        //SetPlayerState(true);
        int CountdownTimeTemp2 = CountdownTimePhase_2;


        for (int i = CountdownTimeTemp2; i >= 0; i--)
        {
            CountdownTimeTemp2 = i;
            CountdownTimeText.text = i.ToString();
            if (CountdownTimeTemp2 == 0)
                CountdownTimeText.text = "Phase 2 : Time up";
            Debug.Log(CountdownTimeTemp2);
            yield return new WaitForSeconds(1);
        }
        if (CountdownTimeTemp2 == 0)
        {
            if (IsHost)
            {
                int RandNum = Random.Range(0, 7);
                PlatFormDropClientRpc(RandNum);

            }
            Debug.Log("wait for next round");
            StartCoroutine(WaitForNextRoundCoroutine(4.0f));
            CountdownTimeTemp2 = CountdownTimePhase_2;
        }
    }
    [ClientRpc]
    public void PlatFormDropClientRpc(int RandNum)
    {
        
        DropPlatFormClientRpc(RandNum);

        WaitToResetPlatFormClientRpc();
        Debug.Log("Complete Loop");
    }
    [ClientRpc]
    public void DropPlatFormClientRpc(int RandNum)
    {

        StartCoroutine(WaitDropPlatFormCoroutine(1.3f, RandNum));

    }
    IEnumerator WaitDropPlatFormCoroutine(float time, int RandNum)
    {
        
        Debug.Log(RandNum);
        switch (RandNum)
        {
            case 0:
                CountdownTimeText.text = "Purple";
                CountdownTimeText.color = new Color(138 / 255f, 43 / 255f, 226 / 255f);
                break;
            case 1:
                CountdownTimeText.text = "Blue";
                CountdownTimeText.color = new Color(0, 0, 255 / 255f);
                break;
            case 2:
                CountdownTimeText.text = "Sky blue";
                CountdownTimeText.color = new Color(0, 255 / 255f, 255 / 255f);
                break;
            case 3:
                CountdownTimeText.text = "Green";
                CountdownTimeText.color = Color.green;
                break;
            case 4:
                CountdownTimeText.text = "Yellow";
                CountdownTimeText.color = Color.yellow;
                break;
            case 5:
                CountdownTimeText.text = "Orange";
                CountdownTimeText.color = new Color(255 / 255f, 153 / 255f, 51 / 255f);
                break;
            case 6:
                CountdownTimeText.text = "Red";
                CountdownTimeText.color = Color.red;
                break;

        }

        yield return new WaitForSeconds(time);
        Platforms[RandNum].SetActive(false);
        

    }
    [ClientRpc]
    public void WaitToResetPlatFormClientRpc()
    {
       
        StartCoroutine(WaitToResetPlatFormCoroutine(3.0f));
    }
    
    IEnumerator WaitToResetPlatFormCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        //if (IsHost)
        //{
        //    PlatformButtons_7Point.Value = 0;
        //    PlatformButtons_6Point.Value = 0;
        //    PlatformButtons_5Point.Value = 0;
        //    PlatformButtons_4Point.Value = 0;
        //    PlatformButtons_3Point.Value = 0;
        //    PlatformButtons_2Point.Value = 0;
        //    PlatformButtons_1Point.Value = 0;
        //}
        Platforms[6].SetActive(true);
        Platforms[5].SetActive(true);
        Platforms[4].SetActive(true);
        Platforms[3].SetActive(true);
        Platforms[2].SetActive(true);
        Platforms[1].SetActive(true);
        Platforms[0].SetActive(true);
    }
    IEnumerator WaitForNextRoundCoroutine(float time)
    {

        yield return new WaitForSeconds(time);
        StartCoroutine(WalkPhaseCountDownCoroutine());
        
    }
    public void PlatformButtons_1()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(1);
        //Debug.Log("PlatformButtons_1 : "+PlatformButtons_1Point);

    }
    public void PlatformButtons_2()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(2);
        //Debug.Log("PlatformButtons_2 : " + PlatformButtons_2Point);



    }
    public void PlatformButtons_3()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(3);
        //Debug.Log("PlatformButtons_3 : " + PlatformButtons_3Point);



    }
    public void PlatformButtons_4()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(4);
        //Debug.Log("PlatformButtons_4 : " + PlatformButtons_4Point);

    }
    public void PlatformButtons_5()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(5);
        //Debug.Log("PlatformButtons_5 : " + PlatformButtons_5Point);

    }
    public void PlatformButtons_6()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(6);
        //Debug.Log("PlatformButtons_6 : " + PlatformButtons_6Point);

    }
    public void PlatformButtons_7()
    {
        setActivePlatformButtons(false);
        //PlatformButtons.SetActive(false);
        AddPlatformButtons_PointServerRpc(7);
        //Debug.Log("PlatformButtons_7 : " + PlatformButtons_7Point);

    }
    public void setActivePlatformButtons(bool setValue)
    {
        //PlatformButtons.SetActive(false);

    }
    [ServerRpc(RequireOwnership = false)]
    public void AddPlatformButtons_PointServerRpc(int number)
    {
        //if(number ==1)
        //    PlatformButtons_1Point.Value++;
        //else if (number == 2)
        //    PlatformButtons_2Point.Value++;
        //else if (number == 3)
        //    PlatformButtons_3Point.Value++;
        //else if (number == 4)
        //    PlatformButtons_4Point.Value++;
        //else if (number == 5)
        //    PlatformButtons_5Point.Value++;
        //else if (number == 6)
        //    PlatformButtons_6Point.Value++;
        //else if (number == 7)
        //    PlatformButtons_7Point.Value++;
    }
    public void CheckWinner()
    {
        showWinnerServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void showWinnerServerRpc()
    {
        showWinnerClientRpc();

    }
    [ClientRpc]
    public void showWinnerClientRpc()
    {
        if (NumberOfPlayer - currentPlayerDead == 1)
        {
            EndGameTextBar.SetActive(true);
            EndGameText.enabled = true;

            if (IsPlayer_1CanWin)
                EndGameText.text = $" Winner : Player 1 ";
            else if (IsPlayer_2CanWin)
                EndGameText.text = $" Winner : Player 2 ";
            else if (IsPlayer_3CanWin)
                EndGameText.text = $" Winner : Player 3 ";
            else if (IsPlayer_4CanWin)
                EndGameText.text = $" Winner : Player 4 ";

            Time.timeScale = 0;
        }
        
    }
}
