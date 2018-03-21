using UnityEngine;
using System.Collections;
using System;

/*
 * Tenhle skript se stará o zatáčení a o inicializaci mozku
 */

public class Handling : MonoBehaviour {

    [HideInInspector()]
    public int seed;    // Seed se nastaví z generátoru
    [HideInInspector()]
    public bool isAlive = false;
    public float fitness = 0;
    public Brain entityBrain;

    public Transform COM;
    public GameObject Thurster;
    public float vectorThrust = 15;
    public float maxHitForce = 4;
    public Vector3 steerVector;
    Transform target;

    Rigidbody2D rb;
    // ParticleSystem ThursterFlame;

    float fuel = 5;

    // Tato funkce se volá z cizího skriptu, aby nahradila Start(). Slouží k předání seedu před inicializací.
    public void Initialise(int _seed, Brain _brain)
    {
        seed = _seed;
        entityBrain = _brain;
        target = GameObject.Find("Target").transform;
        rb = GetComponent<Rigidbody2D>();

        isAlive = true;
    }

    // Deaktivuje objekt a přiřadí mozku fitness
    public void Deactivate()                                            
    {
		// "zabije objekt"
        if (isAlive) {
            isAlive = false;                                            
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with boundary collider deactivate
        if (collision.gameObject.tag == "KillCollider") {
            Deactivate();
        }

        if (collision.relativeVelocity.magnitude > maxHitForce) {
            Deactivate();
        }
    }

    void FixedUpdate () {
        // Alive začíná defaultně na false! Oživuje se až funkcí Initialise()!
        if (isAlive == true) {
            float angularVelocity = Mathf.Clamp(rb.angularVelocity / 120, -1, 1);
            float distance_x = Mathf.Clamp(2 / (1 + (Thurster.transform.position.x - target.transform.position.x)) - 1, -1, 1);
            float distance_y = Mathf.Clamp(2 / (1 + (Thurster.transform.position.y - target.transform.position.y)) - 1, -1, 1);
            float zrotation = Mathf.Clamp(transform.rotation.z / 30, -1, 1);
            float velocity_x = Mathf.Clamp(rb.velocity.x / 6, -1, 1);
            float velocity_y = Mathf.Clamp(rb.velocity.x / 6, -1, 1);
            float processedFuel = fuel / 2.5f - 1;

            float[] processedInputsForBrain = new float[entityBrain.numOfInputs];

            processedInputsForBrain[0] = angularVelocity;
            processedInputsForBrain[1] = distance_x;
            processedInputsForBrain[2] = distance_y;
            processedInputsForBrain[3] = zrotation;
            processedInputsForBrain[4] = velocity_x;
            processedInputsForBrain[5] = velocity_y;
            processedInputsForBrain[6] = processedFuel;

            // Výstup z mozku
            float[] outputs = entityBrain.process(processedInputsForBrain);

            float horizontal = outputs[0];
            float vertical = (outputs[1]) + 1f; //* 2;

            Vector3 direction = (COM.transform.position - Thurster.transform.position).normalized;
            Vector3 directionNormal = new Vector3(direction.y, -direction.x, direction.z).normalized;

            steerVector = direction * vectorThrust * vertical + directionNormal * -horizontal;

            rb.AddForceAtPosition(steerVector, Thurster.transform.position);

            addFitness();
        }
    }

    float normaliseInput(float inp, float range)
    {
        float ret = Mathf.Clamp01(inp / range);

        ret = ret * 2 - 1;
        return ret;
    }

    void addFitness()
    {
        fitness += 1; // bonus za přeživší čas
        fitness += 10 / (1 + Mathf.Pow((Thurster.transform.position - target.transform.position).sqrMagnitude, 2));
        if (IsLanded()) {
            fitness += 10000;
        }
    }

    bool IsLanded()
    {
		// chce to dodělat aby přistávaly na platformu - v tuhle chvíli kdekoliv
        // ALE hlavně je to jen o nárazu
        if (rb.velocity.sqrMagnitude < 0.001)
        {
            return true;
        }
        return false;
    }
}