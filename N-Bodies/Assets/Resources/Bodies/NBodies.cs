using UnityEngine;
using System.Collections;

public class Util
{
    // I got lazy. Just use the one they gave me.
    static public float sqrt(float n)
    {
        return Mathf.Sqrt(n);
    }
    static public bool almost_equals(float a, float b, float e)
    {
        return (Mathf.Abs(a - b) < e);
    }
    static public float max(float a, float b)
    {
        return (a > b ? a : b);
    }

    static public float min(float a, float b)
    {
        return (a < b ? a : b);
    }
}
public class Body
{
    public GameObject mesh; // Mesh of the particle
    public Vector3 position;
    public Vector3 velocity; // Velocity of the particle
    public float dT; // Delta Time
    public float mass;
    public bool ground;
    public bool physics;
    public float planet_cutoff = 50, gas_cutoff = 100, sun_cutoff = 150;
    public enum Types {ASTEROID, PLANET, GAS_GIANT, STAR};
    public Types type;
    public bool found;


    // Constructor
    public Body(GameObject p, float m, Vector3 v, float scale)
    {
        ground = false;
        found = false;
        physics = true;
        mass = m;
        velocity = v;
        type = get_type();

        mesh = p;
        position = mesh.transform.position;
        mesh.transform.localScale = new Vector3(1,1,1) * calc_size(m, scale); 
    }

    public Types get_type()
    {
        if (mass < planet_cutoff)
        {
            return Types.ASTEROID;
        }
        else if (mass < gas_cutoff)
        {
            return Types.PLANET;
        }
        else if (mass < sun_cutoff)
        {
            return Types.GAS_GIANT;
        }
        else
        {
            return Types.STAR;
        }
    }
    // Function to calculate the radius of an object based on its mass
    public float calc_size(float m, float scale)
    {
        return scale * Mathf.Pow(m / (3f/(4f* 3.14159f)), 1f/3f);
        //return Mathf.Log(m) * scale;

    }

    // Apply a force to the object
    public void update_velocity(Vector3 v, float dT)
    {
        velocity = velocity + (v / mass) * dT;
        if (mesh)
        {
            Monitor m = mesh.GetComponent<Monitor>(); 
            m.update_v(velocity);
        }
    }

    // Apply the velocity over a single step
    public void update_position(float dT)
    {
        if (!mesh) return;
        mesh.transform.position += velocity * dT;
        position = mesh.transform.position;
    }

    // Return the distance between the center of one object and the center
    // of the other one.
    public float dist(Body other)
    {
        float x = position.x - other.position.x;
        float y = position.y - other.position.y;
        float z = position.z - other.position.z;
        return Util.sqrt(x * x + y * y + z * z);
    }

    // WLOG, return x scale. Only ever use spheres!
    public float rad()
    {
        if (mesh)
        {
            return mesh.transform.localScale.x;
        }
        return 0f;
    }

    public void setSprite(Sprite s)
    {
        if (ground || type == Types.STAR) s = null;
        mesh.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = s;
    }

