//====================================================================================
//
// Purpose: Provide ability to grab an interactable object when it is being touched
//
// This script must be attached to a Controller within the [CameraRig] Prefab
//
// The SteamVR_ControllerEvents and SteamVR_InteractTouch scripts must also be
// attached to the Controller
//
// Press the default 'Trigger' button on the controller to grab an object
// Released the default 'Trigger' button on the controller to drop an object
//
//====================================================================================

using UnityEngine;
using System.Collections;
using HandControlllerInputs;

public class SteamVR_GrabInteractor : MonoBehaviour
{
    public ControllerManager controllerManager;
    public Rigidbody controllerAttachPoint = null;
    public bool hideControllerOnGrab = false;
    public Collider otherCollider;

    public event ObjectInteractEventHandler ControllerGrabInteractableObject;
    public event ObjectInteractEventHandler ControllerUngrabInteractableObject;

    Joint controllerAttachJoint;
    GameObject grabbedObject = null;

    ControllerLaserPointer laserPointer;
    SteamVR_TrackedObject trackedController;
    SteamVR_ControllerActions controllerActions;

    private int grabEnabledState = 0;
    private bool addedToTip = false;

    public virtual void OnControllerGrabInteractableObject(ObjectInteractEventArgs e)
    {
        if (ControllerGrabInteractableObject != null)
            ControllerGrabInteractableObject(this, e);
    }

    public virtual void OnControllerUngrabInteractableObject(ObjectInteractEventArgs e)
    {
        if (ControllerUngrabInteractableObject != null)
            ControllerUngrabInteractableObject(this, e);
    }

    public void ForceRelease()
    {
        if (grabbedObject.GetComponent<SteamVR_InteractableObject>().AttatchIsTrackObject())
        {
            UngrabTrackedObject();
        }
        else
        {
            ReleaseObject((uint)trackedController.index, false);
        }
    }

    private void Awake()
    {
        if (GetComponent<SteamVR_InteractTouch>() == null)
        {
            Debug.LogError("SteamVR_InteractGrab is required to be attached to a SteamVR Controller that has the SteamVR_InteractTouch script attached to it");
            return;
        }

        if (GetComponent<ControllerLaserPointer>() == null)
        {
            Debug.LogError("SteamVR_InteractGrab is required to be attached to a SteamVR Controller that has the LaserPointerInputModule script attached to it");
            return;
        }

        laserPointer = GetComponent<ControllerLaserPointer>();
        trackedController = GetComponent<SteamVR_TrackedObject>();
        controllerActions = GetComponent<SteamVR_ControllerActions>();
    }

    private void Start()
    {

        ControllerManager.TriggerPressed += new ControllerInteractionEventHandler(DoGrabObject);
        ControllerManager.TriggerReleased += new ControllerInteractionEventHandler(DoReleaseObject);

    }

    private void Update()
    {
        if (!addedToTip && transform.GetChild(0).Find("tip") != null)
        {
            GameObject tip = transform.GetChild(0).Find("tip").gameObject;
            tip.AddComponent<Rigidbody>();
            tip.GetComponent<Rigidbody>().useGravity = false;
            addedToTip = true;
            controllerAttachPoint = tip.transform.GetComponent<Rigidbody>();
        }
    }

    private bool IsObjectGrabbable(GameObject obj)
    {
        return (laserPointer.IsObjectInteractable(obj) && obj.GetComponent<SteamVR_InteractableObject>().isGrabbable);
    }

    private bool IsObjectHoldOnGrab(GameObject obj)
    {
        return (obj && obj.GetComponent<SteamVR_InteractableObject>() && obj.GetComponent<SteamVR_InteractableObject>().holdButtonToGrab);
    }

    private void SnapObjectToGrabToController(GameObject obj)
    {
        //Pause collisions (if allowed on object) for a moment whilst sorting out position to prevent clipping issues
        obj.GetComponent<SteamVR_InteractableObject>().PauseCollisions(0.2f);

        SteamVR_InteractableObject.GrabSnapType grabType = obj.GetComponent<SteamVR_InteractableObject>().grabSnapType;

        if (grabType == SteamVR_InteractableObject.GrabSnapType.Rotation_Snap)
        {
            // Identity Controller Rotation
            this.transform.eulerAngles = new Vector3(0f, 270f, 0f);
            obj.transform.eulerAngles = obj.GetComponent<SteamVR_InteractableObject>().snapToRotation;
        }

        if (grabType != SteamVR_InteractableObject.GrabSnapType.Precision_Snap)
        {
            obj.transform.position = controllerAttachPoint.transform.position;
        }

        CreateJoint(obj);
    }

    private void CreateJoint(GameObject obj)
    {

        if (obj.GetComponent<SteamVR_InteractableObject>().grabAttatchMechanic == SteamVR_InteractableObject.GrabAttatchType.Fixed_Joint)
        {
            controllerAttachJoint = obj.AddComponent<FixedJoint>();
        }
        else if (obj.GetComponent<SteamVR_InteractableObject>().grabAttatchMechanic == SteamVR_InteractableObject.GrabAttatchType.Spring_Joint)
        {
            SpringJoint tempSpringJoint = obj.AddComponent<SpringJoint>();
            tempSpringJoint.spring = obj.GetComponent<SteamVR_InteractableObject>().springJointStrength;
            tempSpringJoint.damper = obj.GetComponent<SteamVR_InteractableObject>().springJointDamper;
            controllerAttachJoint = tempSpringJoint;
        }
        controllerAttachJoint.breakForce = obj.GetComponent<SteamVR_InteractableObject>().detatchThreshold;
        controllerAttachJoint.connectedBody = controllerAttachPoint;
    }

