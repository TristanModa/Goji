using UnityEngine;

public class DangerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Vector2 pos = Vector2.zero;
		pos.x = Mathf.Cos(Time.time / 4) * (47 - 5) / 2.0f;
		pos.y = Mathf.Cos(Time.time) * (27 - 5) / 2.0f;
		transform.position = pos;
	}
}
