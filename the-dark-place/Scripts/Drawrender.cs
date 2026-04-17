using Godot;
using System;
using Godot.Collections;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

public partial class Drawrender : Node2D
{
    private Array<float> _distances = new Array<float>();
     public Array<Hittedthis> _whathitteds = new Array<Hittedthis>();  
      public Array<float> _offsets = new Array<float>();
    [Export] public float MaxDistance = 500f; 
    [Export] public float WallHeightMultiplier = 20000f;
    [Export] public Texture2D WallTexture; // 인스펙터에서 벽 이미지(예: 벽돌)를 넣어주세요.
    [Export] public Texture2D CompleteZoneTexture;
    public override void _Ready()
    {
        var raycast2D = GetNode<RayCastRendering>("%RayCast2D");
        raycast2D.DrawRender += Rendering;
    }

   public void Rendering(Array<float> distances, Array<Hittedthis> whathitteds, Array<float> textureOffsets)
{
    _distances = distances.Duplicate(); 
    _whathitteds = whathitteds.Duplicate();
    _offsets = textureOffsets.Duplicate(); // 이 데이터를 저장해서 사용하세요
    QueueRedraw();
}
    public override void _Draw()
    {
        // 텍스처가 없으면 기본 로직으로 그리거나 리턴합니다.
        if (_distances == null || _distances.Count == 0 || WallTexture == null) return;

        float screenWidth = GetViewportRect().Size.X;
        float screenHeight = GetViewportRect().Size.Y;
        float columnWidth = screenWidth / _distances.Count;
for (int i = 0; i < _distances.Count; i++)
    {
        float dist = _distances[i];
        if (dist > MaxDistance * 2) continue; // 너무 멀면 생략

        Hittedthis whathitCheck = _whathitteds[i];
        float offset = _offsets[i]; // RayCast에서 계산해준 0~1 값
        
        Texture2D currentTex = (whathitCheck == Hittedthis.CompleteArea) ? CompleteZoneTexture : WallTexture; //나중에 더 늘어나면 삼향 말고 인프로 바꾸기
        if (currentTex == null) continue;
    float texWidth = currentTex.GetSize().X;
        float texHeight = currentTex.GetSize().Y;
        // 핵심: 전체 화면 비율이 아니라, 실제 부딪힌 지점의 텍스처 픽셀 계산
        float sampleX = offset * texWidth;

        // 화면에 그려질 세로 줄 영역
        float lineHeight = WallHeightMultiplier / (dist + 0.1f);
        Rect2 destRect = new Rect2(i * columnWidth, (screenHeight - lineHeight) / 2, columnWidth + 1, lineHeight);

        // 텍스처에서 잘라올 세로 줄 영역 (1픽셀 너비)
        Rect2 srcRect = new Rect2(sampleX, 0, 1, texHeight); 

        // 색상 및 안개 처리
        float brightness = 1.0f - Mathf.Clamp(dist / MaxDistance, 0.0f, 1.0f);
        Color wallColor = new Color(brightness/2, brightness/2, brightness/2);

        DrawTextureRectRegion(currentTex, destRect, srcRect, wallColor);
    }
    }
}