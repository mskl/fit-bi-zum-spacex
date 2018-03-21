using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

[Serializable()]
public class Brain {

    /*
     * 
     *  Hlavní schéma:
     *  Inputs >>> process >>> outputs  
     *  
     */


    public Neuron[][] neuronLayers; // jagged array neureonLayers[početLayerů][početNeuronůVLayeru]

    public int numOfInputs;
    public int numOfHiddenLayers;
    public int numOfNeuronsInHiddenLayers;
    public int numOfOutputs;
    public int seed;

    public float fitness;


    /*
     * Struktura neuronLayers - Jagged Array = array of arrays
     * 
     * neuronLayers[layer][Jednotlivé neurony]
     * 
     * {0}
     * neuronLayers[0] = input;
     * 
     * <1, neuronLayers.Lenght - 2>
     * neuronLayers[1... NeuronLayers.Lenght - 1] = hidden layer
     * 
     * {neuronLayers.Lenght - 1}
     * neuronLayers[neuronLayers.Lenght] = output
     * 
     */

    /*
     *  FeedForward proces 
     * 
     * 1.
     * - načtu inputy do nultého layeru
     * 
     * 2. 
     * - procházím všechny layery a beru sumu z předchozího layeru a skrz aktivační funkci
     *  - kromě layeru 0 - tam vstupují už scaled data = input
     *  - kromě layeru 
     */


    public Brain(int _numOfInputs, int _numOfHiddenLayers, int _numOfNeuronsInHiddenLayers, int _numOfOutputs, int _seed)
    {
        numOfInputs = _numOfInputs;
        numOfHiddenLayers = _numOfHiddenLayers;
        numOfNeuronsInHiddenLayers = _numOfNeuronsInHiddenLayers;
        numOfOutputs = _numOfOutputs;
        seed = _seed;

        System.Random rnd = new System.Random(_seed);               // Inicializuj random s daným seedem

        neuronLayers = new Neuron[numOfHiddenLayers + 1 + 1][];     // Počet layerů = numOfHiddenLayers + input + output

        for (int i = 0; i < neuronLayers.Length; i++)                // Projeď všechny layery
        {

            if (i == 0) {                                           // Pokud se jedná o nultý layer = input 
                neuronLayers[i] = new Neuron[numOfInputs];          // V nultém layeru jsou inputy = počet vstupů do sítě. Vytvoř array těchto vstupů neuronLayers[0][a b c]
                for (int a = 0; a < neuronLayers[i].Length; a++)    // Proveď pro každý neuron z tohoto layeru
                {
                    neuronLayers[i][a] = new Neuron(1, rnd.Next(int.MinValue, int.MaxValue));   // každý input má jen jeden vstup (1). Na weights u nich nezáleží, nebudou se používat k výpočtu.
                }
            }

            else if (i == neuronLayers.Length - 1)                  // Pokud se jedná o poslední = ouput layer
            {
                neuronLayers[i] = new Neuron[numOfOutputs];         // vytvoř tolik neuronů, kolik je outputů [NUMOFOUTPUTS]
                for (int a = 0; a < neuronLayers[i].Length; a++)    // Pro každý neuron z tohoto layeru nastav počet vstupů z layeru předchozího a vlož seed
                {
                    neuronLayers[i][a] = new Neuron(neuronLayers[i - 1].Length, rnd.Next(int.MinValue, int.MaxValue));    // Vstupem je počet neuronů z předchozího layeru
                }
            }

            else                                                            // Pokud se jedná o jeden z hiddenLayerů (ani první, ani poslední)
            {
                neuronLayers[i] = new Neuron[numOfNeuronsInHiddenLayers];   // Vytvoř tolik neuronů, kolik je v jednotlivých hidden layerech
                for (int a = 0; a < neuronLayers[i].Length; a++)            // Pro každý neuron z tohoto layeru nastav počet vstupů z layeru předchozího a vlož seed
                {
                    neuronLayers[i][a] = new Neuron(neuronLayers[i - 1].Length, rnd.Next(int.MinValue, int.MaxValue));    // Vstupem bude počet neuronů z předchozího layeru     
                }
            }

        }
    }


    public float[] process(float[] inputs)    // Tahle funkce se volá z handlingu! Výstupem je float mezi -1 a +1
    {
        if (inputs.Length != numOfInputs)    // Zkontroluj, jestli se vkládá správný počet vstupů
        {
            throw new System.ArgumentOutOfRangeException("Byl vložen jiný počet vstupů, než jaký byl specifikován!");
        }

        for (int i = 0; i < inputs.Length; i++)    // V nultém layeru vlož vstupy
        {
            neuronLayers[0][i].inputs[0] = inputs[i];
        }


        for (int i = 1; i < neuronLayers.Length; i++)    // Pro každý další layer (než 0tý) udělej feed forward
        {
            foreach (Neuron n in neuronLayers[i])        // vyber každý neuron v layeru
            {
                float[] valuesFromNeuronFromPrevLayer = new float[neuronLayers[i - 1].Length];  // temp array kam si ukládám hodnoty z předchozího layeru

                for (int x = 0; x < neuronLayers[i - 1].Length; x++)                            // Udělej pro každý neuron z předchozího layeru
                {
                    valuesFromNeuronFromPrevLayer[x] = neuronLayers[i - 1][x].output();         // ulož si (processed) výdledek do temp array
                }

                n.inputs = valuesFromNeuronFromPrevLayer;   // Udělej z nich vstupy pro další layer
            }
        }

        float[] outputs = new float[numOfOutputs];
        for(int i = 0; i < outputs.Length; i++)
        {
            outputs[i] = Mathf.Clamp(neuronLayers[neuronLayers.Length - 1][i].output(), -1F, 1F);
        }

        // Vrať hodnoty, které jsou uzpůsobené na zatáčení - NEED TO REVISE! Funguje jen pro numOfOutputs = 2!!!
        return outputs;
        //return Mathf.Clamp(neuronLayers[neuronLayers.Length - 1][1].output() - neuronLayers[neuronLayers.Length - 1][0].output(), -1, 1);
    }


}