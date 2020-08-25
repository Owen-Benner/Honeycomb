using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogWriter : MonoBehaviour
{

	public Transform player;
	public SimpleMovement move;
	public MazeLogic maze;

	public string fileName;

	public int mode;

	public int frameFreq = 24;

    StreamReader lastRunReader;
	StreamWriter writer;

	private float lastFrame;
	private float frameTime;
	private float runStart;
	private float trialStart;

	private string spc = " ";

	private int frame = 0;
	private int choiceNum;

	private bool writing = false;
	private bool start = true;
	private bool gray = false;

    // Start is called before the first frame update
    void Start()
    {
		frameTime = 1f / (float) frameFreq;
    }

    // Update is called once per frame
    void Update()
    {
        if(writing && Time.time - lastFrame >= frameTime)
        {
			if(start)
				WriteStartFrame();
			else if(gray)
				WriteGrayFrame();
			else
				WriteFrame();
            lastFrame += frameTime;
        }
    }

	public void InitWriter()
	{
        try
        {
            lastRunReader = new StreamReader(fileName);
            Debug.LogError("Repeated log filename! Update Config.txt or remove"
                    + " last log file from directory.");
            Application.Quit();
            return;
        }
        catch {}

        writer = new StreamWriter(fileName);
	}

	public void StartWriting()
	{
		writing = true;
		runStart = Time.time;
		WriteStartFrame();
		lastFrame = runStart;
	}

	private void WriteFrame()
	{
		if(mode == 1)
			writer.WriteLine("Frame " + (frame++).ToString() + ":" + spc
				+ string.Format("{0:N3}", player.position.x) + spc
				+ string.Format("{0:N3}", player.position.z) + spc
				+ string.Format("{0:N3}", player.eulerAngles.y) + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart) + spc
				+ string.Format("{0:N3}", maze.GetDistToGoal()) + spc
				+ string.Format("{0:N3}", maze.GetAngleToGoal()));
		else if(mode == 0)
			writer.WriteLine("Frame " + (frame++).ToString() + ":" + spc
				+ string.Format("{0:N3}", player.position.x) + spc
				+ string.Format("{0:N3}", player.position.z) + spc
				+ string.Format("{0:N3}", player.eulerAngles.y) + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart));
	}

	private void WriteGrayFrame()
	{
		writer.WriteLine("Frame " + (frame++).ToString() + ":" + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	private void WriteStartFrame()
	{
		writer.WriteLine("Frame " + (frame++).ToString() + ":" + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteTrialStart()
	{
		trialStart = Time.time;
		int trial = maze.trial;

		if(mode == 1)
			writer.WriteLine("Trial " + trial.ToString() + ":" + spc
				+ maze.startHexes[trial].GetComponent<HexLogic>()
				.column.ToString() + "-"
				+ maze.startHexes[trial].GetComponent<HexLogic>()
				.row.ToString() + spc
				+ (maze.betas[trial, 0] * 60).ToString() + spc
				+ string.Format("{0:N3}", Time.time - runStart));
		else if(mode == 0)
			writer.WriteLine("Trial " + trial.ToString() + ":" + spc
				+ maze.startHexes[trial].GetComponent<HexLogic>()
				.column.ToString() + "-"
				+ maze.startHexes[trial].GetComponent<HexLogic>()
				.row.ToString() + spc
				+ string.Format("{0:N3}", Time.time - runStart));

		choiceNum = 0;
	}

	public void WriteAction()
	{
		if(mode == 1)
			writer.WriteLine("Action " + maze.trial.ToString() + "."
				+ choiceNum++.ToString() + ":" + spc
				+ maze.lastChoice.ToString() + spc
				+ BoolToString(maze.lastCorrect) + spc
				+ string.Format("{0:N3}", maze.alpha) + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteAutoAction()
	{
		if(mode == 1)
			writer.WriteLine("Auto_Action " + maze.trial.ToString() + "."
				+ choiceNum++.ToString() + ":" + spc
				+ maze.lastChoice.ToString() + spc
				+ BoolToString(maze.lastCorrect) + spc
				+ string.Format("{0:N3}", maze.alpha) + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteAction(float dir)
	{
		if(mode == 0)
			writer.WriteLine("Action " + maze.trial.ToString() + "."
				+ choiceNum++.ToString() + ":" + spc
				+ dir.ToString() + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteChoiceStart(int beta)
	{
		int trial = maze.trial;

		if(mode == 1)
			writer.WriteLine("Start_Choice " + maze.trial.ToString() + "."
				+ choiceNum.ToString() + ":" + spc
				+ maze.curHex.GetComponent<HexLogic>().column.ToString() + "-"
				+ maze.curHex.GetComponent<HexLogic>().row.ToString() + spc
				+ (beta * 60).ToString() + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart));
		else if(mode == 0)
			writer.WriteLine("Start_Choice " + maze.trial.ToString() + "."
				+ choiceNum.ToString() + ":" + spc
				+ maze.curHex.GetComponent<HexLogic>().column.ToString() + "-"
				+ maze.curHex.GetComponent<HexLogic>().row.ToString() + spc
				+ string.Format("{0:N3}", Time.time - trialStart) + spc
				+ string.Format("{0:N3}", Time.time - runStart));
		else
			Debug.LogError("Invalid mode.");
	}

	public void WriteGoal()
	{
		int trial = maze.trial;
		writer.WriteLine("Goal " + trial.ToString() + ":" + spc
			+ maze.curHex.GetComponent<HexLogic>().column.ToString() + "-"
			+ maze.curHex.GetComponent<HexLogic>().row.ToString() + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteGrayScreen()
	{
		int trial = maze.trial;
		writer.WriteLine("Gray_Screen " + trial.ToString() + ":" + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteTimeout()
	{
		int trial = maze.trial;
		writer.WriteLine("Timeout:" + spc + trial.ToString() + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	public void WriteWarning()
	{
		int trial = maze.trial;
		writer.WriteLine("Warning " + trial.ToString() + "."
			+ choiceNum.ToString() + ":" + spc
			+ string.Format("{0:N3}", Time.time - trialStart) + spc
			+ string.Format("{0:N3}", Time.time - runStart));
	}

	private string BoolToString(bool b)
	{
		if(b)
			return "1";
		else
			return "0";
	}

	public void SetStart(bool _start)
	{
		start = _start;
	}

	public void SetGray(bool _gray)
	{
		gray = _gray;
	}

}
