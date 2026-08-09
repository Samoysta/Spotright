using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerData : Node
{
	//||------------------------------Kaydedilmeyecekler-------------------------||

	public int doorID = 0;
	public float energyAmount;
	public int lastDir = -1;
	public int health = 100;
	public Character character;
	public Node2D Items;
	public bool isDied;
	public int currentAbilityid;
	public Dictionary<int, string> killedEnemies = new();

	//||-----------------------------------Kaydedilecekler---------------------------------||

	public int coin;
	public int maxHealth = 500;
	public List<int> openedAbilityIds = new();
	public string savedScene = "test_scene";
	public Vector2 savedPos = Vector2.Zero;
	public int weaponDamage = 1;
	public int weaponDamageKat = 1;
	public int weaponUpgradeNo = 1;
	public Dictionary<int, string> LockedDoors = new();
	public Dictionary<int, string> LockedChests = new();
	public Dictionary<int, string> secretAreas = new();
	public List<string> talkedNpcs = new();
	public List<string> showedTutos = new();
	public List<string> takedHearts = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		health = maxHealth;
		openedAbilityIds.Add(0);
		openedAbilityIds.Add(1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
