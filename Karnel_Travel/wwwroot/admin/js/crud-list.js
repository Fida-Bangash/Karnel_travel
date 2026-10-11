// crud-list.js
// Live search + filter chips for Admin list pages (no page reload).
// Rows need:  data-search="lowercase text"  and (optional)  data-filter="value"
// Chips need: data-filter="value"  (empty value = show all)
(function () {
    var table = document.getElementById('listTable');
    if (!table) return;

    var rows = table.querySelectorAll('tbody tr');
    var search = document.getElementById('listSearch');
    var chips = document.querySelectorAll('#listChips .crud-chip');
    var noMatch = document.getElementById('noMatch');
    var active = '';

    function apply() {
        var q = search ? (search.value || '').trim().toLowerCase() : '';
        var shown = 0;
        rows.forEach(function (r) {
            var okText = r.dataset.search.indexOf(q) !== -1;
            var okFilter = active === '' || r.dataset.filter === active;
            var ok = okText && okFilter;
            r.style.display = ok ? '' : 'none';
            if (ok) shown++;
        });
        if (noMatch) noMatch.style.display = shown === 0 ? '' : 'none';
    }

    if (search) search.addEventListener('input', apply);
    chips.forEach(function (c) {
        c.addEventListener('click', function () {
            chips.forEach(function (x) { x.classList.remove('active'); });
            c.classList.add('active');
            active = c.dataset.filter;
            apply();
        });
    });
})();
