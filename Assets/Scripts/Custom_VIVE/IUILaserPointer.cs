using UnityEngine;


abstract public class IUILaserPointer : MonoBehaviour {

    public float laserThickness = 0.002f;
    public float laserHitScale = 0.02f;
    public bool laserAlwaysOn = false;
    public Color color;

    protected GameObject hitPoint;
    protected float _distanceLimit;

    // Use this for initialization
    protected virtual void Start()
    {
        // initialize concrete class
        Initialize();
            
        // register with the LaserPointerInputModule
        if(LaserPointerInputModule.instance == null) {
            new GameObject().AddComponent<LaserPointerInputModule>();
        }
            

        LaserPointerInputModule.instance.AddController(this);
    }

    void OnDestroy()
    {
        if(LaserPointerInputModule.instance != null)
            LaserPointerInputModule.instance.RemoveController(this);
    }

    protected virtual void Initialize() { }
    public virtual void OnEnterControl(GameObject control) { }
    public virtual void OnExitControl(GameObject control) { }
    abstract public bool GUIButtonDown(); //this is when a GUI element is pressed from the controller
    abstract public bool GUIButtonUp(); //this is when a GUI element is released from the controller

    protected virtual void Update()
    {

    }

    // limits the laser distance for the current frame
    public virtual void LimitLaserDistance(float distance)
    {
        if(distance < 0.0f)
            return;

        if(_distanceLimit < 0.0f)
            _distanceLimit = distance;
        else
            _distanceLimit = Mathf.Min(_distanceLimit, distance);
    }
}
