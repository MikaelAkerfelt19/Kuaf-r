// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Müşteri paneline tıklandığında modal'ları aç
function openCustomerPanel() {
    // Eğer kullanıcı login olmamışsa login modal'ını aç
    if (!document.querySelector('[data-user-authenticated]')) {
        const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
        loginModal.show();
    } else {
        // Eğer login olmuşsa customer area'ya git
        window.location.href = '/Customer/Home';
    }
}
