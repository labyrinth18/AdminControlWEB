// =============================================
// AdminControl - Site JavaScript
// =============================================

document.addEventListener('DOMContentLoaded', function () {
    
    // =============================================
    // SIDEBAR TOGGLE
    // =============================================
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });

        // Close sidebar when clicking outside (mobile)
        document.addEventListener('click', function (event) {
            if (window.innerWidth < 992) {
                if (!sidebar.contains(event.target) && !sidebarToggle.contains(event.target)) {
                    sidebar.classList.remove('show');
                }
            }
        });
    }

    // =============================================
    // AUTO-DISMISS ALERTS
    // =============================================
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000); // Auto-close after 5 seconds
    });

    // =============================================
    // CONFIRM DELETE
    // =============================================
    const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (e) {
            const message = this.getAttribute('data-confirm-delete') || 'Ви впевнені, що хочете видалити?';
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // =============================================
    // FORM VALIDATION STYLING
    // =============================================
    const forms = document.querySelectorAll('form');
    forms.forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // =============================================
    // TOGGLE PASSWORD VISIBILITY
    // =============================================
    const togglePasswordButtons = document.querySelectorAll('[data-toggle-password]');
    togglePasswordButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            const targetId = this.getAttribute('data-toggle-password');
            const input = document.getElementById(targetId);
            const icon = this.querySelector('i');

            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        });
    });

    // =============================================
    // ACTIVE NAV LINK HIGHLIGHT
    // =============================================
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');
    
    navLinks.forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href.toLowerCase()) && href !== '/') {
            link.classList.add('active');
        }
    });

    // =============================================
    // TABLE ROW CLICK (if data-href)
    // =============================================
    const clickableRows = document.querySelectorAll('tr[data-href]');
    clickableRows.forEach(function (row) {
        row.style.cursor = 'pointer';
        row.addEventListener('click', function (e) {
            // Don't navigate if clicking on buttons/links inside the row
            if (e.target.closest('a, button, .btn, input, .dropdown')) {
                return;
            }
            window.location.href = this.getAttribute('data-href');
        });
    });

    // =============================================
    // TOOLTIPS INIT
    // =============================================
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => 
        new bootstrap.Tooltip(tooltipTriggerEl)
    );

    // =============================================
    // SEARCH INPUT DEBOUNCE
    // =============================================
    let searchTimeout;
    const searchInputs = document.querySelectorAll('[data-search-auto]');
    searchInputs.forEach(function (input) {
        input.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                input.form.submit();
            }, 500);
        });
    });

});

// =============================================
// HELPER FUNCTIONS
// =============================================

// Format date
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('uk-UA', options);
}

// Show loading spinner on form submit
function showLoadingOnSubmit(form) {
    form.addEventListener('submit', function () {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Зачекайте...';
        }
    });
}
