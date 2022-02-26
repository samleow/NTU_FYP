using UnityEngine;
using System.Collections;

public class Pacdot : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "pacman")
		{
			// workaround against not triggering exit colliders
			// need use coroutine to wait for one fixed update frame before destroying
			StartCoroutine(DelayDestroy());

		}
	}

	IEnumerator DelayDestroy()
	{
		GameManager.score += 10;
		GameObject[] pacdots = GameObject.FindGameObjectsWithTag("pacdot");

		this.transform.position = new Vector3(-100, -100, 0);

		yield return new WaitForFixedUpdate();

		Destroy(gameObject);

		if (pacdots.Length == 1)
		{
			GameObject.FindObjectOfType<GameGUINavigation>().LoadLevel();
		}
	}

}
