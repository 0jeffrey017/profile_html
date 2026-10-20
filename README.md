# Jeffrey LEE｜ゲームプログラマー ポートフォリオ

リイショウケツ（Jeffrey LEE）のゲーム制作実績を紹介するポートフォリオです。Unity・C#を中心に、個人制作、ハッカソン、企業連携プロジェクトに取り組んでいます。

作品ごとに、ゲームの概要、私の担当範囲、開発中の課題と解決方法、コードサンプルを掲載しています。

**[ポートフォリオを見る](https://0jeffrey017.github.io/profile_html/)** · **[作品一覧](https://0jeffrey017.github.io/profile_html/games.html)** · **[連絡先](https://0jeffrey017.github.io/profile_html/contact.html)**

## 代表作品

### 廻り月（Crescent_Moon）

**東京ゲームショウ2026出展｜13人チーム｜制作期間：5ヶ月｜Windows向け**

月が照らす異世界の回廊から脱出を目指す2D探索アクションです。月を見上げることで、プレイヤーの現在位置をもとにマップが再生成されます。

プランナー3人・プログラマー5人・2Dデザイナー5人で制作しました。私の主な担当は次のとおりです。

- ゲーム進行の非同期ステートマシン（FSM）と状態遷移の競合対策
- 月イベントに連動するマップ再生成と、表示・経路・配置の更新順序の制御
- VContainerによる依存関係の構成、R3・UniTaskによるシステム間のイベント連携
- Addressablesによるアセット読み込みと初期化フロー

使用技術：Unity 6000.3 / C# / VContainer / R3 / UniTask / Addressables / NavMesh2D

[作品詳細・コード解説](https://0jeffrey017.github.io/profile_html/games/Crescent_Moon.html) · [ゲームをダウンロード（Google Drive）](https://drive.google.com/file/d/104uoYHInbPZnVooUKUsclEOVSfv-0T6D/view?usp=sharing) · [ゲームマニュアル](assets/廻り月_ゲームマニュアル.html)

### グレート-O-カーンの侵略物語

**企業連携プロジェクト｜全7チーム中2位（準優勝）｜8人チーム｜制作期間：4ヶ月**

新日本プロレス所属選手を題材にしたランゲームです。メインプログラマーとしてゲーム全体のメインシステムとアーキテクチャ設計を担当しました。FSMによる進行・アクション管理、MVPによるUIとロジックの分離、PC・モバイルの入力制御に取り組みました。

使用技術：Unity 2022.3 / C# / UniRx / UniTask / New Input System / FSM / MVP

[作品詳細・コード解説](https://0jeffrey017.github.io/profile_html/games/GreatOKhanInvasion.html) · [公開ゲーム（新日本プロレス公式サイト）](https://www.njpw.co.jp/games/great-o-khan/)

### Pass The Baton

**ハッカソン制作｜プログラマー5人｜制作期間：1ヶ月**

過去の自分を足場として活用する2Dパズルアクションです。ゲーム全体のアーキテクチャ設計と進行管理を担当し、VContainerとFSMによるフロー制御、EventBusによるリスポーン・エフェクトの連携に取り組みました。

[作品詳細・コード解説](https://0jeffrey017.github.io/profile_html/games/PassTheBaton.html) · [ブラウザでプレイ](https://0jeffrey017.github.io/WebGLGames_01/5_PassTheBaton)

## 技術経験

[Engineering / 設計とコード](engineering.html)では、マップ生成、状態遷移、DI・MVP、アイテム設計、並列計算、オブジェクトの再利用を紹介しています。各テーマを「考えたこと」「設計・実装」「コードの着眼点」と処理の流れで説明し、コード抜粋と作品詳細へつなげています。

- **設計・システム連携：** FSM、MVP、依存性注入、イベント通知、ScriptableObjectによるデータ管理
- **非同期・リアクティブ処理：** UniTask、R3、UniRxを用いた進行制御と状態通知
- **性能改善の取り組み：** ObjectPoolによるオブジェクト再利用、C# Job System・Burst Compilerの検証
- **チーム制作：** 他職種との仕様調整、タスク分担、Git・GitHubを使った共同開発

## コード・実績の確認について

各作品の詳細ページでは「概要」「課題と対応」「コード」のタブから、担当内容と実装例をご覧いただけます。[Scripts](Scripts/)にも、担当した実装の一部をコードサンプルとして掲載しています。

このリポジトリはポートフォリオサイトのHTML・CSS・JavaScriptと、掲載用の素材・コードサンプルを管理しています。

[経歴・資格](https://0jeffrey017.github.io/profile_html/background.html) · [履歴書](https://0jeffrey017.github.io/profile_html/Resume/Resume.html) · [GitHub](https://github.com/0jeffrey017) · [お問い合わせ](https://0jeffrey017.github.io/profile_html/contact.html)
