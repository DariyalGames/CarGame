using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour 
{
    public enum CarStates { Idle, Running, Turning, TurnEnd }
    class Wheel
    {
        public Collider collider;
	    public bool driveWheel = false;
	    public bool steerWheel = false;
	
	    public Vector3 wheelVelocity = Vector3.zero;
        public Vector3 groundSpeed = Vector3.zero;

        public Transform wheelGraphic;
        public Transform tireGraphic;
    }

    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(20.0f, 20.0f, 0.0f);

	public Transform startPostion;

    public Transform turnAxis;

    public Transform[] frontWheels;
    public Transform[] rearWheels;

    public float maxAccelerationMagnitude;
    public float accelerationToRotationMultiplyer;


    private Wheel[] wheels;

    private bool isAccelerating;
    private bool isTurnlerp;
    private float currentAccelerationValue;
    private float startAccelerationValue;
    private float lerpAccelrationStartTime;
    private float lerpAccelrationMaxTime = 2.0f;
    private float lerpDecelrationMaxTime = 1.0f;
    private bool isSteering;
	private float steering;
    private float maxSteerAngle = 30.0f;
    private float wheelRadius = 0.4f;
    private bool hasBackWheelContact = false;

    private CarStates state;
	
	// Use this for initialization
	void Start ()
	{
        rigidbody.centerOfMass = turnAxis.localPosition;
        rigidbody.inertiaTensorRotation = Quaternion.identity;

		transform.position = startPostion.position;
		mainCamera.transform.position = transform.position + cameraOffset;

        wheels = new Wheel[frontWheels.Length + rearWheels.Length];
        SetupWheels();

        isAccelerating = false;
        startAccelerationValue = 0.0f;
	}

    private void SetupWheels()
    {
        int wheelCount = 0;

        foreach(Transform wheel in frontWheels)
        {
            wheels[wheelCount] = SetupWheel(wheel, true);
            wheelCount++;
        }
        foreach (Transform wheel in rearWheels)
        {
            wheels[wheelCount] = SetupWheel(wheel, false);
            wheelCount++;
        }
    }

    private Wheel SetupWheel(Transform wheelTransform, bool isFrontWheel)
    {
        Wheel wheel = new Wheel();
        wheel.collider = wheelTransform.collider;
        wheel.wheelGraphic = wheelTransform;
        wheel.tireGraphic = wheelTransform.GetComponentsInChildren<Transform>()[1];
        wheelRadius = wheel.tireGraphic.renderer.bounds.size.y / 2;

        if (isFrontWheel)
        {
            wheel.steerWheel = true;

            GameObject go = new GameObject(wheelTransform.name + " Steer Column");
            go.transform.position = wheelTransform.position;
            go.transform.rotation = wheelTransform.rotation;
            go.transform.parent = transform;
            wheelTransform.parent = go.transform;
        }
        else
        {
            wheel.driveWheel = true;
        }

        return wheel;
    }
	
	// Update is called once per frame
	void Update () 
	{
        UpdateWheelGraphics();

        Vector3 offset = new Vector3(cameraOffset.x, cameraOffset.y + 5 * currentAccelerationValue / maxAccelerationMagnitude, cameraOffset.z);
		mainCamera.transform.position = transform.position + offset;
    }

    private void UpdateWheelGraphics()
    {
        foreach (Wheel w in wheels)
        {
            if (w.steerWheel)
            {
                w.wheelGraphic.localEulerAngles = new Vector3(0.0f, steering * maxSteerAngle, 0.0f);
            }

            float groundSpeed = rigidbody.velocity.magnitude;
            float angle = (groundSpeed / wheelRadius) * Time.deltaTime * Mathf.Rad2Deg;
            //Debug.Log("[Car]: {direction(right):" + transform.right.ToString() + "}");
            
            w.tireGraphic.Rotate(new Vector3(angle, 0.0f, 0.0f));
        }
    }

    void FixedUpdate()
    {
        GetInput();
        ApplySteering();
        ApplyThrottle();
    }


    private void ApplySteering()
    {
        //transform.Rotate(transform.up, steering);
        rigidbody.AddTorque(transform.up * steering * currentAccelerationValue * accelerationToRotationMultiplyer * rigidbody.mass);
    }

    private void ApplyThrottle()
    {
        if (hasBackWheelContact)
        {
            if (isAccelerating)
            {
                float t = Time.time - lerpAccelrationStartTime;
                currentAccelerationValue = Mathf.Lerp(startAccelerationValue, maxAccelerationMagnitude, t / lerpAccelrationMaxTime);
            }
            else
            {
                float t = Time.time - lerpAccelrationStartTime;
                currentAccelerationValue = Mathf.Lerp(startAccelerationValue, 0.0f, t / lerpDecelrationMaxTime);
            }

            //Debug.Log("[Car] {currentAccelerationValue:" + currentAccelerationValue.ToString() + "}");
            float force = currentAccelerationValue * rigidbody.mass;
            rigidbody.AddForce(transform.forward * force);
        }
    }


	private void GetInput ()
	{
		steering = Input.GetAxis ("Horizontal");

        if (steering == 0)
        {
            isSteering = false;
            if (!isAccelerating)
            {
                //Debug.Log("[Car]: start accelerating");
                isAccelerating = true;
                startAccelerationValue = currentAccelerationValue;
                lerpAccelrationStartTime = Time.time;
            }
        }
        else
        {
            isSteering = true;
            if (isAccelerating)
            {
                isAccelerating = false;
                startAccelerationValue = currentAccelerationValue;
                lerpAccelrationStartTime = Time.time;
            }
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        hasBackWheelContact = true;
    }

    void OnCollisionExit(Collision collision)
    {
        hasBackWheelContact = false;
    }
}
