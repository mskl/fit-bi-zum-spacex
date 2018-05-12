using UnityEngine;
using System.Collections;

public class Neuron {

    public float[] inputs;
    public float[] weights;

    public Neuron(int numOfInputs, int seed) {
        Random.InitState(seed);
        inputs          = new float[numOfInputs];
        weights         = new float[numOfInputs];

        // Initialise fresh Neuron with random values
        for (int x = 0; x < weights.Length; x++) {
            weights[x] = Random.Range(-1F, 1F);
        }
    }

    // Vrátí zpracované informace z neuronu
    public float output() {
        // Pokud se jedná o perceptron (= input), tak ho vrať a dál nic neprocesuj
        // LOGIC WARNING: mohl by nastat problém kdybych měl layer jen s jedním neuronem
		if (isPerceptron()) { 
            return inputs[0];
        }

        float sum = 0;

        // Feed forward
        for (int i = 0; i < inputs.Length; i++) {
            sum += inputs[i] * weights[i];
        }

        return activateFunction(sum);
    }

    // Aktivační funkce
    float activateFunction(float fin) {
        return 2 / (1 + Mathf.Pow((float)System.Math.E, -2 * fin)) - 1;
    }

    // Zkontroluje, jestli se jedná o perceptron
    bool isPerceptron() {
        return inputs.Length == 1;
    }
}
