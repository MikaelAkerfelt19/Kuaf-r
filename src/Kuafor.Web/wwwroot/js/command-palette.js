(function(){
  const overlay = document.getElementById('cmd-overlay');
  const input = document.getElementById('cmd-input');
  const results = document.getElementById('cmd-results');
  const globalSearch = document.getElementById('globalSearch');

  if(!overlay || !input || !results) return;

  const commands = [
    { icon:'✂️', title:'Yeni Randevu', desc:'Hızlıca yeni randevu oluştur', url:'/Appointments/New', keywords:'randevu olustur yeni create' },
    { icon:'🗓️', title:'Randevularım', desc:'Geçmiş ve yaklaşan randevular', url:'/Appointments', keywords:'randevularim liste gecmis yaklasan' },
    { icon:'🧑\u200d🦱', title:'Kuaförler', desc:'Usta listesi ve puanlar', url:'/Stylists', keywords:'kuaforler stilist usta' },
    { icon:'🧴', title:'Hizmetler', desc:'Tüm hizmetleri görüntüle', url:'/Services', keywords:'hizmetler servisler' },
    { icon:'🎁', title:'Fırsatlar', desc:'Kupon ve kampanyalar', url:'/Coupons', keywords:'kupon kampanya indirim' },
    { icon:'⚙️', title:'Profil', desc:'Bilgilerini düzenle', url:'/Profile', keywords:'profil ayarlar hesap' },
    { icon:'❔', title:'Destek', desc:'Yardım & SSS', url:'/Support', keywords:'destek yardim sss' },
    { icon:'🚪', title:'Çıkış Yap', desc:'Hesaptan çıkış', url:'/Account/Logout', keywords:'cikis logout' }
  ];

  let activeIndex = -1;

  function openPalette(prefill=""){
    overlay.classList.remove('d-none');
    setTimeout(()=>{ input.value = prefill; input.focus(); filter(); }, 0);
  }
  function closePalette(){
    overlay.classList.add('d-none');
    activeIndex = -1; results.innerHTML = '';
    if(globalSearch) globalSearch.blur();
  }

  function render(list){
    results.innerHTML = '';
    if(list.length === 0){
      const empty = document.createElement('div');
      empty.className = 'p-3 text-muted';
      empty.textContent = 'Sonuç bulunamadı';
      results.appendChild(empty);
      return;
    }
    list.forEach((c, i)=>{
      const item = document.createElement('div');
      item.className = 'cmd-item';
      item.setAttribute('role','button');
      item.innerHTML = `<div class="cmd-icon">${c.icon||''}</div>
                        <div>
                          <div class="cmd-item-title">${c.title}</div>
                          <div class="cmd-item-desc">${c.desc}</div>
                        </div>`;
      item.addEventListener('click', ()=>{ window.location.href = c.url; });
      results.appendChild(item);
    });
    setActive(0);
  }

  function setActive(i){
    const items = results.querySelectorAll('.cmd-item');
    items.forEach(el=>el.classList.remove('active'));
    if(items[i]){ items[i].classList.add('active'); activeIndex = i; items[i].scrollIntoView({block:'nearest'}); }
  }

  function filter(){
    const q = (input.value || '').toLowerCase().trim();
    const list = !q ? commands : commands.filter(c =>
      (c.title + ' ' + c.desc + ' ' + c.keywords).toLowerCase().includes(q)
    );
    render(list);
  }

  // Global kısayollar
  document.addEventListener('keydown', (e)=>{
    const isMac = navigator.platform.toUpperCase().includes('MAC');
    const mod = isMac ? e.metaKey : e.ctrlKey;
    if(mod && e.key.toLowerCase() === 'k'){
      e.preventDefault();
      openPalette('');
    }
    if(e.key === 'Escape' && !overlay.classList.contains('d-none')){
      closePalette();
    }
    if(!overlay.classList.contains('d-none')){
      const items = results.querySelectorAll('.cmd-item');
      if(e.key === 'ArrowDown'){ e.preventDefault(); setActive(Math.min(activeIndex+1, items.length-1)); }
      if(e.key === 'ArrowUp'){ e.preventDefault(); setActive(Math.max(activeIndex-1, 0)); }
      if(e.key === 'Enter'){ e.preventDefault(); if(items[activeIndex]) items[activeIndex].click(); }
    }
  });

  input.addEventListener('input', filter);

  // Üstteki arama alanına tıklayınca paleti aç
  if(globalSearch){
    globalSearch.addEventListener('focus', ()=> openPalette(globalSearch.value || ''));
    globalSearch.addEventListener('keydown', (e)=>{
      if(e.key === 'Enter'){ e.preventDefault(); openPalette(globalSearch.value || ''); }
    });
  }

  // Overlay dışına tıklama ile kapatma
  overlay.addEventListener('click', (e)=>{
    if(e.target === overlay) closePalette();
  });
})();