using Godot;
using Godot.Collections;
using System;

public partial class PlayerData : Node
{
	public int doorID = 0;
	public int lastDir = -1;
	public Character character;
	public Node2D Items;
	public int coin;
	public SceneManager sm;
	public Dictionary<int, string> LockedDoors = new();
	public Dictionary<int, string> LockedChests = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
