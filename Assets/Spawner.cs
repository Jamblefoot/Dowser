using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public float delayMin = 1f;
    public float delayMax = 5f;

    bool spawning;

    public void Spawn()
    {
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
            Instantiate(prefab, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(delayMin, delayMax));
        }

        spawning = false;
    }
}
