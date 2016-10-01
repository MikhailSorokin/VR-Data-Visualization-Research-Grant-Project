using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HandControlllerInputs;

public class BaseLaserPointer : SteamVR_LaserPointer {

    //public bool moving = false;

    protected GameObject touchedObject = null;
    protected SteamVR_TrackedObject trackedController;
    //These Vectors will be constant scales that will be used throughout this whole class.
    protected Vector3 thinLPScale;
    protected Vector3 thickLPScale;

    SteamVR_LaserPointer steamVR = new SteamVR_LaserPointer();

    protected override void Start()
    {
        base.Start();
    }

}
