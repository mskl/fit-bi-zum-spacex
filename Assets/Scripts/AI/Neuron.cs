using UnityEngine;
using System.Collections;

public class Neuron {

    public float[] inputs;
    public float[] weights;

    public float output()   // Vrátí zpracované informace z neuronu
    {
        if (inputs.Length == 1) // Pokud se jedná o perceptron (= input), tak ho vrať a dál nic neprocesuj
        {
            return inputs[0];
        }

        float sum = 0;

        for(int i = 0; i < inputs.Length; i++)  
        {
            sum += inputs[i] * weights[i];
        }

        return activateFunction(sum);
    }

    float activateFunction(float fin)
    {
        return 2 / (1 + Mathf.Pow((float)System.Math.E, -2 * fin)) - 1;
    }

    public Neuron(int numOfInputs, int seed)
    {
        Random.seed = seed;
        inputs = new float[numOfInputs];
        weights = new float[numOfInputs]; 

        for (int x = 0; x < weights.Length; x++)
        {
            weights[x] = Random.Range(-1F, 1F);
        }
    }


}
