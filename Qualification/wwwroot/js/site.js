/**
 * ==========================================
 * Person Management SPA - Main JavaScript
 * ==========================================
 * 
 * Single Page Application using jQuery & AJAX
 * Author: Senior Frontend Developer
 * Version: 1.0.0
 */

// ==========================================
// CONFIGURATION
// ==========================================
AccessControl

// ==========================================
// APPLICATION STATE
// ==========================================
const AppState = {
    isEditMode: false,
    currentPersonId: 0,
    deletePersonId: 0,
    qualificationIndex: 0
};

// ==========================================
// COMMON AJAX FUNCTION
// ==========================================
/**
 * Reusable AJAX function for all API calls
 * @param {string} method - HTTP method (GET, POST, PUT, DELETE)
 * @param {string} url - API endpoint URL
 * @param {object|null} data - Request body data
 * @param {function} successCallback - Success handler
 * @param {function} errorCallback - Error handler
 */
function apiCall(method, url, data, successCallback, errorCallback) {
    // Show loading overlay
    showLoading();

    // Build AJAX options
    const ajaxOptions = {
        url: url,
        method: method,
        dataType: 'json',
        contentType: 'application/json',

        // ==========================================
        // CORS Headers Configuration
        // ==========================================
        xhrFields: {
            withCredentials: false  // Set to true if you need to send cookies
        },
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            // Custom header to identify the origin (API must be configured to accept this)
            'X-Custom-Origin': window.location.origin
        },

        // ==========================================
        // Before Send - Add any additional headers
        // ==========================================
        beforeSend: function (xhr) {
            // You can add more headers here if needed
            // xhr.setRequestHeader('Authorization', 'Bearer ' + token);
            console.log('Making request to:', url);
        },

        success: function (response) {
            hideLoading();
            if (typeof successCallback === 'function') {
                successCallback(response);
            }
        },
        error: function (xhr, status, error) {
            hideLoading();
            console.error('API Error:', { xhr, status, error });

            // Extract error message
            let errorMessage = 'An error occurred';
            try {
                if (xhr.responseJSON) {
                    errorMessage = xhr.responseJSON.message ||
                        xhr.responseJSON.title ||
                        Object.values(xhr.responseJSON.errors || {}).flat().join(', ') ||
                        errorMessage;
                } else if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
            } catch (e) {
                errorMessage = error || status;
            }

            if (typeof errorCallback === 'function') {
                errorCallback(errorMessage, xhr);
            } else {
                showToast(errorMessage, 'error');
            }
        }
    };

    // Add data for POST/PUT requests
    if (data && (method === 'POST' || method === 'PUT')) {
        ajaxOptions.data = JSON.stringify(data);
    }

    // Execute AJAX call
    $.ajax(ajaxOptions);
}

// ==========================================
// UI HELPER FUNCTIONS
// ==========================================

/**
 * Show loading overlay
 */
function showLoading() {
    $('#loadingOverlay').addClass('show');
}

/**
 * Hide loading overlay
 */
function hideLoading() {
    $('#loadingOverlay').removeClass('show');
}

/**
 * Show toast notification
 * @param {string} message - Message to display
 * @param {string} type - Toast type (success, error, warning, info)
 */
function showToast(message, type = 'success') {
    const icons = {
        success: 'check-circle-fill',
        error: 'x-circle-fill',
        warning: 'exclamation-triangle-fill',
        info: 'info-circle-fill'
    };

    const toastId = 'toast_' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast toast-${type}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-body d-flex align-items-center">
                <i class="bi bi-${icons[type]} me-2 fs-5"></i>
                <span>${escapeHtml(message)}</span>
                <button type="button" class="btn-close btn-close-white ms-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    $('#toastContainer').append(toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 4000 });
    toast.show();

    // Remove from DOM after hidden
    toastElement.addEventListener('hidden.bs.toast', function () {
        $(this).remove();
    });
}

/**
 * Escape HTML to prevent XSS
 * @param {string} text - Text to escape
 * @returns {string} Escaped text
 */
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ==========================================
// PERSON DATA FUNCTIONS
// ==========================================

/**
 * Load all persons and display in grid
 */
function loadPersons() {
    const url = CONFIG.API_BASE_URL + CONFIG.ENDPOINTS.GET_ALL;

    apiCall('GET', url, null,
        function (data) {
            renderPersonTable(data);
        },
        function (error) {
            showToast('Failed to load persons: ' + error, 'error');
            renderPersonTable([]);
        }
    );
}

/**
 * Render person data in table
 * @param {Array} persons - Array of person objects
 */
