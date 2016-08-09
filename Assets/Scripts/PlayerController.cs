using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;
    public float speed;

    public Text statusText;

    public GameObject myo;
    public GameObject force;    //solve the problem of Quanterion to Vecror3
    private Pose _lastPose = Pose.Unknown;

    private Quaternion _antiYaw = Quaternion.identity;
    private float _referenceRoll = 0.0f;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        statusText.text = "idle";
	}
	
	void FixedUpdate () {

        ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo>();

        if (thalmicMyo.pose != _lastPose)
        {
            _lastPose = thalmicMyo.pose;
            if (thalmicMyo.pose == Pose.Fist)
            {
                _antiYaw = Quaternion.FromToRotation(
                new Vector3(myo.transform.forward.x, 0, myo.transform.forward.z),
                new Vector3(0, 0, 1)
                ); // updating reference

                Vector3 referenceZeroRoll = computeZeroRollVector(myo.transform.forward);
                _referenceRoll = rollFromZero(referenceZeroRoll, myo.transform.forward, myo.transform.up);

            }
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            statusText.text = "idle";
        }

        Vector3 zeroRoll = computeZeroRollVector(myo.transform.forward);
        float roll = rollFromZero(zeroRoll, myo.transform.forward, myo.transform.up);
        float relativeRoll = normalizeAngle(roll - _referenceRoll);

        Quaternion antiRoll = Quaternion.AngleAxis(relativeRoll, myo.transform.forward);
        if (thalmicMyo.pose == _lastPose && _lastPose == Pose.Fist)
        {
            statusText.text = "Moving";
            transform.rotation = _antiYaw * antiRoll * Quaternion.LookRotation(myo.transform.forward);
            Vector3 relativePosition = force.transform.position - transform.position;
            Vector3 movement = new Vector3(relativePosition.x, 0.0f, relativePosition.y);
            rb.MovePosition(transform.position+movement*speed*Time.deltaTime);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            other.gameObject.SetActive(false);
        }
    }


    float rollFromZero(Vector3 zeroRoll, Vector3 forward, Vector3 up)
    {
        // The cosine of the angle between the up vector and the zero roll vector. Since both are
        // orthogonal to the forward vector, this tells us how far the Myo has been turned around the
        // forward axis relative to the zero roll vector, but we need to determine separately whether the
        // Myo has been rolled clockwise or counterclockwise.
        float cosine = Vector3.Dot(up, zeroRoll);

        // To determine the sign of the roll, we take the cross product of the up vector and the zero
        // roll vector. This cross product will either be the same or opposite direction as the forward
        // vector depending on whether up is clockwise or counter-clockwise from zero roll.
        // Thus the sign of the dot product of forward and it yields the sign of our roll value.
        Vector3 cp = Vector3.Cross(up, zeroRoll);
        float directionCosine = Vector3.Dot(forward, cp);
        float sign = directionCosine < 0.0f ? 1.0f : -1.0f;

        // Return the angle of roll (in degrees) from the cosine and the sign.
        return sign * Mathf.Rad2Deg * Mathf.Acos(cosine);
    }

    // Compute a vector that points perpendicular to the forward direction,
    // minimizing angular distance from world up (positive Y axis).
    // This represents the direction of no rotation about its forward axis.
    Vector3 computeZeroRollVector(Vector3 forward)  //axis that doesn't change
    {
        Vector3 antigravity = Vector3.up;   //reference axis that rotate about, make angle with
        Vector3 m = Vector3.Cross(myo.transform.forward, antigravity);
        Vector3 roll = Vector3.Cross(m, myo.transform.forward);

        return roll.normalized;
    }

    // Adjust the provided angle to be within a -180 to 180.
    float normalizeAngle(float angle)
    {
        if (angle > 180.0f)
        {
            return angle - 360.0f;
        }
        if (angle < -180.0f)
        {
            return angle + 360.0f;
        }
        return angle;
    }
}
