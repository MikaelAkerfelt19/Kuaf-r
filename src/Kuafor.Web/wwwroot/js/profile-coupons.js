(function () {
    window.copyCoupon = async function (code) {
        try {
            await navigator.clipboard.writeText(code);
            alert('Kupon kodu kopyalandı: ' + code);
        } catch {
            // Eski tarayıcılar için fallback
            const el = document.createElement('textarea');
            el.value = code; document.body.appendChild(el);
            el.select(); document.execCommand('copy'); document.body.removeChild(el);
            alert('Kupon kodu kopyalandı: ' + code);
        }
    };
})();