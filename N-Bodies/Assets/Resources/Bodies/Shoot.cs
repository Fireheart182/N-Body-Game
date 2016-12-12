using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour {

    public float shotSpeed = 10f;
    public Body target;

	// Use this for initialization
	void Start () 
    {
	}
	
	// Update is called once per frame
	void Update () 
    {
        
        if (target == null){ Destroy(gameObject); return;}
        if (target.mesh == null){ Destroy(gameObject); return;}
        if (target.mesh.transform.position == transform.position){ Destroy(gameObject); print("Bye bitch");return;}
        transform.position = Vector3.MoveTowards(transform.position, target.mesh.transform.position, shotSpeed * Time.deltaTime);
	}

    public void kill()
    {
        for (int i = 0; i < transform.childCount; i ++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        Destroy(gameObject);
    }
}
