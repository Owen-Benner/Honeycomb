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
				string line = reader.ReadLine();
				string [] lineArr = line.Split(' ');
				if(lineArr[0] == "Trial")
				{
					string [] coords = lineArr[2].Split('-');
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
					break;
				}
			}
			while(true)
			{
				string line = reader.ReadLine();
				string [] lineArr = line.Split(' ');
				if(lineArr[0] == "Action")
				{
					if(lineArr[3] == "0")
					{
						GameObject arrow = Object.Instantiate(redArrow,
							hex.transform);
						arrow.transform.position += 10 * Vector3.up;
					}
					else if(lineArr[3] == "1")
					{
						GameObject arrow = Object.Instantiate(greenArrow,
							hex.transform);
						arrow.transform.position += 10 * Vector3.up;
					}
				}
				else if(lineArr[0] == "Start_Choice")
				{
					string [] coords = lineArr[2].Split('-');
					hex = maze.maze[int.Parse(coords[0]),
						int.Parse(coords[1])];
				}
				else if(lineArr[0] == "Gray_Screen")
					break;
			}
		}
		catch
		{
			Debug.LogError("Error generating map!");
		}
	}

}
