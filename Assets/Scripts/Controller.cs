using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour {

	public float moveSpeed = 6;

	Rigidbody rigid;
	Camera viewCamera;
	Vector3 velocity;

	void Start () {
		rigid = GetComponent<Rigidbody> ();
		viewCamera = Camera.main;
	}

	void Update () {
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
		transform.LookAt (mousePos + Vector3.up * transform.position.y);
        transform.eulerAngles = Vector3.up * transform.eulerAngles.y;
        velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;
	}

	void FixedUpdate() {
		rigid.MovePosition (rigid.position + velocity * Time.fixedDeltaTime);
	}
}