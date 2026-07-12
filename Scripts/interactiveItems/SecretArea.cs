using Godot;
using System;
using System.Linq;

public partial class SecretArea : TileMapLayer
{
	[Export] AnimationPlayer anim;
	[Export] int id;
	PlayerData pd;
	bool opened;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.secretAreas.ContainsKey(id))
		{
			if (pd.secretAreas[id] == "opened")
			{
				opened = true;
				Visible = false;
				Modulate = new Color(1,1,1,0);
			}
		}
	}

	public void Close()
	{
		if (!opened)
		{
			if (!pd.secretAreas.ContainsKey(id))
			{
				pd.secretAreas.Add(id,"opened");	
			}
			anim.Play("Close");	
			opened = true;
		}
	}
}
