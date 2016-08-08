//====================================================================================
//
// Purpose: Provide basic touch detection of controller to interactable objects
//
// This script must be attached to a Controller within the [CameraRig] Prefab
//
//====================================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public struct ObjectInteractEventArgs
{
    public uint controllerIndex;
    public GameObject target;
}

public delegate void ObjectInteractEventHandler(object sender, ObjectInteractEventArgs e);

public class SteamVR_InteractTouch : MonoBehaviour
{

    public bool hideControllerOnTouch = false;
    public Color globalTouchHighlightColor = Color.clear;

    public event ObjectInteractEventHandler ControllerTouchInteractableObject;
    public event ObjectInteractEventHandler ControllerUntouchInteractableObject;

    private GameObject touchedObject = null;
    private SteamVR_TrackedObject trackedController;
    private SteamVR_ControllerActions controllerActions;

    private Color startColor;

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

    void Awake()
    {
        trackedController = GetComponent<SteamVR_TrackedObject>();
        controllerActions = GetComponent<SteamVR_ControllerActions>();
    }

    void Start()
    {
        if (GetComponent<SteamVR_ControllerActions>() == null)
        {
            Debug.LogError("SteamVR_InteractTouch is required to be attached to a SteamVR Controller that has the SteamVR_ControllerActions script attached to it");
            return;
        }

        //Create trigger box collider for controller
        SphereCollider collider = this.gameObject.AddComponent<SphereCollider>();
        collider.radius = 0.05f;
        collider.center = new Vector3(0f, -0.035f, -0.01f);
        collider.isTrigger = true;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Datapoint")
        {
            startColor = collider.GetComponent<Renderer>().material.color;
            collider.GetComponent<Renderer>().material.color = Color.red;

			/*
			//when sphere touched, repopulate the canvas the the authors articles

			print ("touched " + collider);
			// get the field object
			GameObject fieldObject = GameObject.FindGameObjectWithTag ("Input Field");
			// get its text field
			InputField field = fieldObject.gameObject.GetComponent<InputField> ();
			// get the author of this data point
			*/

        }
        if (collider.tag == "Menu")
        {
            collider.GetComponent<Rigidbody>().isKinematic = false;
            collider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (transform.name == "Controller (left)")
        {
            GameObject.Find("Controller (right)").GetComponent<SphereCollider>().enabled = false;
            if (touchedObject == null && IsObjectInteractable(collider.gameObject))
            {
                touchedObject = collider.gameObject;
                OnControllerTouchInteractableObject(SetControllerInteractEvent(touchedObject));
                touchedObject.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(true, globalTouchHighlightColor);
                touchedObject.GetComponent<SteamVR_InteractableObject>().StartTouching(this.gameObject);
                if (controllerActions.IsControllerVisible() && hideControllerOnTouch)
                {
                    controllerActions.ToggleControllerModel(false);
                }
            }
        }

        if (transform.name == "Controller (right)")
        {
           	GameObject.Find("Controller (left)").GetComponent<SphereCollider>().enabled = false;
            if (touchedObject == null && IsObjectInteractable(collider.gameObject))
            {
                touchedObject = collider.gameObject;
                OnControllerTouchInteractableObject(SetControllerInteractEvent(touchedObject));
                touchedObject.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(true, globalTouchHighlightColor);
                touchedObject.GetComponent<SteamVR_InteractableObject>().StartTouching(this.gameObject);
                if (controllerActions.IsControllerVisible() && hideControllerOnTouch)
                {
                    controllerActions.ToggleControllerModel(false);
                }
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        transform.GetComponent<SteamVR_ControllerActions>().TriggerHapticPulse(1, 3000);
        if (collider.tag == "Datapoint")
        {
            collider.GetComponent<Renderer>().material.color = startColor;
        }
        if (collider.tag == "Menu")
        {
            if (transform.name == "Controller (left)")
            {
                GameObject.Find("Controller (right)").GetComponent<SphereCollider>().enabled = true;
                collider.GetComponent<Rigidbody>().isKinematic = true;
                collider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                if (IsObjectInteractable(collider.gameObject))
                {
                    OnControllerUntouchInteractableObject(SetControllerInteractEvent(collider.gameObject));
                    collider.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(false);
                    collider.GetComponent<SteamVR_InteractableObject>().StopTouching(this.gameObject);
                }
                touchedObject = null;
                if (hideControllerOnTouch)
                {
                    controllerActions.ToggleControllerModel(true);
                }
            }

            if (transform.name == "Controller (right)")
            {
                GameObject.Find("Controller (left)").GetComponent<SphereCollider>().enabled = true;
                collider.GetComponent<Rigidbody>().isKinematic = true;
                collider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                if (IsObjectInteractable(collider.gameObject))
                {
                    OnControllerUntouchInteractableObject(SetControllerInteractEvent(collider.gameObject));
                    collider.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(false);
                    collider.GetComponent<SteamVR_InteractableObject>().StopTouching(this.gameObject);
                }
                touchedObject = null;
                if (hideControllerOnTouch)
                {
                    controllerActions.ToggleControllerModel(true);
                }
            }
        }
    }
}
