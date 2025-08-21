(function () {
    // Tek modal, hem "Ekle" hem "Düzenle" için kullanılıyor.
    const modalEl = document.getElementById('serviceModal');
    if (!modalEl) return;

    const form = modalEl.querySelector('#serviceForm');
    const title = modalEl.querySelector('#serviceModalLabel');
    const submitBtn = modalEl.querySelector('#serviceSubmitBtn');

    modalEl.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const mode = button?.getAttribute('data-mode') || 'create';

        // varsayılanlar
        let id = 0, name = '', duration = 30, price = 0, active = true;

        if (mode === 'edit') {
            id = parseInt(button.getAttribute('data-id') || '0', 10);
            name = button.getAttribute('data-name') || '';
            duration = parseInt(button.getAttribute('data-duration') || '30', 10);
            price = button.getAttribute('data-price') || '0';
            active = (button.getAttribute('data-active') === 'true');
        }

        // Form alanlarını doldur
        form.querySelector('input[name="Id"]').value = id;
        form.querySelector('input[name="Name"]').value = name;
        form.querySelector('input[name="DurationMin"]').value = duration;
        form.querySelector('input[name="Price"]').value = price;
        form.querySelector('input[name="IsActive"]').checked = !!active;

        // Başlık ve action
        if (mode === 'edit') {
            title.textContent = 'Hizmeti Düzenle';
            form.setAttribute('action', '/Admin/Services/Edit');
            submitBtn.textContent = 'Güncelle';
        } else {
            title.textContent = 'Yeni Hizmet';
            form.setAttribute('action', '/Admin/Services/Create');
            submitBtn.textContent = 'Kaydet';
        }
    });

    // Modal kapanınca formu sıfırla
    modalEl.addEventListener('hidden.bs.modal', function () {
        form.reset();
        form.setAttribute('action', '/Admin/Services/Create');
        const title = modalEl.querySelector('#serviceModalLabel');
        if (title) title.textContent = 'Yeni Hizmet';
        const submitBtn = modalEl.querySelector('#serviceSubmitBtn');
        if (submitBtn) submitBtn.textContent = 'Kaydet';
    });
})();
