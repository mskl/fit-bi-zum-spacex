using UnityEngine;
using System.Collections;
using System;

/*
 * Tenhle skript se stará o zatáčení a o inicializaci mozku
 * Taktéž o fitness funkci
 */
public class Handling : MonoBehaviour {
    [HideInInspector()]
    public int seed;                // Seed should be set from generator
    [HideInInspector()]
    public bool isAlive = false;    // If the object is alive, compute fitness

    public float fitness = 0;
    float fuel = 1;

    public Brain entityBrain;               // Neural net

    public Transform COM;
    public GameObject Thurster;

    public Vector2 right_leg_ray = new Vector2(0.584f, -0.133f);
    public Vector2 left_leg_ray = new Vector2(-0.584f, -0.133f);

    public const float vectorThrust = 20;   // The force of the main thurster
    public const float vectorSwivel = 2;    // Swivel multiplier for the engine gimbal
    public const float maxHitForce = 4;     // The force that destroys the lander

    public Transform target;                
    Vector3 steerVector = Vector3.zero;     
    Rigidbody2D rb;


    private void Start() {
        rb = GetComponent<Rigidbody2D>();

        if (target == null) {
            target = GameObject.FindWithTag("LanderTarget").transform;
        }
    }

    // Tato funkce se volá z cizího skriptu, aby nahradila Start(). Slouží k předání seedu před inicializací.
    public void Activate(int _seed, Brain _brain) {
		isAlive = true;
        seed = _seed;
        entityBrain = _brain;
    }

