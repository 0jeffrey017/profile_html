// game.js - Background Animation

(function() {
    "use strict";

    console.log("game.js: Initializing background animation...");

    const canvas = document.getElementById('canvas');
    if (!canvas) {
        console.warn("game.js: Canvas element not found.");
        return;
    }

    const ctx = canvas.getContext('2d');
    let W = canvas.width = window.innerWidth;
    let H = canvas.height = window.innerHeight;
    const pixels = [];
    const fov = 250;

    // Initialize pixels
    for (let x = -400; x < 400; x += 5) {
        for (let z = -250; z < 250; z += 5) {
            pixels.push({ x: x, y: 100, z: z });
        }
    }

    function render(ts) {
        // Use 0 if ts is undefined (first frame)
        const timestamp = ts || 0;
        
        const width = Math.floor(W);
        const height = Math.floor(H);
        
        if (width <= 0 || height <= 0) return;

        const len = pixels.length;
        ctx.fillStyle = 'rgb(155, 224, 173)'; // Original green color

        for (let i = 0; i < len; i++) {
            const pixel = pixels[i];
            const scale = fov / (fov + pixel.z);
            const x2d = Math.round(pixel.x * scale + width / 2);
            const y2d = Math.round(pixel.y * scale + height / 2);

            if (x2d >= 0 && x2d < width && y2d >= 0 && y2d < height) {
                // Using 1x1 rects to match original pixel look
                ctx.fillRect(x2d, y2d, 1, 1);
            }

            pixel.z -= 0.4;
            pixel.y = height / 14 + Math.sin(i / len * 15 + (timestamp / 450)) * 10;
            
            if (pixel.z < -fov) {
                pixel.z += 2 * fov;
            }
        }
    }

    function drawFrame(ts) {
        requestAnimationFrame(drawFrame);
        ctx.fillStyle = '#f0f0f0';
        ctx.fillRect(0, 0, W, H);
        render(ts);
    }

    window.addEventListener('resize', () => {
        W = canvas.width = window.innerWidth;
        H = canvas.height = window.innerHeight;
    });

    // Start animation
    requestAnimationFrame(drawFrame);
    console.log("game.js: Animation started.");

    // UI Interactions
    $(function() {
        console.log("game.js: Initializing UI interactions...");
        
        // Code container folding
        $(document).on('click', '.code-header', function() {
            $(this).parent('.code-container').toggleClass('collapsed');
        });

        // Gameplay Overlay Interactions
        const $overlay = $('#gameplay-overlay');
        const $instructionScreen = $('#instruction-screen');
        const $gameContainer = $('#game-container');
        const $gameIframe = $('#game-iframe');

        $('#play-game-btn').on('click', function() {
            const gameUrl = $(this).data('url');
            if (!gameUrl || gameUrl === '#') return;

            $overlay.fadeIn(300);
            $overlay.removeClass('gameplay-active');
            $instructionScreen.show();
            $('#start-btn-container').show(); // Show start button
            $gameContainer.hide();
            $gameIframe.attr('src', '');
            
            // Store URL for start button
            $('#start-game-btn').data('url', gameUrl);
        });

        $('#start-game-btn').on('click', function() {
            const gameUrl = $(this).data('url');
            $overlay.addClass('gameplay-active');
            $('#start-btn-container').hide(); // Hide button after start
            $gameContainer.fadeIn(300);
            $gameIframe.attr('src', gameUrl);
        });

        $('#close-overlay').on('click', function(e) {
            e.stopPropagation();
            $overlay.fadeOut(300, function() {
                $gameIframe.attr('src', '');
                $overlay.removeClass('gameplay-active');
            });
        });

        // Close overlay on background click
        $overlay.on('click', function(e) {
            if (e.target === this) {
                $overlay.fadeOut(300, function() {
                    $gameIframe.attr('src', '');
                });
            }
        });

        // Close overlay on ESC key
        $(document).on('keydown', function(e) {
            if (e.key === "Escape" && $overlay.is(':visible')) {
                $overlay.fadeOut(300, function() {
                    $gameIframe.attr('src', '');
                });
            }
        });
    });

})();
