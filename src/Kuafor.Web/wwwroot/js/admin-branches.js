(function () {
    const modalEl = document.getElementById('branchModal');
    if (!modalEl) return;

    const form = modalEl.querySelector('#branchForm');
    const title = modalEl.querySelector('#branchModalLabel');
    const submitBtn = modalEl.querySelector('#branchSubmitBtn');

    modalEl.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const mode = button?.getAttribute('data-mode') || 'create';

        // Varsayılanlar
        let id = 0, name = '', address = '', phone = '', active = true;

        if (mode === 'edit') {
            id = parseInt(button.getAttribute('data-id') || '0', 10);
            name = button.getAttribute('data-name') || '';
            address = button.getAttribute('data-address') || '';
            phone = button.getAttribute('data-phone') || '';
            active = (button.getAttribute('data-active') === 'true');
        }

        // Formu doldur
        form.querySelector('input[name="Id"]').value = id;
        form.querySelector('input[name="Name"]').value = name;
        form.querySelector('input[name="Address"]').value = address;
        form.querySelector('input[name="Phone"]').value = phone;
        form.querySelector('input[name="IsActive"]').checked = !!active;

        // Başlık ve action
        if (mode === 'edit') {
            title.textContent = 'Şubeyi Düzenle';
            form.setAttribute('action', '/Admin/Branches/Edit');
            submitBtn.textContent = 'Güncelle';
        } else {
            title.textContent = 'Yeni Şube';
            form.setAttribute('action', '/Admin/Branches/Create');
            submitBtn.textContent = 'Kaydet';
        }
    });

    // Kapanınca sıfırla
    modalEl.addEventListener('hidden.bs.modal', function () {
        form.reset();
        form.setAttribute('action', '/Admin/Branches/Create');
        if (title) title.textContent = 'Yeni Şube';
        if (submitBtn) submitBtn.textContent = 'Kaydet';
    });
})();
