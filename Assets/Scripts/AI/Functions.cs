using UnityEngine;
using System.Collections.Generic;

public class Functions : MonoBehaviour
{
    public static float randomGaus(float aroundNum)
    {
        float sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += Random.Range(0F, 1F);
        }
        float result = ((sum - 6) / 6); // Random Gaussian num kolem 0

        result = result + aroundNum; // Posuň k číslu

        result = Mathf.Clamp(result, -1, 1);

        return result;
    }

    // Vezme z GameObjektů mozky a udělá z nich sorted list
    public static Dictionary<float, Brain> EntitiesToBrainDictionary(List<GameObject> listGO)
    {
        var knihovnaMozku = new Dictionary<float, Brain>();

        foreach (GameObject go in listGO)
        {
            var Handling = go.GetComponent<Handling>();
            Brain mozek = Handling.entityBrain;
            float fitness = Handling.fitness;

            while (knihovnaMozku.ContainsKey(fitness)) // Pokud by knihovna už hodnotu obsahovala, přičti +1
                fitness++;

            knihovnaMozku.Add(fitness, mozek);
        }

        return knihovnaMozku;
    }
}