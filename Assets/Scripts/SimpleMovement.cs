using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

	private CharacterController cc;

	public float moveSpeed;
	public float rotSpeed;
	public float sinkSpeed;

	private float hold;

	private bool forward;

	private bool moving;
	private int facing; // 0 for +X, 3 for -X
	private float destX;
	private float destZ;
	private float cooldown;

	public float hexRadius;
	public float rotateInterval;
	public float rotateCooldown = 0;

	public int mode; // 0 for explore, 1 for maze

	public KeyCode left;
	private int leftFacing;
	public KeyCode right;
	private int rightFacing;

	public GameObject prevHex;

	public GameObject goalHex;

	// Start is called before the first frame update
	void Start()
	{
		cc = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 move = Vector3.zero;
		Vector3 rotate = Vector3.zero;

		// Check movement input
		if(mode == 0 && !moving)
		{
			if(Input.GetAxis("Vertical") > 0.1f)
			{
				// Find movement angle
				facing = (int) ((480 - cc.transform.eulerAngles.y) / 60) % 6;
				StartMove();
			}
		}
		else if(mode == 1 && !moving)
		{
			if(Input.GetKey(left) && !Input.GetKey(right))
			{
				facing = leftFacing;
				StartMove();
			}
			else if(Input.GetKey(right) && !Input.GetKey(left))
			{
				facing = rightFacing;
				StartMove();
			}
		}

		/*
		// Calculate movement if free to move
		if(hold <= 0f)
		{
			float vert = Input.GetAxis("Vertical");
			if(forward){vert = 1f;}
			if(vert > 0f) // Only move forward
				move += Time.deltaTime * vert * moveSpeed *
					cc.transform.forward;

			if(!forward)
				rotate += Time.deltaTime * Input.GetAxis("Horizontal") *
					rotSpeed * Vector3.up;
		}
		else
		{
			hold -= Time.deltaTime;
		}
		*/

		// Check rotation input
		if(cooldown > 0)
			cooldown -= Time.deltaTime;

		if(Input.GetAxis("Horizontal") > 0.1f && cooldown <= 0)
		{
			rotate += rotateInterval * Vector3.up;
			cooldown = rotateCooldown;
		}
		else if(Input.GetAxis("Horizontal") < -0.1f && cooldown <= 0)
		{
			rotate -= rotateInterval * Vector3.up;
			cooldown = rotateCooldown;
		}

		// Determine if at destination
		if(moving)
		{
			float angle = (float) facing * 60;
			move += Time.deltaTime * moveSpeed * (float) Math.Cos(angle *
				Mathf.Deg2Rad) * Vector3.right;
			move += Time.deltaTime * moveSpeed * (float) Math.Sin(angle *
				Mathf.Deg2Rad) * Vector3.forward;

			if(facing == 0 || facing == 1 || facing == 5 || facing == 6)
			{
				if(cc.transform.position.x > destX)
				{
					SnapPos();
					moving = false;
				}
			}
			else if(facing == 2 || facing == 3 || facing == 4)
			{
				if(cc.transform.position.x < destX)
				{
					SnapPos();
					moving = false;
				}
			}
			else { Debug.LogError("Invalid facing: " + facing); }
		}

		// Calculate lower to ground
		if(!cc.isGrounded)
			move += Time.deltaTime * sinkSpeed * Vector3.down;

		// Apply character translation
		cc.Move(move);

		// Apply character rotation
		cc.transform.Rotate(rotate);
	}

	public void BeginHold(float length)
	{
		hold = length;
	}

	public void EndHold()
	{
		hold = 0;
	}

	public bool IsHolding()
	{
		if(hold <= 0)
		{
			return false;
		}
		return true;
	}

	public void SetForward(bool on)
	{
		forward = on;
	}

	// Select hexagon
	public void StartMove()
	{
		moving = true;

		// Find final x coord
		switch(facing)
		{
			case 0:
			case 6:
				destX = cc.transform.position.x + hexRadius;
				break;
			case 1:
			case 5:
				destX = cc.transform.position.x + hexRadius / 2;
				break;
			case 2:
			case 4:
				destX = cc.transform.position.x - hexRadius / 2;
				break;
			case 3:
				destX = cc.transform.position.x - hexRadius;
				break;
			default:
				Debug.LogError("Invalid facing: " + facing);
				break;
		}

		// Find final z coord
		destZ = cc.transform.position.z + hexRadius * (float) Math.Sin((float)
			facing * 60 * Mathf.Deg2Rad);
	}

	public void SnapPos()
	{
		Vector3 pos = new Vector3(destX, cc.transform.position.y, destZ);
		cc.transform.position = pos;
	}

	public void SetChoices(int left, int right)
	{
		leftFacing = left;
		rightFacing = right;
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(hit.gameObject.GetInstanceID() != prevHex.GetInstanceID() &&
			mode == 1)
		{
			if(hit.gameObject.GetInstanceID() == goalHex.GetInstanceID())
			{
				
			}
			hit.gameObject.SendMessage("Enter", gameObject);
			prevHex.SendMessage("Exit", gameObject);
			prevHex = hit.gameObject;
		}
	}

}
