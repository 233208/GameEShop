import React, { useState, useEffect } from 'react';

// Ten komponent bêdzie odpowiedzialny tylko za pobieranie i wyœwietlanie listy produktów
function ProductsComponent({ onAddToCart }) {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const apiUrl = import.meta.env.VITE_API_URL;
        fetch(`${apiUrl}/api/Product`)
            .then(res => {
                if (!res.ok) {
                    throw new Error('Network response was not ok');
                }
                return res.json();
            })
            .then(data => {
                setProducts(data);
                setLoading(false);
            })
            .catch(err => {
                setError(err.message);
                setLoading(false);
            });
    }, []);

    if (loading) return <p>Loading products...</p>;
    if (error) return <p>Error loading products: {error}</p>;
    if (!products.length) return <p>No products found.</p>;

    return (
        <div className="products-container">
            {products.map(product => (
                <div className="product-card" key={product.id}>
                    {/* Mo¿esz tu dodaæ product-image-placeholder jeœli chcesz */}
                    {/* <div className="product-image-placeholder">Image Placeholder</div> */}
                    <div className="product-name">{product.name}</div>
                    {/* <div className="product-description">{product.description || 'No description available.'}</div> */}
                    {/* <div className="product-price">${product.price || 'N/A'}</div> */}
                    <button
                        className="btn btn-secondary add-to-cart-btn" // U¿ycie klas globalnych
                        onClick={() => onAddToCart(product)}
                    >
                        Add to Cart
                    </button>
                </div>
            ))}
        </div>
    );
}

export default ProductsComponent;