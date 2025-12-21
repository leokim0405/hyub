using UnityEngine;
using Unity.Cinemachine; // 만약 에러가 나면 Unity.Cinemachine으로 바꿔보세요.

[SaveDuringPlay] [AddComponentMenu("")] 
public class CinemachineLockY : CinemachineExtension
{
    [Tooltip("카메라를 고정할 Y축 좌표값입니다.")]
    public float m_YPosition = 0;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // Body 단계(카메라 이동 결정 단계)에서 Y값을 강제로 고정합니다.
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 pos = state.RawPosition;
            pos.y = m_YPosition;
            state.RawPosition = pos;
        }
    }
}