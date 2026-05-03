// Shoppky E-Commerce JavaScript

class ShoppingCartManager {
    constructor() {
        this.cartKey = 'shoppky-cart';
        this.initializeCartBadge();
    }

    initializeCartBadge() {
        const cartCount = this.getCartCount();
        if (cartCount > 0) {
            this.updateCartBadge(cartCount);
        }
    }

    getCartCount() {
        const cart = sessionStorage.getItem(this.cartKey);
        if (!cart) return 0;
        try {
            return JSON.parse(cart).items.length;
        } catch {
            return 0;
        }
    }

    updateCartBadge(count) {
        const badge = document.getElementById('cart-badge');
        if (badge) {
            badge.textContent = count;
            badge.style.display = count > 0 ? 'inline-block' : 'none';
        }
    }

    addToCart(productId, productTitle, quantity = 1) {
        return fetch('/Home/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `productId=${productId}&quantity=${quantity}`
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                this.updateCartBadge(data.cartCount);
                this.showNotification(`${productTitle} added to cart!`, 'success');
                return true;
            }
            return false;
        })
        .catch(error => {
            console.error('Error:', error);
            this.showNotification('Failed to add to cart', 'error');
            return false;
        });
    }

    removeFromCart(productId) {
        return fetch('/Home/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `productId=${productId}`
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                this.updateCartBadge(data.cartCount);
                return true;
            }
            return false;
        })
        .catch(error => {
            console.error('Error:', error);
            return false;
        });
    }

    updateQuantity(productId, quantity) {
        return fetch('/Home/UpdateCartQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `productId=${productId}&quantity=${quantity}`
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                this.updateCartBadge(data.cartCount);
                return data;
            }
            return null;
        })
        .catch(error => {
            console.error('Error:', error);
            return null;
        });
    }

    showNotification(message, type = 'info') {
        const alertClass = type === 'success' ? 'alert-success' : 
                          type === 'error' ? 'alert-danger' : 'alert-info';

        const alertDiv = document.createElement('div');
        alertDiv.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.setAttribute('role', 'alert');
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        document.body.appendChild(alertDiv);

        // Auto-dismiss after 4 seconds
        const timeout = setTimeout(() => {
            alertDiv.remove();
        }, 4000);

        // Allow manual close to clear timeout
        alertDiv.querySelector('.btn-close').addEventListener('click', () => {
            clearTimeout(timeout);
        });
    }

    clearCart() {
        sessionStorage.removeItem(this.cartKey);
        this.updateCartBadge(0);
    }
}

// Initialize cart manager globally
const cartManager = new ShoppingCartManager();

// DOM Ready
document.addEventListener('DOMContentLoaded', function () {
    // Add affiliate link tracking
    document.querySelectorAll('a[href*="amazon"]').forEach(link => {
        link.setAttribute('target', '_blank');
        link.setAttribute('rel', 'noopener noreferrer nofollow');
    });

    // Initialize tooltips if Bootstrap is loaded
    if (typeof bootstrap !== 'undefined') {
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            new bootstrap.Tooltip(el);
        });
    }
});

// Utility function to format currency
function formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
    }).format(amount);
}
