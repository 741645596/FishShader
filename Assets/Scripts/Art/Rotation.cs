using UnityEngine;

public class Rotation : MonoBehaviour
{
	public float RotationSpeedX = 90f;

	public float RotationSpeedY;

	public float RotationSpeedZ;


	private void Update()
	{
		transform.Rotate(new Vector3(RotationSpeedX, RotationSpeedY, RotationSpeedZ) * Time.deltaTime);
	}
}
