using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float time; // 현재 시간 (0.0f ~ 1.0f 사이의 값, 하루: 1)
    public float fullDayLength; // 하루의 길이 (초 단위)
    public float startTime = 0.4f; // 시작 시간 (0.4는 하루의 40% 지점, 아침이나 오후로 설정 가능)
    private float timeRate; // 시간 진행 속도
    public Vector3 noon; // 태양과 달이 정오(최고점)에 위치할 방향

    [Header("Sun")]
    public Light sun; // 태양 역할의 빛
    public Gradient sunColor; // 태양 색 변화 (시간에 따라 변함)
    public AnimationCurve sunIntensity; // 태양 밝기 변화 (시간에 따라 변함)

    [Header("Moon")]
    public Light moon; // 달 역할의 빛
    public Gradient moonColor; // 달 색 변화 (시간에 따라 변함)
    public AnimationCurve moonIntensity; // 달 밝기 변화 (시간에 따라 변함)

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplier; // 전체 환경광 조명 변화 (시간에 따라 변함)
    public AnimationCurve reflectionIntensityMultiplier; // 반사광 변화 (시간에 따라 변함)

    private void Start()
    {
        timeRate = 1.0f / fullDayLength; // 시간 진행 속도 계산 (1초마다 하루의 일부 진행)
        time = startTime; // 시작 시간을 설정
    }

    private void Update()
    {
        // 시간 진행 (1을 넘으면 다시 0으로 순환)
        time = (time + timeRate * Time.deltaTime) % 1.0f;

        // 태양과 달 조명 업데이트
        UpdateLighting(sun, sunColor, sunIntensity);
        UpdateLighting(moon, moonColor, moonIntensity);

        // 환경광과 반사광 업데이트
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionIntensityMultiplier.Evaluate(time);
    }

    void UpdateLighting(Light lightSource, Gradient colorGradiant, AnimationCurve intensityCurve)
    {
        float intensity = intensityCurve.Evaluate(time); // 현재 시간에 따른 빛의 강도 계산

        // 빛의 회전 값 설정 (태양과 달이 하루 동안 움직이는 방향)
        lightSource.transform.eulerAngles = (time - (lightSource == sun ? 0.25f : 0.75f)) * noon * 4.0f;  
        //30 나누기 0.5 = 180이니 0.25를 빼야 정오가 된다. 반대로 달은 해와 다르게 정확하게 180도 관계 있으니 0.5를 더해 0.75를 뺀다. 
        //90에 0.25를 곱하면 90이 안나오니 4를 곱한다.

        lightSource.color = colorGradiant.Evaluate(time); // 색상 업데이트
        lightSource.intensity = intensity; // 밝기 업데이트

        // 빛이 완전히 사라지면 비활성화, 밝아지면 활성화
        GameObject go = lightSource.gameObject;
        if (lightSource.intensity == 0 && go.activeInHierarchy)
            go.SetActive(false);
        else if (lightSource.intensity > 0 && !go.activeInHierarchy)
            go.SetActive(true);
    }
}
