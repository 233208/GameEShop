import React from 'react';

function CartPage() {
    // Na razie prosty placeholder
    // W przysz³oœci tutaj bêdzie logika wyœwietlania produktów w koszyku
    return (
        <div className="page-container">
            <h1 className="centered-title">Your Shopping Cart</h1>
            <p>Your cart is currently empty.</p>
            {/* Przyk³ad jak mog³yby wygl¹daæ produkty w koszyku */}
            {/* <div className="cart-item">
                <span>Game Title</span>
                <span>Quantity: 1</span>
                <span>Price: $59.99</span>
                <button>Remove</button>
            </div> */}
        </div>
    );
}

export default CartPage;