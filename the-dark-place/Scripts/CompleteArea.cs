using Godot;
using System;

public partial class CompleteArea : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += ifInComplete;
	}
	private void ifInComplete(Node2D body)
	{
		if(body is Player)
		{
			GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/complete.tscn");
			return;
		}
	}
	
}
