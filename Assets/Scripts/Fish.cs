using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    /* Setting */
    public FishSetting setting;
    private Manager manager;

    public Gene gene = new Gene();

    /* States */
    public float fat = 1.0f;
    public float muscle = 1.0f;
    public float age = 0.0f;
    public float maxAge = 0.0f;
    public string status = "None";
    public float speed_sp;
    public float maxSpeed = 4.0f;
    public float idleSpeed = 1.0f;

    /* Energy */
    public float basalMetabolismEnergy = 0.0f;
    public float swimmingEnergy = 0.0f;
    public float growthEnergy = 0.0f;
    public float reproductionEnergy = 0.0f;
    public float planktonEnergy = 0.0f;
    public float predationEnergy = 0.0f;

    /* Physical States */
    public float mass = 1.0f;
    public Vector3 heading_, P_, V_, W_;
    public float speed_;
    public float boundRadius;
    public float scale;

    /* Gene Status */
    public float adultMass;
    public float idealMuscleRatio;

    /* View */
    public float viewingRange;
    public int numSame = 0;
    public int numMate = 0;
    public int numPrey = 0;
    public int numPredator = 0;

    /* Flocking */
    private Vector3 avgFlockHeading;
    private Vector3 offsetToFlockCetre;
    private Vector3 seperationHeading;
    private Vector3 offsetToMate;
    private Vector3 offsetToPrey;
    private Vector3 offsetToPredator;

    /* Components */
    private Rigidbody rb;
    private CapsuleCollider cl;
    private Renderer[] rd;
    private Transform hbtf;
    private Animator animator;

    /* Gizmo */
    public enum GizmoType { Never, SelectedOnly, Always }
    public GizmoType showHabitat;

    public class Gene
    {
        public float adultMass = 0.4f;
        public float idealMuscleRatio = 0.5f;
        public Vector3 habitatPos = Vector3.zero;

        public Gene()
        {
        }

        public Gene(Gene gene)
        {
            adultMass = gene.adultMass;
            idealMuscleRatio = gene.idealMuscleRatio;
            habitatPos = gene.habitatPos;
        }

        public void Mutate(Gene otherGene, float mutationRate, Vector3 P){
            adultMass = (adultMass + otherGene.adultMass)/2.0f;
            adultMass = (Random.Range(0.0f,1.0f)<mutationRate) ? Mathf.Clamp(adultMass*Mathf.Exp(Random.Range(-0.26f,0.26f)), 0.15f, 4.0f) : adultMass;
            idealMuscleRatio = (idealMuscleRatio + otherGene.idealMuscleRatio)/2.0f;
            idealMuscleRatio = (Random.Range(0.0f,1.0f)<mutationRate) ? Mathf.Clamp(idealMuscleRatio + Random.Range(-0.1f,0.1f), 0.1f, 0.9f) : idealMuscleRatio;
            habitatPos = (habitatPos + otherGene.habitatPos + P)/3.0f;
            habitatPos += Random.insideUnitSphere*NormalRandom(0, 1.0f);
        }
        private float NormalRandom(float mean, float std){
            float u1 = Random.Range(0.0000000001f,1);
            float u2 = Random.Range(0.0000000001f,1);
            return mean + std * Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        }
        
        public bool isSameSpecies(Gene otherGene, float limit)
        {
            return Mathf.Abs(Mathf.Log(adultMass/otherGene.adultMass)) < limit*2.6
            && Mathf.Abs(idealMuscleRatio-otherGene.idealMuscleRatio) < limit;
        }

    }

    void Start() {
        viewingRange = setting.maxViewingRange;

        rb = GetComponent<Rigidbody>();
        cl = GetComponent<CapsuleCollider>();
        rd = GetComponentsInChildren<Renderer>();
        animator = GetComponentInChildren<Animator>();

        manager = FindObjectsOfType<Manager>()[0];
        hbtf = transform.Find("Habitat").transform;
        
        GameObjectUpdate();
        StateUpdate();
        rb.velocity = Random.onUnitSphere*idleSpeed*0.25f;
        
    }

    void Update() {
        GameObjectUpdate();
        StateUpdate();
        
        View();
        // Control();
    }

    private void StateUpdate() {
        age += Time.deltaTime;

        /* Get Physical States*/
        heading_ = transform.forward;
        P_ = transform.position;
        V_ = rb.velocity;
        W_ = rb.angularVelocity;
        speed_ = V_.magnitude;
        boundRadius = cl.radius*transform.localScale.z;

        /* Energy & Speed */
        float energyLoss = setting.basalMetabolismCoeff*Mathf.Pow(muscle, 1.0f);
        basalMetabolismEnergy += energyLoss;
        fat -= energyLoss;
        energyLoss = setting.dragCoeff*speed_*speed_*boundRadius*boundRadius;
        swimmingEnergy += energyLoss;
        fat -= energyLoss;
        
        if(muscle < mass*gene.idealMuscleRatio && muscle < gene.adultMass*gene.idealMuscleRatio*1.3f){
            energyLoss = fat*setting.fatToMuscleTimeCoeff*Time.deltaTime;
            growthEnergy += energyLoss;
            muscle += energyLoss;
            fat -= energyLoss;

        }
        
        mass = fat + muscle;

        maxSpeed = setting.maxSpeedCoeff*Mathf.Sqrt(muscle/boundRadius/boundRadius);
        idleSpeed = setting.idleSpeedCoeff*gene.adultMass*gene.idealMuscleRatio/Mathf.Pow(gene.adultMass,2.0f/3.0f);;

        /* Dying Condition */
        if(fat<0) {
            Debug.Log("Starve to death.");
            Die();
        }

        maxAge = setting.maxAge*Mathf.Sqrt(gene.adultMass);
        if(age>maxAge) {
            Debug.Log("Max age.");
            Die();
        }

        /* Gene */
        adultMass = gene.adultMass;
        idealMuscleRatio = gene.idealMuscleRatio;
    }

    private void GameObjectUpdate() {
        scale = Mathf.Pow(mass/1000.0f/setting.fishPrefabDefaultVolume,1.0f/3.0f);
        transform.localScale = new Vector3(1.0f,1.0f,1.0f)*scale;
        if (age < 1.0f){
            transform.localScale *= age;
        }
        rb.mass = mass;
        rd[0].materials[0].SetColor("_BaseColor",Color.HSVToRGB((Mathf.Log(gene.adultMass)/3.6f+100.0f)%1.0f, 1.0f, 1.0f));
        Color FinColor = Color.HSVToRGB((gene.idealMuscleRatio*3.0f+0.5f)%1.0f, 1.0f, 1.0f);
        rd[0].materials[1].SetColor("_BaseColor",FinColor);

        if(manager.showHabitat) {
            rd[1].material.SetColor("_Color", new Color (rd[0].material.color.r, rd[0].material.color.g, rd[0].material.color.b, 0.004f));
            hbtf.localScale = new Vector3(1.0f,1.0f,1.0f)*setting.habitatRadius*2.0f/scale;
            hbtf.position = gene.habitatPos;
        }
        else {
            rd[1].material.SetColor("_Color", new Color (0.0f, 0.0f, 0.0f, 0.0f));
        }

        animator.speed = speed_/scale*0.35f;
    }

    // private void Control() {
        
    // }

    private void View() {

        if(isHungry()) {
            Collider[] planktonHitColliders = Physics.OverlapSphere(P_, 0.3f, setting.PlanktonMask);
            foreach (var planktonHitCollider in planktonHitColliders)
            {
                Plankton otherPlankton = planktonHitCollider.gameObject.GetComponent<Plankton>();
                if(otherPlankton.size > mass){
                    float energyGet = setting.predationEfficiency*otherPlankton.mass*0.5f/Mathf.Sqrt(mass);
                    planktonEnergy += energyGet;
                    fat += energyGet;
                    otherPlankton.Die();
                }
            }
        }

        numSame = 0;
        numMate = 0;
        numPrey = 0;
        numPredator = 0;
        Fish mate = null;
        Fish prey = null;
        float minMateDist = setting.maxViewingRange;
        float minPreyDist = setting.maxViewingRange;
        avgFlockHeading = Vector3.zero;
        offsetToFlockCetre = Vector3.zero;
        seperationHeading = Vector3.zero;
        offsetToMate = Vector3.zero;
        offsetToPrey = Vector3.zero;
        offsetToPredator = Vector3.zero;


        Collider[] hitColliders = Physics.OverlapSphere(P_, viewingRange, setting.FishMask);
        foreach (var hitCollider in hitColliders){
            Fish otherFish = hitCollider.gameObject.GetComponent<Fish>();

            if(this == otherFish){
                continue;
            }

            Vector3 offset = otherFish.P_-P_;
            float dist = offset.magnitude;
            
            if(isSameSpecies(otherFish)){
                numSame++;
                avgFlockHeading += otherFish.V_;
                offsetToFlockCetre += offset;
                seperationHeading -= offset/dist/dist;

                if(isReproductive() && otherFish.isAdult()){
                    numMate++;
                    if(dist < minMateDist){
                        minMateDist = dist;
                        offsetToMate = offset;
                        mate = otherFish;
                    }
                    if(dist < (boundRadius+otherFish.boundRadius)*2.0f){
                        Debug.Log("Reproduce.");
                        Reproduce(otherFish.gene);
                    }
                }
            } else {
                if(isHungry() && isPrey(otherFish) && dist < viewingRange/2.0f){
                    numPrey++;
                    if(dist < minPreyDist){
                        minPreyDist = dist;
                        offsetToPrey = offset;
                        prey = otherFish;
                    }
                    if(dist < (boundRadius + otherFish.boundRadius)*2.0f+0.04f){
                        Debug.Log("Eat.");
                        float energyGet = setting.predationEfficiency*otherFish.mass;
                        predationEnergy += energyGet;
                        fat += energyGet;
                        otherFish.Die();
                        numPrey = 0;
                        minPreyDist = setting.maxViewingRange;
                    }
                }
                if(otherFish.isPrey(this) && dist < viewingRange/2.0f){
                    numPredator++;
                    offsetToPredator -= offset/dist/dist;
                }
            }
            
        }

        if(numSame != 0){
            avgFlockHeading /= numSame;
            offsetToFlockCetre /= numSame;
            seperationHeading /= numSame;
        }

        if(numSame < 10 || (isReproductive() && numMate<1)){
            viewingRange *=1.1f;
        }
        else{
            viewingRange /=1.1f;
        }
        viewingRange = Mathf.Clamp(viewingRange, boundRadius*5.0f, setting.maxViewingRange);
        speed_sp = idleSpeed;
        Vector3 acceleration = Vector3.zero;
        Vector3 offsetToHabitatCenter = gene.habitatPos - P_;

        if(offsetToHabitatCenter.magnitude > setting.habitatRadius*Mathf.Sqrt(mass)) {
            acceleration += setting.habitatWeight*(offsetToHabitatCenter.normalized*idleSpeed - V_);
        }
        
        if(numSame != 0){
            acceleration += setting.alignWeight*(avgFlockHeading.normalized*idleSpeed -V_);
            acceleration += setting.cohesionWeight*(offsetToFlockCetre*setting.cohesionCoeff + avgFlockHeading*idleSpeed - V_);
            acceleration += setting.seperationWeight*(seperationHeading*setting.seperationCoeff + avgFlockHeading*idleSpeed - V_);
        } else {
            if(isReproductive()) {
                Debug.Log("Self Reproduce.");
                Reproduce(gene);
            }
        }
        
        if(IsHeadingForCollision()){
            acceleration += setting.obstacleAvoidWeight*(ObstacleRays()*idleSpeed - V_);
        }
        if(mate != null) {
            acceleration += setting.mateFollowWeight*(offsetToMate.normalized*idleSpeed - V_);
            Debug.DrawLine(P_, mate.P_, Color.blue);
        }
        if(prey != null) {
            acceleration += setting.preyChaseWeight*(prey.V_ + offsetToPrey.normalized*maxSpeed - V_);
            Debug.DrawLine(P_, prey.P_, Color.red);
        }
        
        if(numPredator != 0) {
            acceleration += setting.predatorAvoidWeight*(offsetToPredator*maxSpeed - V_);
        }
        
        rb.velocity += acceleration*Time.deltaTime;
        rb.velocity = Vector3.ClampMagnitude (rb.velocity, maxSpeed);
        if (V_.magnitude >= 0.0001f){
            transform.forward = V_.normalized;
        }
    }

    private bool IsHeadingForCollision() {
        RaycastHit hit;
        if (Physics.SphereCast (P_, 3.0f*boundRadius, V_, out hit, V_.magnitude * setting.obstacleAvoidWeight + boundRadius*3.0f, setting.obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    private Vector3 ObstacleRays() {
        Vector3[] rayDirections = FishHelper.directions;
        float rayLength = V_.magnitude * setting.obstacleAvoidWeight + boundRadius*6;
        float castRadiusCoeff = 3.0f;
        RaycastHit hit;
        float obstacleDistance = Mathf.Infinity;
        Vector3 obstacleDir = V_;

        for (int j = 0; j < rayDirections.Length; j++) {
            Vector3 dir = Quaternion.FromToRotation(Vector3.forward, V_)*rayDirections[j];
            Ray ray = new Ray (P_+dir*2*boundRadius, dir);
            if (Physics.SphereCast(ray, castRadiusCoeff*boundRadius, out hit, rayLength, setting.obstacleMask)) {
                if (hit.distance < obstacleDistance){
                    obstacleDistance = hit.distance;
                    obstacleDir = dir;
                }
            }
            else {
                return dir.normalized;
            }
        }

        for (int i = 0; i < 3; i++){
            castRadiusCoeff *= 0.7f;
            rayLength *= 0.7f;
            for (int j = 0; j < rayDirections.Length; j++) {
                Vector3 dir = Quaternion.FromToRotation(Vector3.forward, V_)*rayDirections[j];
                Ray ray = new Ray (P_+dir*2*boundRadius, dir);
                if (Physics.SphereCast(ray, castRadiusCoeff*boundRadius, out hit, rayLength, setting.obstacleMask)) {
                    if (hit.distance < obstacleDistance){
                        obstacleDistance = hit.distance;
                        obstacleDir = dir;
                    }
                }
                else {
                    return (dir - obstacleDir*0.2f).normalized;
                }
            }
        }

        
        return -obstacleDir.normalized;
    }

    void Reproduce(Gene otherGene){
        Gene tempGene = new Gene(gene);
        tempGene.Mutate(otherGene, setting.mutationRate, P_);

        Vector3 pos = P_ + Random.onUnitSphere*boundRadius*Random.Range(3.5f,4.5f);
        Fish offspring = Instantiate(setting.FishPrefab);

        /* set basic states */
        offspring.transform.position = pos;
        offspring.transform.forward = heading_;
        offspring.fat = setting.childAdultRatio*gene.adultMass*(1.0f - gene.idealMuscleRatio);
        offspring.muscle = setting.childAdultRatio*gene.adultMass*gene.idealMuscleRatio;
        offspring.gene = tempGene;

        /* remove fat */
        float energyLoss = (offspring.fat+offspring.muscle)/(setting.birthEfficiency/Mathf.Pow(gene.adultMass, 1.0f/3.0f));
        reproductionEnergy += energyLoss;
        fat -= energyLoss;
        }

    public void Die() {
        Destroy(gameObject);
    }

    Vector3 SteerTowards (Vector3 vector, float speedSetPoint) {
        Vector3 v = vector.normalized * speedSetPoint - V_;
        return Vector3.ClampMagnitude (v, setting.maxSteerForce);
    }
    Vector3 SteerTowards2 (Vector3 vector1, Vector3 vector2, float speedSetPoint) {
        Vector3 v = vector1 + vector2.normalized * speedSetPoint - V_;
        return Vector3.ClampMagnitude (v, setting.maxSteerForce);
    }
    
    public bool isSameSpecies(Fish otherFish) {
        return gene.isSameSpecies(otherFish.gene, setting.geneDiffLimit);
    }

    bool isReproductive() {
        return isAdult()
        && fat > gene.adultMass*(1.0f - gene.idealMuscleRatio);
    }
    
    bool isHungry() {
        if(fat > muscle/gene.idealMuscleRatio*(1.0f - gene.idealMuscleRatio)*1.5f){
            return false;
        }
        return true;
    }
    
    bool isAdult() {
        return muscle > gene.adultMass*gene.idealMuscleRatio;
    }

    public bool isPrey(Fish otherFish){
        return mass*setting.predationMassRatio > otherFish.mass && mass*setting.minPredationMassRatio < otherFish.mass;
    }

    private void OnDrawGizmos () {
        if (showHabitat == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showHabitat == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {
        Gizmos.color = new Color (rd[0].material.color.r, rd[0].material.color.g, rd[0].material.color.b, 0.01f);
        
        Gizmos.DrawSphere(gene.habitatPos, setting.habitatRadius);
    }
}
