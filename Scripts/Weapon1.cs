using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;

public partial class Weapon1 : Area2D
{
	[Export] Character character;
	[Export] int itemId;
	bool selected;
	public bool canShoot;
	bool canSelect;
	[Export] bool Golden;
	[Export] bool Demon;
	[Export] Texture2D blueWeapon;
	[Export] Texture2D goldenWeapon;
	[Export] Texture2D demonWeapon;
	[Export] Camera2d cam;
	[Export] int bulDeg;
	[Export] Vector2 weaponMaxPos;
	[Export] float shootCoolDown;
	[Export] PackedScene bul1;
	[Export] Node2D bulletPos;
	[Export] Node2D effectPos;
	[Export] AnimationPlayer anim2;
	CollisionShape2D col;
	PlayerData pd;
	float shootcd;
	Sprite2D gunSprite;
	[Export] PackedScene fireEf;
	[Export] RayCast2D ray;
	[Export] AudioStreamPlayer2D shootAudio;
	[Export] CpuParticles2D demonEf;
	[Export] PackedScene demonFireEf;
	[Export] Node2D demonEfPos;
	public Queue<Effect> fireEfs = new();
	public Queue<Effect> fireEfsDemon = new();
	Tween t;
	Vector2 pos;
	int damageKati;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		gunSprite = GetNode<Sprite2D>("Sprite2D");
		Golden = pd.weaponUpgradeNo == 2;
		Demon = pd.weaponUpgradeNo == 3;
		
		canSelect = true;
		if (!Demon)
		{
			for (int i = 0; i < 6; i++)
			{
				Effect ef = (Effect)fireEf.Instantiate();
				GetTree().CurrentScene.CallDeferred("add_child",ef);
				fireEfs.Enqueue(ef);
			}	
		}
		else
		{
			for (int i = 0; i < 6; i++)
			{
				Effect ef = (Effect)demonFireEf.Instantiate();
				GetTree().CurrentScene.CallDeferred("add_child",ef);
				fireEfsDemon.Enqueue(ef);
			}	
		}
		col = GetNode<CollisionShape2D>("CollisionShape2D");
		cam = character.camera;
		if (pd.currentAbilityid == itemId)
		{
			open();
		}
		else
		{
			Scale = Vector2.Zero;
		}
		Position = weaponMaxPos * Mathf.Sign(pd.lastDir) * Vector2.Right;
		gunSprite.FlipV = pd.lastDir < 0;
		if (pd.lastDir < 0)
		{
			GlobalRotationDegrees = 180f;	
		}
		pos = Position;
		ray.TargetPosition = pos;
		if (Golden)
		{
			pd.weaponDamageKat = 2;
			gunSprite.Texture = goldenWeapon;
		}
		else if (Demon)
		{
			pd.weaponDamageKat = 3;
			gunSprite.Texture = demonWeapon;
		}
		else
		{
			gunSprite.Texture = blueWeapon;
			pd.weaponDamageKat = 1;
		}
	}

	public void Init(Character player)
	{
		character = player;
		cam = character.camera;
		fireEfs.Clear();
		for (int i = 0; i < 6; i++)
		{
			Effect ef = (Effect)fireEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child",ef);
			fireEfs.Enqueue(ef);
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
		if (selected)
		{
			if (shootcd > 0)
			{
				shootcd -= (float)delta;
			}
			if (!character.cantInput)
			{
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
				Vector2 scale = character.characterSprite.Scale;
				if (scale.X > 0)
				{
					pos = new Vector2(weaponMaxPos.X, 0);
					gunSprite.FlipV = (character.GlobalPosition.X - GlobalPosition.X) * pos.X > 0;

				}
				else
				{
					pos = new Vector2(-weaponMaxPos.X, 0);
					gunSprite.FlipV = (character.GlobalPosition.X - GlobalPosition.X) * pos.X < 0;
				}

				if (Input.IsActionJustPressed("X") && shootcd <= 0 && canShoot && !character.swordAnim.IsPlaying())
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
					shootcd = shootCoolDown;
					Fire(0);
					if (Demon)
					{
						fireEffectDemon();
					}
					else
					{
						fireEffect();	
					}
					anim2.Play("Fire");
					anim2.Seek(0);
					canShoot = false;
					shootAudio.Stop();
            		shootAudio.Play();
				}	
			}
		}
		
	}


	public void close()
	{
		if (Demon)
		{
			demonEf.Emitting = false;	
		}
		selected = false;
		t?.Kill();
		t = CreateTween();
		t.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		t.TweenProperty(this, "scale", Vector2.Zero, 0.2f);
	}

	public void open()
	{
		Visible = true;
		t?.Kill();
		t = CreateTween();
		t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		t.TweenProperty(this, "scale", new Vector2(1,1), 0.2f).Finished += () =>
		{
			if (Demon)
			{
				demonEf.Emitting = true;	
			}
		};
		selected = true;
		shootcd = shootCoolDown;
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
	void fireEffectDemon()
	{
		Effect ef = fireEfsDemon.Dequeue();
		ef.GlobalPosition = demonEfPos.GlobalPosition;
		ef.GlobalRotation = GlobalRotation;
		ef.setOn();
		fireEfsDemon.Enqueue(ef);
	}
}
