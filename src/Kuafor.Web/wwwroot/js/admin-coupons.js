(function () {
    const modalEl = document.getElementById('couponModal');
    if (!modalEl) return;

    const form = modalEl.querySelector('#couponForm');
    const title = modalEl.querySelector('#couponModalLabel');
    const submitBtn = modalEl.querySelector('#couponSubmitBtn');

    modalEl.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const mode = button?.getAttribute('data-mode') || 'create';

        // Varsayılanlar
        let id = 0, code = '', ctitle = '', dtype = '1', amount = '0', minspend = '', expires = '', active = true;

        if (mode === 'edit') {
            id = parseInt(button.getAttribute('data-id') || '0', 10);
            code = button.getAttribute('data-code') || '';
            ctitle = button.getAttribute('data-title') || '';
            dtype = button.getAttribute('data-type') || '1';
            amount = button.getAttribute('data-amount') || '0';
            minspend = button.getAttribute('data-minspend') || '';
            expires = button.getAttribute('data-expires') || ''; // yyyy-MM-dd
            active = (button.getAttribute('data-active') === 'true');
        }

        // Formu doldur
        form.querySelector('input[name="Id"]').value = id;
        form.querySelector('input[name="Code"]').value = code;
        form.querySelector('input[name="Title"]').value = ctitle;
        form.querySelector('select[name="DiscountType"]').value = dtype;
        form.querySelector('input[name="Amount"]').value = amount;
        form.querySelector('input[name="MinSpend"]').value = minspend;
        form.querySelector('input[name="ExpiresAt"]').value = expires;
        form.querySelector('input[name="IsActive"]').checked = !!active;

        // Başlık ve action
        if (mode === 'edit') {
            title.textContent = 'Kuponu Düzenle';
            form.setAttribute('action', '/Admin/Coupons/Edit');
            submitBtn.textContent = 'Güncelle';
        } else {
            title.textContent = 'Yeni Kupon';
            form.setAttribute('action', '/Admin/Coupons/Create');
            submitBtn.textContent = 'Kaydet';
        }
    });

    // Kapanınca sıfırla
    modalEl.addEventListener('hidden.bs.modal', function () {
        form.reset();
        form.setAttribute('action', '/Admin/Coupons/Create');
        if (title) title.textContent = 'Yeni Kupon';
        if (submitBtn) submitBtn.textContent = 'Kaydet';
    });
})();
