using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasResize : MonoBehaviour {

	void Start() {
		// Scale our canvas the same way our Camera has been scaled.
		CanvasScaler scaler = GetComponent<CanvasScaler>();
		scaler.scaleFactor = CameraResize.screenScale;
	}
}
