using UnityEngine;

/*
 * Guilty Lane — アイテムシステム（担当：メインアイテムシステム・イベントフラグ管理）
 *
 * ScriptableObject を基盤に「抽象基底クラス + 各アイテム1クラス」で構成。
 * 新しいアイテムは ItemSOBase を継承して UseItem() を書くだけで追加でき、
 * 既存コードを一切変更しない（開放閉鎖原則）。効果は GameContext の
 * イベントフラグを通じてゲーム本体へ伝搬させ、UI とロジックを疎結合に保つ。
 */

/// <summary>
/// 全アイテム共通のデータ（名前・価格・説明など）と「使用」インターフェースを定義する抽象基底。
/// ScriptableObject 化することで、プランナーが Unity エディタ上でアセットとして
/// アイテムを量産・調整できる（プログラマーを介さないワークフロー）。
/// </summary>
public abstract class ItemSOBase : ScriptableObject
{
    [Header("Item Data")]
    public Sprite itemSprite;
    public string itemName;
    public int itemID;
    [TextArea] public string itemDescription;
    public int price;
    public bool permanent; // 永続効果か使い切りか
    public int quantity;

    /// <summary>各アイテム固有の効果。派生クラスが個別に実装する。</summary>
    public abstract void UseItem(GameContext context);
}

/// <summary>
/// 具体アイテムの例：「投資」。10%の確率で相続が5倍になるハイリスクな賭け。
/// 効果は context のイベントフラグを立てるだけに留め、実際の倍率反映は
/// ゲーム本体が同フラグを参照して行う（責務の分離）。
/// </summary>
[CreateAssetMenu(menuName = "NewItem/Investment")]
public class Investment : ItemSOBase
{
    public override void UseItem(GameContext context)
    {
        // 10% を引ければ成功フラグを立てる
        if (Random.Range(0, 100) <= 10)
        {
            context.IsInvestmentEffect = true;
            Debug.Log("Investment success");
        }
        else
        {
            Debug.Log("Investment failed");
        }
    }
}

/// <summary>
/// インベントリ管理（抜粋）。スロットの選択／使用を仲介する。
/// アイテムの「中身」を一切知らず、ItemSOBase 越しに UseItem() を呼ぶだけなので、
/// アイテムが何種類増えてもこのクラスは変更不要。
/// </summary>
public class NewInventoryManager : MonoBehaviour
{
    private ItemSOBase _currentItemSO;

    /// <summary>使用ボタンから呼ばれる。選択中アイテムの効果を発動する。</summary>
    public void UseItem()
    {
        if (_currentItemSO == null)
        {
            Debug.Log("No item selected");
            return;
        }

        // ポリモーフィズムにより、具体的な効果は各アイテムクラスへ委譲される
        _currentItemSO.UseItem(/* GameContext */ null);

        // 使用済みアイテムを消費して選択状態をリセット
        _currentItemSO = null;
    }
}
