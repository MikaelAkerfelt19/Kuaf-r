(function(){
    function toMinutes(hhmm){
        if(!hhmm) return null;
        const [h,m] = hhmm.split(':').map(Number);
        if(isNaN(h)||isNaN(m)) return null;
        return h*60+m;
    }

    window.profileNotifSave = function(){
        const qf = document.getElementById('quietFrom')?.value || '';
        const qt = document.getElementById('quietTo')?.value || '';
        const fromMin = toMinutes(qf), toMin = toMinutes(qt);

        if(fromMin===null || toMin===null) {
            alert('Lütfen sessiz saatleri SS:DD biçiminde girin.');
            return;
        }
        if(fromMin===toMin) {
            alert('Sessiz başlangıç ve bitiş saatleri aynı olamaz.');
            return;
        }
        // Kanallar/kategoriler
        const channels = ['nEmail','nSms','nPush','nWa'].map(id => [id, document.getElementById(id)?.checked]);
        const cats = ['cRem','cCamp','cCrit'].map(id => [id, document.getElementById(id)?.checked]);

        alert('Mock: Bildirim tercihleri kaydedildi.'
            + `\nSessiz saatler: ${qf} → ${qt} `
            + `\nKanallar: ${channels.map(x=>x[0]+':'+(x[1]?'✓':'×')).join(', ')}`
            + `\nKategoriler: ${cats.map(x=>x[0]+':'+(x[1]?'✓':'×')).join(', ')}`);
    };
})();