function renderPersonTable(persons) {
    const tbody = $('#personTableBody');
    tbody.empty();

    if (!persons || persons.length === 0) {
        tbody.html(`
            <tr class="empty-row">
                <td colspan="6">
                    <i class="bi bi-inbox"></i>
                    <p class="mb-0">No persons found. Add a new person using the form above.</p>
                </td>
            </tr>
        `);
        $('#totalRecords').text('0');
        return;
    }

    persons.forEach((person, index) => {
        const row = `
            <tr data-id="${person.personId}">
                <td><strong>${index + 1}</strong></td>
                <td>${escapeHtml(person.name)}</td>
                <td>
                    <i class="bi bi-phone text-muted me-1"></i>
                    ${escapeHtml(person.mobileNo)}
                </td>
                <td>
                    <span class="badge bg-secondary">${person.age} yrs</span>
                </td>
                <td>${escapeHtml(person.address) || '<span class="text-muted">-</span>'}</td>
                <td>
                    <div class="action-buttons">
                        <button class="btn btn-info btn-action btn-view" 
                                data-id="${person.personId}" title="View Details">
                            <i class="bi bi-eye"></i> View
                        </button>
                        <button class="btn btn-warning btn-action btn-edit" 
                                data-id="${person.personId}" title="Edit">
                            <i class="bi bi-pencil"></i> Edit
                        </button>
                        <button class="btn btn-danger btn-action btn-delete" 
                                data-id="${person.personId}" 
                                data-name="${escapeHtml(person.name)}" title="Delete">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                    </div>
                </td>
            </tr>
        `;
        tbody.append(row);
    });

    $('#totalRecords').text(persons.length);
}

/**
 * Get person by ID for editing
 * @param {number} personId - Person ID
 */
function getPersonById(personId) {
    const url = CONFIG.API_BASE_URL + CONFIG.ENDPOINTS.GET_BY_ID.replace('{id}', personId);

    apiCall('GET', url, null,
        function (person) {
            populateFormForEdit(person);
        },
        function (error) {
            showToast('Failed to load person details: ' + error, 'error');
        }
    );
}

/**
 * View person details in modal
 * @param {number} personId - Person ID
 */
function viewPersonDetails(personId) {
    const url = CONFIG.API_BASE_URL + CONFIG.ENDPOINTS.GET_BY_ID.replace('{id}', personId);

    apiCall('GET', url, null,
        function (person) {
            renderViewModal(person);
        },
        function (error) {
            showToast('Failed to load person details: ' + error, 'error');
        }
    );
}

/**
 * Render view modal with person details
 * @param {object} person - Person object
 */
function renderViewModal(person) {
    let qualificationsHtml = '<p class="text-muted">No qualifications</p>';

    if (person.qualifications && person.qualifications.length > 0) {
        qualificationsHtml = `
            <table class="table table-bordered qual-table">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Qualification</th>
                        <th>Marks (%)</th>
                    </tr>
                </thead>
                <tbody>
                    ${person.qualifications.map((q, i) => `
                        <tr>
                            <td>${i + 1}</td>
                            <td>${escapeHtml(q.qualificationName)}</td>
                            <td><span class="badge bg-primary">${q.marks}%</span></td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;
    }

    const html = `
        <div class="detail-section">
            <h6><i class="bi bi-person me-2"></i>Personal Information</h6>
            <div class="detail-item">
                <span class="detail-label">Name:</span>
                <span class="detail-value">${escapeHtml(person.name)}</span>
            </div>
            <div class="detail-item">
                <span class="detail-label">Mobile No:</span>
                <span class="detail-value">${escapeHtml(person.mobileNo)}</span>
            </div>
            <div class="detail-item">
                <span class="detail-label">Age:</span>
                <span class="detail-value">${person.age} years</span>
            </div>
            <div class="detail-item">
                <span class="detail-label">Address:</span>
                <span class="detail-value">${escapeHtml(person.address) || '-'}</span>
            </div>
        </div>
        <div class="detail-section">
            <h6><i class="bi bi-mortarboard me-2"></i>Qualifications (${person.qualifications?.length || 0})</h6>
            ${qualificationsHtml}
        </div>
    `;

    $('#viewModalBody').html(html);
    const modal = new bootstrap.Modal(document.getElementById('viewModal'));
    modal.show();
}

/**
 * Save person (Create or Update)
 */
function savePerson() {
    // Validate form
    if (!validateForm()) {
        return;
    }

    // Collect form data
    const personData = collectFormData();

    if (AppState.isEditMode) {
        // Update existing person
        const url = CONFIG.API_BASE_URL + CONFIG.ENDPOINTS.UPDATE.replace('{id}', AppState.currentPersonId);

        apiCall('PUT', url, personData,
            function (response) {
                showToast('Person updated successfully!', 'success');
                resetForm();
                loadPersons();
            },
            function (error) {
                showToast('Failed to update person: ' + error, 'error');
            }
        );
    } else {
        // Create new person
        const url = CONFIG.API_BASE_URL + CONFIG.ENDPOINTS.SAVE;

        apiCall('POST', url, personData,
            function (response) {
                showToast('Person created successfully!', 'success');
                resetForm();
                loadPersons();
            },
            function (error) {
                showToast('Failed to create person: ' + error, 'error');
            }
        );
    }
}

