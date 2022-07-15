using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public float delayMin = 1f;
    public float delayMax = 5f;

    bool spawning;
    public bool isFunctional;

    int spawnMax = 20;
    GameObject[] spawns;

    void Awake()
    {
        spawns = new GameObject[spawnMax];
    }

    public void Spawn()
    {
        isFunctional = true;
        if(!spawning)
            StartCoroutine(SpawnCo());
    }

    public void StopSpawn()
    {
        spawning = false;
    }

    IEnumerator SpawnCo()
    {
        spawning = true;
        while(spawning)
        {
            //Instantiate(prefab, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), Quaternion.identity);
            for(int i = 0; i < spawnMax; i++)
            {
                if(spawns[i] == null)
                {
                    spawns[i] = Instantiate(prefab, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), Quaternion.identity);
                    break;
                }
            }
            yield return new WaitForSeconds(Random.Range(delayMin, delayMax));
        }

        spawning = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }
    void OnEnable()
    {
        if(isFunctional)
            Spawn();
    }
}
