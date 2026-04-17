using Godot;
using System;
using Godot.Collections;

  [Flags]
    public enum Hittedthis
    {
        none = 1<<0, //0
        Enemy = 1<<1, //1
        Door = 1<<2, //2
        Wall = 1<<3, //4
        CompleteArea = 1<<4
    }
public partial class RayCastRendering : RayCast2D
{
   [Signal]
public delegate void DrawRenderEventHandler(Array<float> distances, Array<Hittedthis> whathitteds, Array<float> textureOffsets);
    public Array<float> distances = new Array<float>();
    public Array<Hittedthis> whathitteds = new Array<Hittedthis>();  
    private CharacterBody2D player;
    [Export] TileMapLayer walls;
    [Export] Area2D Complete;
    public Array<float> textureOffsets = new Array<float>(); // 새로 추가
    
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
    whathitteds.Clear();
    textureOffsets.Clear();

    float playerRot = player.GlobalRotation;
    float startfov = playerRot - Mathf.DegToRad(30);
    float endfov = playerRot + Mathf.DegToRad(30);

    // RayCast2D가 Area2D도 감지할 수 있도록 강제 설정 (코드에서 설정 가능)
    CollideWithAreas = true; 

    for (float fovnow = startfov; fovnow <= endfov; fovnow += rayStep)
    {
        GlobalRotation = fovnow + Mathf.Pi;
        ForceRaycastUpdate();

        if (IsColliding())
        {
            var whatHit = GetCollider();
            Vector2 collisionPoint = GetCollisionPoint();
            Vector2 normal = GetCollisionNormal();
            float dist = GlobalPosition.DistanceTo(collisionPoint);
            float correctedDistance = dist * Mathf.Cos(fovnow - playerRot);

            float hitLocation = 0f;
            float currentTileSize = 64.0f; // 기본 벽 타일 크기
            Hittedthis type = Hittedthis.none;

            if (whatHit == walls)
            {
                type = Hittedthis.Wall;
                currentTileSize = 64.0f; // 본인의 타일셋 크기에 맞게 수정
                hitLocation = (Mathf.Abs(normal.X) > 0.5f) ? collisionPoint.Y : collisionPoint.X;
            }
            else if (whatHit == Complete)
            {
               type = Hittedthis.CompleteArea;
            var shape = Complete.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D;
    
            Vector2 localHit = Complete.ToLocal(collisionPoint);
    
            // [-너비/2, +너비/2] 범위를 [0, 1] 범위로 변환하는 마법의 공식
            float ratio = (localHit.X / shape.Size.X) + 0.5f;
    
         distances.Add(correctedDistance);
         whathitteds.Add(type);
            // currentTileSize 연산 없이 바로 비율(offset)을 넣어줌
            textureOffsets.Add(Mathf.Clamp(ratio, 0.0f, 1.0f)); 
            continue; // 다음 루프로 점프
            }

            // 리스트에 데이터 추가 (이게 빠져있었음)
            distances.Add(correctedDistance);
            whathitteds.Add(type);
            textureOffsets.Add(Mathf.PosMod(hitLocation, currentTileSize) / currentTileSize);
        }
        else
        {
            distances.Add(10000f);
            whathitteds.Add(Hittedthis.none);
            textureOffsets.Add(0f);
        }
    }

    // 모든 레이 계산이 끝난 후 "한 번만" 시그널 전송
    EmitSignal(SignalName.DrawRender, distances, whathitteds, textureOffsets);
    GlobalRotation = player.GlobalRotation;
}
}

  
