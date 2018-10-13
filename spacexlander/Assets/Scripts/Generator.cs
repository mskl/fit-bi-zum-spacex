using UnityEngine;
using System.Collections.Generic;

public class Generator : MonoBehaviourSingleton<Generator> {

    /*
     *  Generátor je generátor pro GA
     * 
     * Postup:
     *  1) vytvoř jednotlivce s random weights
     *  2) nech je žít a vyhodnoť jejich fitness
     *  3) spáruj je podle fitness
     *  4) další generace
     * 
     */

    public GameObject entity;
    public Transform target;

    [Header("Genetic algorithm settings")]
    public int GA_NumOfEntitiesInGeneration;
    public float GA_MutationRateInPercent01 = 0.03F;

    [Header("Neural network settings")]
    private int brain_numOfInputs = 6;
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

    public bool generator_enabled = false;

    // Use this for initialization
    void Start () {
        entityList = new List<GameObject>();    // Inicializuj entityList
        Application.runInBackground = true;     // App will run in background
    }

    // First generation
    public void FirstGen() {
        generator_enabled = true;
        globalSeed = System.DateTime.Now.GetHashCode();
        CleanScene();
        FirstGenerate();
    }

    // On fixed update (physics)
    void FixedUpdate() {
        if (generator_enabled) {
            tickCounter++;

            if (tickCounter >= 700 || Input.GetKeyDown(KeyCode.N)) {
                NextGeneration();
                tickCounter = 0;
                generation++;
            }
        }
    }

    // Tady probíhá iterace jednotlivých generací
    public void NextGeneration() {
        // Get the average fitness
        PrintGenerationInfo();

        var newEntityBrainList = Genetic.ChildrenBrainList( Functions.EntitiesToBrainDictionary(entityList), 
            GA_MutationRateInPercent01, globalSeed + seedIterator);
        
        CleanScene();
        GenerateFromBrains(newEntityBrainList);
    }

    // Destroy everything
    private void CleanScene() {
        foreach (GameObject go in entityList) {
            go.GetComponent<Handling>().StopLearning();
            Destroy(go);
        }
        entityList.Clear();
    }

    // Generate the first generation with random brains
    private void FirstGenerate() {
        List<Brain> newBrains = new List<Brain>();
        for (int x = 0; x < GA_NumOfEntitiesInGeneration; x++) {                                                         
            Brain newBrain = new Brain(brain_numOfInputs, 
                                       brain_numOfHiddenLayers, 
                                       brain_numOfNeuronsInHiddenLayer, 
                                       brain_numOfOutputs, 
                                       globalSeed + seedIterator);      

            newBrains.Add(newBrain);                                                                            
            seedIterator++;
        }

        GenerateFromBrains(newBrains);
    }

    // Generate entities with brains from the parameter
    private void GenerateFromBrains(List<Brain> _newBrains) {
        foreach (Brain br in _newBrains) {

            // Random pos of every rocket
            Vector3 randPos = GetRandomPosition(transform.position);

            // Vygeneruj entitu na start
            GameObject ga = Instantiate(entity, randPos, Quaternion.Euler(0, 0, 0)) as GameObject;  

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

    // Get the average fitness of the objects currently in the entityList
    private float GetAverageFitness() {
        double fitess_sum = 0;
        foreach (GameObject ent in entityList) {
            fitess_sum += ent.GetComponent<Handling>().fitness;
        }
        return (float)(fitess_sum / entityList.Count);
    }
	
    // Print debug console and draw graph
    private void PrintGenerationInfo() {
        float average_fitness = GetAverageFitness();

        // Plot the onscreen graph
        PlotGraph.Instance.AddValueAverage(average_fitness);

        // Print the information on the screen
        ScreenConsoleController.Instance.Append("generation: " + generation + " average fitness: " + average_fitness);

        // Print the information to the console
        Debug.Log("generation: " + generation + " average fitness: " + average_fitness);
    }

    // Get a random position based on the spawner
    private static Vector3 GetRandomPosition(Vector3 spawnerPosition) {
        return new Vector3(Random.Range(-13, 13), spawnerPosition.y, spawnerPosition.z);
    }
}
