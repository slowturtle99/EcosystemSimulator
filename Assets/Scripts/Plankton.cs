using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plankton : MonoBehaviour
{
    /// Components
    private Rigidbody rb;
    
    /// Physical States
    public float age = 0.0f;
    public float mass = 0.01f;
    public int eatCount = 10;
    public float size;

    void Start() {
        // size = Random.Range(0.1f,2.0f);
        size = ChiSquared(6)/12.0f+0.035f;
    }

    public void Die()
    {
        if(eatCount < 0){
            Destroy(gameObject);
        } else {
            eatCount--;
        }
        
    }

    void Update() {
        age += Time.deltaTime;
        if(age > 360.0f){
            Destroy(gameObject);
        }    
    }

    private float NormalRandom(float mean, float std){
        float u1 = Random.Range(0.0000000001f,1);
        float u2 = Random.Range(0.0000000001f,1);
        return mean + std * Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
    }
    private float ChiSquared(int k){
        float s = 0.0f;
        for (int i = 0; i < k; i++)
        {
            s += Mathf.Pow(NormalRandom(0.0f, 1.0f),2.0f);
        }
        return s;
    }

}
