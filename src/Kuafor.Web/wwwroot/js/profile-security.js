(function () {
    function hasNumber(s) { return /\d/.test(s); }
    function hasLetter(s) { return /[A-Za-zĞğÜüİıÖöÇç]/.test(s); }

    window.secChangePassword = function () {
        const cur = document.getElementById("pwCurrent")?.value || '';
        const nw = document.getElementById("pwNew")?.value || '';
        const cf = document.getElementById("pwConfirm")?.value || '';
        if (!cur || !nw || !cf) { alert("Lütfen tüm alanları doldurun."); return; }
        if (nw.length < 6 || !hasNumber(nw) || !hasLetter(nw)) {
            alert("Yeni şifre en az 6 karakter uzunluğunda olmalı, bir rakam ve bir harf içermelidir."); return;
        }
        if (nw !== cf) { alert("Yeni şifreler eşleşmiyor."); return; }
        alert("Mock: Şifreniz değiştirildi.");
    };

    window.secCopy = async function (id) {
        const el = document.getElementById(id);
        if (!el) return;
        try { await navigator.clipboard.writeText(el.value || el.textContent || ''); alert("Kopyalandı!"); }
        catch { alert("Kopyalama başarısız."); }
    };

    window.secEnable2FA = function () {
        const code = (document.getElementById("totpCode")?.value || '').trim();
        if (!/^\d{6}$/.test(code)) { alert('Lütfen 6 haneli kod girin.'); return; }
        alert("Mock: 2FA etkinleştirildi.");
        // Gerçekte: POST -> enable; sayfayı yenile veya UI durumunu güncelle
    };
    
    window.secDisable2FA = function () {
        if (confirm('2FA devre dışı bırakılsın mı?')) alert('Mock: 2FA devre dışı bırakıldı.');
    };

    window.secDownloadCodes = function () {
        const codes = Array.from(document.querySelectorAll('#tab-security code')).map(c => c.textContent).filter(Boolean);
        const blob = new Blob([codes.join('\n')], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a'); a.href = url; a.download = 'recovery-codes.txt'; a.click();
        URL.revokeObjectURL(url);
    };

    window.secSignOutAll = function () {
        if (confirm('Tüm oturumlar (bu cihaz hariç) kapatılsın mı?')) alert('Mock: Tüm oturumlar kapatıldı.');
    };

    window.secSignOut = function (id) {
        if (confirm('Bu oturum kapatılsın mı? (' + id + ')')) alert('Mock: Oturum kapatıldı: ' + id);
    };
})();