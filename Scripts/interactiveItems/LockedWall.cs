using Godot;
using System;

public partial class LockedWall : Node2D
{
	[Export] int id;
	[Export] Node2D wallUp;
	[Export] Node2D wallUpTargetPos;
	[Export] Node2D wallDown;
	[Export] Node2D wallDownTargetPos;
	[Export] float Speed;
	[Export] Node2D LockKey;
	[Export] Sprite2D up;
	[Export] Sprite2D down;
	[Export] float keySpeed;
	bool selected;
	bool opened;
	Tween t;
	Tween t2;
	PlayerData pd;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.LockedDoors.Count > 0)
		{
			if (pd.LockedDoors.ContainsKey(id))
			{
				if (pd.LockedDoors[id] == "opened")
				{
					up.Visible = true;
					down.Visible = true;
					selected = false;
					LockKey.Visible = false;
					opened = true;
					wallDown.Position = wallDownTargetPos.Position;
					wallUp.Position = wallUpTargetPos.Position;
				}
			}	
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (selected && !opened)
		{
			if (LockKey.GlobalPosition.DistanceTo(GlobalPosition) < 1)
			{
				OpenDoor();
			}
		}
		
	}

	public void BodyEntered2D(Node2D body)
	{
		if (body is Character && !opened && !selected)
		{
			selected = true;
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
			t.TweenProperty(LockKey, "global_position", GlobalPosition, keySpeed);
		}
	}
	public void OpenDoor()
	{
		up.Visible = true;
		down.Visible = true;
		selected = false;
		LockKey.Visible = false;
		opened = true;
		t?.Kill();
		t = CreateTween();
		t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		t.TweenProperty(wallUp, "position", wallUpTargetPos.Position, Speed);
		t2?.Kill();
		t2 = CreateTween();
		t2.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		t2.TweenProperty(wallDown, "position", wallDownTargetPos.Position, Speed);
		pd.LockedDoors.Add(id,"opened");
	}
}
