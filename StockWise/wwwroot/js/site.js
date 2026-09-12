// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Default expiry offsets, in days from today, by storage category name.
var categoryExpiryOffsetDays = { Fridge: 3, Freezer: 90, Pantry: 7 };

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('select[data-expiry-target]').forEach(function (select) {
        var target = document.getElementById(select.dataset.expiryTarget);
        if (!target) {
            return;
        }

        select.addEventListener('change', function () {
            var option = select.options[select.selectedIndex];
            var category = option ? option.dataset.category : null;
            var offsetDays = category ? categoryExpiryOffsetDays[category] : null;
            if (!offsetDays) {
                return;
            }

            var date = new Date();
            date.setDate(date.getDate() + offsetDays);
            target.value = date.toISOString().slice(0, 10);
        });
    });
});

// Disables the "opened" category checkboxes and the expiry-after-opening field unless "Can be opened" is checked.
document.addEventListener('DOMContentLoaded', function () {
    var isOpenable = document.getElementById('IsOpenable');
    if (!isOpenable) {
        return;
    }

    var dependents = document.querySelectorAll('.openable-dependent');
    function syncOpenableDependents() {
        dependents.forEach(function (element) {
            element.disabled = !isOpenable.checked;
            if (element.disabled && element.type === 'checkbox') {
                element.checked = false;
            }
        });
    }

    isOpenable.addEventListener('change', syncOpenableDependents);
    syncOpenableDependents();
});