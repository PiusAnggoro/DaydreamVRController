using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quiver : MonoBehaviour{
	public GameObject arrowPrefab;
	private GameObject _heldArrow;

	private Vector3 _throwVelocity;
	private Vector3 _previousPosition;
	private float _pullBackAmount = 0.0f;

	public Vector3 maxFireVelocity = new Vector3(0, 30, 0);
	private Vector3 _fireVelocity;

	private LineRenderer _trajectoryLineRenderer;

	void Start() {
	  _trajectoryLineRenderer = GetComponent<LineRenderer>();
	}

	void Update() {
		//if (GvrControllerInput.ClickButtonDown && ArmIsInPosition) {
		if (GvrControllerInput.ClickButtonDown) {
			CreateArrow();
		}

		if (_heldArrow == null) return;

		PollTouchpad();
		//SimulateTrajectory();
		CalculateThrowVelocity();
		CalculateFireVelocity();

		if (GvrControllerInput.ClickButtonUp) {
			ReleaseArrow();
		}
	}

	private void CreateArrow() {
		GameObject arrow = Instantiate(arrowPrefab);
		HoldArrow(arrow);
	}

	private void HoldArrow(GameObject arrow) {
		_heldArrow = arrow;
		_heldArrow.transform.SetParent(transform, false);
		_heldArrow.transform.localPosition = new Vector3(0, 0, 1);
		_heldArrow.transform.localEulerAngles = new Vector3(90, 0, 0);
	}

	private void ReleaseArrow() {
	    // change the parent to the world
		_heldArrow.transform.SetParent(null, true);

		// nullify the current velocity
		Rigidbody arrowRigidbody = _heldArrow.GetComponent<Rigidbody>();
		arrowRigidbody.velocity = Vector3.zero;
		arrowRigidbody.isKinematic = false;

		if (IsFiring) {
		  // fire the object when releasing while aiming
		  arrowRigidbody.AddRelativeForce(_fireVelocity, ForceMode.VelocityChange);
		}
		else {
		  // throw the object when releasing while held
		  arrowRigidbody.AddForce(_throwVelocity, ForceMode.VelocityChange);
		}

		//TrailRenderer trailRenderer = _heldArrow.GetComponent<TrailRenderer>();
		//trailRenderer.enabled = true;

		_trajectoryLineRenderer.enabled = false;
		_heldArrow = null;
	}

	private void CalculateThrowVelocity() {
		// the velocity is based on the previous position
		_throwVelocity = (_heldArrow.transform.position - _previousPosition) / Time.deltaTime;

		// update previous position
		_previousPosition = _heldArrow.transform.position;
	}

	private bool IsFiring {
		get { return _pullBackAmount > 0.5f; }
	}

	private void PollTouchpad() {
	  _pullBackAmount = GvrControllerInput.TouchPos.y;
	  PositionArrow();
	}

	private void PositionArrow() {
	  // update the position of the arrow locally based on the pullback amount.
	  // Since the touchpad ranges from 0(top)..1(bottom), we need to invert the amount it's coming in
	  const float initialOffset = 0.25f;
	  Vector3 transformLocalPosition = _heldArrow.transform.localPosition;
	  transformLocalPosition.z = initialOffset + 1.0f - _pullBackAmount;
	  _heldArrow.transform.localPosition = transformLocalPosition;
	}

	private void CalculateFireVelocity() {
	  _fireVelocity = maxFireVelocity * _pullBackAmount;
	}

	private bool ArmIsInPosition {
	  get {
	    // The rotation to test against
	    const float compareAgainstRotation = 90.0f;

	    // The wiggle room afforded to the rotation check.
	    const float compareEpsilon = 65.0f;

	    // get the rotation from the GameObject's transform, which is set by the arm controller
	    float observedRotation = transform.parent.localEulerAngles.x;

	    return Mathf.Abs(compareAgainstRotation - observedRotation) < compareEpsilon;
	  }
	}

	private void SimulateTrajectory() {
	  // only show if the arrow is being fired
	  _trajectoryLineRenderer.enabled = IsFiring;

	  Vector3 initialPosition = _heldArrow.transform.position;
	  Vector3 initialVelocity = _heldArrow.transform.rotation * _fireVelocity;

	  const int numberOfPositionsToSimulate = 50;
	  const float timeStepBetweenPositions = 0.2f;

	  // setup the initial conditions
	  Vector3 simulatedPosition = initialPosition;
	  Vector3 simulatedVelocity = initialVelocity;

	  // update the position count
	  _trajectoryLineRenderer.positionCount = numberOfPositionsToSimulate;

	  for (int i = 0; i < numberOfPositionsToSimulate; i++) {
	    // set each position of the line renderer
	    _trajectoryLineRenderer.SetPosition(i, simulatedPosition);

	    // change the velocity based on Gravity and the time step.
	    simulatedVelocity += Physics.gravity * timeStepBetweenPositions;

	    // change the position based on Gravity and the time step.
	    simulatedPosition += simulatedVelocity * timeStepBetweenPositions;
	  }
	}
}
