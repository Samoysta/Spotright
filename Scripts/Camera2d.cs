using Godot;
using System;

public partial class Camera2d : Camera2D
{
    [Export] public float ShakeStrength = 0f;
    [Export] public float ShakeFade = 5f;
    [Export] public float NoiseSpeed = 20f;

    private FastNoiseLite noise;
    private float time = 0f;

    private Vector2 originalOffset;

    // directional shake
    private Vector2 direction = Vector2.Zero;

    public override void _Ready()
    {
        originalOffset = Offset;

        noise = new FastNoiseLite();
        noise.Seed = (int)GD.Randi();
        noise.Frequency = 1.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ShakeStrength > 0.01f)
        {
            time += (float)delta * NoiseSpeed;

            ShakeStrength = Mathf.Lerp(ShakeStrength, 0, ShakeFade * (float)delta);

            float x = noise.GetNoise2D(time, 0);
            float y = noise.GetNoise2D(0, time);

            Vector2 shake = new Vector2(x, y) * ShakeStrength;

            // 🔥 directional ekleme
            shake += direction * ShakeStrength;

            Offset = originalOffset + shake;
        }
        else
        {
            ShakeStrength = 0f;
            direction = Vector2.Zero;
            Offset = originalOffset;
        }
    }

    // 🔥 normal shake
    public void Shake(float strength)
    {
        ShakeStrength = strength;
        direction = Vector2.Zero;
    }

    // 🔥 directional shake (mesela vurulduğun yön)
    public void ShakeDirectional(float strength, Vector2 dir)
    {
        ShakeStrength = strength;
        direction = dir.Normalized();
    }
}