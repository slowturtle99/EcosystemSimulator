using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using System.Text;
using System.IO;
using System;

public class FishManager : MonoBehaviour {

    public TextMeshProUGUI ScriptTxt;

    private Fish[] Fishes;
    private Plankton[] Planktons;

    private int numFish = 0;
    private int numPlankton = 0;

    public string fileName = "gene_statistics";
    private string filePath;
    private StreamWriter outStream;

    private float simulationTime = 0.0f;
    private float lastWriteTime = 59.0f;
    


    void Start () {
        ScriptTxt.text =  "";

        filePath = GetPath();
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        fileName += ".csv";
        outStream = new StreamWriter(filePath + fileName);
        outStream.Close();
    }

    void Update () {
        simulationTime += Time.deltaTime;
        lastWriteTime += Time.deltaTime;
        Fishes = FindObjectsOfType<Fish>();
        numFish = Fishes.Length;
        numPlankton = FindObjectsOfType<Plankton>().Length;
        ScriptTxt.text =  "Fish: " + numFish + "\n"+ "Plankton: " + numPlankton;
        
        if(lastWriteTime > 60.0f){
            lastWriteTime = 0.0f;
            outStream = new StreamWriter(filePath + fileName, true);
            for (int i = 0; i < numFish; i++)
            {
                outStream.WriteLine("{0},{1},{2}", simulationTime, Fishes[i].gene.adultMass, Fishes[i].gene.idealMuscleRatio);
            }
            outStream.Close();
        }
        
    }


    private static string GetPath()
    {
#if UNITY_EDITOR
        return Application.dataPath + "/Resources/CSV/";
#elif UNITY_ANDROID
        return Application.persistentDataPath + "/Resources/CSV/";
#elif UNITY_IPHONE
        return Application.persistentDataPath +"/Resources/CSV/";
#else
        return Application.dataPath +"/Resources/CSV/";
#endif
    }

}