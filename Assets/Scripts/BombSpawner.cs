using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombSpawner : NetworkBehaviour
{
    public GameObject bombPrefab;
    public GameObject[] spawnPoints;

    public float timeCD = 2.0f;

    public bool canSpawnBomb = false;

    private List<GameObject> spawnedBomb = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (canSpawnBomb)
        {
            RandomBomb();
        }


    }
    void RandomBomb()
    {
        if (IsHost)
        {
            int RandNum = Random.Range(0, spawnPoints.Length - 1);
            SpawnBomb(RandNum);
        }
    }

    void SpawnBomb(int RandNum)
    {
        canSpawnBomb = false;
        Vector3 spawnPos = spawnPoints[RandNum].transform.position;
        Quaternion spawnRot = transform.rotation;
        GameObject bomb = Instantiate(bombPrefab, spawnPos, spawnRot);
        spawnedBomb.Add(bomb);
        bomb.GetComponent<Bomb>().bombspawner = this;
        bomb.GetComponent<NetworkObject>().Spawn(true);
        Debug.Log("spawn bomb");
        StartCoroutine(CoolDownBomb(timeCD));

    }

    public IEnumerator CoolDownBomb(float timeCD)
    {
        yield return new WaitForSeconds(timeCD);
        canSpawnBomb = true;

    }
    IEnumerator WaitBombDrop(float Droptime)
    {
        yield return new WaitForSeconds(Droptime);
    }
    [ServerRpc (RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectId)
    {
        if (!IsOwner) return;

        GameObject toDestroy = findBombFromNetworkId(networkObjectId);
        if (toDestroy == null) return;

        waitToDestroy(toDestroy);
        spawnedBomb.Remove(toDestroy);
        Destroy(toDestroy,1.5f);

    }
    IEnumerator waitToDestroy(GameObject toDestroy)
    {
        yield return new WaitForSeconds(1.5f);
        toDestroy.GetComponent<NetworkObject>().Despawn();

    }
    private GameObject findBombFromNetworkId(ulong networkObjectId)
    {
        foreach(GameObject bomb in spawnedBomb)
        {
            ulong bombId = bomb.GetComponent<NetworkObject>().NetworkObjectId;
            if(bombId == networkObjectId)
            {
                return bomb;
            }
        }
        return null;
    }
}
