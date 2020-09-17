using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapLogic : MonoBehaviour
{

	public GameObject redArrow;
	public GameObject greenArrow;
	public GameObject number;

	public MazeLogic maze;

	public string fileName;

    // Start is called before the first frame update
    void Start()
    {
        maze = gameObject.GetComponent<MazeLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void DrawMap()
	{
		StreamReader reader = new StreamReader("opb_maze_14.xml");
		Debug.Log("Drawing");

		try
		{
			GameObject hex;
			while(true)
			{
				Debug.Log("Loop 0");
				string line = reader.ReadLine();
				string [] lineArr = line.Split(' ');
				Debug.Log(lineArr[0]);
				if(lineArr[0] == "Trial")
				{
					string [] coords = lineArr[2].Split('-');
					Debug.Log(int.Parse(coords[0]) + "-"
						+ int.Parse(coords[1]));
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
					Debug.Log("Breaking");
					break;
				}
			}
			float angle = 0;
			while(true)
			{
				Debug.Log("Loop 1");
				string line = reader.ReadLine();
				string [] lineArr = line.Split(' ');
				Debug.Log(lineArr[0]);
				// TODO: Include auto-action.
				if(lineArr[0] == "Action")
				{
					if(lineArr[3] == "0")
					{
						Debug.Log("Red");
						GameObject arrow = Instantiate(redArrow,
							hex.transform);
						arrow.transform.position += 5 * Vector3.up;
						arrow.transform.eulerAngles += Vector3.up *
							(angle + float.Parse(lineArr[4]) - 60);
						arrow.SetActive(true);
					}
					else if(lineArr[3] == "1")
					{
						Debug.Log("Green");
						GameObject arrow = Instantiate(greenArrow,
							hex.transform);
						arrow.transform.position += 5 * Vector3.up;
						arrow.transform.eulerAngles += Vector3.up *
							(angle + float.Parse(lineArr[4]) - 60);
						arrow.SetActive(true);
					}
				}
				else if(lineArr[0] == "Start_Choice")
				{
					string [] coords = lineArr[2].Split('-');
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
				}
				// TODO: Make this better.
				else if(lineArr[0] == "Frame")
				{
					angle = float.Parse(lineArr[4]) + float.Parse(lineArr[8]);
				}
				else if(lineArr[0] == "Gray_Screen")
					break;
			}
		}
		catch(Exception e)
		{
			Debug.LogError("Error generating map!");
			Debug.LogError(e.ToString());
		}
	}

}
