using Godot;
using System;

public partial class Weapon1 : Area2D
{
	Character character;
	bool selected;
	[Export] Camera2d cam;
	[Export] int ShootAmount;
	[Export] Vector2 weaponMaxPos;
	[Export] float shootCoolDown;
	[Export] PackedScene bul1;
	[Export] Node2D bulletPos;
	float shootcd;
	Vector2 pos;
	Sprite2D gunSprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gunSprite = GetNode<Sprite2D>("Sprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (selected)
		{
			if (shootcd > 0)
			{
				shootcd -= (float)delta;
			}
			Vector2 scale = character.characterSprite.Scale;
			if (scale.X > 0)
			{
				pos = new Vector2(weaponMaxPos.X, 0);
				gunSprite.FlipV = false;
			}
			else
			{
				pos = new Vector2(-weaponMaxPos.X, 0);
				gunSprite.FlipV = true;
			}
			if (Input.IsActionPressed("Up"))
			{
				pos = new Vector2(0,-weaponMaxPos.Y);
			}
			if (Input.IsActionPressed("Down"))
			{
				pos = new Vector2(0, weaponMaxPos.Y);
			}
			if (Input.IsActionPressed("Right"))
			{
				if (Input.IsActionPressed("Up"))
				{
					pos = new Vector2(weaponMaxPos.X,-weaponMaxPos.Y);
				}
				else if (Input.IsActionPressed("Down"))
				{
					pos = new Vector2(weaponMaxPos.X,weaponMaxPos.Y);
				}
				else
				{
					pos = new Vector2(weaponMaxPos.X,0);
				}
			}
			else if (Input.IsActionPressed("Left"))
			{
				if (Input.IsActionPressed("Up"))
				{
					pos = new Vector2(-weaponMaxPos.X,-weaponMaxPos.Y);
				}
				else if (Input.IsActionPressed("Down"))
				{
					pos = new Vector2(-weaponMaxPos.X,weaponMaxPos.Y);
				}
				else
				{
					pos = new Vector2(-weaponMaxPos.X,0);
				}
			}
			Position = Position.Lerp(pos,10 * (float)delta);
			LookAt(GlobalPosition + Position);

			if (Input.IsActionJustPressed("X") && ShootAmount > 0 && shootcd <= 0)
			{
				if (!character.IsOnFloor())
				{
					character.AddForce(new Vector2(-800,0).Rotated(GlobalRotation));
				}
				cam.Shake(5);
				ShootAmount--;
				shootcd = shootCoolDown;
				for (int i = -1; i < 2; i++)
				{
					Fire(i);
				}
			}
			if (ShootAmount <= 0 || !character.canDie)
			{
				QueueFree();
			}
		}
		
	}

	void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			character = (Character)body;
			selected = true;
			CallDeferred("reparent", character);
		}
	}

	void Fire(int index)
	{
		if (character.bul1s.Count > 0)
		{
			Wepaon1Bullet bul = character.bul1s.Dequeue();
			bul.GlobalPosition = bulletPos.GlobalPosition;
			bul.GlobalRotationDegrees = GlobalRotationDegrees + (index * 15f);
			bul.setOn();
		}
		else
		{
			Wepaon1Bullet bul = (Wepaon1Bullet)bul1.Instantiate();
			bul.GlobalPosition = bulletPos.GlobalPosition;
			bul.GlobalRotationDegrees = GlobalRotationDegrees + (index * 15f);
			GetTree().CurrentScene.AddChild(bul);
			bul.Init(character);
			bul.setOn();
		}
	}
}
