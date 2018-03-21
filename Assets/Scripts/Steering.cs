using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Steering : MonoBehaviour {

    Rigidbody2D rb;

    // COM = Center of mass
    public Transform COM;

    // The thursted GO
    public GameObject Thurster;
    ParticleSystem ThursterFlame;

    public float vectorThrust = 15;

	public Vector3 steerVector;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        rb.centerOfMass = COM.localPosition;
        ThursterFlame = Thurster.GetComponent<ParticleSystem>();

        Randomise();
	}

    float vertical;
	void Update () {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical_in = Input.GetAxis("Vertical");

        vertical = Mathf.Clamp01(vertical + vertical_in/15);

		Vector3 direction = (COM.transform.position - Thurster.transform.position).normalized;
        Vector3 directionNormal = new Vector3(direction.y, -direction.x, direction.z).normalized;

        steerVector = direction * vectorThrust * vertical + directionNormal * -horizontal;

        if (Input.GetKey(KeyCode.Space) || vertical > 0)
        {
            if (ThursterFlame.isPlaying == false)
                ThursterFlame.Play();
			rb.AddForceAtPosition(steerVector, Thurster.transform.position);
        }
        else
        {
            if (ThursterFlame.isPlaying == true)
                ThursterFlame.Stop();
        }

		if (Input.GetKey (KeyCode.R)) {
			Restart ();
		}
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(Thurster.transform.position, -steerVector);
    }

    void Randomise()
    {
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15, 15));
        rb.velocity = new Vector3(Random.Range(-3, 3), Random.Range(0, -3), 0);
        transform.position = new Vector3(Random.Range(-12, 12), 12, 0);
    }

	void Restart(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
		
	void OnCollisionEnter2D (Collision2D col) {
		if (col.relativeVelocity.magnitude > 4) {
			Restart ();
		}
		Debug.Log(col.relativeVelocity.magnitude);
	}

    bool IsLanded()
    {
        if(rb.velocity.sqrMagnitude < 0.001)
        {
            return true;
        }
        return false;
        // chce to dodělat aby přistávaly na platformu
    }
}
