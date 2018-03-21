using UnityEngine;
using System.Collections.Generic;

public class Genetic : MonoBehaviour {

    public static Brain CrossoverAndMutation(Brain b1, Brain b2, float mutationChanceInPercent01, int _seed)
    {
        int numOfWeights = 0;

        // Spočítej počet neuronů v mozku, musí mít stejný počet tho!
        foreach (Neuron[] n1 in b1.neuronLayers) 
        {
            foreach(Neuron n2 in n1)
            {
                foreach(float w in n2.weights)
                {
                    numOfWeights++;
                }
            }
        }

        int seed = b1.seed + b2.seed + _seed + System.DateTime.Now.GetHashCode();   // Seed pro generátor

        System.Random rnd = new System.Random(seed);
        Random.seed = seed;

        int crossoverPoint = rnd.Next(0, numOfWeights + 2);                 // O 1 delší, protože míst kde useknout je o 1 víc (+1 je na konci) +1 páš je to exclusive

        Brain newBrain = new Brain(b1.numOfInputs, b1.numOfHiddenLayers, b1.numOfNeuronsInHiddenLayers, b1.numOfOutputs, seed);

        int counter = 0;
        for(int a = 0; a < b1.neuronLayers.Length; a++)            // 1. layer = array layerů
        {
            for(int b = 0; b < b1.neuronLayers[a].Length; b++)     // 2. layer = array neuronů
            {
                for(int c = 0; c < b1.neuronLayers[a][b].weights.Length; c++)  // 3. layer = array weights v jednotlivých neuronech
                {
                    float weightToAssign = 0;
                    if (counter <= crossoverPoint)                          // Až se na něj dostanu, tak přestanu přeřazovat a budu jen mutovat
                    {
                        weightToAssign = b2.neuronLayers[a][b].weights[c];
                    }
                    else                                                    // V podstatě přiřadím stejnou hodnotu
                    {
                        weightToAssign = b1.neuronLayers[a][b].weights[c];
                    }
                    counter++;

                    if(mutationChanceInPercent01 > Random.Range(0F, 1F))    // V případě, že budu mutovat
                    {
                        weightToAssign = Functions.randomGaus(weightToAssign);  // Vygeneruj gausovskou hodnotu kolem
                    }

                    newBrain.neuronLayers[a][b].weights[c] = weightToAssign;
                }
            }
        }

        return newBrain;
    }

    public static List<Brain> ChildrenBrainList(Dictionary<float, Brain> _parentBrainDictionary,float _mutationChanceInPercent01, int _seed)
    {
        // Přiřazení seedu
        int seed = _seed;
        Random.seed = seed;

        List<Brain> childrenBrainListToReturn = new List<Brain>();

        // Nejprve vyhodnotím celkovou fitness
        float totalFitness = 0;
        foreach(KeyValuePair<float, Brain> kvp in _parentBrainDictionary)
        {
            totalFitness += kvp.Key;
        }

        while (_parentBrainDictionary.Count != childrenBrainListToReturn.Count)   // Dělej dokud není stejně dětí jako rodičů
        {
            List<Brain> dvaMozkyNaSpareni = new List<Brain>();

            while (dvaMozkyNaSpareni.Count < 2)
            {
                // Podle roulette selection vyberu mozek na páření
                float randomFloatOnWheel = Random.Range(0F, totalFitness);

                float iterator = 0;
                foreach (KeyValuePair<float, Brain> kvp in _parentBrainDictionary)
                {
                    iterator += kvp.Key;
                    if (iterator >= randomFloatOnWheel)
                    {
                        dvaMozkyNaSpareni.Add(kvp.Value);
                        break;
                    }
                }
            }
            childrenBrainListToReturn.Add(CrossoverAndMutation(dvaMozkyNaSpareni[0], dvaMozkyNaSpareni[1], _mutationChanceInPercent01, seed));
        }
        return childrenBrainListToReturn;
    }
}
