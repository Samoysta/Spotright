using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerData : Node
{
	public int doorID = 0;
	public int lastDir = -1;
	public int health = 100;
	public int maxHealth = 100;
	public int weaponDamage = 1;
	public int weaponDamageKat = 1;
	public int weaponUpgradeNo = 1;
	public Character character;
	public Node2D Items;
	public string savedScene = "test_scene";
	public Vector2 savedPos = Vector2.Zero;
	public bool isDied;
	public int coin;
	public int currentAbilityid;
	public List<int> openedAbilityIds = new();
	public Dictionary<int, string> LockedDoors = new();
	public Dictionary<int, string> LockedChests = new();
	public Dictionary<int, string> killedEnemies = new();
	public Dictionary<int, string> secretAreas = new();
	public List<string> talkedNpcs = new();
	public List<string> showedTutos = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		openedAbilityIds.Add(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
