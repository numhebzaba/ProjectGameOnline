using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class WallRandomUpDown : NetworkBehaviour
{
    public GameObject[] Walls;
    public float TimeWallRemain = 3f;
    public float speed = 1;
    public bool IsWallCanUp = false;
    public Animator[] m_Animators;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (IsWallCanUp)
        {
            IsWallCanUp = false;
            
            if (IsHost)
            {
                for (int i = 0; i < 3; i++)
                {
                    int RandNum = Random.Range(0, m_Animators.Length - 1);
                    RandomWallUpClientRpc(RandNum);
                }
                
            }
        }


    }
    [ClientRpc]
    public void RandomWallUpClientRpc(int RandNum)
    {
        StartCoroutine(WaitWallDown(RandNum));
         
    }
    public IEnumerator WaitWallCanUp(float time)
    {
        yield return new WaitForSeconds(time);
        IsWallCanUp = !IsWallCanUp;


    }
    IEnumerator WaitWallDown(int RandomNumber)
    {
        yield return new WaitForSeconds(TimeWallRemain);

        IsWallCanUp = false;
 
        m_Animators[RandomNumber].SetBool("IsUp", true); ;
        StartCoroutine(WaitToResetWall(3.0f));

    }
    IEnumerator WaitToResetWall(float time)
    {
        yield return new WaitForSeconds(time);
        for (int i = 0; i < 12; i++)
        {
            m_Animators[i].SetBool("IsUp", false);
        }
        IsWallCanUp = true;
    }
}