    // Collide two object and eat up B. Unfortunately have to destroy B's
    // mesh elsewhere. Also remember to remove B from the list of bodies!
    // Also, if B is selected, select A.
    public void consume(Body other, float scale, Material[] a, Material[] p, Material[] s)
    {
        float oldmass = mass;
        velocity = (velocity * mass + other.mass * other.velocity) / (mass + other.mass);
        mass += other.mass;
        mesh.transform.localScale = new Vector3(1,1,1) * (calc_size(mass, scale));
        Vector3 wm1 = mesh.transform.position * mass; // Weighted mass 1
        Vector3 wm2 = other.mesh.transform.position * other.mass; //Weighted mass 2
        mesh.transform.position = (wm1 + wm2) / (mass + other.mass);
        Monitor m = mesh.GetComponent<Monitor>(); 
        m.update_m(mass);
        found = found || other.found;
        
        if (other.ground || ground)
        {
            ground = true;
            GameObject.Find("Player").GetComponent<Character>().updateGroundExternal(this);
        }
        if (oldmass < sun_cutoff && mass > sun_cutoff)
        {
            // if (other.type == Types.SUN)
            // {

            // }
            // else
            // {
                int mat = Random.Range(0,s.Length-1);
                mesh.GetComponent<Renderer>().material = s[mat];
                if (mat == 0)
                {
                    mesh.GetComponent<Renderer>().material.SetColor("_EmissionColor", 
                        Color.HSVToRGB(Random.Range(0f,1f), .8f, 0.8823529f));
                }
                GameObject lightGameObject = new GameObject("The Light");
                Light lightComp = lightGameObject.AddComponent<Light>();
                lightComp.color = Color.white;
                lightGameObject.transform.position = mesh.transform.position;
                lightComp.range = 1000;
                lightComp.intensity = 3f;
                lightComp.transform.parent = mesh.transform;
                type = Types.STAR;
            //}
        }
    }

}

public class NBodies : MonoBehaviour {

    public bool pause = false;
    public int w,h,d;
    public float scale;
    public float bang;
    public int offset;
    public float G;
    public float acceleration;
    public Vector3 lower_bound;
    public Vector3 upper_bound;
    public bool grid;
    public bool sphere;

    public int Bodies;
    public Sprite defaultHitbox;
    public Sprite selectedHitbox;

    public Material[] asteroids;
    public Material[] planets;
    public Material[] suns;

    public float mass_total;
    

    private Body[] bodies;
    private Character player;
    private bool flag;
    public ArrayList visibleBodies;
    private ArrayList bodyQueue = new ArrayList();
    

    // Use this for initialization
    void Start () 
    {
        player = GameObject.Find("Player").GetComponent<Character>();
        initiate();
    }

    public void addBodyExternal(Body b)
    {
        bodyQueue.Add(b);
    }

    public void clearBodyQueue()
    {
        Body[] temp = new Body[bodies.Length + bodyQueue.Count];
        for (int i = 0; i < bodies.Length; i ++) {temp[i] = bodies[i];}

        int c = 0;
        foreach(Body b in bodyQueue)
        {
            temp[bodies.Length + c] = b;
            c ++;
        }
        bodies = temp;
        bodyQueue = new ArrayList();
    }

    void initiate()
    {
        if (!sphere && !grid) grid = true;
        scale = scale > 0 ? scale: 1;
        G = G > 0 ? G: 1;
        w = w >= 0 ? w: 1;
        d = d >= 0 ? d: 1;
        h = h >= 0 ? h: 1;
        acceleration = acceleration > 0 ? acceleration: 1;
        mass_total = 0f;

        Object r = Resources.Load("Bodies/b");
        bodies = new Body[w * d * h];

        if (!flag)
        {
            lower_bound.x += transform.position.x;
            lower_bound.y += transform.position.y;
            lower_bound.z += transform.position.z;

            upper_bound.x += transform.position.x;
            upper_bound.y += transform.position.y;
            upper_bound.z += transform.position.z;
        }

        for (int i = 0; i < bodies.Length; i ++)
        {
            int a = i / (h * w);
            int b = (i / w) % h;
            int c  = i % w;
            float x,y,z;
            if (grid)
            {
                x = (a - d/2) * offset * scale + transform.position.x;
                y = (b - h/2) * offset * scale + transform.position.y;
                z = (c - w/2) * offset * scale + transform.position.z;
            }
            else if (sphere)
            {
                float rad = ((float)(d + w + h)/3f)/2f * offset * scale;
                Vector3 random = Random.insideUnitSphere;
                x = random.x * rad + transform.position.x;
                y = random.y * rad + transform.position.y;
                z = random.z * rad + transform.position.z;
            }
            else
            {
                x = y = z = 0f;
            }

            float mass = Random.Range(1, 10);
            bodies[i] = new Body((GameObject) Instantiate(
                r, // model (make sure its a prefab!)
                new Vector3(x,y,z), //position
                Quaternion.identity), //rotation
                mass,
                new Vector3(Random.Range(-bang,bang),
                            Random.Range(-bang,bang),
                            Random.Range(-bang,bang)),
                scale);
            Monitor m = bodies[i].mesh.GetComponent<Monitor>(); 
            m.update_m(mass); 
            m.update_v(bodies[i].velocity);
            mass_total += mass;
        }
        Bodies = bodies.Length;
        visibleBodies = new ArrayList();
        GameObject.Find("Player").GetComponent<Character>().instantiateGround();

    }

