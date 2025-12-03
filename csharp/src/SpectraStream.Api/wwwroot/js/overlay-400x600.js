(() => {
    const video = document.getElementById('animation-video');
    if (!video) return;

    video.setAttribute('muted', '');
    video.setAttribute('playsinline', '');
    video.setAttribute('loop', '');

    function tryPlay() {
        video.play().catch(() => {
        });
    }

    if (video.readyState >= 2) {
        tryPlay();
    } else {
        video.addEventListener('canplay', tryPlay, { once: true });
    }
})();
