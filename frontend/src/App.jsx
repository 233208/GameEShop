import { useState } from 'react'; // Usuniêto useEffect i useNavigate, jeœli nie s¹ tu bezpoœrednio potrzebne
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';

import Navbar from './Navbar';
import HomePage from './HomePage';
import ProductsPage from './ProductsPage';
import Login from './Login'; // U¿ywamy istniej¹cego komponentu Login
import CartPage from './CartPage';

function App() {
    // Przyk³adowa funkcja dodawania do koszyka (na razie tylko alert)
    // W przysz³oœci mo¿na to rozbudowaæ o zarz¹dzanie stanem koszyka (np. przez Context API)
    const [cartItems, setCartItems] = useState([]);

    const handleAddToCart = (product) => {
        setCartItems(prevItems => {
            const itemExists = prevItems.find(item => item.id === product.id);
            if (itemExists) {
                // Mo¿na zwiêkszyæ iloœæ, jeœli produkt ju¿ jest w koszyku
                alert(`${product.name} is already in the cart! (Or update quantity)`);
                return prevItems;
            } else {
                alert(`Added ${product.name} to cart!`);
                return [...prevItems, { ...product, quantity: 1 }];
            }
        });
        console.log('Current cart:', cartItems); // Do debugowania
    };

    return (
        <Router>
            <div className="app-root"> {/* Klasa app-root z App.css */}
                <Navbar />
                <main className="main-content"> {/* Dodatkowy kontener na zawartoœæ strony */}
                    <Routes>
                        <Route path="/" element={<HomePage />} />
                        <Route
                            path="/products"
                            element={<ProductsPage onAddToCart={handleAddToCart} />}
                        />
                        <Route path="/login" element={<Login />} />
                        <Route path="/cart" element={<CartPage cartItems={cartItems} />} /> {/* Przekazujemy cartItems do CartPage */}
                    </Routes>
                </main>
            </div>
        </Router>
    );
}

export default App;