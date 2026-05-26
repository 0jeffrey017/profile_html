// main.js - Dynamic Game Gallery Configuration

(function($) {
    "use strict";

    console.log("main.js: Initializing game grid...");

    const gameData = [
        {
            id: "game01",
            name: "SpaceCat",
            desc: "2Dのアクションゲームです",
            image: "Image/Gemini_Generated_Image_sjdsvksjdsvksjds.png",
            link: "games/Game01.html",
            status: "released"
        },
        {
            id: "game02",
            name: "逢魔々城",
            desc: "武器を設置し、怪奇を倒すゲームです。",
            image: "Image/Gemini_Generated_Image_hyd9tdhyd9tdhyd9.png",
            link: "games/Game02.html",
            status: "released"
        },
        {
            id: "game03",
            name: "ペンギン紙相撲",
            desc: "紙相撲ゲーム",
            image: "Image/ペンギン紙相撲.png",
            link: "games/Game03.html",
            status: "released"
        },
        {
            id: "game04",
            name: "クリックゲーム",
            desc: "クリックゲーム",
            image: "Image/マウスでボタンをクリックで点数が増えるゲーム.png",
            link: "games/Game04.html",
            status: "released"
        },
        {
            id: "game05",
            name: "余にひれ伏せ愚民ども<br>グレート-O-カーンの侵略物語",
            desc: "グレート-O-カーン選手のランナーゲームです。",
            image: "Image/余にひれ伏せ愚民ども.png",
            link: "games/Game05.html",
            status: "released"
        },
        {
            id: "game06",
            name: "SFML_project01",
            desc: "C++の練習プロジェクト",
            image: "Image/SFML_01.png",
            link: "games/Game06.html",
            status: "released"
        },
        {
            id: "game07",
            name: "PassTheBaton",
            desc: "Hackathon　”つなぐ”",
            image: "Image/PassTheBaton_01.png",
            link: "games/Game07.html",
            status: "released"
        },
        {
            id: "game08",
            name: "デカめの一杯",
            desc: "Hackathon　”いっぱい”",
            image: "Image/Ika_01.png",
            link: "games/Game08.html",
            status: "released"
        },
        {
            id: "game09",
            name: "Guilty Lane",
            desc: "Hackathon　”いかす”",
            image: "Image/PassTheBaton_01.png",
            link: "games/Game09.html",
            status: "Unreleased"
        },
        {
            id: "game99",
            name: "未公開タイトル",
            desc: "開発中のゲームです。",
            image: "Image/GameCode.png",
            link: "#",
            status: "released"
        }
    ];

    function renderGameGrid() {
        const $grid = $('.game-grid');
        if (!$grid.length) {
            console.warn("main.js: .game-grid element not found.");
            return;
        }

        console.log("main.js: Rendering " + gameData.length + " games.");
        $grid.empty();

        const releasedGames = gameData.filter(game => game.status === "released");

        releasedGames.forEach(game => {
            const gameCard = `
                <div class="game-card" data-id="${game.id}">
                    <a href="${game.link}" class="game-link">
                        <div class="game-image-wrapper">
                            <img src="${game.image}" alt="${game.name}" onerror="this.src='https://via.placeholder.com/400x300?text=Image+Not+Found'">
                        </div>
                        <div class="game-info">
                            <h3 class="game-name">${game.name}</h3>
                            <p class="game-desc">${game.desc}</p>
                        </div>
                    </a>
                </div>
            `;
            $grid.append(gameCard);
        });
        
        console.log("main.js: Grid rendered successfully.");
    }

    $(function() {
        renderGameGrid();
    });

})(jQuery);
