using Godot;
using System;
using System.ComponentModel;
using System.Linq;

public partial class Sarmasik : Node2D
{
	[Export] float Speed;
	[Export] float segmentAmount;
	[Export] float length;
	[Export] float egim;
	[Export] float dalgaYogunlugu;
	[Export] Line2D line;
	float t;
	float b;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		line.ClearPoints();
		b = length / (segmentAmount - 1);
		for (int i = 0; i < segmentAmount; i++)
		{
			float a = i / ((segmentAmount - 1) / 5f);
			a = Mathf.Clamp(a,0,1);
			line.AddPoint(new Vector2(a * egim * Mathf.Sin(i * dalgaYogunlugu), i * b));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		t += Speed * (float)delta;
		b = length / (segmentAmount - 1);
		for (int i = 0; i < segmentAmount; i++)
		{
			float a = i / ((segmentAmount - 1) / 5f);
			a = Mathf.Clamp(a,0,1);
			line.SetPointPosition(i,new Vector2(a * egim * Mathf.Sin((i + t)* dalgaYogunlugu), i * b));
		}
	}
}
