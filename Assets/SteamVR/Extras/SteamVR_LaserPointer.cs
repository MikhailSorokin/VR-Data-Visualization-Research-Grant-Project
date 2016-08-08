using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HandControlllerInputs;

public struct ObjectInteractEventArgs
{
    public uint controllerIndex;
    public GameObject target;
}

public delegate void ObjectInteractEventHandler(object sender, ObjectInteractEventArgs e);

public struct PointerEventArgs
{
    public SteamVR_Controller.Device controller;
    public uint controllerIndex;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);

public class SteamVR_LaserPointer : MonoBehaviour
{
    public bool active = true;
    public Color color;
    public float thickness = 0.002f;
    public GameObject holder;
    public GameObject pointer;
    bool isActive = false;
    public bool addRigidBody = false;
    public Transform reference;
    public bool moving = false;
    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;
    public static SplineDecorator categorySpline;

    Transform previousContact = null;

    private GameObject touchedObject = null;
    private SteamVR_TrackedObject trackedController;
    //These Vectors will be constant scales that will be used throughout this whole class.
    private Vector3 thinLPScale;
    private Vector3 thickLPScale;

    private float dist;

    public event ObjectInteractEventHandler ControllerTouchInteractableObject;
    public event ObjectInteractEventHandler ControllerUntouchInteractableObject;


    public virtual void OnControllerTouchInteractableObject(ObjectInteractEventArgs e)
    {
        if (ControllerTouchInteractableObject != null)
            ControllerTouchInteractableObject(this, e);
    }

    public virtual void OnControllerUntouchInteractableObject(ObjectInteractEventArgs e)
    {
        if (ControllerUntouchInteractableObject != null)
            ControllerUntouchInteractableObject(this, e);
    }

    public ObjectInteractEventArgs SetControllerInteractEvent(GameObject target)
    {
        ObjectInteractEventArgs e;
        e.controllerIndex = (uint)trackedController.index;
        e.target = target;
        return e;
    }

    public GameObject GetTouchedObject()
    {
        return touchedObject;
    }

    public bool IsObjectInteractable(GameObject obj)
    {
        return (obj && obj.GetComponent<SteamVR_InteractableObject>());
    }

    public PointerEventArgs SetPointerInteractEvent(GameObject target, float distance, uint flags)
    {
        PointerEventArgs pe;
        pe.controller = SteamVR_Controller.Input((int)trackedController.index); ;
        pe.controllerIndex = (uint)trackedController.index;
        pe.target = target.transform;
        pe.distance = distance;
        pe.flags = flags;
        return pe;
    }

    void Awake()
    {
        trackedController = GetComponent<SteamVR_TrackedObject>();
        thinLPScale = new Vector3(thickness, thickness, 0f);
        thickLPScale = new Vector3(thickness * 5f, thickness * 5f, 0f);
    }

    void Start()
    {
        //Set up the origin of the laser pointer
        holder = new GameObject("Laser Pointer");
        holder.transform.position = Vector3.zero;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        holder.transform.parent = this.transform;
        holder.transform.position = Vector3.zero;
        holder.transform.localPosition = Vector3.zero;

        //Set up the laser pointer cube material position and material
        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = Vector3.zero;
        pointer.transform.localPosition = new Vector3(0f, 0f, -50f);
        BoxCollider collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody)
        {
            if (collider)
            {
                collider.isTrigger = true;
            }
            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if (collider)
            {
                Object.Destroy(collider);
            }
        }
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
        categorySpline = GameObject.FindGameObjectWithTag("Menu").GetComponent<SplineDecorator>();