    float gravitational_force(float m1, float m2, float r)
    {
        if (r == 0)
        {
            return 0;
        }
        float F = G * (m1 * m2) / (r * r * r);
        return F;
    }

    Vector3 two_body(Body A, Body B)
    {   
        float m1 = A.mass;
        float m2 = B.mass;
        float d = A.dist(B);
        float F = gravitational_force(m1,m2,d);
        float x_comp = B.position.x - A.position.x;
        float y_comp = B.position.y - A.position.y;
        float z_comp = B.position.z - A.position.z;
        float x = F * x_comp;
        float y = F * y_comp;
        float z = F * z_comp;

        Vector3 force = new Vector3(x,y,z);
        return force;
    }

    // naive, n^2 algorithm
    void n_body_physics(Body b, Body[] bodies)
    {
        Vector3 force = new Vector3(0,0,0);
        for (int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] == null) continue;
            if (!bodies[i].physics) continue;
            Vector3 instance_force = two_body(b,bodies[i]);
            b.update_velocity(instance_force, Time.deltaTime * acceleration);
            force += instance_force;
        }
        Monitor m = b.mesh.GetComponent<Monitor>(); 
        m.update_f(force);  
    }
    // Update is called once per frame
    void Update () 
    {
        if (bodies.Length == 1)
        {
            reset();
            return;
        }
        if (Input.GetKeyDown("r"))
        {
            reset();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) pause = !pause;
        if (pause) return;

        if (bodyQueue.Count > 0) clearBodyQueue();
        mass_total = 0;
        // Update all the positions
        for (int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] == null || !bodies[i].mesh) continue;
            bodies[i].update_position(Time.deltaTime * acceleration);
            mass_total += bodies[i].mass;
            if (lower_bound != upper_bound)
            {
                if (bodies[i].mesh.transform.position.x < lower_bound.x || bodies[i].mesh.transform.position.x > upper_bound.x)
                {
                    bodies[i].velocity.x = -bodies[i].velocity.x;
                }
                if (bodies[i].mesh.transform.position.y < lower_bound.y || bodies[i].mesh.transform.position.y > upper_bound.y)
                {
                    bodies[i].velocity.y = -bodies[i].velocity.y;
                }
                if (bodies[i].mesh.transform.position.z < lower_bound.z || bodies[i].mesh.transform.position.z > upper_bound.z)
                {
                    bodies[i].velocity.z = -bodies[i].velocity.z;
                }
            }
        }

        // Any bodies that inhabit the same space become one bigger body with
        // the sum of masses, velocities, and scales.
        int tick = 0;
        for (int i = 0; i < bodies.Length; i ++)
        {
            Body A = bodies[i];
            if (A != null)
            {
                for (int j = 0; j < bodies.Length; j ++)
                {
                    Body B = bodies[j];
                    if (A != B && B != null && 
                        Util.almost_equals(A.dist(B), 0f, Util.max (A.rad(), B.rad())))
                    {
                        A.consume(B, scale, asteroids, planets, suns);
                        for (int k = 0; k < bodies[j].mesh.transform.childCount; k ++)
                        {
                            Destroy(bodies[j].mesh.transform.GetChild(k).gameObject);
                        }
                        Destroy(bodies[j].mesh);
                        bodies[j] = null;
                        tick ++;
                    }
                }
            }
        }

        // Create a new, smaller list, to save memory and so I don't have to check
        // all sorts of rubbish "null" nodes
        Body[] temp = new Body[bodies.Length - tick];
        int tempI = 0;
        for (int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] != null)
            {
                temp[tempI] = bodies[i];
                tempI ++;
            }
        }
        bodies = temp;

        // Apply whatever n_body heuristic I feel like.
        for (int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] != null && bodies[i].physics) n_body_physics(bodies[i], bodies);
        }

        for (int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] == null) continue;
            bodies[i].setSprite(getHitBox(bodies[i]));
        }
        Bodies = bodies.Length;
    }

    Sprite getHitBox(Body b)
    {
        GameObject m = b.mesh;
        if (!b.ground && Vector3.Distance(m.transform.position, player.cam.transform.position) < player.targetMax)
        {
            Rect r = ObjectToRect(m);
            if (r.Contains(new Vector2(player.mouseX, player.mouseY)))
            {        
                return selectedHitbox;
            }

            else return defaultHitbox;
        }
        else return null;
    }

    void reset()
    {
        for (int i = 0; i < bodies.Length; i ++)
        {
            Destroy(bodies[i].mesh);
            bodies[i].ground = false;
        }
        flag = true;
        initiate();
    }

    // Returns the nearest body
    public Body nearest(Vector3 pos)
    {
        Body res = null;
        float best = float.MaxValue;
        for(int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] != null)
            {
                float test = Vector3.Distance(pos, bodies[i].position);
                if (test < best)
                {
                    res = bodies[i];
                    best = test;
                }
            }
        }
        return res;
    }

    public void getVisible(Camera c)
    {
        float min = player.targetMin;
        float max = player.targetMax;
        ArrayList A = new ArrayList();
        for (int i = 0; i < bodies.Length; i ++)
        {
            if (bodies[i] == null) continue;
            if (!bodies[i].mesh) continue;
            if (bodies[i].ground) continue;
            if (bodies[i].type == Body.Types.STAR) continue;
            Vector3 pos = c.WorldToScreenPoint(bodies[i].mesh.transform.position);
            if (pos.z < min || pos.z > max) continue; //Don't target anything that clips

            A.Add(bodies[i]);
        }
        visibleBodies = A; //remove_overlap(c, A);
    }

    public Body findTarget(Camera c, float x, float y)
    {
        Body res = null; 
        float best = 999999999; // Big
        int buffer = 10;
        foreach (Body b in visibleBodies)
        {
            if (b.mesh != null)
            {
                Vector3 pos = c.WorldToScreenPoint(b.mesh.transform.position);
                Rect r = ObjectToRect(b.mesh);
                r = growRect(r, buffer);
                if (r.Contains(new Vector2(x, y)))
                {
                    if (pos.z < best && b.mesh.GetComponent<Renderer>().isVisible)
                    {
                        res = b;
                        best = pos.z;
                    }
                }
            }
        }
        return res;
    }

    Rect growRect(Rect r, float s)
    {
        r.x -= s/2;
        r.y -= s/2;
        r.width += s;
        r.height += s;
        return r;
    }
    // Code provided by Luloak2 
    // <http://answers.unity3d.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html>
    public static Rect ObjectToRect(GameObject go)
    {
      Vector3 cen = go.GetComponent<Renderer>().bounds.center;
      Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
      Vector2[] extentPoints = new Vector2[8]
       {
           WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
           WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
           WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
           WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
           WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
           WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
           WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
           WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
       };
      Vector2 min = extentPoints[0];
      Vector2 max = extentPoints[0];
      foreach (Vector2 v in extentPoints)
      {
          min = Vector2.Min(min, v);
          max = Vector2.Max(max, v);
      }
      return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
      Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
      screenPoint.y = (float) Screen.height - screenPoint.y;
      return screenPoint;
    }

    // End code provided by Luloak2
}