/**
 * Delete person
 * @param {number} personId - Person ID to delete
 */
function deletePerson(personId) {
    const url = CONFIG.API_BASE_URL + CONFIG.ENDPOINTS.DELETE.replace('{id}', personId);

    apiCall('DELETE', url, null,
        function (response) {
            showToast('Person deleted successfully!', 'success');
            bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
            loadPersons();
        },
        function (error) {
            showToast('Failed to delete person: ' + error, 'error');
        }
    );
}

// ==========================================
// FORM FUNCTIONS
// ==========================================

/**
 * Validate form fields
 * @returns {boolean} Is form valid
 */
function validateForm() {
    let isValid = true;
    const errors = [];

    // Clear previous validation states
    $('.form-control').removeClass('is-invalid is-valid');

    // Name validation
    const name = $('#txtName').val().trim();
    if (!name || name.length < 2) {
        $('#txtName').addClass('is-invalid');
        errors.push('Name is required (minimum 2 characters)');
        isValid = false;
    } else {
        $('#txtName').addClass('is-valid');
    }

    // Mobile validation
    const mobile = $('#txtMobile').val().trim();
    if (!/^[0-9]{10}$/.test(mobile)) {
        $('#txtMobile').addClass('is-invalid');
        errors.push('Mobile number must be exactly 10 digits');
        isValid = false;
    } else {
        $('#txtMobile').addClass('is-valid');
    }

    // Age validation
    const age = parseInt($('#txtAge').val());
    if (!age || age < 1 || age > 150) {
        $('#txtAge').addClass('is-invalid');
        errors.push('Age must be between 1 and 150');
        isValid = false;
    } else {
        $('#txtAge').addClass('is-valid');
    }

    // Qualification validation
    const qualRows = $('.qualification-row');
    if (qualRows.length === 0) {
        errors.push('At least one qualification is required');
        isValid = false;
    } else {
        qualRows.each(function (index) {
            const $row = $(this);
            const qualName = $row.find('.qual-name').val().trim();
            const marks = parseFloat($row.find('.qual-marks').val());

            if (!qualName) {
                $row.find('.qual-name').addClass('is-invalid');
                errors.push(`Qualification ${index + 1}: Name is required`);
                isValid = false;
            } else {
                $row.find('.qual-name').addClass('is-valid');
            }

            if (isNaN(marks) || marks < 0 || marks > 100) {
                $row.find('.qual-marks').addClass('is-invalid');
                errors.push(`Qualification ${index + 1}: Marks must be between 0 and 100`);
                isValid = false;
            } else {
                $row.find('.qual-marks').addClass('is-valid');
            }
        });
    }

    // Show first error
    if (!isValid && errors.length > 0) {
        showToast(errors[0], 'error');
    }

    return isValid;
}

/**
 * Collect form data into object
 * @returns {object} Person data object
 */
function collectFormData() {
    const qualifications = [];

    $('.qualification-row').each(function () {
        const $row = $(this);
        qualifications.push({
            qualificationId: parseInt($row.find('.qual-id').val()) || 0,
            qualificationName: $row.find('.qual-name').val().trim(),
            marks: parseFloat($row.find('.qual-marks').val())
        });
    });

    return {
        personId: parseInt($('#personId').val()) || 0,
        name: $('#txtName').val().trim(),
        mobileNo: $('#txtMobile').val().trim(),
        age: parseInt($('#txtAge').val()),
        address: $('#txtAddress').val().trim() || null,
        qualifications: qualifications
    };
}

/**
 * Populate form for editing
 * @param {object} person - Person data
 */
function populateFormForEdit(person) {
    // Set edit mode
    AppState.isEditMode = true;
    AppState.currentPersonId = person.personId;
    AppState.qualificationIndex = 0;

    // Update UI
    $('#formCard').addClass('edit-mode');
    $('#formTitle').text('Edit Person');
    $('#btnSubmitText').text('Update Person');

    // Fill form fields
    $('#personId').val(person.personId);
    $('#txtName').val(person.name);
    $('#txtMobile').val(person.mobileNo);
    $('#txtAge').val(person.age);
    $('#txtAddress').val(person.address || '');

    // Clear and add qualifications
    $('#qualificationContainer').find('.qualification-row').remove();

    if (person.qualifications && person.qualifications.length > 0) {
        person.qualifications.forEach(qual => {
            addQualificationRow(qual);
        });
    }

    toggleNoQualificationMsg();

    // Scroll to form
    $('html, body').animate({
        scrollTop: $('#formCard').offset().top - 20
    }, 500);

    // Focus on name field
    $('#txtName').focus();
}