        ControllerManager.TriggerPressed += new ControllerInteractionEventHandler(ChangePointerThicker);
        ControllerManager.TriggerReleased += new ControllerInteractionEventHandler(ChangePointerThinner);
    }

    public virtual void OnPointerIn(PointerEventArgs e)
    {
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerOut(PointerEventArgs e)
    {
        if (PointerOut != null)
            PointerOut(this, e);
    }


    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            isActive = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }

        dist = 100f;

        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();

        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit);

        if (previousContact && previousContact != hit.transform)
        {
            PointerEventArgs args = new PointerEventArgs();
            if (controller != null)
            {
                args.controller = SteamVR_Controller.Input((int)controller.controllerIndex);
                args.controllerIndex = controller.controllerIndex;
            }
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
            previousContact = null;
            pointer.GetComponent<MeshRenderer>().material.color = Color.white;
            pointer.transform.localScale = Vector3.zero;

            //
            // Disable allow Grab here
            //
            if (IsObjectInteractable(touchedObject))
            {
                GameObject.Find("Controller (left)").GetComponent<SteamVR_LaserPointer>().enabled = true;
                GameObject.Find("Controller (left)").GetComponent<SteamVR_LaserPointer>().pointer.GetComponent<MeshRenderer>().material.color = Color.white;

                GameObject.Find("Controller (right)").GetComponent<SteamVR_LaserPointer>().enabled = true;
                GameObject.Find("Controller (right)").GetComponent<SteamVR_LaserPointer>().pointer.GetComponent<MeshRenderer>().material.color = Color.white;

                moving = false;
                OnPointerOut(SetPointerInteractEvent(touchedObject, args.distance, args.flags));
                OnControllerTouchInteractableObject(SetControllerInteractEvent(touchedObject));
                touchedObject.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(false);
                touchedObject.GetComponent<SteamVR_InteractableObject>().StopTouching(this.gameObject);
            }
            touchedObject = null;
        }

        if (bHit && previousContact != hit.transform && hit.collider.gameObject.GetComponent<SplineDecorator>() != null &&
            hit.collider.GetComponent<SplineDecorator>().expanded)
        {
            PointerEventArgs argsIn = new PointerEventArgs();
            if (controller != null)
            {
                argsIn.controller = SteamVR_Controller.Input((int)controller.controllerIndex);
                argsIn.controllerIndex = controller.controllerIndex;
            }

            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerIn(argsIn);
            previousContact = hit.transform;
            pointer.GetComponent<MeshRenderer>().material.color = Color.green;
            pointer.transform.localScale = TempVectorUpdated(thinLPScale, hit.distance);

            //
            // Trigger allow Grab here
            //

            //5 - UI Layer
            if (touchedObject == null && IsObjectInteractable(hit.collider.gameObject))
            {
                //Disable the other controller, non-used controller in order to prevent weird things from happen
                if (controller.gameObject.name == "Controller (left)" && GameObject.Find("Controller (right)") != null)
                {
                    GameObject.Find("Controller (right)").GetComponent<SteamVR_LaserPointer>().enabled = false;
                    GameObject.Find("Controller (right)").GetComponent<SteamVR_LaserPointer>().pointer.transform.localScale = Vector3.zero;
                }
                else if (controller.gameObject.name == "Controller (right)" && GameObject.Find("Controller (left)") != null)
                {
                    GameObject.Find("Controller (left)").GetComponent<SteamVR_LaserPointer>().enabled = false;
                    GameObject.Find("Controller (left)").GetComponent<SteamVR_LaserPointer>().pointer.transform.localScale = Vector3.zero;
                }

                moving = true;
                touchedObject = hit.collider.gameObject;
                OnControllerUntouchInteractableObject(SetControllerInteractEvent(touchedObject));
                OnPointerIn(SetPointerInteractEvent(touchedObject, argsIn.distance, argsIn.flags));
                touchedObject.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(true, Color.clear);
                touchedObject.GetComponent<SteamVR_InteractableObject>().StartTouching(this.gameObject);
            }
        }

        if (!bHit)
        {
            previousContact = null;
        }
        if (bHit && hit.distance < 100f)
        {
            dist = hit.distance;
        }

        if (controller != null && (controller.triggerPressed || ViveControllerInput.Instance.guiSelected))
        {
            pointer.transform.localScale = TempVectorUpdated(thickLPScale);
            if (touchedObject != null && touchedObject.GetComponent<Rigidbody>() == null)
            {
                touchedObject.gameObject.AddComponent<Rigidbody>();
                touchedObject.GetComponent<Rigidbody>().useGravity = false;
                touchedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                touchedObject.GetComponent<Rigidbody>().detectCollisions = true;
            }

            if (touchedObject != null && touchedObject.GetComponent<Rigidbody>() != null)
            {
                touchedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                touchedObject.GetComponent<Rigidbody>().detectCollisions = true;
            }
        }

        pointer.transform.localPosition = new Vector3(0f, 0f, -dist / 2f);
    }

    void ChangePointerThicker(ControllerInteractionEventArgs e)
    {
        if (GetComponent<SteamVR_TrackedController>().controllerIndex == e.controllerIndex 
            && pointer.transform.localScale != Vector3.zero)
        {
            pointer.transform.localScale = TempVectorUpdated(thickLPScale);
        }
    }

    void ChangePointerThinner(ControllerInteractionEventArgs e)
    {
        if (GetComponent<SteamVR_TrackedController>().controllerIndex == e.controllerIndex
            && pointer.transform.localScale != Vector3.zero)
        {
            pointer.transform.localScale = TempVectorUpdated(thinLPScale);
        }
    }

    private Vector3 TempVectorUpdated(Vector3 wantedScale)
    {
        Vector3 tempVectorForDist = wantedScale;
        tempVectorForDist.z = dist;
        return tempVectorForDist;
    }

    private Vector3 TempVectorUpdated(Vector3 wantedScale, float paramDist)
    {
        Vector3 tempVectorForDist = wantedScale;
        tempVectorForDist.z = paramDist;
        return tempVectorForDist;
    }

}
