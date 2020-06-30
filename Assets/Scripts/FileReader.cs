using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReader : MonoBehaviour
{

	public SimpleMovement move;
	public MazeLogic maze;
	public LogWriter writer;
	//public LogReader logReader;
	//public EyeDots eyeDots;

	private int mode;
	private int numTrials;

	private string partStr;
	private string runStr;
	private string startDelayStr;
	private string endDelayStr;
	private string grayTimeStr;
	private string warnTimeStr;
	private string choiceTimeStr;
	private string timeLimitStr;

	private string travelTimeStr;
	private string rotRateStr;
	private string fovStr;

	private string[] betasArr;
	private string[][] betasArr2d;

	private string startHexStr;
	private string[] startHexArr;

	private string goalHexStr;

	// Awake is called when the script instance is being loaded
	void Awake()
	{
		try
		{
			StreamReader reader = new StreamReader("Config.txt");

			partStr = reader.ReadLine();
			runStr = reader.ReadLine();
			mode = int.Parse(reader.ReadLine());
			startDelayStr = reader.ReadLine();
			endDelayStr = reader.ReadLine();
			grayTimeStr = reader.ReadLine();
			warnTimeStr = reader.ReadLine();
			choiceTimeStr = reader.ReadLine();
			timeLimitStr = reader.ReadLine();

			travelTimeStr = reader.ReadLine();
			rotRateStr = reader.ReadLine();
			fovStr = reader.ReadLine();

			numTrials = int.Parse(reader.ReadLine());
			betasArr = new string[numTrials];

			startHexStr = reader.ReadLine();

			reader.Close();
		}
		catch(Exception e)
		{
			Debug.LogError("Error reading file!");
			Debug.LogError(e);
			Application.Quit();
		}
	}

	void Start()
	{
		try
		{
			/*
			writer.mode = mode;
			//logReader.mode = mode;
			writer.partCode = partStr;
			//logReader.partCode = partStr;
			writer.runNum = int.Parse(runStr);
			//logReader.runNum = writer.runNum;
			*/
			move.mode = mode;
		}
		catch(Exception e)
		{
			Debug.LogError("Error parsing file (1)!");
			Debug.LogError(e);
			Application.Quit();
		}

		/*
		if(mode == 3)
		{
			logReader.enabled = true;
		}
		else
		{
			writer.enabled = true;
		}
		*/

		try
		{
			move.moveSpeed = 10f / float.Parse(travelTimeStr);

			foreach(Camera c in move.gameObject.GetComponentsInChildren
				<Camera>())
			{
				c.fieldOfView = float.Parse(fovStr);
			}

			//Debug.Log("Halfway done parsing!");

			/*
			contextArr = contextStr.Split(' ');
			holdArr = holdStr.Split(' ');
			leftObjArr = leftObjStr.Split(' ');
			leftRewArr = leftRewStr.Split(' ');
			rightObjArr = rightObjStr.Split(' ');
			rightRewArr = rightRewStr.Split(' ');
			postHitArr = postHitStr.Split(' ');

			demon.contexts = new int[contextArr.Length];
			demon.holds = new float[holdArr.Length];
			demon.leftObjects = new int[leftObjArr.Length];
			demon.leftRewards = new int[leftRewArr.Length];
			demon.rightObjects = new int[rightObjArr.Length];
			demon.rightRewards = new int[rightRewArr.Length];
			demon.postHits = new float[postHitArr.Length];

			for(int i = 0; i < contextArr.Length; i++)
			{
				demon.contexts[i] = int.Parse(contextArr[i]);
			}
			for(int i = 0; i < holdArr.Length; i++)
			{
				demon.holds[i] = float.Parse(holdArr[i]);
			}
			for(int i = 0; i < leftObjArr.Length; i++)
			{
				demon.leftObjects[i] = int.Parse(leftObjArr[i]);
			}
			for(int i = 0; i < leftRewArr.Length; i++)
			{
				demon.leftRewards[i] = int.Parse(leftRewArr[i]);
			}
			for(int i = 0; i < rightObjArr.Length; i++)
			{
				demon.rightObjects[i] = int.Parse(rightObjArr[i]);
			}
			for(int i = 0; i < rightRewArr.Length; i++)
			{
				demon.rightRewards[i] = int.Parse(rightRewArr[i]);
			}
			for(int i = 0; i < postHitArr.Length; i++)
			{
				demon.postHits[i] = int.Parse(postHitArr[i]);
			}
			*/
		}
		catch(Exception e)
		{
			Debug.LogError("Error parsing file (2)!!");
			Debug.LogError(e);
			Application.Quit();
		}
	}

}
