using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MoveSpeed = 100.0f;
	[Export] public float roteSpeed = 3;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
    public override void _PhysicsProcess(double delta)
    {
        float rotationDirection = Input.GetAxis("left", "right"); // 기울기 방향 인풋 받기

		Rotation += rotationDirection * roteSpeed * (float)delta; //인풋 방향으로 기울기 스피드 만큼 기울기

		float MoveDirection = Input.GetAxis("up","down"); // 앞, 뒤 받기

		Vector2 direction = Vector2.Up.Rotated(Rotation); //기울어진 쪽으로 가라의 값을 저장

		if (MoveDirection != 0) //인풋이 없어서 멈췄다면
        {
            // moveDirection이 -1(Up)이면 전진, 1(Down)이면 후진
            Velocity = direction * -MoveDirection * MoveSpeed; // -movespeed를 하는 이유는, 방향도 앞쪽을 바라봄. 마이너스 곱하기 마이너스는 플러스니깐 이리 해놈
        }
        else
        {
            // 키를 떼면 부드럽게 멈춤 (혹은 즉시 멈춤 Velocity = Vector2.Zero)
            Velocity = Velocity.MoveToward(Vector2.Zero, MoveSpeed);
        }

        //고도 엔진의 물리 이동 함수 호출
        MoveAndSlide();
    }
}
