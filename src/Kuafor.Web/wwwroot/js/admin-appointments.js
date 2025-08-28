(function () {
    // DETAY MODAL
    const detailEl = document.getElementById('appointmentDetailModal');
    if (detailEl) {
        detailEl.addEventListener('show.bs.modal', function (event) {
            const btn = event.relatedTarget;
            const set = (id, val) => { const el = detailEl.querySelector(id); if (el) el.textContent = val || ''; };

            set('#apd-id', btn.getAttribute('data-id'));
            set('#apd-when', btn.getAttribute('data-when'));
            set('#apd-duration', (btn.getAttribute('data-duration') || '0') + ' dk');
            set('#apd-customer', btn.getAttribute('data-customer'));
            set('#apd-service', btn.getAttribute('data-service'));
            set('#apd-branch', btn.getAttribute('data-branch'));
            set('#apd-stylist', btn.getAttribute('data-stylist'));
            set('#apd-price', (btn.getAttribute('data-price') || '0') + ' ₺');
            set('#apd-status', btn.getAttribute('data-status'));
            set('#apd-note', btn.getAttribute('data-note') || '-');
        });
    }

    // ERTELE MODAL
    const resEl = document.getElementById('rescheduleModal');
    if (resEl) {
        const form = resEl.querySelector('#rescheduleForm');
        resEl.addEventListener('show.bs.modal', function (event) {
            const btn = event.relatedTarget;
            const id = btn.getAttribute('data-id') || '0';
            const current = btn.getAttribute('data-current') || '';
            const duration = btn.getAttribute('data-duration') || '30';

            form.querySelector('input[name="Id"]').value = parseInt(id, 10);
            form.querySelector('input[name="NewStartAt"]').value = current;
            form.querySelector('input[name="DurationMin"]').value = duration;
        });

        resEl.addEventListener('hidden.bs.modal', function () {
            form.reset();
        });
    }

    // İPTAL MODAL
    const cancelEl = document.getElementById('cancelModal');
    if (cancelEl) {
        const form = cancelEl.querySelector('#cancelForm');
        cancelEl.addEventListener('show.bs.modal', function (event) {
            const btn = event.relatedTarget;
            const id = btn.getAttribute('data-id') || '0';
            form.querySelector('input[name="Id"]').value = parseInt(id, 10);
        });

        cancelEl.addEventListener('hidden.bs.modal', function () {
            form.reset();
        });
    }
})();

function loadAppointments() {
    $.ajax({
        url: '/api/v1/appointments',
        method: 'GET',
        success: function(data) {
            renderAppointments(data);
        },
        error: function(xhr) {
            showError('Randevular yüklenemedi: ' + xhr.responseText);
        }
    });
}

function createAppointment() {
    var formData = $('#createAppointmentForm').serialize();
    
    $.ajax({
        url: '/api/v1/appointments',
        method: 'POST',
        data: formData,
        success: function(data) {
            showSuccess('Randevu başarıyla oluşturuldu');
            $('#createAppointmentModal').modal('hide');
            loadAppointments();
        },
        error: function(xhr) {
            showError('Randevu oluşturulamadı: ' + xhr.responseText);
        }
    });
}

function cancelAppointment(appointmentId) {
    if (confirm('Bu randevuyu iptal etmek istediğinizden emin misiniz?')) {
        // Form oluştur ve submit et
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Admin/Appointments/Cancel';
        
        // CSRF token ekle
        const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const csrfInput = document.createElement('input');
        csrfInput.type = 'hidden';
        csrfInput.name = '__RequestVerificationToken';
        csrfInput.value = csrfToken;
        form.appendChild(csrfInput);
        
        // Appointment ID ekle
        const idInput = document.createElement('input');
        idInput.type = 'hidden';
        idInput.name = 'id';
        idInput.value = appointmentId;
        form.appendChild(idInput);
        
        // Formu sayfaya ekle ve submit et
        document.body.appendChild(form);
        form.submit();
    }
}
