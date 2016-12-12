using UnityEngine;
using System.Collections;

public class Monitor : MonoBehaviour {
	public float mass;
	public Vector3 velocity;
	public Vector3 force;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void update_v(Vector3 v)
	{
		velocity = v;
	}

	public void update_m(float m)
	{
		mass = m;
	}

	public void update_f(Vector3 f)
	{
		force = f;
	}
}
