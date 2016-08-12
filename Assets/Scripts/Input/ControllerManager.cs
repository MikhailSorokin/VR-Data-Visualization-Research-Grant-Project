namespace HandControlllerInputs
{
    using UnityEngine;
    using System.Collections;

    public struct ControllerInteractionEventArgs
    {
        public uint controllerIndex;
        public Vector2 touchpadAxis;
        public float touchpadAngle;
        public float buttonPressure;
    }

    //TODO: Make a struct with an enum variable inside that detects whether or not the callback
    //method will be used with a controller or key press

    public delegate void ControllerInteractionEventHandler(ControllerInteractionEventArgs cie);
    public delegate void DoubleControllerInteractionEventHandler();
    public delegate void KeyboardInteractionEventHandler();
    public delegate void MouseInteractionEventHandler();

    public class ControllerManager : MonoBehaviour
    {

        //Events that could happen with Input presses on the controllers
        public static event ControllerInteractionEventHandler MenuPressed;
        public static event ControllerInteractionEventHandler MenuReleased;

        public static event ControllerInteractionEventHandler GripPressed;
        public static event ControllerInteractionEventHandler GripReleased;

        public static event ControllerInteractionEventHandler TriggerPressed;
        public static event ControllerInteractionEventHandler TriggerReleased;

        public static event ControllerInteractionEventHandler TouchpadTouchStart;
        public static event ControllerInteractionEventHandler TouchpadTouchEnd;
        public static event ControllerInteractionEventHandler TouchpadPressed;
        public static event ControllerInteractionEventHandler TouchpadReleased;
        public static event ControllerInteractionEventHandler TouchpadAxisChanged;

        //Double press
        public static event DoubleControllerInteractionEventHandler DoubleControllerGUI;
        public static event DoubleControllerInteractionEventHandler DoubleTriggerPressed;
        public static event DoubleControllerInteractionEventHandler DoubleTriggerReleased;
        public static event DoubleControllerInteractionEventHandler DoubleTouchpadPressed;
        public static event DoubleControllerInteractionEventHandler DoubleTouchpadReleased;
        public static event DoubleControllerInteractionEventHandler Intro;
        public static event DoubleControllerInteractionEventHandler RigidbodyFreeze;

        //Keyboard Event Presses
        public static event KeyboardInteractionEventHandler MapGripToKeyPress;
        public static event KeyboardInteractionEventHandler MapGripToKeyRelease;

        public static event MouseInteractionEventHandler MapTriggerToMouseMoveOrClick;
        public static event MouseInteractionEventHandler MapTriggerToStopMouseMove;

        //public static event MouseInteractionEventHandler MapTouchpadStartToKeyPress;
        //public static event MouseInteractionEventHandler MapTouchpadEndToKeyPress;
        public static event MouseInteractionEventHandler MapTouchpadAxisChangedToMouseWheel;

        public static event KeyboardInteractionEventHandler MapTouchpadPressToKeyPress;
        public static event KeyboardInteractionEventHandler MapTouchpadReleaseToKeyRelease;

        public static event KeyboardInteractionEventHandler MapMenuPressedToKeyPress;
        public static event KeyboardInteractionEventHandler MapMenuReleasedToKeyPress;

        public static event KeyboardInteractionEventHandler MapDoubleTouchpadPressedToKeyPress;
        public static event KeyboardInteractionEventHandler MapDoubleTouchpadReleasedToKeyPress;

        public bool triggerPressed = false;
        public bool triggerAxisChanged = false;
        public bool menuPressed = false;
        public bool touchpadPressed = false;
        public bool touchpadTouched = false;
        public bool touchpadAxisChanged = false;
        public bool gripPressed = false;
        public bool bothTouchpadsPressed = false;

        //This is the parent spline object, the first thing that everyone sees once data is loaded in.
        public static GameObject refToMainSplineGO;

        //Vive Controllers and their indexes
        public static SteamVR_TrackedObject leftController;
        public static SteamVR_Controller.Device left_controller
        {
            get
            {
                SteamVR_Controller.Device inputDevice;
                if (leftController != null && leftController.index != SteamVR_TrackedObject.EIndex.None)
                    inputDevice = SteamVR_Controller.Input((int)leftController.index);
                else
                    inputDevice = null;

                return inputDevice;
            }
        }
        public static SteamVR_TrackedObject rightController;
        public static SteamVR_Controller.Device right_controller
        {
            get
            {
                SteamVR_Controller.Device inputDevice;
                if (rightController != null && rightController.index != SteamVR_TrackedObject.EIndex.None)
                    inputDevice = SteamVR_Controller.Input((int)rightController.index);
                else
                    inputDevice = null;

                return inputDevice;
            }
        }

        //All Input buttons will be referenced, and handled, here
        private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
        private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
        private Valve.VR.EVRButtonId touchPad = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
        private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
        private Valve.VR.EVRButtonId powerButton = Valve.VR.EVRButtonId.k_EButton_Max;

        //What was the last frame's axis on the touchpad?
        private Vector2 lastFrameLeftAxis = Vector2.zero;
        private Vector2 lastFrameRightAxis = Vector2.zero;

        /*Grip Press and Release Events*/
        public static void OnGripPressed(ControllerInteractionEventArgs e)
        {
            if (GripPressed != null)
                GripPressed(e);
        }

        /*For Mouse/Keyboard events*/
        public static void OnGripPressed()
        {
            if (MapGripToKeyPress != null)
                MapGripToKeyPress();
        }

        public static void OnGripReleased(ControllerInteractionEventArgs e)
        {
            if (GripReleased != null)
                GripReleased(e);
        }

        /*For Mouse/Keyboard events*/
        public static void OnGripReleased()
        {
            if (MapGripToKeyRelease != null)
                MapGripToKeyRelease();
        }

        /*Trigger Press and Release Events*/
        public static void OnTriggerPressed(ControllerInteractionEventArgs e)
        {
            if (TriggerPressed != null)
                TriggerPressed(e);
        }

        public static void OnTriggerPressed()
        {
            if (MapTriggerToMouseMoveOrClick != null)
                MapTriggerToMouseMoveOrClick();
        }

        public static void OnTriggerReleased(ControllerInteractionEventArgs e)
        {
            if (TriggerReleased != null)
                TriggerReleased(e);
        }

        public static void OnTriggerReleased()
        {
            if (MapTriggerToStopMouseMove != null)
                MapTriggerToStopMouseMove();
        }

        /*Simultaneous Trigger Press and Release Events*/
        public static void OnDoubleTouchpadPressed()
        {
            if (DoubleTouchpadPressed != null)
                DoubleTouchpadPressed();
        }

        public static void OnDoubleTouchpadReleased()
        {
            if (DoubleTouchpadReleased != null)
                DoubleTouchpadReleased();
        }

        public static void OnKeyDoubleTouchpadPressed()
        {
            if (MapDoubleTouchpadPressedToKeyPress != null)
                MapDoubleTouchpadPressedToKeyPress();
        }

        public static void OnKeyDoubleTouchpadReleased()
        {
            if (MapDoubleTouchpadReleasedToKeyPress != null)
                MapDoubleTouchpadReleasedToKeyPress();
        }


        public static void OnDoubleTriggerPressed()
        {
            if (DoubleTriggerPressed != null)
                DoubleTriggerPressed();
        }

        public static void OnDoubleTriggerReleased()
        {
            if (DoubleTriggerReleased != null)
                DoubleTriggerReleased();
        }

        public static void OnTouchpadPressed()
        {
            if (MapTouchpadPressToKeyPress != null)
                MapTouchpadPressToKeyPress();
        }

        public static void OnTouchpadPressed(ControllerInteractionEventArgs e)
        {
            if (TouchpadPressed != null)
                TouchpadPressed(e);
        }

        public static void OnTouchpadReleased()
        {
            if (MapTouchpadReleaseToKeyRelease != null)
                MapTouchpadReleaseToKeyRelease();
        }

        public static void OnTouchpadReleased(ControllerInteractionEventArgs e)
        {
            if (TouchpadReleased != null)
            {
                TouchpadReleased(e);
            }
        }

        public static void OnTouchpadTouchStart(ControllerInteractionEventArgs e)
        {
            if (TouchpadTouchStart != null)
                TouchpadTouchStart(e);
        }

        public static void OnTouchpadTouchEnd(ControllerInteractionEventArgs e)
        {
            if (TouchpadTouchEnd != null)
                TouchpadTouchEnd(e);
        }

        public static void OnTouchpadAxisChanged()
        {
            if (MapTouchpadAxisChangedToMouseWheel != null)
                MapTouchpadAxisChangedToMouseWheel();
        }

        public static void OnTouchpadAxisChanged(ControllerInteractionEventArgs e)
        {
            if (TouchpadAxisChanged != null)
                TouchpadAxisChanged(e);
        }

        /* Handles left and right controller ting of the position in worldspace.*/

        public static void OnMenuPressed()
        {
            if (MapMenuPressedToKeyPress != null)
                MapMenuPressedToKeyPress();
        }

        public static void OnMenuPressed(ControllerInteractionEventArgs e)
        {
            if (MenuPressed != null)
                MenuPressed(e);
        }

        public static void OnMenuReleased()
        {
            if (MapMenuReleasedToKeyPress != null)
                MapMenuReleasedToKeyPress();
        }

        public static void OnMenuReleased(ControllerInteractionEventArgs e)
        {
            if (MenuReleased != null)
                MenuReleased(e);
        }

        public static void OnIntro()
        {
            if (Intro != null)
                Intro();
        }

        public static void OnRigidbodyFreeze()
        {
            if (RigidbodyFreeze != null)
                RigidbodyFreeze();
        }

        public static void OnDoubleControllerGUI()
        {
            if (DoubleControllerGUI != null)
                DoubleControllerGUI();
        }

        //Dealing with only one device at once
        ControllerInteractionEventArgs SetButtonEvent(SteamVR_Controller.Device device, ref bool buttonBool, bool value, float buttonPressure)
        {
            buttonBool = value;
            ControllerInteractionEventArgs e;
            e.controllerIndex = device.index;
            e.buttonPressure = buttonPressure;
            e.touchpadAxis = device.GetAxis();

            float angle = Mathf.Atan2(e.touchpadAxis.y, e.touchpadAxis.x) * Mathf.Rad2Deg;
            angle = 90.0f - angle;
            if (angle < 0)
                angle += 360.0f;

            e.touchpadAngle = angle;

            return e;
        }

        //Dealing with multiple devices at once
        ControllerInteractionEventArgs SetButtonEvent(Vector3 left_device_pos, Vector3 right_device_pos, ref bool buttonBool, bool value, float buttonPressure)
        {
            buttonBool = value;
            ControllerInteractionEventArgs e;
            e.controllerIndex = (uint)5;
            e.buttonPressure = buttonPressure;
            e.touchpadAxis = Vector2.zero; //For now, this can never be accessed from this function call
            e.touchpadAngle = 0.0f; //For now, this can never be accessed from this function call

            return e;
        }

        bool Vector2ShallowEquals(Vector2 a, Vector2 b)
        {
            return (a.x.ToString("F1") == b.x.ToString("F1")
                && a.y.ToString("F1") == b.y.ToString("F1"));
        }

        void Start()
        {
            refToMainSplineGO = GameObject.FindGameObjectWithTag("Menu");
        }

        // Update is called once per frame
        void Update()
        {
            if (GameObject.Find("Controller (left)") != null)
            {
                leftController = GameObject.Find("Controller (left)").GetComponent<SteamVR_TrackedObject>();
            }

            if (GameObject.Find("Controller (right)") != null)
            {
                rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                OnTouchpadAxisChanged();
            }

            /*TODO: Make an event for this, might want to use a separate event call since 
             * the ones for singular ones work specifically for them
             */
            if (left_controller != null && right_controller != null)
            {
                OnIntro();
                OnDoubleControllerGUI();
            }

            if (left_controller != null && right_controller != null && !triggerPressed)
            {
                OnRigidbodyFreeze();
            }

            /*-------------------------------------------------------Touchpad Axis Change------------------------------------------------------------------------------------*/
            if (left_controller != null)
            {
                Vector2 leftTouchpadAxis = left_controller.GetAxis();
                Vector2 currentLeftTouchpadAxis = new Vector2(leftTouchpadAxis.x, leftTouchpadAxis.y);

                if (Vector2ShallowEquals(lastFrameLeftAxis, currentLeftTouchpadAxis))
                {
                    touchpadAxisChanged = false;
                }
                else
                {
                    OnTouchpadAxisChanged(SetButtonEvent(left_controller, ref touchpadAxisChanged, true, 1f));
                    touchpadAxisChanged = true;
                }

                leftTouchpadAxis = Vector2.zero;
                leftTouchpadAxis = new Vector2(currentLeftTouchpadAxis.x, currentLeftTouchpadAxis.y);
            }
            if (right_controller != null)
            {
                Vector2 rightTouchpadAxis = right_controller.GetAxis();
                Vector2 currentRightTouchpadAxis = new Vector2(rightTouchpadAxis.x, rightTouchpadAxis.y);

                if (Vector2ShallowEquals(lastFrameRightAxis, currentRightTouchpadAxis))
                {
                    touchpadAxisChanged = false;
                }
                else
                {
                    OnTouchpadAxisChanged(SetButtonEvent(right_controller, ref touchpadAxisChanged, true, 1f));
                    touchpadAxisChanged = true;
                }

                rightTouchpadAxis = Vector2.zero;
                rightTouchpadAxis = new Vector2(currentRightTouchpadAxis.x, currentRightTouchpadAxis.y);
            }

            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


            /*-------------------------------------------------------Grip Press and Release------------------------------------------------------------------------------------*/
            //Left Grip Button Press
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnGripPressed();
            }

            else if (Input.GetKeyUp(KeyCode.R))
            {
                OnGripReleased();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnKeyDoubleTouchpadPressed();
            }

            else if (Input.GetKeyUp(KeyCode.Space))
            {
                OnKeyDoubleTouchpadReleased();
            }

            //Left Grip Button Release
            if (left_controller != null && left_controller.GetPressUp(gripButton))
            {
                OnGripPressed(SetButtonEvent(left_controller, ref gripPressed, true, 1f));
            }

            //Left Grip Button Release
            if (left_controller != null && left_controller.GetPressUp(gripButton))
            {
                OnGripReleased(SetButtonEvent(left_controller, ref gripPressed, false, 0f));
            }

            //Right Grip Button Press
            if (right_controller != null && right_controller.GetPressDown(gripButton))
            {
                OnGripPressed(SetButtonEvent(right_controller, ref gripPressed, true, 1f));
            }

            //Right Grip Button Release
            if (right_controller != null && right_controller.GetPressUp(gripButton))
            {
                OnGripReleased(SetButtonEvent(right_controller, ref gripPressed, false, 0f));
            }
            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


            /*-------------------------------------------------------TouchPad Press, Release and Axis Change----------------------------------------------------------------------------------*/
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnTouchpadPressed();
            }
            else if (Input.GetKeyUp(KeyCode.Return))
            {
                OnTouchpadReleased();
            }


            //Left Touchpad Press
            if (left_controller != null && left_controller.GetPressDown(touchPad))
            {
                OnTouchpadPressed(SetButtonEvent(left_controller, ref touchpadPressed, true, 1f));
            }

            //Left Touchpad Release
            if (left_controller != null && left_controller.GetPressUp(touchPad))
            {
                OnTouchpadReleased(SetButtonEvent(left_controller, ref touchpadPressed, false, 0f));
            }

            //Right Touchpad Press
            if (right_controller != null && right_controller.GetPressDown(touchPad))
            {
                OnTouchpadPressed(SetButtonEvent(right_controller, ref touchpadPressed, true, 1f));
            }

            //Right Touchpad Release
            if (right_controller != null && right_controller.GetPressUp(touchPad))
            {
                OnTouchpadReleased(SetButtonEvent(right_controller, ref touchpadPressed, false, 0f));
            }

            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

            /*----------------------------------------------------------------Double Trigger Press or Release--------------------------------------------------------------------*/
            //Double Trigger Press
            if (left_controller != null && left_controller.GetPress(triggerButton) &&
                right_controller != null && right_controller.GetPress(triggerButton))
            {
                OnDoubleTriggerPressed();
            }
            else
            {
                OnDoubleTriggerReleased();
            }
            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

            /*----------------------------------------------------------------Double Touchpad Press or Release--------------------------------------------------------------------*/


            if ((left_controller != null && left_controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && right_controller != null && right_controller.GetPressDown(touchPad))
                || (left_controller != null && left_controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && right_controller != null && right_controller.GetPress(touchPad))
                )
            {
                OnDoubleTouchpadPressed();
            }
            else
            {
                OnDoubleTouchpadReleased();
            }

            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

            /*----------------------------------------------------------------Singular Trigger Press or Release------------------------------------------------------------------*/
            //Left Trigger Press
            if (left_controller != null && left_controller.GetPress(triggerButton))
            {
                OnTriggerPressed(SetButtonEvent(SteamVR_Controller.Input((int)leftController.index), ref triggerPressed, true, 1f));
            }

            //Left Trigger Release
            else if (left_controller != null && left_controller.GetPressUp(triggerButton))
            {
                OnTriggerReleased(SetButtonEvent(SteamVR_Controller.Input((int)leftController.index), ref triggerPressed, false, 0f));
            }

            //Right Trigger Press
            else if (right_controller != null && right_controller.GetPress(triggerButton))
            {
                OnTriggerPressed(SetButtonEvent(SteamVR_Controller.Input((int)rightController.index), ref triggerPressed, true, 1f));
            }

            //Right Trigger Release
            else if (right_controller != null && right_controller.GetPressUp(triggerButton))
            {
                OnTriggerReleased(SetButtonEvent(SteamVR_Controller.Input((int)rightController.index), ref triggerPressed, false, 0f));
            }
            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


            /*----------------------------------------------------------------Application Menu Press or Release------------------------------------------------------------------*/
            //Left Application Menu Button Press
            if (left_controller != null)
            {

                if (left_controller.GetPressDown(menuButton))
                {
                    OnMenuPressed(SetButtonEvent(left_controller, ref menuPressed, true, 1f));
                }

                if (left_controller.GetPressUp(menuButton))
                {
                    OnMenuReleased(SetButtonEvent(left_controller, ref menuPressed, false, 0f));
                }
            }

            if (right_controller != null)
            {

                if (right_controller.GetPressDown(menuButton))
                {
                    OnMenuPressed(SetButtonEvent(right_controller, ref menuPressed, true, 1f));
                }

                if (right_controller.GetPressUp(menuButton))
                {
                    OnMenuReleased(SetButtonEvent(right_controller, ref menuPressed, false, 0f));
                }
            }
            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


            /*----------------------------------------------------------------Power Button Press or Release------------------------------------------------------------------*/
            //Left Application Menu Button Press
            if (left_controller != null && left_controller.GetPress(powerButton))
            {
                Debug.Log("Power Button Pressed");
                //OnPowerPressed (SetButtonEvent(left_controller, ref powerPressed, true, 1f));
            }

            if (left_controller != null && left_controller.GetPressUp(powerButton))
            {
                Debug.Log("Power Button Released");
                //OnPowerPressed(SetButtonEvent(left_controller, ref powerPressed, false, 0f));
            }

            if (right_controller != null && right_controller.GetPress(powerButton))
            {
                Debug.Log("Power Button Pressed");
                //OnPowerPressed(SetButtonEvent(right_controller, ref powerPressed, true, 1f));
            }

            if (right_controller != null && right_controller.GetPressUp(powerButton))
            {
                Debug.Log("Power Button Released");
                //OnPowerPressed(SetButtonEvent (right_controller, ref powerPressed, false, 0f));
            }
            /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


        }
    }
}
