using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
	public LogWriter writer;

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

	private bool canTurn = false;
	private bool canMove = false;

	// Start is called before the first frame update
	void Start()
	{
		cc = GetComponent<CharacterController>();
		mazeLogic.SetMode(mode);
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
		else if(canMove && mode == 1 && !moving)
		{
			if(Input.GetKey(left) && !Input.GetKey(right))
			{
				LeftChoice(false);
			}
			else if(Input.GetKey(right) && !Input.GetKey(left))
			{
				RightChoice(false);
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

		if(!moving)
		{
			if(canTurn && Input.GetAxis("Horizontal") > 0.1f && cooldown <= 0)
			{
				rotate += rotateInterval * Vector3.up;
				cooldown = rotateCooldown;
			}
			else if(canTurn && Input.GetAxis("Horizontal") < -0.1f
				&& cooldown <= 0)
			{
				rotate -= rotateInterval * Vector3.up;
				cooldown = rotateCooldown;
			}
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
					else
						writer.WriteChoiceStart(0);
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
					else
						writer.WriteChoiceStart(0);
				}
			}
			else { Debug.LogError("Invalid facing: " + facing); }
		}

		// Calculate lower to ground
		if(!cc.isGrounded)
			move += Time.deltaTime * sinkSpeed * Vector3.down;

		if(canMove)
		{
			// Apply character translation
			cc.Move(move);
		}

		if(canTurn)
		{
			// Apply character rotation
			cc.transform.Rotate(rotate);
		}
	}

	public void LeftChoice(bool auto)
	{
		if(mazeLogic.leftFacing == -1)
			return;
		facing = mazeLogic.leftFacing;
		mazeLogic.LeftChoice(auto);
		mazeLogic.lastFacing = facing;
		StartMove();
	}

	public void RightChoice(bool auto)
	{
		if(mazeLogic.rightFacing == -1)
			return;
		facing = mazeLogic.rightFacing;
		mazeLogic.RightChoice(auto);
		mazeLogic.lastFacing = facing;
		StartMove();
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
		if(mode == 0)
			writer.WriteAction(facing * 60f);

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

	public void SnapRot()
	{
		int rotation = Mathf.RoundToInt(cc.transform.eulerAngles.y
			/ rotateInterval);
		rotation *= (int) rotateInterval;
		cc.transform.eulerAngles = new Vector3(cc.transform.eulerAngles.x,
			(float) rotation, cc.transform.eulerAngles.z);
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Debug.Log("hit");

		mazeLogic.Hit(hit.transform.parent.gameObject);
	}

	// Move player to new starting hex
	public void StartTrial(float xPos, float zPos)
	{
		Vector3 pos = new Vector3(xPos, cc.transform.position.y, zPos);
		cc.Move(pos - cc.transform.position);
		//Debug.Log("teleporting");
	}

	public Vector3 GetPosition()
	{
		return cc.transform.position;
	}

	public float GetFacing()
	{
		return cc.transform.eulerAngles.y;
	}

	public void SetFacing(float angle)
	{
		cc.transform.eulerAngles = new Vector3(cc.transform.eulerAngles.x,
			angle, cc.transform.eulerAngles.z);
	}

	public void SetCanMove(bool _canMove)
	{
		canMove = _canMove;
	}

	public void SetCanTurn(bool _canTurn)
	{
		canTurn = _canTurn;
	}

	/*
	public bool GetMoving()
	{
		return moving;
	}
	*/

}
