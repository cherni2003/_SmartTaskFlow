// ============================================
// Smart Task Flow - Custom JavaScript
// ============================================

// Filtrer les tâches par statut
function filterTasks(status) {
    const taskCards = document.querySelectorAll('.task-card');

    taskCards.forEach(card => {
        if (status === 'all') {
            card.style.display = 'block';
            card.classList.add('fade-in');
        } else if (status === 'overdue') {
            const isOverdue = card.dataset.overdue === 'true';
            card.style.display = isOverdue ? 'block' : 'none';
            if (isOverdue) card.classList.add('fade-in');
        } else {
            const cardStatus = card.dataset.status;
            card.style.display = cardStatus === status ? 'block' : 'none';
            if (cardStatus === status) card.classList.add('fade-in');
        }
    });

    // Mettre à jour le bouton actif
    document.querySelectorAll('.filter-buttons .btn').forEach(btn => {
        btn.classList.remove('active');
    });
    event.target.classList.add('active');
}

// Recherche en temps réel
function searchTasks() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    const searchTerm = searchInput.value.toLowerCase();
    const taskCards = document.querySelectorAll('.task-card');

    taskCards.forEach(card => {
        const title = card.querySelector('.card-title')?.textContent.toLowerCase() || '';
        const description = card.querySelector('.card-text')?.textContent.toLowerCase() || '';

        if (title.includes(searchTerm) || description.includes(searchTerm)) {
            card.style.display = 'block';
        } else {
            card.style.display = 'none';
        }
    });
}

// Confirmation de suppression avec style
function confirmDelete(taskTitle) {
    return confirm(`⚠️ Êtes-vous sûr de vouloir supprimer la tâche "${taskTitle}" ?\n\nCette action est irréversible.`);
}

// Animation au chargement de la page
document.addEventListener('DOMContentLoaded', function () {
    // Ajouter l'animation fade-in aux cartes
    const cards = document.querySelectorAll('.card, .task-card');
    cards.forEach((card, index) => {
        setTimeout(() => {
            card.classList.add('fade-in');
        }, index * 50);
    });

    // Auto-dismiss des alerts après 5 secondes
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            const closeBtn = alert.querySelector('.btn-close');
            if (closeBtn) closeBtn.click();
        }, 5000);
    });
});

// Trier les tâches
function sortTasks(sortBy) {
    const container = document.querySelector('.row');
    if (!container) return;

    const taskCards = Array.from(container.querySelectorAll('.task-card'));

    taskCards.sort((a, b) => {
        switch (sortBy) {
            case 'priority':
                const priorityA = parseInt(a.dataset.priority) || 0;
                const priorityB = parseInt(b.dataset.priority) || 0;
                return priorityB - priorityA;

            case 'deadline':
                const deadlineA = new Date(a.dataset.deadline || '9999-12-31');
                const deadlineB = new Date(b.dataset.deadline || '9999-12-31');
                return deadlineA - deadlineB;

            case 'duration':
                const durationA = parseInt(a.dataset.duration) || 0;
                const durationB = parseInt(b.dataset.duration) || 0;
                return durationA - durationB;

            case 'title':
                const titleA = a.querySelector('.card-title')?.textContent || '';
                const titleB = b.querySelector('.card-title')?.textContent || '';
                return titleA.localeCompare(titleB);

            default:
                return 0;
        }
    });

    // Réorganiser les cartes
    taskCards.forEach(card => container.appendChild(card.parentElement));
}

// Ajouter un compteur de caractères pour les descriptions
function addCharacterCounter() {
    const textarea = document.querySelector('textarea[name="Description"]');
    if (!textarea) return;

    const maxLength = 2000;
    const counter = document.createElement('small');
    counter.className = 'text-muted';
    counter.style.display = 'block';
    counter.style.marginTop = '0.25rem';

    const updateCounter = () => {
        const remaining = maxLength - textarea.value.length;
        counter.textContent = `${remaining} caractères restants`;
        counter.style.color = remaining < 100 ? '#e74c3c' : '#6c757d';
    };

    textarea.addEventListener('input', updateCounter);
    textarea.parentElement.appendChild(counter);
    updateCounter();
}

// Validation du formulaire en temps réel
function setupFormValidation() {
    const form = document.querySelector('form');
    if (!form) return;

    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');

    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.value.trim() === '') {
                this.classList.add('is-invalid');
            } else {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            }
        });
    });
}

// Calculer et afficher le temps restant
function updateTimeRemaining() {
    const deadlineCells = document.querySelectorAll('[data-deadline]');

    deadlineCells.forEach(cell => {
        const deadline = new Date(cell.dataset.deadline);
        const now = new Date();
        const diff = deadline - now;

        if (diff < 0) {
            cell.classList.add('text-danger', 'fw-bold');
        } else if (diff < 24 * 60 * 60 * 1000) {
            cell.classList.add('text-warning', 'fw-bold');
        }
    });
}

// Initialiser tout au chargement
window.addEventListener('DOMContentLoaded', function () {
    addCharacterCounter();
    setupFormValidation();
    updateTimeRemaining();

    // Ajouter un indicateur de chargement
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const form = this.closest('form');
            if (form && form.checkValidity()) {
                this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Chargement...';
                this.disabled = true;
            }
        });
    });
});

// Fonction pour mettre à jour dynamiquement la durée formatée
function updateDurationDisplay() {
    const durationInput = document.querySelector('input[name="EstimatedDuration"]');
    if (!durationInput) return;

    const display = document.createElement('small');
    display.className = 'text-info d-block mt-1';

    const updateDisplay = () => {
        const minutes = parseInt(durationInput.value) || 0;
        if (minutes < 60) {
            display.textContent = `${minutes} minutes`;
        } else {
            const hours = Math.floor(minutes / 60);
            const mins = minutes % 60;
            display.textContent = mins > 0 ? `${hours}h ${mins}min` : `${hours}h`;
        }
    };

    durationInput.addEventListener('input', updateDisplay);
    durationInput.parentElement.appendChild(display);
    updateDisplay();
}

// Ajouter cette fonction à l'initialisation
window.addEventListener('DOMContentLoaded', updateDurationDisplay);

// Fonction pour afficher des notifications toast
function showToast(message, type = 'success') {
    const toastContainer = document.getElementById('toastContainer') || createToastContainer();

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0 show`;
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toastContainer';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '9999';
    document.body.appendChild(container);
    return container;
}