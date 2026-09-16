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
    var isOpenable = document.querySelector('.openable-toggle');
    if (!isOpenable) {
        return;
    }

    var dependents = document.querySelectorAll('.openable-dependent');
    var dependentHeaders = document.querySelectorAll('.openable-col-header');
    function syncOpenableDependents() {
        dependents.forEach(function (element) {
            element.disabled = !isOpenable.checked;
            if (element.disabled && element.type === 'checkbox') {
                element.checked = false;
            }

            var container = element.closest('.form-row, td');
            if (container) {
                container.classList.toggle('is-openable-disabled', element.disabled);
            }
        });

        dependentHeaders.forEach(function (header) {
            header.classList.toggle('is-openable-disabled', !isOpenable.checked);
        });
    }

    isOpenable.addEventListener('change', syncOpenableDependents);
    syncOpenableDependents();
});

// Bulk-add scan page: builds a running list of scanned barcodes client-side, aggregating repeats into quantities.
document.addEventListener('DOMContentLoaded', function () {
    var input = document.getElementById('bulkBarcodeInput');
    if (!input) {
        return;
    }

    var lookupScript = document.getElementById('bulkItemLookup');
    var lookupItems = lookupScript ? JSON.parse(lookupScript.textContent) : [];
    var namesByBarcode = {};
    lookupItems.forEach(function (item) {
        namesByBarcode[item.Barcode] = item.Name;
    });

    var scanned = [];
    var list = document.getElementById('bulkScanList');
    var batchInput = document.getElementById('bulkBatchInput');
    var reviewButton = document.getElementById('bulkReviewButton');

    function render() {
        list.innerHTML = '';
        scanned.forEach(function (entry, index) {
            var li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';

            var label = document.createElement('span');
            label.textContent = (entry.name || ('Unknown: ' + entry.barcode)) + ' × ' + entry.count;
            if (!entry.name) {
                label.classList.add('text-warning');
            }

            var removeButton = document.createElement('button');
            removeButton.type = 'button';
            removeButton.className = 'btn-icon';
            removeButton.setAttribute('aria-label', 'Remove');
            removeButton.textContent = '✕';
            removeButton.addEventListener('click', function () {
                scanned.splice(index, 1);
                render();
            });

            li.appendChild(label);
            li.appendChild(removeButton);
            list.appendChild(li);
        });

        batchInput.value = scanned.map(function (entry) { return entry.barcode + '|' + entry.count; }).join(',');
        reviewButton.disabled = scanned.length === 0;
    }

    input.addEventListener('keydown', function (event) {
        if (event.key !== 'Enter') {
            return;
        }

        event.preventDefault();
        var barcode = input.value.trim();
        input.value = '';
        if (!barcode) {
            return;
        }

        var existing = scanned.find(function (entry) { return entry.barcode === barcode; });
        if (existing) {
            existing.count++;
        } else {
            scanned.push({ barcode: barcode, name: namesByBarcode[barcode] || null, count: 1 });
        }

        render();
    });
});

// Home page: toggles the inline "checkout multiple" quantity prompt open/closed.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.checkout-multi-toggle').forEach(function (toggle) {
        toggle.addEventListener('click', function () {
            var form = toggle.parentElement.querySelector('.checkout-multi-form');
            if (!form) {
                return;
            }

            form.classList.toggle('d-none');
            form.classList.toggle('d-flex');
            if (!form.classList.contains('d-none')) {
                var input = form.querySelector('input[type="number"]');
                if (input) {
                    input.focus();
                    input.select();
                }
            }
        });
    });
});

// Home page: lets a printed "quick action" QR code (scanned into the barcode field) trigger a button on the scanned item's card directly, instead of being looked up as a barcode. Only acts when exactly one matching element is on the page, to avoid guessing between multiple stock rows.
document.addEventListener('DOMContentLoaded', function () {
    var scanForm = document.querySelector('form[method="get"]');
    var scanInput = scanForm ? scanForm.querySelector('input[name="Barcode"]') : null;
    if (!scanForm || !scanInput) {
        return;
    }

    var scanActions = {
        'SW-ACTION:CHECKOUT1': 'checkout1'
    };

    scanForm.addEventListener('submit', function (event) {
        var action = scanActions[scanInput.value.trim()];
        if (!action) {
            return;
        }

        event.preventDefault();
        scanInput.value = '';

        var targets = document.querySelectorAll('[data-scan-action="' + action + '"]');
        if (targets.length !== 1) {
            return;
        }

        var target = targets[0];
        if (target.tagName === 'FORM') {
            target.requestSubmit();
        } else {
            target.click();
        }
    });
});