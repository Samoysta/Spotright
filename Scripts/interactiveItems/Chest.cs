using Godot;
using System;
using System.Collections.Generic;

public partial class Chest : StaticBody2D
{
	[Export] int id;
	[Export] int coinAmount;
	[Export] Camera2d cam;
	PlayerData pd;
	bool opened;
	[Export] CpuParticles2D imEf2;
	[Export] float Force;
	[Export] float SpeedCoin;
	[Export] AnimationPlayer anim;
	[Export] PackedScene Coin;
	[Export] AnimatedSprite2D ImpactEf;
	Character character;
	RandomNumberGenerator rnd = new();
	bool canOpen;
	Coin[] coins;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		coins = new Coin[coinAmount];
		rnd.Randomize();
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.LockedChests.ContainsKey(id))
		{
			if (pd.LockedChests[id] == "opened")
			{
				opened = true;
				canOpen = false;
				anim.Play("Opened");
			}
		}
		if (!opened)
		{
			for (int i = 0; i < coinAmount; i++)
			{
				Coin co = (Coin)Coin.Instantiate();
				co.GlobalPosition = GlobalPosition;
				GetTree().CurrentScene.CallDeferred("add_child",co);
				co.Visible = false;
				co.ProcessMode = ProcessModeEnum.Disabled;
				coins[i] = co;
			}	
		}
		ImpactEf.Frame = 8;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (canOpen && !opened)
		{
			if (character.isDashing)
			{
				ImpactEf.Play("Impact");
				ImpactEf.Frame = 0;
				ImpactEf.GlobalPosition = character.GlobalPosition;
				imEf2.Emitting = true;
				ApplyForce();
				anim.Play("Open");
				opened = true;
				pd.LockedChests.Add(id,"opened");
				CallDeferred("SpawnCoin");
				cam.Shake(20f);
			}
		}
	}

	public void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			character = (Character)body;
			canOpen = true;
		}
	}

	public void BodyExited2D(Node2D body)
	{
		if (body is Character)
		{
			canOpen = false;
		}
	}

	public void ApplyForce()
	{
		character.isDashing = false;
		int dir = Mathf.Sign(character.GlobalPosition.X - GlobalPosition.X);
		character.AddForce(new Vector2(dir * Force, -Force));
	}

	public void SpawnCoin()
	{
		for (int i = 0; i < coinAmount; i++)
		{
			float degree = rnd.RandfRange(75,105);
			Coin co = coins[i];
			co.Visible = true;
			co.ProcessMode = ProcessModeEnum.Always;
			co.character = character;
			co.AddForce(new Vector2(SpeedCoin,0).Rotated(degree));
		}
	}
}
