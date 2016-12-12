using UnityEngine;
using System.Collections;

public class BodyWakeUp : MonoBehaviour {
    public bool insertIntoBodyList;
    public NBodies sim;
    public bool physics = true;

	// Use this for initialization
	void Start () 
    {
        sim = GameObject.Find("N-Body Simulator").GetComponent<NBodies>();
        if (sim && insertIntoBodyList)
        {
            float mass = massFromScale(transform.localScale.x * sim.scale);
            Body self = new Body(this.gameObject, mass, Vector3.zero, sim.scale);
            self.physics = physics;
            sim.addBodyExternal(self);
        }
	}
	
    float massFromScale (float r)
    {
        return (4f/3f * 3.14159f) * Mathf.Pow(r, 3f);
    }
	// Update is called once per frame
	void Update () {
	
	}
}
