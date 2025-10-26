// PharmaDNA Web Application JavaScript

// Global variables
let currentUser = null;
let selectedNFT = null;

// Initialize application
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

function initializeApp() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Utility functions
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Insert at the top of the main content
    const main = document.querySelector('main');
    if (main) {
        main.insertBefore(alertDiv, main.firstChild);
    }
}

function showLoading(element) {
    if (element) {
        element.innerHTML = '<div class="text-center"><div class="spinner-border spinner-border-sm" role="status"><span class="visually-hidden">Loading...</span></div></div>';
    }
}

function hideLoading(element, originalContent) {
    if (element && originalContent) {
        element.innerHTML = originalContent;
    }
}

// API helper functions
async function apiCall(url, options = {}) {
    try {
        const response = await fetch(url, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('API call failed:', error);
        throw error;
    }
}

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;
    
    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;
    
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });
    
    return isValid;
}

// Address formatting
function formatAddress(address) {
    if (!address) return '';
    return `${address.slice(0, 6)}...${address.slice(-4)}`;
}

// Date formatting
function formatDate(dateString) {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('vi-VN');
}

function formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('vi-VN');
}

// Status color mapping
function getStatusColor(status) {
    const statusMap = {
        'created': 'primary',
        'in_transit': 'warning',
        'in_pharmacy': 'success',
        'pending': 'secondary',
        'approved': 'success',
        'rejected': 'danger'
    };
    return statusMap[status?.toLowerCase()] || 'secondary';
}

// Role color mapping
function getRoleColor(role) {
    const roleMap = {
        'admin': 'danger',
        'manufacturer': 'primary',
        'distributor': 'success',
        'pharmacy': 'warning'
    };
    return roleMap[role?.toLowerCase()] || 'secondary';
}

// Role name mapping
function getRoleName(role) {
    const roleMap = {
        'admin': 'Quản trị viên',
        'manufacturer': 'Nhà sản xuất',
        'distributor': 'Nhà phân phối',
        'pharmacy': 'Nhà thuốc'
    };
    return roleMap[role?.toLowerCase()] || role;
}

// File upload helpers
function handleFileUpload(input, callback) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        callback(file);
    }
}

function validateFileType(file, allowedTypes) {
    return allowedTypes.includes(file.type);
}

function validateFileSize(file, maxSizeMB) {
    return file.size <= maxSizeMB * 1024 * 1024;
}

// Table helpers
function createTableRow(data, columns) {
    const row = document.createElement('tr');
    columns.forEach(column => {
        const cell = document.createElement('td');
        if (column.render) {
            cell.innerHTML = column.render(data[column.key]);
        } else {
            cell.textContent = data[column.key] || '';
        }
        row.appendChild(cell);
    });
    return row;
}

// Modal helpers
function showModal(modalId) {
    const modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}

function hideModal(modalId) {
    const modal = bootstrap.Modal.getInstance(document.getElementById(modalId));
    if (modal) {
        modal.hide();
    }
}

// QR Code scanner (placeholder)
function startQRScanner(callback) {
    // This would integrate with a QR scanner library like QuaggaJS or ZXing
    console.log('QR Scanner would be implemented here');
    // For now, simulate a scan
    setTimeout(() => {
        if (callback) {
            callback('LOT2024001'); // Simulated QR code result
        }
    }, 2000);
}

// Blockchain helpers
function connectWallet() {
    // This would integrate with MetaMask or other wallet providers
    console.log('Wallet connection would be implemented here');
    return Promise.resolve({
        address: '0x1234567890123456789012345678901234567890',
        chainId: 1
    });
}

// IPFS helpers
function getIPFSUrl(hash) {
    return `https://gateway.pinata.cloud/ipfs/${hash}`;
}

// Error handling
function handleError(error, context = '') {
    console.error(`Error in ${context}:`, error);
    showAlert(`Có lỗi xảy ra: ${error.message}`, 'danger');
}

// Export functions for global use
window.PharmaDNA = {
    showAlert,
    showLoading,
    hideLoading,
    apiCall,
    validateForm,
    formatAddress,
    formatDate,
    formatDateTime,
    getStatusColor,
    getRoleColor,
    getRoleName,
    handleFileUpload,
    validateFileType,
    validateFileSize,
    createTableRow,
    showModal,
    hideModal,
    startQRScanner,
    connectWallet,
    getIPFSUrl,
    handleError
};
