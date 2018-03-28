using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generation : MonoBehaviour
{
    List<GameObject> entities;
    List<Brain> brains;

    public GameObject prefab;

    public Generation(List<Brain> _brains) {
        brains = _brains;
    }

    public void SimulateGeneration(Vector2 _position) {
        // Create all entities
        CreateEntities(_position);

        // Wait until all are finished

        // Get the average fitness

        // Brains will be picked up from genetic function

        // Destroy entities
        DestroyEntities();
    }

    public List<Brain> GetBrains() {
        return brains;
    }

	private void CreateEntities(Vector2 _position) {
		foreach (Brain br in brains) {
            SpawnEntity(_position, br);
		}
    }

    private void SpawnEntity(Vector2 _position, Brain _brain) {
        // Create the gameobject
        GameObject ga = Instantiate(prefab, _position, Quaternion.identity);
		
        // Get the handling script
        Handling ha = ga.GetComponent<Handling>();

        // Assign the brain
        ha.entityBrain = _brain;
    }

    private void DestroyEntities() {
        foreach(GameObject ent in entities) {
            Destroy(ent);
        }
    }

    public float GetAverageFitness() {
        double fitness_sum = 0;

        foreach (GameObject ent in entities) {
            Handling ha = ent.GetComponent<Handling>();
            fitness_sum += ha.fitness;
        }

        return (float)(fitness_sum / brains.Count);
    }
}