    private Rigidbody ReleaseGrabbedObjectFromController(bool withThrow)
    {
        var jointGameObject = controllerAttachJoint.gameObject;
        var rigidbody = jointGameObject.GetComponent<Rigidbody>();
        if (withThrow)
        {
            Object.DestroyImmediate(controllerAttachJoint);
        }
        else
        {
            Object.Destroy(controllerAttachJoint);
        }
        controllerAttachJoint = null;

        return rigidbody;
    }

    private void ThrowReleasedObject(Rigidbody rb, uint controllerIndex)
    {
        var origin = trackedController.origin ? trackedController.origin : trackedController.transform.parent;
        var device = SteamVR_Controller.Input((int)controllerIndex);
        if (origin != null)
        {
            rb.velocity = origin.TransformVector(device.velocity);
            rb.angularVelocity = origin.TransformVector(device.angularVelocity);
        }
        else
        {
            rb.velocity = device.velocity;
            rb.angularVelocity = device.angularVelocity;
        }
        rb.maxAngularVelocity = rb.angularVelocity.magnitude;
    }

    private void GrabInteractedObject()
    {
        if (controllerAttachJoint == null && grabbedObject == null &&
            IsObjectGrabbable(laserPointer.GetTouchedObject()))
        {
            InitGrabbedObject();
            SnapObjectToGrabToController(grabbedObject);

        }
    }

    private void GrabTrackedObject()
    {
        if (grabbedObject == null &&
            IsObjectGrabbable(laserPointer.GetTouchedObject()))
        {
            InitGrabbedObject();
        }
    }

    private void InitGrabbedObject()
    {
        grabbedObject = laserPointer.GetTouchedObject();

        OnControllerGrabInteractableObject(laserPointer.SetControllerInteractEvent(grabbedObject));

        grabbedObject.GetComponent<SteamVR_InteractableObject>().Grabbed(this.gameObject);
        if (grabbedObject)
        {
            grabbedObject.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(false);
        }
        if (hideControllerOnGrab)
        {
            controllerActions.ToggleControllerModel(false);
        }
    }

    private void UngrabInteractedObject(uint controllerIndex, bool withThrow)
    {
        if (grabbedObject != null && controllerAttachJoint != null)
        {
            Rigidbody releasedObjectRigidBody = ReleaseGrabbedObjectFromController(withThrow);
            if (withThrow)
            {
                ThrowReleasedObject(releasedObjectRigidBody, controllerIndex);
            }
            InitUngrabbedObject();
        }
    }

    private void UngrabTrackedObject()
    {
        if (grabbedObject != null)
        {
            InitUngrabbedObject();
        }
    }

    private void InitUngrabbedObject()
    {
        OnControllerUngrabInteractableObject(laserPointer.SetControllerInteractEvent(grabbedObject));
        grabbedObject.GetComponent<SteamVR_InteractableObject>().Ungrabbed(this.gameObject);

        if (hideControllerOnGrab)
        {
            controllerActions.ToggleControllerModel(true);
        }

        if (grabbedObject.GetComponent<Rigidbody>() != null)
        {
            grabbedObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            //grabbedObject.GetComponent<Rigidbody>().detectCollisions = false;
            //grabbedObject.GetComponent<Rigidbody>().detectCollisions = true;
        }
        grabbedObject.GetComponent<SteamVR_InteractableObject>().ToggleHighlight(false);
        grabbedObject = null;
    }

    private void ReleaseObject(uint controllerIndex, bool withThrow)
    {
        UngrabInteractedObject(controllerIndex, withThrow);
        grabEnabledState = 0;
    }

    private bool IsValidGrab()
    {
        GameObject lasObj = laserPointer.GetTouchedObject();
        return (lasObj != null && laserPointer.IsObjectInteractable(lasObj));
    }

    private void DoGrabObject(ControllerInteractionEventArgs e)
    {
        if (IsValidGrab())
        {
            if (laserPointer.GetTouchedObject() != null && laserPointer.GetTouchedObject().GetComponent<SteamVR_InteractableObject>().AttatchIsTrackObject())
            {
                GrabTrackedObject();
            }
            else
            {
                GrabInteractedObject();
            }

            if (!IsObjectHoldOnGrab(laserPointer.GetTouchedObject()))
            {
                grabEnabledState++;
            }
        }
    }

    private void DoReleaseObject(ControllerInteractionEventArgs e)
    {
        if (IsObjectHoldOnGrab(grabbedObject) || grabEnabledState >= 2)
        {
            if (grabbedObject.GetComponent<SteamVR_InteractableObject>().AttatchIsTrackObject())
            {
                UngrabTrackedObject();
            }
            else
            {
                ReleaseObject(e.controllerIndex, false);
            }
        }
    }
}