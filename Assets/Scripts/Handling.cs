using UnityEngine;
using System.Collections;
using System;

/*
 * Tenhle skript se stará o zatáčení a o inicializaci mozku
 * Taktéž o fitness funkci
 */
public class Handling : MonoBehaviour {
    [HideInInspector()]
    public int seed;                // Seed is set from generator

    [HideInInspector()]
    public bool ALIVE = false;      // If the object is alive, compute fitness
    private bool SIMULATED = false;  // If it is simulated, the steering is disabled

    // Starting fitness and fuel
    public float fitness = 0; 
    public float fuel = 1;

    [Header("Neural network")]
    public Brain entityBrain;               

    [Header("Components of the lander")]
    public GameObject COM;              // Center Of Mass
    public GameObject MainThurster;     // Main Thruster
    public Transform SideThurster;      // Top side thurster

    [Header("Positions of the legs")]
    public Vector2 right_leg_ray    = new Vector2(0.584f, -0.133f);
    public Vector2 left_leg_ray     = new Vector2(-0.584f, -0.133f);

    [Header("Settings")]
    public const float mainThrust = 20;     // The force of the main thurs
    public const float sideThrust = 1;
    public const float vectorSwivel = 2F;   // Swivel multiplier for the engine gimbal
    public const float maxHitForce = 8;     // The force that destroys the lander

    [Header("Target platform")]
    public Transform target;

    // Thurst vectors
    private Vector3 steerVector = Vector3.zero;
    private float sideThursterVector = 0;

    // Rigidbody of the rocket is obtained at start
    private Rigidbody2D rb;

    // Initialise the lander
    private void Start() {
        rb = GetComponent<Rigidbody2D>();

        // TODO: Should be removed
        if (target == null) {
            target = GameObject.FindWithTag("LanderTarget").transform;
        }
    }

    // Should be called before spawning
    public void SetTarget (Transform _target) {
        target = _target;
    }

    // Assign the brain and set the GameObject to be alive
    public void StartLearning(int _seed, Brain _brain) {
        ALIVE = true;
        SIMULATED = true;
        seed = _seed;
        entityBrain = _brain;
    }

    // Stop the calculation of the fitness
    public void StopLearning() {
        ALIVE = false;
        // Decrease the entity count
        rb.bodyType = RigidbodyType2D.Static;
    }

