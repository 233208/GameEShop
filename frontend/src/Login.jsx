import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './App.css'; // Za³ó¿my, ¿e style dla login s¹ nadal w App.css

function Login() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            const apiUrl = import.meta.env.VITE_USER_API_URL;
            const response = await fetch(`${apiUrl}/api/Login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Invalid credentials' }));
                setError(errorData.message || 'Invalid credentials');
                return;
            }
            // Za³ó¿my, ¿e API zwraca token lub dane u¿ytkownika po zalogowaniu
             const data = await response.json();
             localStorage.setItem('token', data.token); // Przyk³ad przechowywania tokenu

            alert('Login successful!');
            navigate('/products'); // Przekierowanie na stronê produktów po zalogowaniu

        } catch (err) {
            setError('Login failed. Please try again.');
            console.error('Login error:', err);
        }
    };

    return (
        <div className="login-container"> {/* Styl z App.css */}
            <h2>Login</h2>
            <form onSubmit={handleSubmit} className="login-form"> {/* Styl z App.css */}
                <div className="form-group"> {/* Styl z App.css */}
                    <label htmlFor="username">Username</label> {/* Dodano label dla lepszej dostêpnoœci */}
                    <input
                        id="username"
                        type="text"
                        placeholder="Username"
                        value={username}
                        onChange={e => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div className="form-group"> {/* Styl z App.css */}
                    <label htmlFor="password">Password</label> {/* Dodano label */}
                    <input
                        id="password"
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" className="btn btn-primary">Login</button> {/* U¿ycie klas globalnych */}
                {error && <div className="login-error">{error}</div>} {/* Styl z App.css */}
            </form>
        </div>
    );
}

export default Login;