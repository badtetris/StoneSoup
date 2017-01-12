using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public float cameraFollowSpeed = 10f;
	
	void Start() {
		Vector2 newCameraPos = transform.position;
		// If we're in single room mode, we behave differently than in the other modes.
		if (GameManager.gameMode == GameManager.GameMode.SingleRoom) {
			// Basically, in single room mode, keep us centered over a single room.
			// Note: we actually target 1/2 tile length below the room to make room for the UI.
			newCameraPos = GameManager.instance.currentRoomPosition-Vector2.up*(Tile.TILE_SIZE/2f);
		}
		// Make sure our z-coordinate is left unchanged.
		transform.position = new Vector3(newCameraPos.x, newCameraPos.y, transform.position.z);
	}


	void Update() {
		Vector2 newCameraPos = transform.position;
		// If we're in single room mode, we behave differently than in the other modes.
		if (GameManager.gameMode == GameManager.GameMode.SingleRoom) {
			// Basically, in single room mode, keep us centered over a single room.
			// Note: we actually target 1/2 tile length below the room to make room for the UI.
			newCameraPos = Vector2.Lerp(newCameraPos, GameManager.instance.currentRoomPosition-Vector2.up*(Tile.TILE_SIZE/2), cameraFollowSpeed*Time.deltaTime);
		}

		// Make sure our z-coordinate is left unchanged.
		transform.position = new Vector3(newCameraPos.x, newCameraPos.y, transform.position.z);
	}


}
