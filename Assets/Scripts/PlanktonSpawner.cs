using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanktonSpawner : MonoBehaviour
{
    public enum GizmoType { Never, SelectedOnly, Always }

    public Plankton prefab;

    public float spawnRadius = 10;
    public float spawnDepth = 10;
    public float spawnRate = 1.0f;
    public float spawnCount = 1;
    public Color colour;
    public GizmoType showSpawnRegion;

    private float volume;


    // Start is called before the first frame update
    void Start()
    {
        volume = 4.0f*spawnRadius*spawnRadius*spawnDepth;

        for (int i = 0; i < (int)(spawnCount*volume); i++)
        {
            Vector3 pos = transform.position + new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnDepth, 0), Random.Range(-spawnRadius, spawnRadius));
            Plankton plankton = Instantiate (prefab);
            plankton.transform.position = pos;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < (int)(spawnRate*volume); i++){
            Vector3 pos = transform.position + new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnDepth, 0), Random.Range(-spawnRadius, spawnRadius));
            Plankton plankton = Instantiate (prefab);
            plankton.transform.position = pos;
        }

        if (Random.Range(0.0f,1.0f) < (spawnRate*volume)%1.0f){
            Vector3 pos = transform.position + new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnDepth, 0), Random.Range(-spawnRadius, spawnRadius));
            Plankton plankton = Instantiate (prefab);
            plankton.transform.position = pos;
        }
    }

    

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {

        Gizmos.color = new Color (colour.r, colour.g, colour.b, spawnRate*200.0f);
        Gizmos.DrawCube(transform.position + new Vector3(0, -spawnDepth/2, 0), new Vector3(2.0f*spawnRadius, spawnDepth, 2.0f*spawnRadius));
    }
}
