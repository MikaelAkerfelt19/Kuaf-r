(function () {
  const input = document.getElementById('avatarInput');
  const img = document.getElementById('avatarPreview');
  if (input && img) {
    input.addEventListener('change', (e) => {
      const file = e.target.files && e.target.files[0];
      if (!file) return;
      const reader = new FileReader();
      reader.onload = () => { img.src = reader.result; };
      reader.readAsDataURL(file);
    });
  }
  window.profileIdentitySave = function () { alert('Mock: Kimlik & iletişim kaydedildi.'); };
  window.profileIdentityReset = function () { location.reload(); };
})();