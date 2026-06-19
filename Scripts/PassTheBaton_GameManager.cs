using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲーム全体の進行を統括するマネージャー。
/// 「入力の検知」はこの GameManager が担い、「実際の挙動」はプレイヤー側に委譲する
/// という責務分離を徹底。これにより入力仕様の変更がプレイヤー実装に波及せず、
/// イントロ演出 → ゲーム開始といったフロー制御も一箇所で安全に管理できる。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private BigfloppaControls _playerControls;
    [SerializeField] private StartSpawner _startSpawner;

    private bool _gameStarted = false;

    void Awake()
    {
        // シングルトンの重複生成を防ぐ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 開始時はイントロ演出のみを走らせ、操作はまだ受け付けない
        _startSpawner.Initialize(_playerControls);
        _startSpawner.StartIntroSequence();
    }

    void Update()
    {
        if (!_gameStarted) return; // イントロ中は入力を無視
        HandlePlayerInput();
    }

    /// <summary>
    /// 入力を「検知」して値・イベントとしてプレイヤーへ橋渡しするだけに留め、
    /// 移動やジャンプの実装詳細は BigfloppaControls 側へ委譲する。
    /// </summary>
    private void HandlePlayerInput()
    {
        // 水平移動の入力値を -1 / 0 / +1 に正規化して渡す
        float moveInput = 0f;
        if (Keyboard.current.aKey.isPressed) moveInput = -1f;
        if (Keyboard.current.dKey.isPressed) moveInput = 1f;
        _playerControls.SetMovementInput(moveInput);

        // ジャンプは「押した瞬間／離した瞬間」をイベントとして通知（溜めジャンプに対応）
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            _playerControls.OnJumpPressed();

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            _playerControls.OnJumpReleased();
    }

    /// <summary>イントロ演出の完了通知を受けて、ゲームプレイを解禁する。</summary>
    public void OnGameIntroComplete()
    {
        _gameStarted = true;
        _playerControls.EnableGameplay();
    }
}
