using UnityEngine;
using System.Collections;

public class DebugVectors : MonoBehaviour {

	public Transform target;
	public Transform COM;
	public Transform Thurster;
	Rigidbody2D rb;

	Steering strScript;

	void Start(){
		strScript = GetComponent<Steering> ();
		rb = GetComponent<Rigidbody2D> ();
	}
		
	void Update () {/*
		Debug.Log (Mathf.Clamp(rb.angularVelocity/120, -1, 1));
		Debug.DrawLine (Thurster.transform.position, target.transform.position);
		Debug.DrawLine (COM.transform.position, COM.transform.position + new Vector3 (transform.rotation.z,0,0) * 4);
		Debug.DrawLine (COM.transform.position, COM.transform.position + new Vector3 (Mathf.Clamp(rb.angularVelocity/120, -1, 1),0,0));
		Debug.DrawLine (COM.transform.position, COM.transform.position + (Vector3)rb.velocity);
		Debug.DrawLine (Thurster.transform.position, Thurster.transform.position - strScript.steerVector);*/
	}
}
