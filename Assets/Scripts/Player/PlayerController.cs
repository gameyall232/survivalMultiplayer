using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] ClientInput input;
	[SerializeField] Rigidbody rigidBody;
	[SerializeField] Camera cam;

	Vector2 inputVector = new Vector2(0, 0);
	Vector2 mouseVector = new Vector2(0, 0);
	float xRot = 0;

	[SerializeField] LayerMask environmentMask;
	[SerializeField] float sensitivity;
	[SerializeField] float speed = 5;
	[SerializeField] float jumpStregth;
	
	void Start()
	{
		Camera.main.enabled = false;
		cam.enabled = true;

		#region Bind Inputs
		input.Primary.Move.performed += ctx => inputVector = ctx.ReadValue<Vector2>();
		input.Primary.Mouse.performed += ctx => mouseVector = ctx.ReadValue<Vector2>();
		input.Primary.Jump.performed += _ => Jump();
		#endregion
	}
	
	void Update()
	{
		#region Movement
		Vector3 moveVector = (transform.right * inputVector.x + transform.forward * inputVector.y).normalized * speed /* * (alt ? 1.5f : 1)*/;
		rigidBody.linearVelocity = new Vector3(moveVector.x, rigidBody.linearVelocity.y, moveVector.z);

		if (Cursor.lockState == CursorLockMode.Locked)
		{
			xRot = Mathf.Clamp(xRot -= mouseVector.y * sensitivity, -90, 90);
			transform.Rotate(0f, mouseVector.x * sensitivity, 0f);
			cam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
		}
		#endregion
	}
	
	void Jump()
	{
		if (Physics.CheckSphere(transform.position, .3f, environmentMask))
		{
			rigidBody.linearVelocity = Vector3.up * jumpStregth;
		}
	}
}