    private void Crash(float crash_magnitude) {
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 1f, 0.2f);
        fitness = fitness / crash_magnitude;
        StopLearning();
    }

    private void Land() {
        GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0.2f);
        fitness += 1000;
        StopLearning();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "TopKillCollider") {
			// On collision with boundary collider deactivate
            Crash(collision.relativeVelocity.magnitude / 1);
        } else if (collision.gameObject.tag == "SideKillCollider") {
            // On collision with boundary collider deactivate
            Crash(collision.relativeVelocity.magnitude / 5);
        } else if (collision.gameObject.tag == "Sea") {
            // On collision with the sea
            Crash(collision.relativeVelocity.magnitude / 7);
        } else if (collision.relativeVelocity.magnitude > maxHitForce) {
            // Collision with anything
            Crash(collision.relativeVelocity.magnitude / 10);
        }
    }

    void FixedUpdate () {
        if (!SIMULATED) {
            PlayerControl();
        } else if (ALIVE == true){
            BrainControl();
        }
    }

    private void BrainControl() {
        // The inputs for the brain
        Vector2 distance = _brain_input_distance();
        Vector2 velocity = _brain_input_velocity();

        float[] processedInputsForBrain = new float[entityBrain.numOfInputs];
        processedInputsForBrain[0] = _brain_input_zrotation_velocity();
        processedInputsForBrain[1] = distance.x;
        processedInputsForBrain[2] = distance.y;
        processedInputsForBrain[3] = _brain_input_zrotation();
        processedInputsForBrain[4] = velocity.x;
        processedInputsForBrain[5] = velocity.y;
        processedInputsForBrain[6] = _brain_input_fuel();        // Fuel

        // Výstup z mozku
        float[] outputs = entityBrain.process(processedInputsForBrain);

        float horizontal = outputs[0];
        float vertical = outputs[1] + 1f;
        sideThursterVector = outputs[2];
  
        // Move the actual rocket
        Steer(vertical, horizontal, sideThursterVector);
		
		// Calculate the fitness
		AddFitness();

        if (CheckIfLanded()) {
            Land();
        }
    }

    private void PlayerControl() {
        // Get the arrow key inputs
		float vertical = Input.GetAxis("Vertical");
		float horizontal = Input.GetAxis("Horizontal");
		
        // Get the side thurster inputs
		if (Input.GetKey(KeyCode.Y)) {
			sideThursterVector += 0.1f;
		} else if (Input.GetKey((KeyCode.X))) {
			sideThursterVector -= 0.1f;
		} else {
			sideThursterVector /= 2;
		}
		sideThursterVector = Mathf.Clamp(sideThursterVector, -1, 1);
                  
        // TODO: remove unlimited fuel
		// fuel = 3; 

        // Do the actual locomotion
		Steer(vertical, horizontal, sideThursterVector);
    }

    // Move the actual ship
    private void Steer (float _vertical, float _horizontal, float _side_vector) {
        // If it has any fuel..
        if (fuel > 0) {
            // Direction down
            Vector3 direction = (COM.transform.position - MainThurster.transform.position).normalized;
            Vector3 directionNormal = new Vector3(direction.y, -direction.x, direction.z).normalized;

            // Calculate the main thurst
            steerVector = (direction * mainThrust * _vertical) + (directionNormal * -_horizontal * vectorSwivel);

			// Subtract fuel TODO do it better
            fuel -= (steerVector.magnitude / 5000);
            fuel -= (_side_vector / 350);

            // Add the force
            rb.AddForceAtPosition(directionNormal * _side_vector * sideThrust, SideThurster.transform.position);
            rb.AddForceAtPosition(steerVector, MainThurster.transform.position);
        }
    }

    /******************************************************************************************************************************/

    private Vector2 _brain_input_distance() {
        // Measured from the thurster
        Vector2 ret = new Vector2(target.transform.position.x - MainThurster.transform.position.x,
                                  target.transform.position.y - MainThurster.transform.position.y);
        ret.x = Mathf.Clamp(ret.x / 13, -1, 1);
        ret.y = Mathf.Clamp(ret.y / 13, -1, 1);
        return ret;
    }

    private Vector2 _brain_input_velocity() {
        Vector2 ret = Vector2.zero;
        ret.x = Mathf.Clamp(rb.velocity.x / 6, -1, 1);
        ret.y = Mathf.Clamp(rb.velocity.x / 6, -1, 1);
        return ret;
    }

    private float _brain_input_zrotation() {
        return Mathf.Clamp(transform.rotation.z * 1.8f, -1, 1);
    }

    private float _brain_input_zrotation_velocity() {
        return Mathf.Clamp(rb.angularVelocity / 100, -1, 1);
    }

    private float _brain_input_fuel() {
        return (fuel * 2) - 1;
    }

    private float _brain_input_landed() {
        return CheckIfLanded() ? -1f : 1f;
    }

    /******************************************************************************************************************************/

    private void AddFitness() {
        fitness += 0.1f;

        // Postih za špatnou rotaci
        if (transform.rotation.z < -90 || transform.rotation.z > 90) {
            fitness = fitness / 10;
        } else {
            // Bonus for being close to the target
            fitness += 3 / (0.1f + Mathf.Pow((MainThurster.transform.position.y - target.transform.position.y), 2f));
            fitness += 5 / (0.1f + Mathf.Pow((MainThurster.transform.position.x - target.transform.position.x), 4f));
        }
    }

    // Check if the rocket is landed
    private bool CheckIfLanded() {
        Vector2 thurster_pos = new Vector2(MainThurster.transform.position.x, MainThurster.transform.position.y);
        RaycastHit2D left_hit = Physics2D.Raycast(thurster_pos + left_leg_ray, -transform.up, 1f);
        RaycastHit2D right_hit = Physics2D.Raycast(thurster_pos + right_leg_ray, -transform.up, 1f);

        if (left_hit.collider != null && right_hit.collider != null) {
            if (left_hit.distance < 0.1f && left_hit.transform.gameObject.tag == "Lander") {
                if (right_hit.distance < 0.1f && right_hit.transform.gameObject.tag == "Lander") {
                    if (rb.velocity.sqrMagnitude > 0.01) {
                        return false;
                    } else {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /******************************************************************************************************************************/

    // DEBUG to draw the Gizmos
    private void OnDrawGizmos() {
        if (Application.isPlaying) {
            // Draw the steer vector
            Debug.DrawLine(MainThurster.transform.position, MainThurster.transform.position - steerVector / 10, Color.red);

            // Draw the side thurster
            Debug.DrawLine(SideThurster.transform.position, SideThurster.transform.position + transform.right * -sideThursterVector, Color.red);

            // Draw the remaining fuel
            Debug.DrawLine(transform.position + new Vector3(0 - 0.5f, 2.1f), transform.position + new Vector3(fuel - 0.5f, 2.1f), Color.green);

            // Draw the z rotation
            Debug.DrawLine(transform.position + new Vector3(0, 2f), transform.position + new Vector3(_brain_input_zrotation(), 2f), Color.magenta);

            // Draw the z rotation speed
            Debug.DrawLine(transform.position + new Vector3(0, 1.9f),
                           transform.position + new Vector3(_brain_input_zrotation_velocity(), 1.9f), Color.blue);

            // Draw the speed
            Vector2 speed = _brain_input_velocity();
            Debug.DrawLine(transform.position, transform.position + new Vector3(speed.x, speed.y), Color.yellow);

            // Draw the distance
            Vector2 distance = _brain_input_distance();
            Debug.DrawLine(MainThurster.transform.position, MainThurster.transform.position + new Vector3(distance.x, distance.y), Color.cyan);

            // The rays for checking if landed
            Debug.DrawRay(MainThurster.transform.position + new Vector3(left_leg_ray.x, left_leg_ray.y),
                          -transform.up / 10, Color.red);
            Debug.DrawRay(MainThurster.transform.position + new Vector3(right_leg_ray.x, right_leg_ray.y),
                          -transform.up / 10, Color.red);

            if (CheckIfLanded()) {
                Gizmos.color = new Color(1, 0, 1, 0.4f);
                Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y - 2, transform.position.z), Vector3.one / 3);
            }
        }
    }
}