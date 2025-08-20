(function () {
    window.privacySaveConsents = function () {
        const data = {
            email: document.getElementById('consEmail')?.checked || false,
            sms: document.getElementById('consSms')?.checked || false,
            push: document.getElementById('consPush')?.checked || false,
            whatsapp: document.getElementById('consWa')?.checked || false
        };
        alert('Mock: Pazarlama izinleri güncellendi.\n' +
            `E-posta:${data.email ? '✓' : '×'} SMS:${data.sms ? '✓' : '×'} Push:${data.push ? '✓' : '×'} WhatsApp:${data.whatsapp ? '✓' : '×'}`);
    };

    function download(filename, text, mime) {
        const blob = new Blob([text], { type: mime });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a'); a.href = url; a.download = filename; a.click();
        URL.revokeObjectURL(url);
    }

    window.privacyExport = function (fmt) {
        // Mock veri: gerçek backend geldiğinde serverdan hazırlanmış dosya indirilir
        const payload = {
            profile: { fullName: 'Mock User', email: 'user@example.com' },
            preferences: { branch: 'Kadıköy', stylist: 'Ayşe K.' },
            appointments: [
                { id: 501, when: '2025-09-02T14:30:00+03:00', service: 'Saç Kesimi' }
            ]
        };
        if (fmt === 'json') {
            download('export.json', JSON.stringify(payload, null, 2), 'application/json');
        } else {
            const rows = [
                ['Section', 'Key', 'Value'],
                ['profile', 'fullName', payload.profile.fullName],
                ['profile', 'email', payload.profile.email],
                ['preferences', 'branch', payload.preferences.branch],
                ['preferences', 'stylist', payload.preferences.stylist],
                ...payload.appointments.map(a => ['appointments', 'item', `${a.id}|${a.when}|${a.service}`])
            ];
            const csv = rows.map(r => r.map(x => `"${String(x).replace(/"/g, '""')}"`).join(',')).join('\n');
            download('export.csv', csv, 'text/csv');
        }
        alert('Mock: Veri dışa aktarıldı.');
    };

    window.privacyCloseAccount = function () {
        const v = (document.getElementById('dangerConfirm')?.value || '').trim().toUpperCase();
        if (v !== 'KAPAT') { alert('Onay metni hatalı. "KAPAT" yazmalısın.'); return; }
        if (confirm('Hesabını kapatmak istediğinden emin misin? Bu işlem mock olarak simüle edilecek.')) {
            alert('Mock: Hesap kapatma isteğin alındı.');
        }
    };
})();
