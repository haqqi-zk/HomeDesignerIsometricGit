using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject spawnablePrefab = null;
    public float cycleTime = 1.0f;
    float nextSpawnTime = 1.0f;

	// Use this for initialization
	void Start () {
        nextSpawnTime = cycleTime;
	}
	
	// Update is called once per frame
	void Update () {
        nextSpawnTime -= Time.deltaTime;
        if (nextSpawnTime < 0)
        {
            nextSpawnTime = cycleTime;
            if (spawnablePrefab)
                Instantiate(spawnablePrefab, this.transform.position, this.transform.rotation);
        }
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(this.transform.position, 1.0f);
    }
}