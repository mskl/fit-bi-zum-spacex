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

    /*  Hlavní schéma:
     *  Inputs >>> process >>> outputs */

    // Jagged array neureonLayers[početLayerů][početNeuronůVLayeru]
    public Neuron[][] neuronLayers; 

    public int numOfInputs;
    public int numOfHiddenLayers;
    public int numOfNeuronsInHiddenLayers;
    public int numOfOutputs;
    public int seed;

    public float fitness;


    /* Struktura neuronLayers - Jagged Array = array of arrays
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
     * neuronLayers[neuronLayers.Lenght] = output */

    /* FeedForward proces 
     * 
     * 1.
     * - načtu inputy do nultého layeru
     * 
     * 2. 
     * - procházím všechny layery a beru sumu z předchozího layeru a skrz aktivační funkci
     *  - kromě layeru 0 - tam vstupují už scaled data = input
     *  - kromě layeru */


    public Brain(int _numOfInputs, int _numOfHiddenLayers, int _numOfNeuronsInHiddenLayers, int _numOfOutputs, int _seed) {
        numOfInputs                 = _numOfInputs;
        numOfHiddenLayers           = _numOfHiddenLayers;
        numOfNeuronsInHiddenLayers  = _numOfNeuronsInHiddenLayers;
        numOfOutputs                = _numOfOutputs;
        seed                        = _seed;

        System.Random rnd = new System.Random(_seed);               // Inicializuj random s daným seedem

        // Počet layerů = numOfHiddenLayers + input + output
        neuronLayers = new Neuron[numOfHiddenLayers + 1 + 1][];     

        // Projeď všechny layery
        for (int i = 0; i < neuronLayers.Length; i++) {
            // Pokud se jedná o nultý layer = input 
            if (i == 0) {
                // V nultém layeru jsou inputy = počet vstupů do sítě. Vytvoř array těchto vstupů neuronLayers[0][a b c]
                neuronLayers[i] = new Neuron[numOfInputs];        
                // Proveď pro každý neuron z tohoto layeru
                for (int a = 0; a < neuronLayers[i].Length; a++) {
                    // Každý input má jen jeden vstup (1). Na weights u nich nezáleží, nebudou se používat k výpočtu.
                    neuronLayers[i][a] = new Neuron(1, rnd.Next(int.MinValue, int.MaxValue));   
                }
            }
            // Pokud se jedná o poslední = ouput layer
            else if (i == neuronLayers.Length - 1) {
                // vytvoř tolik neuronů, kolik je outputů [NUMOFOUTPUTS]
                neuronLayers[i] = new Neuron[numOfOutputs];     
                // Pro každý neuron z tohoto layeru nastav počet vstupů z layeru předchozího a vlož seed
                for (int a = 0; a < neuronLayers[i].Length; a++) {
                    // Vstupem je počet neuronů z předchozího layeru
                    neuronLayers[i][a] = new Neuron(neuronLayers[i - 1].Length, rnd.Next(int.MinValue, int.MaxValue));    
                }
            }
            // Pokud se jedná o jeden z hiddenLayerů (ani první, ani poslední)
            else {
                // Vytvoř tolik neuronů, kolik je v jednotlivých hidden layerech
                neuronLayers[i] = new Neuron[numOfNeuronsInHiddenLayers];  
                // Pro každý neuron z tohoto layeru nastav počet vstupů z layeru předchozího a vlož seed
                for (int a = 0; a < neuronLayers[i].Length; a++) {
                    // Vstupem bude počet neuronů z předchozího layeru 
                    neuronLayers[i][a] = new Neuron(neuronLayers[i - 1].Length, rnd.Next(int.MinValue, int.MaxValue));    
                }
            }

        }
    }

    // Tahle funkce se volá z handlingu! Výstupem je float mezi -1 a +1
    public float[] process(float[] inputs) {
        // Zkontroluj, jestli se vkládá správný počet vstupů
        if (inputs.Length != numOfInputs) {
            throw new System.ArgumentOutOfRangeException("Byl vložen jiný počet vstupů, než jaký byl specifikován!");
        }

        // V nultém layeru vlož vstupy
        for (int i = 0; i < inputs.Length; i++) {
            neuronLayers[0][i].inputs[0] = inputs[i];
        }

        // Pro každý další layer (než 0tý) udělej feed forward
        for (int i = 1; i < neuronLayers.Length; i++) {
            // vyber každý neuron v layeru
            foreach (Neuron n in neuronLayers[i]) {
                // temp array kam si ukládám hodnoty z předchozího layeru
                float[] valuesFromNeuronFromPrevLayer = new float[neuronLayers[i - 1].Length]; 

                // Udělej pro každý neuron z předchozího layeru
                for (int x = 0; x < neuronLayers[i - 1].Length; x++) {
                    // ulož si (processed) výdledek do temp array
                    valuesFromNeuronFromPrevLayer[x] = neuronLayers[i - 1][x].output();         
                }

                // Udělej z nich vstupy pro další layer
                n.inputs = valuesFromNeuronFromPrevLayer;   
            }
        }

        // Setup outputs
        float[] outputs = new float[numOfOutputs];
        for (int i = 0; i < outputs.Length; i++) {
            outputs[i] = Mathf.Clamp(neuronLayers[neuronLayers.Length - 1][i].output(), -1F, 1F);
        }

        // Vrať hodnoty, které jsou uzpůsobené na zatáčení - NEED TO REVISE! Funguje jen pro numOfOutputs = 2!!!
        return outputs;
    }
}