(function(){
    let editingId = 0;
    const modalEl = document.getElementById('addrModal');
    const modal = modalEl ? new bootstrap.Modal(modalEl) : null;

    function $id(x){ return document.getElementById(x); }

    function getFields(){
        return {
            title: $id('addrTitle').value.trim(),
            line1: $id('addrLine1').value.trim(),
            district: $id('addrDistrict').value.trim(),
            city: $id('addrCity').value.trim(),
            zip: $id('addrZip').value.trim(),
            isDefault: $id('addrIsDefault').checked
        };
    }

    window.addrNew = function(){
        editingId = 0;
        $id('addrModalLabel').textContent = 'Yeni Adres';
        ['addrTitle', 'addrLine1', 'addrDistrict', 'addrCity', 'addrZip'].forEach(k=> $id(k).value = '');
        $id('addrIsDefault').checked = false;
    }

    window.addrEdit = function(id){
    editingId = id;
    $id('addrModalLabel').textContent = 'Adresi Düzenle';
    const card = document.getElementById('addrDefault_'+id).closest('.card');
    const lines = card.querySelectorAll('.text-muted');
    $id('addrTitle').value = card.querySelector('.fw-semibold').textContent.replace(' Varsayılan','').trim();
    $id('addrLine1').value = lines[0].textContent.trim();
    const cityLine = lines[1].textContent.trim(); // "İlçe / Şehir Zip"
    const parts = cityLine.split('/');
    $id('addrDistrict').value = (parts[0]||'').trim();
    const cityZip = (parts[1]||'').trim().split(' ');
    $id('addrCity').value = cityZip[0]||'';
    $id('addrZip').value = cityZip[1]||'';
    $id('addrDefault').checked = document.getElementById('addrDefault_'+id).checked;
    modal && modal.show();
  };

  window.addrSave = function(){
    const f = getFields();
    if(!f.title || !f.line1 || !f.city){
      alert('Başlık, Adres satırı ve Şehir zorunlu.');
      return;
    }
    alert('Mock: Adres kaydedildi. (id='+(editingId||'yeni')+')');
    modal && modal.hide();
  };

  window.addrDelete = function(id){
    if(confirm('Bu adres silinsin mi? (id='+id+')')){
      alert('Mock: Adres silindi.');
    }
  };

  window.addrMakeDefault = function(id){
    alert('Mock: Varsayılan adres id='+id);
  };
})();