// game-tabs.js — ゲーム詳細ページのタブ切り替え（概要 / 問題解決 / コード）

(function () {
    "use strict";

    function activate(name) {
        document.querySelectorAll('.tab-btn').forEach(function (b) {
            b.classList.toggle('is-active', b.dataset.tab === name);
        });
        document.querySelectorAll('.tab-panel').forEach(function (p) {
            p.classList.toggle('is-active', p.dataset.panel === name);
        });
        // 切り替え時はパネル先頭へ
        var active = document.querySelector('.tab-panel.is-active');
        if (active) active.scrollTop = 0;
    }

    document.addEventListener('click', function (e) {
        // 概要内のジャンプボタン
        var jump = e.target.closest('[data-tab-target]');
        if (jump) {
            e.preventDefault();
            activate(jump.dataset.tabTarget);
            return;
        }
        // タブバーのボタン
        var tab = e.target.closest('.tab-btn');
        if (tab) {
            activate(tab.dataset.tab);
        }
    });

})();
