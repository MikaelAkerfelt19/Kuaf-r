(function () {
    const el = document.getElementById("up-next-countdown");
    if (!el) return;
    const startIso = el.getAttribute("data-start");
    const start = startIso ? new Date(startIso) : null;
    if (!start) return;

    function fmt(ms) {
        if (ms <= 0) return 'Başlamak üzere';
        const s = Math.floor(ms / 1000);
        const d = Math.floor(s / 86400);
        const h = Math.floor((s % 86400) / 3600);
        const m = Math.floor((s % 3600) / 60);
        if (d > 0) return `${d} gün ${h} saat ${m} dakika`;
        if (h > 0) return `${h} saat ${m} dakika`;
        return `${m} dakika`;
    }
    function tick() {
        el.textContent = fmt(start - new Date());
    }
    tick();
    setInterval(tick, 60 * 1000);
})();