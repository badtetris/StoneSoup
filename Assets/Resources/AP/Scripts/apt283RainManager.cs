using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283RainManager : MonoBehaviour {

	protected static apt283RainManager _instance = null;

	public GameObject rainDropPrefab;

	public int numDropsPerFrame = 5;

	public static apt283RainManager instance {
		get {
			return _instance;
		}
	}

	void Awake() {
		if (_instance == null) {
			_instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	void Update() {
		for (int i = 0; i < numDropsPerFrame; i++) {
			int randomGridX = Random.Range(0, LevelGenerator.ROOM_WIDTH*GameManager.instance.roomGrid.GetLength(0));
			int randomGridY = Random.Range(0, LevelGenerator.ROOM_HEIGHT*GameManager.instance.roomGrid.GetLength(1));
			Vector2 worldPos = Tile.toWorldCoord(randomGridX, randomGridY);
			GameObject dropObj = Instantiate(rainDropPrefab);
			dropObj.transform.position = worldPos;
			dropObj.GetComponent<Tile>().init();
		}
	}


}
