using UnityEngine;
using System.Collections;

public class CameraResize : MonoBehaviour {

	public static float screenScale = 1f;

	public static float PTMRatio = 10f;

	public float scaleOneScreenWidth = 320f;
	public float scaleOneScreenHeight = 180f;



	// Use this for initialization
	void Awake() {
		setCameraSize();
	}

	// We try to set our camera to be the best integer scale of our scaleOneSize.
	protected void setCameraSize () {
		Camera camera = GetComponent<Camera>();

		float screenWidth = Screen.width;
		float screenHeight = Screen.height;
		
		int widthScale = Mathf.FloorToInt(screenWidth / scaleOneScreenWidth);
		int heightScale = Mathf.FloorToInt(screenHeight / scaleOneScreenHeight);
		screenScale = Mathf.Min(widthScale, heightScale);

		camera.orthographicSize = (screenHeight/(2f*PTMRatio*screenScale));
	}
	
}
