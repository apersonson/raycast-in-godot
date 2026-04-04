using Godot;
using System;
using Godot.Collections;

public partial class RayCastRendering : RayCast2D
{
    [Signal]
    public delegate void DrawRenderEventHandler(Array<float> distances);
    
    public Array<float> distances = new Array<float>();
    private CharacterBody2D player;
    [Export] TileMapLayer walls;
    
    // 시야각 설정 (5도 간격은 너무 듬성듬성하므로 1도 이하를 추천합니다)
    private float rayStep = Mathf.DegToRad(1f); 

    public override void _Ready()
    {
        player = GetNode<CharacterBody2D>("%Player");
        
        // 레이캐스트가 플레이어 자신과 충돌하지 않도록 예외 처리
        AddException(player); 
    }

    public override void _PhysicsProcess(double delta)
    {
        distances.Clear();
        
        // 플레이어의 현재 회전값
        float playerRot = player.GlobalRotation;

        float startfov = playerRot - Mathf.DegToRad(30);
        float endfov = playerRot + Mathf.DegToRad(30);
        
        for(float fovnow = startfov; fovnow <= endfov; fovnow += rayStep)
        {
            // 1. 레이캐스트 각도 변경
            GlobalRotation = fovnow + Mathf.Pi;
            
            // 2. 물리 엔진 강제 업데이트 (핵심!)
            ForceRaycastUpdate();
            
            // 3. 충돌 검사
            if (IsColliding())
            {
                GodotObject WhatHit = GetCollider();
                
                if (WhatHit == walls)
                {
                    Vector2 collisionPoint = GetCollisionPoint();
                    float distance = GlobalPosition.DistanceTo(collisionPoint);
                    
                    // 어안 렌즈 왜곡 보정
                    float correctedDistance = distance * Mathf.Cos(fovnow - playerRot);
                    distances.Add(correctedDistance);
                }
                else
                {
                    // 벽이 아닌 다른 것에 부딪혔을 때 (무한대처럼 보이게 큰 값)
                    distances.Add(10000f); 
                }
            }
            else
            {
                // 아무것도 부딪히지 않았을 때 (무한대처럼 보이게 큰 값)
                distances.Add(10000f); 
            }
        }
        
        // 시그널 전송
        EmitSignal(SignalName.DrawRender, distances);
        
        // 각도 원상복구
        GlobalRotation = player.GlobalRotation;
    }
}