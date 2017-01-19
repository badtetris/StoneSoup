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
			newCameraPos = GameManager.instance.currentRoom.roomCenter-Vector2.up*(Tile.TILE_SIZE/2f);
		}
		else {
			Camera camera = GetComponent<Camera>();
			// Combined and chaos mode have a floating camera.
			int numXRooms = GameManager.instance.roomGrid.GetLength(0);
			int numYRooms = GameManager.instance.roomGrid.GetLength(1);

			float screenTop = camera.orthographicSize;
			float screenBottom = -screenTop;
			float screenRight = camera.orthographicSize*camera.aspect;
			float screenLeft = -screenRight;


			float minCameraX = -Tile.TILE_SIZE - screenLeft;
			float minCameraY = -2*Tile.TILE_SIZE - screenBottom;
			float maxCameraX = (numXRooms-1)*Tile.TILE_SIZE*LevelGenerator.ROOM_WIDTH+LevelGenerator.ROOM_WIDTH*Tile.TILE_SIZE+Tile.TILE_SIZE - screenRight;
			float maxCameraY = (numYRooms-1)*Tile.TILE_SIZE*LevelGenerator.ROOM_HEIGHT + LevelGenerator.ROOM_HEIGHT*Tile.TILE_SIZE + Tile.TILE_SIZE - screenTop;

			newCameraPos = Player.instance.transform.position;
			newCameraPos = new Vector2(Mathf.Clamp(newCameraPos.x, minCameraX, maxCameraX), Mathf.Clamp(newCameraPos.y, minCameraY, maxCameraY));
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
			newCameraPos = Vector2.Lerp(newCameraPos, GameManager.instance.currentRoom.roomCenter-Vector2.up*(Tile.TILE_SIZE/2), cameraFollowSpeed*Time.unscaledDeltaTime);
		}
		else {

			Camera camera = GetComponent<Camera>();
			// Combined and chaos mode have a floating camera.
			int numXRooms = GameManager.instance.roomGrid.GetLength(0);
			int numYRooms = GameManager.instance.roomGrid.GetLength(1);

			float screenTop = camera.orthographicSize;
			float screenBottom = -screenTop;
			float screenRight = camera.orthographicSize*camera.aspect;
			float screenLeft = -screenRight;


			float minCameraX = -Tile.TILE_SIZE - screenLeft;
			float minCameraY = -2*Tile.TILE_SIZE - screenBottom;
			float maxCameraX = (numXRooms-1)*Tile.TILE_SIZE*LevelGenerator.ROOM_WIDTH+LevelGenerator.ROOM_WIDTH*Tile.TILE_SIZE+Tile.TILE_SIZE - screenRight;
			float maxCameraY = (numYRooms-1)*Tile.TILE_SIZE*LevelGenerator.ROOM_HEIGHT + LevelGenerator.ROOM_HEIGHT*Tile.TILE_SIZE + Tile.TILE_SIZE - screenTop;


			newCameraPos = Vector2.Lerp(newCameraPos, Player.instance.transform.position, cameraFollowSpeed*Time.deltaTime);
			newCameraPos = new Vector2(Mathf.Clamp(newCameraPos.x, minCameraX, maxCameraX), Mathf.Clamp(newCameraPos.y, minCameraY, maxCameraY));
		}

		// Make sure our z-coordinate is left unchanged.
		transform.position = new Vector3(newCameraPos.x, newCameraPos.y, transform.position.z);
	}


}
