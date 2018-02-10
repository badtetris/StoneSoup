using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283PulseEffect : MonoBehaviour {


	public float pulsePeriod = 1f;
	public float pulseMinAmp = 0.75f;
	public float pulseMaxAmp = 1.5f;

	protected float _startTime;

	// Use this for initialization
	void Start () {
		_startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		float omega = 2*Mathf.PI / pulsePeriod;
		float t = Time.time-_startTime;
		float amp = (Mathf.Sin(omega*t) + 1f)/2f;
		amp = pulseMinAmp + (pulseMaxAmp-pulseMinAmp)*amp;
		transform.localScale = new Vector3(amp, amp, 1);
	}
}
