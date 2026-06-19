using System;
using Audio;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトル演出・ゲーム開始・リスタートの一連のフローを UniTask で制御するマネージャー。
/// コルーチンや多重コールバックに頼らず async/await で記述することで、
/// 「入力待ち」と「カメラ演出」を並行させつつ可読性の高いフロー制御を実現。
/// destroyCancellationToken でシーン破棄時の安全なキャンセルも保証する。
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup uiCanvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI tileText;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private StageController stageController;
    [SerializeField] private Transform lookAtTarget;

    private float _x;
    private float _z;

    private async void Start()
    {
        try
        {
            messageText.text = "Press S to Start";
            uiCanvasGroup.alpha = 0;
            // タイトル演出（カメラ回転＋入力待ち）が終わるまで待機
            await CameraMovement();
        }
        catch (OperationCanceledException e)
        {
            // シーン破棄などでキャンセルされた場合は安全に抜ける
            Debug.LogException(e);
        }
    }

    private void OnEnable()  => stageController.GameOverCallback += HandleGameOver;
    private void OnDisable() => stageController.GameOverCallback -= HandleGameOver;

    private void HandleGameOver(string winner) => WaitForRestart().Forget();

    /// <summary>ゲームオーバー後、S キー入力を待ってシーンをリロードする。</summary>
    private async UniTask WaitForRestart()
    {
        // 毎フレーム Yield しながら入力を監視（コールバック地獄を回避）
        while (!Keyboard.current.sKey.wasPressedThisFrame)
            await UniTask.Yield(PlayerLoopTiming.Update);

        stageController.isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// タイトル中はカメラを円軌道で旋回させ、S キーが押されたら本編へ遷移する。
    /// 「演出（カメラ移動）」と「入力待ち」を 1 つの while ループで同時に表現できるのが
    /// async/await の利点。
    /// </summary>
    private async UniTask CameraMovement()
    {
        AudioManager.Instance.PlayBGM(AudioManager.BGMType.BGM01);
        Vector3 currPos = mainCamera.transform.position;
        Quaternion currRot = mainCamera.transform.rotation;

        while (!Keyboard.current.sKey.wasPressedThisFrame)
        {
            // 三角関数でカメラを被写体の周囲に円軌道で配置
            _x = 10 * Mathf.Sin(Time.time * 0.5f);
            _z = 10 * Mathf.Cos(Time.time * 0.5f);
            mainCamera.transform.position = new Vector3(_x, currPos.y, _z);
            mainCamera.transform.LookAt(lookAtTarget);

            // シーン破棄時にトークン経由で自動キャンセルされる
            await UniTask.WaitForEndOfFrame(this.destroyCancellationToken);
        }

        // 演出終了：カメラを元に戻してゲーム本編を開始
        mainCamera.transform.position = currPos;
        mainCamera.transform.rotation = currRot;
        stageController.StartGame();
        uiCanvasGroup.alpha = 1;
        tileText.gameObject.SetActive(false);
    }
}
