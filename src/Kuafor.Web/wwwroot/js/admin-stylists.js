s(function () {
    const modalEl = document.getElementById('stylistModal');
    if (!modalEl) return;

    const form = modalEl.querySelector('#stylistForm');
    const title = modalEl.querySelector('#stylistModalLabel');
    const submitBtn = modalEl.querySelector('#stylistSubmitBtn');

    modalEl.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const mode = button?.getAttribute('data-mode') || 'create';

        // Defaults
        let id = 0, name = '', rating = 0, bio = '', branchId = '', active = true;

        if (mode === 'edit') {
            id = parseInt(button.getAttribute('data-id') || '0', 10);
            name = button.getAttribute('data-name') || '';
            rating = button.getAttribute('data-rating') || '0';
            bio = button.getAttribute('data-bio') || '';
            branchId = button.getAttribute('data-branchid') || '';
            active = (button.getAttribute('data-active') === 'true');
        }

        // Fill form fields
        form.querySelector('input[name="Id"]').value = id;
        form.querySelector('input[name="Name"]').value = name;
        form.querySelector('input[name="Rating"]').value = rating;
        form.querySelector('textarea[name="Bio"]').value = bio;
        const branchSelect = form.querySelector('select[name="BranchId"]');
        if (branchSelect) branchSelect.value = String(branchId);
        form.querySelector('input[name="IsActive"]').checked = !!active;

        // Title & action
        if (mode === 'edit') {
            title.textContent = 'Kuaförü Düzenle';
            form.setAttribute('action', '/Admin/Stylists/Edit');
            submitBtn.textContent = 'Güncelle';
        } else {
            title.textContent = 'Yeni Kuaför';
            form.setAttribute('action', '/Admin/Stylists/Create');
            submitBtn.textContent = 'Kaydet';
        }
    });

    // Reset on hide
    modalEl.addEventListener('hidden.bs.modal', function () {
        form.reset();
        form.setAttribute('action', '/Admin/Stylists/Create');
        if (title) title.textContent = 'Yeni Kuaför';
        if (submitBtn) submitBtn.textContent = 'Kaydet';
    });
})();
