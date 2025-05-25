import React from 'react';
import ProductsComponent from './ProductsComponent'; // Importujemy nowy komponent

function ProductsPage({ onAddToCart }) {
    return (
        <div className="page-container">
            <h1 className="centered-title">Our Games</h1>
            <ProductsComponent onAddToCart={onAddToCart} />
        </div>
    );
}

export default ProductsPage;