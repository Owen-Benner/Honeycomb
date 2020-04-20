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
	private int facing;
	private float destX;
	private float destZ;

	public float hexRadius;

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

		if(Input.GetAxis("Vertical") > 0.1f && !moving)
		{
			StartMove();
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

		// Determine rotation
		rotate += Time.deltaTime * Input.GetAxis("Horizontal") * rotSpeed * 
			Vector3.up;

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

		// Find movement angle
		facing = (int) ((480 - cc.transform.eulerAngles.y) / 60) % 6;

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

}
