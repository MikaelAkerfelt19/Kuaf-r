(function () {
  const sw = document.getElementById("onlyAvailSwitch");
  const grid = document.getElementById("slotGrid");
  if (sw && grid) {
    function apply() {
      const on = sw.checked;
      grid.querySelectorAll("[data-available='false']").forEach((el) => {
        el.style.display = on ? "none" : "";
      });
    }
    sw.addEventListener("change", apply);
    apply();
  }
})();