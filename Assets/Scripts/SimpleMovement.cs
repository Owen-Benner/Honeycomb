using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{

	private CharacterController cc;

	public float moveSpeed;
	//public float rotSpeed;
	public float sinkSpeed;

	private float hold;

	private bool forward;

	private bool moving;
	private float destX;
	private float destZ;
	private int facing; // 0 for +Z, 3 for -Z
	private float cooldown;

	public float hexRadius;
	public float rotateInterval;
	public float rotateCooldown = 0;

	public KeyCode left;
	public KeyCode right;

	public MazeLogic mazeLogic;

	public int mode; // 0 for explore, 1 for maze

	// Start is called before the first frame update
	void Start()
	{
		cc = GetComponent<CharacterController>();
		mazeLogic.SetMode(mode);
		if(mode == 1)
			mazeLogic.UpdateHexes();
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
				// TODO: Fix facing of 6?
				facing = Mathf.RoundToInt(cc.transform.eulerAngles.y / 60);
				facing = (facing + 6) % 6;

				if(mazeLogic.CheckEdge(facing))
				{
					StartMove();
				}
			}
		}
		else if(mode == 1 && !moving)
		{
			if(Input.GetKey(left) && !Input.GetKey(right))
			{
				facing = mazeLogic.leftFacing;
				mazeLogic.leftChoice();
				mazeLogic.lastFacing = facing;
				StartMove();
			}
			else if(Input.GetKey(right) && !Input.GetKey(left))
			{
				facing = mazeLogic.rightFacing;
				mazeLogic.rightChoice();
				mazeLogic.lastFacing = facing;
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
			move += Time.deltaTime * moveSpeed * (float) Math.Sin(angle *
				Mathf.Deg2Rad) * Vector3.right;
			move += Time.deltaTime * moveSpeed * (float) Math.Cos(angle *
				Mathf.Deg2Rad) * Vector3.forward;

			if(facing == 5 || facing == 6 || facing == 0 || facing == 1)
			{
				if(cc.transform.position.z > destZ)
				{
					SnapPos();
					moving = false;
					if(mode == 1)
						mazeLogic.UpdateHexes();
				}
			}
			else if(facing == 2 || facing == 3 || facing == 4)
			{
				if(cc.transform.position.z < destZ)
				{
					SnapPos();
					moving = false;
					if(mode == 1)
						mazeLogic.UpdateHexes();
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
				destZ = cc.transform.position.z + hexRadius;
				break;
			case 1:
			case 5:
				destZ = cc.transform.position.z + hexRadius / 2;
				break;
			case 2:
			case 4:
				destZ = cc.transform.position.z - hexRadius / 2;
				break;
			case 3:
				destZ = cc.transform.position.z - hexRadius;
				break;
			default:
				Debug.LogError("Invalid facing: " + facing);
				break;
		}

		// Find final z coord
		destX = cc.transform.position.x + hexRadius * (float) Math.Sin((float)
			facing * 60 * Mathf.Deg2Rad);
	}

	public void SnapPos()
	{
		Vector3 pos = new Vector3(destX, cc.transform.position.y, destZ);
		cc.transform.position = pos;
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Debug.Log("hit");

		mazeLogic.Hit(hit.transform.parent.gameObject);
	}

}
