using UnityEngine;
using System.Collections;

public class Link
{

    public Link child;
    public Link parent;
    public GameObject mesh;

    public Vector3 pivot_left;
    public Vector3 pivot_right;

    public Link(GameObject m, Link c, Link p)
    {
        mesh = m;
        child = c;
        parent = p;

        float offset = 2;
        mesh.transform.position += -mesh.transform.forward * offset;
        pivot_left = mesh.transform.position;
        mesh.transform.position += mesh.transform.forward * offset;
        mesh.transform.position += mesh.transform.forward * offset;
        pivot_right = mesh.transform.position;
        mesh.transform.position += -mesh.transform.forward * offset;
    }

    public void move(Vector3 pos)
    {
        Vector3 a = pivot_left;
        Vector3 b = pos;
        Vector3 c = mesh.transform.forward + mesh.transform.position;

        Vector3 axis = Vector3.Cross(b - a, c - a);

        Debug.DrawLine(a, b, Color.blue, 300f);
        Debug.DrawLine(a, c, Color.blue, 300f);
        Debug.DrawLine(b, c, Color.blue, 300f);

        // Debug.DrawLine(a, b, Color.red, 300f);
        // Debug.DrawLine(b, axis, Color.red, 300f);
        // Debug.DrawLine(a, axis, Color.black, 300f);

        Vector3 proj = new Vector3(0,0,0); //Fuck me
        Debug.DrawLine(b,proj,Color.green,300f);
        Debug.DrawLine(a,proj,Color.green,300f);

        float adj = Vector3.Distance(proj,a);
        float hyp = Vector3.Distance(b,a);
        float theta = 0;//(180f * Mathf.Acos(adj/hyp)) / 3.14159f;

        Debug.Log(adj);
        Debug.Log(hyp);
        Debug.Log(theta);

        mesh.transform.RotateAround(pivot_left, axis, theta);
    }
}

public class Rope : MonoBehaviour 
{
    public Link test;
	// Use this for initialization
	void Start () {
        GameObject mesh = (GameObject) Instantiate(Resources.Load("Chain/link_p"), transform.position, Random.rotation);
        test = new Link(mesh, null, null);
        Vector3 target = new Vector3(5,5,5);
        Instantiate(Resources.Load("Bodies/b"), test.pivot_left, Quaternion.identity);
        Instantiate(Resources.Load("Bodies/b"), test.pivot_right, Quaternion.identity);

        Instantiate(Resources.Load("Bodies/b"), target, Quaternion.identity);
        test.move(target);
	}
	
	// Update is called once per frame
	void Update () {
	   
	}
}
