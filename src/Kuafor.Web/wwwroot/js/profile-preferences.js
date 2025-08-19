(function (){
    window.profilePrefsSave = function(){
        const branch  = document.querySelector('[name="PreferredBranch"]')?.value;
        const stylist = document.querySelector('[name="PreferredStylist"]')?.value;
        const band    = document.querySelector('[name="PreferredTimeBand"]')?.value;
        const flex    = document.querySelector('[name="FlexMinutes"]')?.value;

        if(flex !== undefined){
            const n = Number(flex);
            if(isNaN(n) || n < 0 || n > 30){
                alert('Lütfen esneklik dakikasını 0 ile 30 arasında bir değer olarak giriniz.');
                return;
            }
        }
        alert(`Mock: Tercihler kaydedildi.\nŞube: ${branch||'-'}\nKuaför: ${stylist||'-'}\nZaman: ${band||'-'}\nEsneklik: ±${flex||0}dk`)
    }
})();