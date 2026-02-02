using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, ITeleportable
{
    public enum EnemyState { Idle, Patrol, Alert, Chase }
    public EnemyState _currentState = EnemyState.Patrol;

    [Header("UI 설정")]
    public GameObject detectionMark;

    public EnemyState currentState
    {
        get => _currentState;
        set
        {
            // 값이 같으면 실행하지 않음 (최적화)
            if (_currentState == value) return;

            _currentState = value;

            // 상태가 바뀔 때 자동으로 '!' 활성화 여부 결정
            if (detectionMark != null)
            {
                detectionMark.SetActive(_currentState == EnemyState.Chase);
            }
        }
    }

    public virtual void OnAssassinated()
    {
        if (currentState != EnemyState.Chase)
        {
            Debug.Log($"{gameObject.name} 암살됨");
            Destroy(gameObject);
        }
        else
        {
            // 추격 중일 때는 암살이 되지 않음을 알림 (필요 시 작성)
            Debug.Log($"{gameObject.name}이 플레이어를 발견한 상태라 암살에 실패했습니다.");
        }
    }

    public abstract Transform GetTransform();
    public abstract void OnTeleport();

    protected virtual void OnValidate()
    {
        if (detectionMark != null)
        {
            detectionMark.SetActive(_currentState == EnemyState.Chase);
        }
    }
}
