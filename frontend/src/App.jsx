import { useState } from 'react'
import './App.css'

function App() {
  const [steamInput, setSteamInput] = useState('');
  const [response, setResponse] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setResponse(null);

    const steamId64 = steamInput.trim().split('/').filter(Boolean).pop();

    try {
      const res = await fetch(`http://localhost:5001/api/steam/inventory/${steamId64}`);

      if (!res.ok) {
        throw new Error(`Server error: ${res.status}`);
      }

      const data = await res.json();
      setResponse(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Steam ID Lookup</h2>

      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Paste steam profile link"
          value={steamInput}
          onChange={(e) => setSteamInput(e.target.value)}
          required
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Sending...' : 'Check'}
        </button>
      </form>

      <hr />

      {error && (
        <div>
          <strong>Błąd:</strong> {error}
        </div>
      )}

      {response && (
        <div>
          <h3>Odpowiedź z API:</h3>
          <pre>
            {JSON.stringify(response, null, 2)}
          </pre>
        </div>
      )}
    </div>
  )
}

export default App
