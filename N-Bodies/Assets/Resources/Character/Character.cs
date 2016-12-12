using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Character : MonoBehaviour {

    public float walkSpeed;
    public Body ground;
    public GameObject setGround;
    public int fallSpeed;
    public Texture target;
    public float targetMin = 0;
    public float targetMax = 100;
    private GameObject level;
    private GameObject shot;

    public Camera cam;
    public float mouseX, mouseY;
    public float mouseSensitivity;
    public float camSpeed;
    public float shotSpeed = 10f;

    private ArrayList visibleBodies;
    private bool flying;
    private float startingToFlyCounter;
    private int claimed;
    private GUIStyle guiStyle = new GUIStyle();
    public GameObject Flag; 

	// Use this for initialization
	void Start () 
    {
        if (setGround)
        {
            ground = new Body(setGround, 1, Vector3.zero, 1);
        }
        level = GameObject.Find("N-Body Simulator");
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        mouseX = Screen.width/2;
        mouseY = Screen.height/2;
        mouseSensitivity = mouseSensitivity > 0 ? mouseSensitivity: 20f;
        guiStyle.fontSize = 22; //change the font size
        guiStyle.normal.textColor = Color.white;
	}

    public void instantiateGround()
    {
        claimed = 0;
        if (level)
        {
            ground = level.GetComponent<NBodies>().nearest(transform.position);
            if (ground != null) ground.ground = true; 
        }
    }
	
	// Update is called once per frame
	void Update () 
    {   
        if (level) level = GameObject.Find("N-Body Simulator");
        if (!flying)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position -= getDirFromCam(cam.transform.right) * walkSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += getDirFromCam(cam.transform.right) * walkSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += getDirFromCam(cam.transform.forward) * walkSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= getDirFromCam(cam.transform.forward) * walkSpeed * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseClick();
            Cursor.lockState = CursorLockMode.Locked;
        }
        update_ground(); 
        update_rotation();
        float acc = level ? level.GetComponent<NBodies>().acceleration:0;
        
        transform.position += ground.velocity * Time.deltaTime * acc;
        
        level.GetComponent<NBodies>().getVisible(cam);
	}

    Vector3 getDirFromCam(Vector3 camDir)
    {
        Vector3 res = normal(cam.transform.position, cam.transform.position + camDir, transform.position);
        return res;
    }

    Vector3 normal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b-a;
        Vector3 s2 = c-a;
        Vector3 res =  Vector3.Cross(s1,s2);
        return res.magnitude == 0 ? Vector3.zero:res / res.magnitude;
    }


    void update_ground()
    {
        if (ground == null) instantiateGround();
        float dist = Mathf.Abs(Vector3.Distance(transform.position, ground.position));
        if (dist > ground.rad())
        {
            float step = fallSpeed * Time.deltaTime;
            Vector3 offset_target = Vector3.MoveTowards(ground.position, transform.position, ground.rad());
            transform.position = Vector3.MoveTowards(transform.position, offset_target, step);
        }
        else
        {
            flying = false;
            if (!ground.found)
            {
                ground.found = true;
                claimed ++;
                float flagDist = 2f;
                GameObject f = (GameObject)Instantiate(Flag, 
                    transform.position + getDirFromCam(cam.transform.right) * flagDist , 
                    Quaternion.identity);
                f.transform.parent = ground.mesh.transform;
                f.transform.LookAt(ground.mesh.transform.position);
            }
        }
    }
    void update_rotation()
    {
        if (ground == null) return;
        if (flying)
        {
            float flyTime = .5f;
            if (startingToFlyCounter < flyTime)
            {
                startingToFlyCounter += Time.deltaTime;
            }
            else
            {
                Quaternion oldRot = transform.rotation;
                transform.LookAt(ground.position, transform.up);
                Quaternion newRot = transform.rotation;
                float speed = .05f;
                transform.rotation = Quaternion.Lerp(oldRot, newRot, speed);
            }
        }
        else
        {
            transform.LookAt(ground.position, transform.up);
        }
    }

    public void updateGroundExternal(Body b)
    {
        ground = b;
    }

    void leftMouseClick()
    {
        if (level)
        {
            Body check = level.GetComponent<NBodies>().findTarget(cam, mouseX, mouseY);
            if (check != null)
            {
                ground.ground = false;
                ground = check;
                ground.ground = true;
                flying = true;
                startingToFlyCounter = 0;
                // shot = (GameObject)Instantiate(Resources.Load("Bodies/b_shot"), cam.transform.position, Quaternion.identity);
                // shot.GetComponent<Shoot>().target = check;
                // shot.GetComponent<Shoot>().shotSpeed = shotSpeed;
            }
        }
    }

    void OnGUI()
    {
        int w = 100;
        GUI.DrawTexture(new Rect(mouseX-w/2, mouseY - w/2, w, w), target);
        GUI.Label(new Rect(Screen.width - 250, 25, 100,100), "Planets Discovered: " + claimed.ToString(), guiStyle);
    }

    

    float bound(float check, float low, float high)
    {
        if (check < low) return low;
        else if (check > high) return high;
        else return check;
    }


}