/**
 * Reset form to initial state
 */
function resetForm() {
    // Reset state
    AppState.isEditMode = false;
    AppState.currentPersonId = 0;
    AppState.qualificationIndex = 0;

    // Reset UI
    $('#formCard').removeClass('edit-mode');
    $('#formTitle').text('Add New Person');
    $('#btnSubmitText').text('Save Person');

    // Clear form
    $('#personForm')[0].reset();
    $('#personId').val(0);

    // Clear qualifications
    $('#qualificationContainer').find('.qualification-row').remove();
    toggleNoQualificationMsg();

    // Clear validation states
    $('.form-control').removeClass('is-invalid is-valid');
}

// ==========================================
// QUALIFICATION FUNCTIONS
// ==========================================

/**
 * Add qualification row
 * @param {object|null} data - Qualification data (for edit mode)
 */
function addQualificationRow(data = null) {
    const template = document.getElementById('qualificationTemplate');
    const clone = template.content.cloneNode(true);
    const $row = $(clone).find('.qualification-row');

    AppState.qualificationIndex++;

    if (data) {
        $row.find('.qual-name').val(data.qualificationName);
        $row.find('.qual-marks').val(data.marks);
        $row.find('.qual-id').val(data.qualificationId || 0);
    }

    $('#qualificationContainer').append($row);
    updateQualificationNumbers();
    toggleNoQualificationMsg();

    // Focus on new row
    $row.find('.qual-name').focus();
}

/**
 * Remove qualification row
 * @param {jQuery} $button - Remove button element
 */
function removeQualificationRow($button) {
    $button.closest('.qualification-row').fadeOut(200, function () {
        $(this).remove();
        updateQualificationNumbers();
        toggleNoQualificationMsg();
    });
}

/**
 * Update qualification row numbers
 */
function updateQualificationNumbers() {
    $('.qualification-row').each(function (index) {
        $(this).find('.qual-number').text(index + 1);
    });
}

/**
 * Toggle no qualification message
 */
function toggleNoQualificationMsg() {
    const hasQuals = $('.qualification-row').length > 0;
    $('#noQualificationMsg').toggle(!hasQuals);
}

// ==========================================
// EVENT HANDLERS
// ==========================================

/**
 * Initialize all event handlers
 */
function initEventHandlers() {
    // Form submit
    $('#personForm').on('submit', function (e) {
        e.preventDefault();
        savePerson();
    });

    // Reset button
    $('#btnReset').on('click', function () {
        resetForm();
    });

    // Refresh button
    $('#btnRefresh').on('click', function () {
        loadPersons();
    });

    // Add qualification button
    $('#btnAddQualification').on('click', function () {
        addQualificationRow();
    });

    // Remove qualification (delegated)
    $('#qualificationContainer').on('click', '.btn-remove-qual', function () {
        removeQualificationRow($(this));
    });

    // View button (delegated)
    $('#personTableBody').on('click', '.btn-view', function () {
        const personId = $(this).data('id');
        viewPersonDetails(personId);
    });

    // Edit button (delegated)
    $('#personTableBody').on('click', '.btn-edit', function () {
        const personId = $(this).data('id');
        getPersonById(personId);
    });

    // Delete button (delegated)
    $('#personTableBody').on('click', '.btn-delete', function () {
        const personId = $(this).data('id');
        const personName = $(this).data('name');

        AppState.deletePersonId = personId;
        $('#deletePersonName').text(personName);

        const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        modal.show();
    });

    // Confirm delete button
    $('#btnConfirmDelete').on('click', function () {
        deletePerson(AppState.deletePersonId);
    });

    // Mobile number - digits only
    $('#txtMobile').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '').slice(0, 10);
    });

    // Age - positive numbers only
    $('#txtAge').on('input', function () {
        if (this.value < 0) this.value = '';
    });

    // Marks validation (delegated)
    $('#qualificationContainer').on('input', '.qual-marks', function () {
        const val = parseFloat(this.value);
        if (val < 0) this.value = 0;
        if (val > 100) this.value = 100;
    });
}

// ==========================================
// INITIALIZATION
// ==========================================

/**
 * Initialize application on document ready
 */
$(document).ready(function () {
    console.log('Person Management SPA initialized');

    // Initialize event handlers
    initEventHandlers();

    // Load initial data
    loadPersons();

    // Show initial qualification message
    toggleNoQualificationMsg();
});