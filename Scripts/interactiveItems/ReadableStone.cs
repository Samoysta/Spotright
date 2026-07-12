using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class ReadableStone : Area2D
{
    [Export] Character character;
    [Export] Node2D readBox;
    Tween t;
    bool characterEntered;
    bool canRead;
    [Export] string[] Texts;
    [Export] float[] textSpeeds;
    RichTextLabel text;
    [Export] float textSpeed;
    bool textingStarted;
    int currentTextId;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        character.dialogAnim.AnimationFinished += (animName) => {AnimFinished(animName);};  
        text = character.dialogText;
        text.VisibleRatio = 0;
        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i] = Texts[i].Replace("\\n","\n");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (characterEntered)
        {
            if (character.Velocity.Y == 0)
            {
                if (!canRead)
                {
                    canRead = true;
                    AskAnimStart();
                }
            }
            else
            {
                if (canRead)
                {
                    canRead = false;
                    AskAnimEnd();
                }
            }
        }
        else
        {
            if (canRead)
            {
                canRead = false;
                AskAnimEnd();
            }
        }

        if (canRead)
        {
            if (Input.IsActionJustPressed("Down") && !character.cantInput)
            {
                character.cantInput = true;
                character.velocity = Vector2.Zero;
                character.Velocity = Vector2.Zero;
                character.characterSprite.Play("Idle");
                AskAnimEnd();
                character.dialogAnim.Play("Opening");
            }
        }

        if (textingStarted)
        {
            float i = textSpeeds[currentTextId];
            int visibleLength = Regex.Replace(Texts[currentTextId], @"\[.*?\]", "").Length;
            text.VisibleRatio += Mathf.Clamp(i * (float)delta * textSpeed * (1f / visibleLength),0,1);
            if (text.VisibleCharacters > 0)
            {
                if (Texts[currentTextId].ElementAt(text.VisibleCharacters-1).ToString() == " ")
                {
                    text.VisibleCharacters++;
                }   
            }
            if (Input.IsActionJustPressed("Z"))
            {
                if (text.VisibleRatio < 1)
                {
                    text.VisibleRatio = 1;
                }
                else
                {
                    if (Texts.Length > currentTextId + 1)
                    {
                        currentTextId ++;
                        text.VisibleRatio = 0;
                        text.Text = Texts[currentTextId];     
                    }
                    else
                    {
                        character.dialogAnim.Play("Closing");
                    }
                }
            }
        }
    }

    void AnimFinished(string animName)
    {
        if (animName == "Opening")
        {
            textingStarted = true;   
            text.VisibleRatio = 0;
            text.Text = Texts[0];
            currentTextId = 0;
        }
        else if(animName == "Closing")
        {
            textingStarted = false;
            character.cantInput = false;
            text.Text = "";
            AskAnimStart();
        }
    }

    void AskAnimStart()
    {
        t?.Kill();
        t = CreateTween();
        t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        t.TweenProperty(readBox, "scale", new Vector2(1, 1), 0.8f);
    }

    void AskAnimEnd()
    {
        t?.Kill();
        t = CreateTween();
        t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        t.TweenProperty(readBox, "scale", new Vector2(0, 0), 0.5f);
    }

    public void BodyEntered2D(Node2D body)
    {
        if (body is Character)
        {
            characterEntered = true;
        }
    }

    public void BodyExited2D(Node2D body)
    {
        if (body is Character)
        {
            characterEntered = false;
        }
    }
}
