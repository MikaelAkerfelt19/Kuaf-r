(function () {
    // data-dt="2025-08-21T14:30:00" gibi ISO değerleri alır ve TR formatına çevirir
    const tz = 'Europe/Istanbul';
    const lang = 'tr-TR';

    function fmt(el) {
        const iso = el.getAttribute('data-dt');
        if (!iso) return;

        const type = el.getAttribute('data-dt-type') || 'datetime'; // 'datetime' | 'date' | 'time'
        const d = new Date(iso);
        if (isNaN(d.getTime())) return;

        let options;
        if (type === 'date') {
            options = { year: 'numeric', month: '2-digit', day: '2-digit', timeZone: tz };
        } else if (type === 'time') {
            options = { hour: '2-digit', minute: '2-digit', timeZone: tz, hour12: false };
        } else {
            options = {
                year: 'numeric', month: '2-digit', day: '2-digit',
                hour: '2-digit', minute: '2-digit',
                timeZone: tz, hour12: false
            };
        }

        try {
            el.textContent = new Intl.DateTimeFormat(lang, options).format(d);
        } catch (_) { /* yut */ }
    }

    document.querySelectorAll('[data-dt]').forEach(fmt);
})();
