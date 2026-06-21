using UnityEngine;

namespace RYZECHo;

/// <summary>
/// 既存シーンに残っているコンポーネントとの互換用。
/// 実際のゲーム/UI初期化は RyzechoGameController が自動で行う。
/// </summary>
[DefaultExecutionOrder(-100)]
public sealed class SceneSetup : MonoBehaviour
{
    private void Awake()
    {
        if (FindAnyObjectByType<RyzechoGameController>() == null)
        {
            gameObject.AddComponent<RyzechoGameController>();
        }
    }
}
