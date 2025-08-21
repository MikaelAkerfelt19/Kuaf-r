(function () {
    // Sayfadaki tüm toast'ları otomatik göster
    const nodes = document.querySelectorAll('.toast');
    if (!nodes.length) return;
    nodes.forEach(function (el) {
        const toast = new bootstrap.Toast(el);
        toast.show();
    });
})();
