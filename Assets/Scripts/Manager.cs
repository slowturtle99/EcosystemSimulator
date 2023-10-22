using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Manager : MonoBehaviour
{
    public bool showHabitat = false;
    public bool limitFishPopulation = true;
    public int fishPopulationLimit = 2000;

    public TextMeshProUGUI FishStatusTMP;
    public TextMeshProUGUI HelpTMP;

    public GameObject camera1;
    public GameObject camera2;
    public GameObject camera3;
    public GameObject camera4;

    private bool fishCamEnabled = false; 

    private Fish[] Fishes;
    private int numFish = 0;
    private Fish fish;

    // Start is called before the first frame update
    void Start()
    {
        CameraReset();
        camera1.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H)){
            showHabitat = !showHabitat;
        }

        if(Input.GetKeyDown(KeyCode.L)){
            limitFishPopulation = !limitFishPopulation;
        }

        if(Input.GetKeyDown(KeyCode.C)){
            HelpTMP.gameObject.SetActive(!HelpTMP.gameObject.activeSelf);
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)){
            CameraReset();
            camera1.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            CameraReset();
            camera2.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            CameraReset();
            camera3.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)){
            CameraReset();
            camera4.SetActive(true);
            fishCamEnabled = true;
            FishStatusTMP.gameObject.SetActive(true);

            Fishes = FindObjectsOfType<Fish>();
            numFish = Fishes.Length;
            fish = Fishes[Random.Range(0, numFish)];
            
        }

        if(limitFishPopulation){
            Fishes = FindObjectsOfType<Fish>();
            numFish = Fishes.Length;
            if(numFish > fishPopulationLimit){
                Debug.Log("Exceeded Population Limit.");
                Fishes[Random.Range(0, numFish)].Die();
            }
        }

        if(fishCamEnabled){
            if (fish != null) {
                camera4.transform.position = 0.05f*camera4.transform.position + 0.95f*(fish.P_ - camera4.transform.forward*0.8f);
                camera4.transform.forward = (0.05f*fish.heading_ + 0.95f*camera4.transform.forward).normalized;
                ShowFishStatus(fish);
            }
            else {
                Fishes = FindObjectsOfType<Fish>();
                numFish = Fishes.Length;
                fish = Fishes[Random.Range(0, numFish)];
            }
            
        }
        
    }

    void CameraReset() {
        camera1.SetActive(false);
        camera2.SetActive(false);
        camera3.SetActive(false);
        camera4.SetActive(false);
        fishCamEnabled = false;
        FishStatusTMP.text =  "";
        FishStatusTMP.gameObject.SetActive(false);
    }

    void ShowFishStatus(Fish fish) {
        Fishes = FindObjectsOfType<Fish>();
        numFish = Fishes.Length;
        int numSameSpecies = -1;
        for (int i = 0; i < numFish; i++) {
            if (fish.isSameSpecies(Fishes[i])) {
                numSameSpecies++;
            }
        }
        FishStatusTMP.text = string.Format(
            "Status \t- Mass: {0:F4} | Fat: {1:F4} | Muscle: {2:F4} | Age: {3:F1} \n" + 
            "Speed \t- Speed: {4:F4} | Max Speed: {5:F4} | Idle Speed: {6:F4} \n" +
            "Gene \t\t- Adult Mass: {7:F4} | Ideal Muscle Ratio: {8:F2} | Max Age: {9:F1} \n" +
            "Energy In \t- Plankton: {10:F4} | Predation: {11:F4} \n" +
            "Energy Out \t- Basal Metabolism: {12:F4} | Swimming: {13:F4} \n\t\t- Growth: {14:F4} | Reproduction: {15:F4} \n" +
            "View \t\t- Viewing Range: {16:F2} | Same: {17:D2} | Mate: {18} | Prey: {19} | Predator: {20} \n" +
            "Species \t- Same Species: {21}"
            ,fish.mass, fish.fat, fish.muscle, fish.age,
            fish.speed_, fish.maxSpeed, fish.idleSpeed,
            fish.gene.adultMass, fish.gene.idealMuscleRatio, fish.maxAge,
            fish.planktonEnergy, fish.predationEnergy,
            fish.basalMetabolismEnergy, fish.swimmingEnergy, fish.growthEnergy, fish.reproductionEnergy,
            fish.viewingRange, fish.numSame, fish.numMate, fish.numPrey, fish.numPredator,
            numSameSpecies);
    }

}

