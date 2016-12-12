using UnityEngine;
using System.Collections;

public class menuItem : MonoBehaviour {

    public Color emissionColor;
    public string goTo;

    private float offset;
    private Character player;
	// Use this for initialization
	void Start () 
    {
	   GetComponent<Renderer>().material.SetColor("_EmissionColor", emissionColor);
       offset = Random.Range(0f,10f);
       player = GameObject.Find("Player").GetComponent<Character>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	   transform.position += new Vector3(0, Mathf.Sin(Time.time + offset) * Time.deltaTime, 0);

       if (Input.GetMouseButtonDown(0))
       {
            float x = player.mouseX;
            float y = player.mouseY;
            Rect r = NBodies.ObjectToRect(this.gameObject);
            if (r.Contains(new Vector2 (x, y)) && 
                r.x > 0 && r.y > 0 && 
                r.x + r.width < Screen.width && 
                r.y + r.height < Screen.height)
            {
                StartCoroutine(onClick());
            }
       }
	}

    IEnumerator onClick()
    {
        yield return new WaitForSeconds(.25f);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Levels/" + goTo); // Load level by name
    }

}
