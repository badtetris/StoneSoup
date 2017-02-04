using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Some nice static util functions. Should look familiar from lecture.
public class GlobalFuncs {
	
	// Utility functions for choosing randomly from arrays and lists and the like 
	public static T randElem<T>(T[] array) { 
		return array[Random.Range(0, array.Length)];
	}
	
	public static T randElem<T>(List<T> list)  { 
		return list[Random.Range(0, list.Count)];
	}
	
	public static void partialShuffle<T>(List<T> list, int howManyTimes) { 
		int i = 0; 
		int index1, index2; 
		T element; 
		while (i < howManyTimes) {
			index1 = Random.Range(0, list.Count);
			index2 = Random.Range(0, list.Count);
			element = list[index2]; 
			list[index2] = list[index1]; 
			list[index1] = element; 
			i++; 
		} 
	}
	
	public static void partialShuffle<T>(T[] array, int howManyTimes) { 
		int i = 0; 
		int index1, index2; 
		T element; 
		while (i < howManyTimes) { 
			index1 = Mathf.FloorToInt(Random.Range(0, array.Length));
			index2 = Mathf.FloorToInt(Random.Range(0, array.Length)); 
			element = array[index2]; 
			array[index2] = array[index1]; 
			array[index1] = element;
			i++;
		} 
	} 
	
	public static void shuffle<T>(List<T> list) {
		partialShuffle(list, list.Count*4);
	} 
	
	public static void shuffle<T>(T[] array) {
		partialShuffle(array, array.Length*4);	
	}

}
