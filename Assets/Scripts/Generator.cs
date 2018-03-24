using UnityEngine;
using System.Collections.Generic;

public class Generator : MonoBehaviourSingleton<Generator> {

    /*
     *  Generátor je generátor pro GA
     * 
     * Postup:
     *  1) vytvoř jednotlivce s random weights
     *  2) nech je žít a vyhodnoť jejich fitness
     *  3) vyhodnoť jejich fitness
     *  4) spař je podle fitness
     *  5) další generace
     * 
     */

    public GameObject entity;
    public Transform target;

    [Header("Genetic algorithm settings")]
    public int GA_NumOfEntitiesInGeneration;
    public float GA_MutationRateInPercent01 = 0.03F;

    /* Nastavení */
    private int brain_numOfInputs = 7;
    public int brain_numOfHiddenLayers = 6;
    public int brain_numOfNeuronsInHiddenLayer = 10;
    private int brain_numOfOutputs = 3;

    [Header("Seed settings")]
    public int globalSeed;
    private int seedIterator = 0;

    [HideInInspector()]
    public List<GameObject> entityList;

    private int tickCounter = 0;
    private int generation = 0;

    public bool GeneratorEnabled = false;

    // Use this for initialization
    void Start () {
        entityList = new List<GameObject>();    // Inicializuj entityList
        Application.runInBackground = true;     // App will run in background
    }

    void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.F)) {
            Enable();
        }

        if (GeneratorEnabled) {
            tickCounter++;

            if (tickCounter >= 600 || Input.GetKeyDown(KeyCode.N)) {
                //transform.position = new Vector3(Random.Range(-13, 13), transform.position.y, transform.position.z);
                CreateNextGenerationAndKillPrevious();
                tickCounter = 0;
                generation++;
                Debug.Log("Generation: " + generation);
            }
        }
    }

    public void Enable() {
        destroyAllEntities();
        GeneratorEnabled = true;
        globalSeed = System.DateTime.Now.GetHashCode();
        FirstGenerate();
    }

    public void Disable() {
        GeneratorEnabled = false;
        destroyAllEntities();
    }

    // Tady probíhá iterace jednotlivých generací
    public void CreateNextGenerationAndKillPrevious() {
        // Get the average fitness
        double fitess_sum = 0;
        foreach (GameObject ent in entityList) {
            fitess_sum += ent.GetComponent<Handling>().fitness;
        }
        float average_fitness = (float)(fitess_sum / entityList.Count);

        Drawing.Instance.AddValue(average_fitness);
        ScreenConsoleController.Instance.Append("generation: " + generation + " average fitness: " + average_fitness);
        Debug.Log("Average fitness: " + average_fitness);

        var newEntityBrainList = Genetic.ChildrenBrainList(Functions.EntitiesToBrainDictionary(entityList), GA_MutationRateInPercent01, globalSeed + seedIterator);
        destroyAllEntities();
        GenerateFromBrains(newEntityBrainList);
    }

    public void destroyAllEntities() {
        foreach (GameObject go in entityList) {
            go.GetComponent<Handling>().StopLearning();
            Destroy(go);
        }
        entityList.Clear();
    }

    public void FirstGenerate() {
        for (int x = 0; x < GA_NumOfEntitiesInGeneration; x++) {
            GameObject ga = Instantiate(entity, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;   // vygeneruj entity na start

            // nastav parent transform (kvůli přehlednosti)
            ga.transform.SetParent(transform);                                                                  

            Brain newBrain = new Brain(brain_numOfInputs, brain_numOfHiddenLayers, brain_numOfNeuronsInHiddenLayer, brain_numOfOutputs, globalSeed + seedIterator); // Inicializuj mozek

            // Setup the entity
            ga.GetComponent<Handling>().SetTarget(target);
            ga.GetComponent<Handling>().StartLearning(globalSeed + seedIterator, newBrain);                     

            // Save the entity to the list
            entityList.Add(ga);                                                                                 
            seedIterator++;
        }
    }

    public void GenerateFromBrains(List<Brain> _newBrains) {
        foreach (Brain br in _newBrains) {
            // Vygeneruj entitu na start
            GameObject ga = Instantiate(entity, transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;  

            // Nastav parent transform (kvůli přehlednosti)
            ga.transform.SetParent(transform);

            // Setup the entity
            ga.GetComponent<Handling>().SetTarget(target);
            ga.GetComponent<Handling>().StartLearning(globalSeed + seedIterator, br);

            // Save the new entity to the list
            entityList.Add(ga);
            seedIterator++;
        }
    }
	
}
