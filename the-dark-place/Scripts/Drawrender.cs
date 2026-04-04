using Godot;
using System;
using Godot.Collections;

public partial class Drawrender : Node2D
{
    private Array<float> _distances = new Array<float>();
    
    [Export] public float MaxDistance = 500f; 
    [Export] public float WallHeightMultiplier = 20000f;
    [Export] public Texture2D WallTexture; // 인스펙터에서 벽 이미지(예: 벽돌)를 넣어주세요.

    public override void _Ready()
    {
        var raycast2D = GetNode<RayCastRendering>("%RayCast2D");
        raycast2D.DrawRender += Rendering;
    }

    public void Rendering(Array<float> distances)
    {
        _distances = distances.Duplicate(); 
        QueueRedraw();
    }

    public override void _Draw()
    {
        // 텍스처가 없으면 기본 로직으로 그리거나 리턴합니다.
        if (_distances == null || _distances.Count == 0 || WallTexture == null) return;

        float screenWidth = GetViewportRect().Size.X;
        float screenHeight = GetViewportRect().Size.Y;
        float columnWidth = screenWidth / _distances.Count;
        float textureWidth = WallTexture.GetSize().X;
        float textureHeight = WallTexture.GetSize().Y;

        for (int i = 0; i < _distances.Count; i++)
        {
            float dist = _distances[i];
            float xPos = i * columnWidth;

            // 1. 벽의 높이 계산
            float lineHeight = WallHeightMultiplier / (dist + 0.1f);
            // 원근감을 위해 화면 높이보다 커질 수 있게 둡니다 (단, 너무 크면 성능 위해 제한 가능)
            
            // 2. 그릴 영역 설정 (화면상 어디에?)
            Rect2 destRect = new Rect2(xPos, (screenHeight - lineHeight) / 2, columnWidth + 1, lineHeight);

            // 3. 텍스처 샘플링 영역 설정 (이미지에서 어디를?)
            // i / _distances.Count는 0~1 사이의 값입니다. 
            // 이를 텍스처 가로 길이에 곱하면 텍스처를 전체 벽면에 한 번 펼친 모양이 됩니다.
            float sampleX = (float)i / _distances.Count * textureWidth;
            Rect2 srcRect = new Rect2(sampleX, 0, 1, textureHeight); 

            // 4. 거리 기반 어둡기(안개) 처리
            float brightness = 1.0f - Mathf.Clamp(dist / MaxDistance*3f, 0.0f, 1.0f);
            // 벽의 원래 색감을 유지하면서 거리만큼 어둡게 만듭니다.
            Color wallColor = new Color(brightness/2, brightness/2, brightness/2);

            // 5. 핵심: 텍스처의 특정 세로 줄만 그려서 벽을 형성
            DrawTextureRectRegion(WallTexture, destRect, srcRect, wallColor);
        }
    }
}