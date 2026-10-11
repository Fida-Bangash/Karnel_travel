// crud-form.js
// Small helper for Admin create/edit pages:
//  1. Live image preview (with type and size check)
//  2. Character counter for textareas
(function () {
    var MAX_BYTES = 2 * 1024 * 1024;                 // 2 MB (same limit as ImageUploader.cs)
    var ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];

    // ---- Image preview ----
    document.querySelectorAll('input[type=file][data-preview]').forEach(function (input) {
        var img = document.getElementById(input.dataset.preview);
        var nameEl = document.getElementById(input.dataset.name);
        var placeholder = document.getElementById(input.dataset.placeholder);
        var error = document.getElementById(input.dataset.error);
        var box = input.closest('.upload-box');

        input.addEventListener('change', function () {
            var file = input.files[0];
            if (error) error.textContent = '';
            if (!file) return;

            // Wrong type or too big: clear the choice and show a message
            if (ALLOWED.indexOf(file.type) === -1) {
                input.value = '';
                if (error) error.textContent = 'Only JPG, PNG or WEBP images are allowed.';
                return;
            }
            if (file.size > MAX_BYTES) {
                input.value = '';
                if (error) error.textContent = 'Image is too large. Maximum size is 2 MB.';
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                img.src = e.target.result;
                img.classList.add('show');
                if (placeholder) placeholder.style.display = 'none';
                if (nameEl) nameEl.textContent = file.name;
            };
            reader.readAsDataURL(file);
        });

        // Highlight the box while a file is dragged over it
        if (box) {
            ['dragenter', 'dragover'].forEach(function (ev) {
                box.addEventListener(ev, function () { box.classList.add('drag'); });
            });
            ['dragleave', 'drop'].forEach(function (ev) {
                box.addEventListener(ev, function () { box.classList.remove('drag'); });
            });
        }
    });

    // ---- Character counter ----
    document.querySelectorAll('textarea[data-counter]').forEach(function (ta) {
        var out = document.getElementById(ta.dataset.counter);
        var max = ta.getAttribute('maxlength');
        function update() { if (out) out.textContent = ta.value.length + ' / ' + max; }
        ta.addEventListener('input', update);
        update();
    });
})();
