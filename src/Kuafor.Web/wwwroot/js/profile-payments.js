(function () {
    const modalEl = document.getElementById('cardModal');
    const modal = modalEl ? new bootstrap.Modal(modalEl) : null;

    function digitsOnly(s) { return (s || '').replace(/\D+/g, ''); }
    function maskCard(num) {
        const d = digitsOnly(num);
        if (d.length < 4) return "**** **** **** " + d.padStart(4, '*').slice(-4);
        return "**** **** **** " + d.slice(-4);
    }

    window.payCardNew = function () {
        ['cardHolder', 'cardNumber', 'cardExpM', 'cardExpY', 'cardCvc'].forEach(id => {
            const el = document.getElementById(id); if (el) el.value = '';
        });
        const def = document.getElementById('cardDefault'); if (def) def.checked = false;
    };

    window.payCardSave = function () {
        const holder = document.getElementById('cardHolder')?.value.trim();
        const number = document.getElementById('cardNumber')?.value.trim();
        const m = Number(document.getElementById('cardExpM')?.value || 0);
        const y = Number(document.getElementById('cardExpY')?.value || 0);
        const cvc = (document.getElementById('cardCvc')?.value || '').trim();
        const makeDefault = document.getElementById('cardDefault')?.checked;

        if (!holder || digitsOnly(number).length < 12) { alert('Kart bilgilerini kontrol edin.'); return; }
        if (!(m >= 1 && m <= 12) || !(y >= 2025 && y <= 2035)) { alert('Son kullanma tarihini kontrol edin.'); return; }
        if (cvc.length < 3) { alert('CVC hatalı.'); return; }

        const list = document.querySelector('#tab-payments .row.g-3');
        // Basit DOM ekleme (mock)
        if (list) {
            const id = Math.floor(Math.random() * 100000);
            const col = document.createElement('div');
            col.className = 'col-12 col-md-6 col-lg-4';
            col.innerHTML = `
        <div class="border rounded p-3 h-100 d-flex flex-column">
          <div class="d-flex justify-content-between align-items-start">
            <div>
              <div class="fw-semibold">${maskCard(number)}</div>
              <div class="text-muted small">${holder} · ${m}/${y}</div>
            </div>
            ${makeDefault ? '<span class="badge bg-success">Varsayılan</span>' : ''}
          </div>
          <div class="mt-auto d-flex gap-2">
            ${makeDefault ? '' : `<button class="btn btn-sm btn-outline-primary" onclick="window.payCardMakeDefault(${id})">Varsayılan Yap</button>`}
            <button class="btn btn-sm btn-outline-danger" onclick="window.payCardDelete(${id})">Sil</button>
          </div>
        </div>`;
            list.prepend(col);
        }
        modal && modal.hide();
        alert('Mock: Kart eklendi.');
    };

    window.payCardDelete = function (id) {
        if (confirm('Kart silinsin mi?')) {
            // Mock: görsel olarak ilk uygun kart kutusunu kaldır
            const btn = Array.from(document.querySelectorAll('#tab-payments button'))
                .find(b => (b.getAttribute('onclick') || '').includes(`payCardDelete(${id})`));
            const card = btn ? btn.closest('.col-12,.col-md-6,.col-lg-4') : null;
            if (card) card.remove();
            alert('Mock: Kart silindi.');
        }
    };

    window.payCardMakeDefault = function (id) {
        // Mevcut "Varsayılan" rozetini kaldır, bu karta ekle (mock)
        document.querySelectorAll('#tab-payments .badge.bg-success').forEach(x => x.remove());
        const btn = Array.from(document.querySelectorAll('#tab-payments button'))
            .find(b => (b.getAttribute('onclick') || '').includes(`payCardMakeDefault(${id})`));
        if (btn) {
            const header = btn.closest('.border')?.querySelector('.d-flex.justify-content-between');
            const badge = document.createElement('span'); badge.className = 'badge bg-success'; badge.textContent = 'Varsayılan';
            btn.closest('.border')?.querySelector('.d-flex.justify-content-between')?.appendChild(badge);
            btn.remove(); // artık varsayılan
        }
        alert('Mock: Varsayılan kart ayarlandı.');
    };

    window.payBillingSave = function () {
        const type = document.getElementById('invType')?.value || 'BIREYSEL';
        const name = document.getElementById('invName')?.value?.trim();
        const tax = document.getElementById('invTax')?.value?.trim();
        const email = document.getElementById('invEmail')?.value?.trim();
        const addr = document.getElementById('invAddr')?.value?.trim();
        const dist = document.getElementById('invDistrict')?.value?.trim();
        const city = document.getElementById('invCity')?.value?.trim();

        if (!name || !email || !addr || !dist || !city) {
            alert('Lütfen zorunlu alanları doldurun.'); return;
        }
        if (type === 'SIRKET' && (!tax || tax.length < 10)) {
            alert('Şirket tipi için geçerli VKN giriniz.'); return;
        }
        alert('Mock: Fatura bilgileri kaydedildi.');
    };
})();
