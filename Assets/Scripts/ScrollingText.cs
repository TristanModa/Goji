using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace Goji
{
	public class ScrollingText : MonoBehaviour
	{
		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
			{
				SceneManager.LoadScene("StartScreenScene");
			}

			if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
			{
				transform.position += 200 * Time.deltaTime * Vector3.down;
			}

			if (UnityEngine.Input.GetKey(KeyCode.DownArrow))
			{
				transform.position += 200 * Time.deltaTime * Vector3.up;
			}
		}
	}
}