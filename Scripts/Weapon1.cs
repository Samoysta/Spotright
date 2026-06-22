using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;

public partial class Weapon1 : Area2D
{
	Character character;
	bool selected;
	bool canShoot;
	bool canSelect;
	Vector2 firstPos;
	float spawnCoolDown;
	[Export] SceneManager sm;
	[Export] public int Id;
	[Export] public int roomId;
	[Export] bool Golden;
	[Export] Texture2D blueWeapon;
	[Export] Texture2D goldenWeapon;
	[Export] Camera2d cam;
	[Export] int bulDeg;
	[Export] Vector2 weaponMaxPos;
	[Export] float shootCoolDown;
	[Export] PackedScene bul1;
	[Export] Node2D bulletPos;
	[Export] Node2D effectPos;
	[Export] AnimationPlayer anim;
	[Export] AnimationPlayer anim2;
	CollisionShape2D col;
	PlayerData pd;
	float shootcd;
	Sprite2D gunSprite;
	[Export] PackedScene fireEf;
	[Export] RayCast2D ray;
	public Queue<Effect> fireEfs = new();
	Tween t;
	Vector2 pos;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		canShoot = true;
		pd = GetNode<PlayerData>("/root/PlayerData");
		gunSprite = GetNode<Sprite2D>("Sprite2D");
		if (Golden)
		{
			gunSprite.Texture = goldenWeapon;
		}
		else
		{
			gunSprite.Texture = blueWeapon;
		}
		canSelect = true;
		firstPos = GlobalPosition;
		
		for (int i = 0; i < 6; i++)
		{
			Effect ef = (Effect)fireEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child",ef);
			fireEfs.Enqueue(ef);
		}
		col = GetNode<CollisionShape2D>("CollisionShape2D");
		character = pd.character;
	}

	public void Init(Character player)
	{
		character = player;
		character.selected = true;
		cam = character.camera;
		fireEfs.Clear();
		for (int i = 0; i < 6; i++)
		{
			Effect ef = (Effect)fireEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child",ef);
			fireEfs.Enqueue(ef);
		}
		sm = character.sm;
		if (sm.weaponIds.Contains(Id))
		{
			int index = Array.IndexOf(sm.weaponIds,Id);
			sm.weapons[index].QueueFree();
			sm.weapons[index] = this;
		}

	}
    public override void _PhysicsProcess(double delta)
    {
		if (selected)
		{
			ray.GlobalPosition = character.GlobalPosition;	
		}
		else
		{
			ray.GlobalPosition = GlobalPosition;
		}
    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (character.IsOnFloor())
		{
			canShoot = true;
		}
		else if (character.isRightWalled || character.isLeftWalled)
		{
			if (character.Velocity.Y > character.wallSpeed)
			{
				canShoot = true;
			}
		}
		col.CallDeferred("set_disabled", character.selected);
		if (spawnCoolDown > 0)
		{
			spawnCoolDown -= (float)delta;
		}
		else
		{
			if (Scale.Length() < 0.01f)
			{
				open();
			}
		}
		if (selected)
		{
			if (shootcd > 0)
			{
				shootcd -= (float)delta;
			}
			if (!character.cantInput)
			{
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
				ray.TargetPosition = pos;
				Vector2 targetPos;
				if (ray.IsColliding())
				{
					targetPos = character.ToLocal(ray.GetCollisionPoint());
				}
				else
				{
					targetPos = pos;
				}
				Position = Position.Lerp(targetPos,10 * (float)delta);
				LookAt(GlobalPosition + Position);

				if (Input.IsActionJustPressed("X") && shootcd <= 0 && canShoot)
				{
					if (!character.IsOnFloor())
					{
						if (Golden)
						{
							character.AddForce(new Vector2(-800,0).Rotated(GlobalRotation) + character.velocity);
						}
						else
						{
							character.AddForce(new Vector2(-800,0).Rotated(GlobalRotation));	
						}
					}
					cam.Shake(8);
					shootcd = shootCoolDown;
					for (int i = -1; i < 2; i++)
					{
						Fire(i);
					}
					fireEffect();
					anim2.Play("Fire");
					anim2.Seek(0);
					canShoot = false;
				}	
			}
			if (!character.canDie)
			{
				close();
			}
		}
		
	}


	void close()
	{
		selected = false;
		spawnCoolDown = 5f;
		character.selected = false;
		CallDeferred("reparent", GetTree().CurrentScene);
		sm.gunLimitTileLayer.AnimPlay("Ending");
		t?.Kill();
		t = CreateTween();
		t.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		t.TweenProperty(this, "scale", Vector2.Zero, 0.5f).Finished += () =>
		{
			if (sm.roomId != roomId)
			{
				QueueFree();
			}
		};
	}

	void open()
	{
		anim.Play("RESET");
		GlobalPosition = firstPos;
		GlobalRotationDegrees = 0;
		gunSprite.FlipV = false;
		t?.Kill();
		t = CreateTween();
		t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
		t.TweenProperty(this, "scale", new Vector2(1,1), 0.8f);
		canSelect = true;
		col.CallDeferred("set_disabled", false);
	}

	void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			if (canSelect)
			{
				if (!character.selected)
				{
					anim.Play("queue");
					selected = true;
					sm.gunLimitTileLayer.AnimPlay("Starting");
					CallDeferred("reparent", character.Items);
					canSelect = false;
					character.selected = true;
					col.CallDeferred("set_disabled", true);
				}	
			}
		}
		else if (body.GetParent().GetParent() == sm.gunLimitTileLayer)
		{
			close();
		}
	}

	void Fire(int index)
	{
		if (character.bul1s.Count > 0)
		{
			Wepaon1Bullet bul = character.bul1s.Dequeue();
			bul.GlobalPosition = bulletPos.GlobalPosition;
			bul.GlobalRotationDegrees = GlobalRotationDegrees + (index * bulDeg);
			bul.setOn();
		}
		else
		{
			Wepaon1Bullet bul = (Wepaon1Bullet)bul1.Instantiate();
			bul.GlobalPosition = bulletPos.GlobalPosition;
			bul.GlobalRotationDegrees = GlobalRotationDegrees + (index * bulDeg);
			GetTree().CurrentScene.AddChild(bul);
			bul.Init(character);
			bul.setOn();
		}
	}

	void fireEffect()
	{
		Effect ef = fireEfs.Dequeue();
		ef.GlobalPosition = effectPos.GlobalPosition;
		ef.GlobalRotation = GlobalRotation;
		ef.setOn();
		fireEfs.Enqueue(ef);
	}
}
