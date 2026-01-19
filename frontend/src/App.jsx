import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [steamInput, setSteamInput] = useState('');
  const [valuation, setValuation] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [importExportLoading, setImportExportLoading] = useState(false);
  const [importExportMessage, setImportExportMessage] = useState(null);
  const [theme, setTheme] = useState(() => localStorage.getItem('theme') || 'dark');

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prev => prev === 'dark' ? 'light' : 'dark');
  };

  const handleExport = async (format) => {
    setImportExportLoading(true);
    setImportExportMessage(null);
    
    try {
      const res = await fetch(`http://localhost:5001/api/import-export/export/${format}`);
      
      if (!res.ok) {
        throw new Error(`Export failed: ${res.status}`);
      }
      
      const data = await res.json();
      
      if (!data.success) {
        throw new Error(data.error || 'Export failed');
      }
      
      // Create and download file
      const blob = new Blob([data.data], { type: format === 'json' ? 'application/json' : 'application/xml' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `steam-inventory-export-${new Date().toISOString().split('T')[0]}.${format}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      
      setImportExportMessage({ type: 'success', text: `Successfully exported ${data.count} valuations to ${format.toUpperCase()}` });
    } catch (err) {
      setImportExportMessage({ type: 'error', text: err.message });
    } finally {
      setImportExportLoading(false);
    }
  };

  const handleImport = async (file, format) => {
    setImportExportLoading(true);
    setImportExportMessage(null);
    
    try {
      const content = await file.text();
      
      const res = await fetch(`http://localhost:5001/api/import-export/import/${format}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ 
          [format === 'json' ? 'jsonData' : 'xmlData']: content 
        }),
      });
      
      if (!res.ok) {
        const errorData = await res.json();
        throw new Error(errorData.error || `Import failed: ${res.status}`);
      }
      
      const data = await res.json();
      
      if (!data.success) {
        throw new Error(data.error || 'Import failed');
      }
      
      setImportExportMessage({ type: 'success', text: data.message });
    } catch (err) {
      setImportExportMessage({ type: 'error', text: err.message });
    } finally {
      setImportExportLoading(false);
    }
  };

  const handleFileUpload = (e, format) => {
    const file = e.target.files?.[0];
    if (file) {
      handleImport(file, format);
    }
    e.target.value = ''; // Reset input
  };

  const handleResetDatabase = async () => {
    if (!window.confirm('Are you sure you want to delete ALL inventory valuations from the database? This action cannot be undone!')) {
      return;
    }
    
    setImportExportLoading(true);
    setImportExportMessage(null);
    
    try {
      const res = await fetch('http://localhost:5001/api/import-export/reset', {
        method: 'DELETE',
      });
      
      if (!res.ok) {
        const errorData = await res.json();
        throw new Error(errorData.error || `Reset failed: ${res.status}`);
      }
      
      const data = await res.json();
      
      if (!data.success) {
        throw new Error(data.error || 'Reset failed');
      }
      
      setImportExportMessage({ type: 'success', text: data.message });
      // Clear current valuation display if any
      setValuation(null);
    } catch (err) {
      setImportExportMessage({ type: 'error', text: err.message });
    } finally {
      setImportExportLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setValuation(null);

    // Extract Steam ID from profile URL or use direct ID
    const steamId64 = steamInput.trim().split('/').filter(Boolean).pop();

    try {
      const res = await fetch(`http://localhost:5001/api/value/steam/profile?steamId64=${encodeURIComponent(steamId64)}`);

      if (!res.ok) {
        const errorText = await res.text();
        throw new Error(errorText || `Server error: ${res.status}`);
      }

      const data = await res.json();
      setValuation(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <button className="theme-toggle" onClick={toggleTheme} aria-label="Toggle theme">
        {theme === 'dark' ? '☀️' : '🌙'}
      </button>
      
      <header className="header">
        <h1>🎮 Steam Inventory Appraiser</h1>
        <p className="subtitle">Value your CS:GO inventory in seconds</p>
      </header>

      <div className="search-section">
        <form onSubmit={handleSubmit} className="search-form">
          <input
            type="text"
            placeholder="Enter Steam profile URL or ID (e.g., 76561198012345678)"
            value={steamInput}
            onChange={(e) => setSteamInput(e.target.value)}
            required
            className="search-input"
            disabled={loading}
          />
          <button type="submit" disabled={loading} className="search-button">
            {loading ? (
              <>
                <span className="spinner"></span>
                Valuing...
              </>
            ) : (
              'Value Inventory'
            )}
          </button>
        </form>

        {loading && (
          <div className="loading-message">
            <p>Fetching inventory and calculating values...</p>
            <p className="loading-note">This may take a moment for large inventories</p>
          </div>
        )}
      </div>

      {error && (
        <div className="error-box">
          <strong>❌ Error:</strong> {error}
        </div>
      )}

      {valuation && (
        <div className="results">
          <div className="total-value">
            <h2>Total Inventory Value</h2>
            <div className="value-display">
              ${valuation.totalValueUSD?.toFixed(2) || '0.00'}
            </div>
            <p className="steam-id">Steam ID: {valuation.steamId64}</p>
          </div>

          {valuation.items && valuation.items.length > 0 ? (
            <div className="items-section">
              <h3>Valued Items ({valuation.items.length})</h3>
              <div className="items-grid">
                {valuation.items.map((item, index) => (
                  <div key={index} className="item-card">
                    <div className="item-name">{item.marketHashName}</div>
                    <div className="item-value">
                      ${item.valueUSD?.toFixed(2) || '0.00'}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="empty-state">
              <p>No marketable items found in inventory</p>
            </div>
          )}
        </div>
      )}

      <div className="import-export-section">
        <h3>Import / Export Database</h3>
        <p className="import-export-description">Export all valuations or import previous data</p>
        
        {importExportMessage && (
          <div className={`import-export-message ${importExportMessage.type}`}>
            {importExportMessage.text}
          </div>
        )}
        
        <div className="import-export-grid">
          <div className="import-export-card">
            <h4>📤 Export</h4>
            <div className="button-group">
              <button 
                onClick={() => handleExport('json')} 
                disabled={importExportLoading}
                className="export-button"
              >
                Export JSON
              </button>
              <button 
                onClick={() => handleExport('xml')} 
                disabled={importExportLoading}
                className="export-button"
              >
                Export XML
              </button>
            </div>
          </div>
          
          <div className="import-export-card">
            <h4>📥 Import</h4>
            <div className="button-group">
              <label className="import-button">
                Import JSON
                <input 
                  type="file" 
                  accept=".json"
                  onChange={(e) => handleFileUpload(e, 'json')}
                  disabled={importExportLoading}
                  style={{ display: 'none' }}
                />
              </label>
              <label className="import-button">
                Import XML
                <input 
                  type="file" 
                  accept=".xml"
                  onChange={(e) => handleFileUpload(e, 'xml')}
                  disabled={importExportLoading}
                  style={{ display: 'none' }}
                />
              </label>
            </div>
          </div>
        </div>
        
        <div className="reset-section">
          <button 
            onClick={handleResetDatabase} 
            disabled={importExportLoading}
            className="reset-button"
          >
            🗑️ Reset Database
          </button>
          <p className="reset-warning">⚠️ Warning: This will permanently delete all stored inventory valuations</p>
        </div>
      </div>

      <footer className="footer">
        <p>Note: Only items worth more than $0.01 are shown. Private inventories cannot be valued.</p>
      </footer>
    </div>
  )
}

export default App