    // Deaktivuje objekt a přiřadí mozku fitness
    public void Deactivate() {
        if (isAlive) {
            isAlive = false;       
            GetComponent<SpriteRenderer>().color = new Color(1f, 0, 1f, 0.3f);
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with boundary collider deactivate
        if (collision.gameObject.tag == "KillCollider") {
            fitness = fitness / 4;
            Deactivate();
        }

        // On collision with boundary collider deactivate
        if (collision.gameObject.tag == "Sea")
        {
            fitness = fitness / 2;
            Deactivate();
        }

        if (collision.relativeVelocity.magnitude > maxHitForce) {
            Deactivate();
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Draw the steer vector
            Debug.DrawLine(Thurster.transform.position, Thurster.transform.position - steerVector / 100, Color.red);

            // Draw the remaining fuel
            Debug.DrawLine(transform.position + new Vector3(0 - 0.5f, 2.1f), transform.position + new Vector3(fuel - 0.5f, 2.1f), Color.green);

            // Draw the z rotation
            Debug.DrawLine(transform.position + new Vector3(0, 2f), transform.position + new Vector3(brain_input_zrotation(), 2f), Color.magenta);

            // Draw the z rotation speed
            Debug.DrawLine(transform.position + new Vector3(0, 1.9f), 
                           transform.position + new Vector3(brain_input_zrotation_velocity(), 1.9f), Color.blue);

            // Draw the speed
            Vector2 speed = brain_input_velocity();
            Debug.DrawLine(transform.position, transform.position + new Vector3(speed.x, speed.y), Color.yellow);

            // Draw the distance
            Vector2 distance = brain_input_distance();
            Debug.DrawLine(Thurster.transform.position, Thurster.transform.position + new Vector3(distance.x, distance.y), Color.cyan);
			
            // The rays for checking if landed
			Debug.DrawRay(Thurster.transform.position + new Vector3(left_leg_ray.x, left_leg_ray.y),
			              -transform.up, Color.red);
			Debug.DrawRay(Thurster.transform.position + new Vector3(right_leg_ray.x, right_leg_ray.y),
			              -transform.up, Color.red);

            if(IsLanded()) {
                Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y - 2, transform.position.z), Vector3.one / 8);
            }
        }

        
    }

    void FixedUpdate2() {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        //Debug.Log(1 / (brain_input_distance().magnitude - 1));
        float fs = 10 / (1 + Mathf.Pow((Thurster.transform.position - target.transform.position).sqrMagnitude, 2));
        Debug.Log(fs);
        Steer(vertical, horizontal);
    }


    void FixedUpdate () {
        // Alive začíná defaultně na false! Oživuje se až funkcí Initialise()!
        if (isAlive == true) {
            // The inputs for the brain
            Vector2 distance = brain_input_distance();
            Vector2 velocity = brain_input_velocity();

            float[] processedInputsForBrain = new float[entityBrain.numOfInputs];
            processedInputsForBrain[0] = brain_input_zrotation_velocity();
            processedInputsForBrain[1] = distance.x;
            processedInputsForBrain[2] = distance.y;
            processedInputsForBrain[3] = brain_input_zrotation();
            processedInputsForBrain[4] = velocity.x;
            processedInputsForBrain[5] = velocity.y;
            processedInputsForBrain[6] = brain_input_fuel();        // Fuel
            processedInputsForBrain[7] = brain_input_landed();      // If it is landed

            // Výstup z mozku
            float[] outputs = entityBrain.process(processedInputsForBrain);

            float horizontal = outputs[0];
            float vertical = outputs[1] + 1f;

            AddFitness();
            Steer(vertical, horizontal);
        }
    }

    // Move the actual ship
    void Steer (float vertical, float horizontal) {
        if (fuel > 0) {
            // Subtract fuel
            Vector2 input_vector = new Vector2(vertical, horizontal);
            fuel -= (input_vector.magnitude / 200);

            // Direction down
            Vector3 direction = (COM.transform.position - Thurster.transform.position).normalized;

            // Normal to the direction vector
            Vector3 directionNormal = new Vector3(direction.y, -direction.x, direction.z).normalized;

            // Calculate the out vector
            steerVector = (direction * vectorThrust * vertical) + (directionNormal * -horizontal * vectorSwivel);

            // Add the force
            rb.AddForceAtPosition(steerVector, Thurster.transform.position);
        }
    }

    Vector2 brain_input_distance() {
        // Measured from the thurster
        Vector2 ret = new Vector2(target.transform.position.x - Thurster.transform.position.x,
                                  target.transform.position.y - Thurster.transform.position.y);
        ret.x = Mathf.Clamp(ret.x / 7, -1, 1);
        ret.y = Mathf.Clamp(ret.y / 7, -1, 1);
        return ret;
    }

    Vector2 brain_input_velocity() {
        Vector2 ret = Vector2.zero;
        ret.x = Mathf.Clamp(rb.velocity.x / 6, -1, 1);
        ret.y = Mathf.Clamp(rb.velocity.x / 6, -1, 1);
        return ret;
    }

    float brain_input_zrotation() {
        return Mathf.Clamp(transform.rotation.z * 1.8f, -1, 1);
    }

    float brain_input_zrotation_velocity(){
        return Mathf.Clamp(rb.angularVelocity / 100, -1, 1);
    }

    float brain_input_fuel() {
        return (fuel * 2) - 1;
    }

    float brain_input_landed() {
        return IsLanded() ? -1f : 1f;
    }

    // Make input normalised between -1 and 1
    float normaliseInput (float inp, float range) {
        float ret = Mathf.Clamp01(inp / range);

        ret = ret * 2 - 1;
        return ret;
    }

    void AddFitness() {
        fitness += 7 / (1 + Mathf.Pow((Thurster.transform.position - target.transform.position).sqrMagnitude, 2));
        if (IsLanded()) {
            fitness += 10000;
        }
    }

    bool IsLanded() {
        Ray left_ray = new Ray();
        Ray right_ray = new Ray();
        RaycastHit left_hit;
        RaycastHit right_hit;
        int hits = 0;

        if (Physics.Raycast(left_ray, out left_hit, 1f)) {
            if(left_hit.distance > 0.1f){// || left_hit.transform.gameObject.tag != "Lander") {
                return false;
            }
            hits++;
        }

        if (Physics.Raycast(right_ray, out right_hit, 1f)) {
            if (right_hit.distance > 0.1f){// || right_hit.transform.gameObject.tag != "Lander") {
                return false;
            }
            hits++;
        }

        if (hits != 2) {
            return false; 
        }


        if (rb.velocity.sqrMagnitude > 0.01) {
            return false;
        }

        return true;
    }
}