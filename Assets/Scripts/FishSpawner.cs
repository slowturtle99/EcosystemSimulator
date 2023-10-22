using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public enum GizmoType { Never, SelectedOnly, Always }

    public Fish prefab;
    public float spawnRadius = 1;
    public int spawnCount = 1;
    public float idealMuscleRatio = 0.5f;
    public float adultMass = 0.4f;
    public Color color;
    public GizmoType showSpawnRegion;

    void Awake () {
        for (int i = 0; i < spawnCount; i++) {
            adultMass = Mathf.Exp(Random.Range(-1.8f,0.69f));
            if (Random.Range(0.0f,1.0f)<0.75f)
            {
                adultMass = Mathf.Exp(Random.Range(-1.8f,-1.0f));
            }
            idealMuscleRatio = Random.Range(0.3f,0.6f);
            Fish.Gene tempGene = new Fish.Gene();
            tempGene.idealMuscleRatio = idealMuscleRatio;
            tempGene.adultMass = adultMass;

            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Fish fish = Instantiate (prefab);
            tempGene.habitatPos = pos;
            fish.transform.position = pos;
            fish.transform.forward = Random.onUnitSphere;
            float frac = Random.Range(0.2f,1.0f);
            fish.muscle = adultMass*idealMuscleRatio*frac;
            fish.fat = adultMass*(1.0f-idealMuscleRatio)*frac;
            fish.gene = tempGene;
            fish.age = Random.Range(0.0f, fish.setting.maxAge*Mathf.Sqrt(adultMass));
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
        Gizmos.color = new Color (color.r, color.g, color.b, 0.3f);
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }
}
