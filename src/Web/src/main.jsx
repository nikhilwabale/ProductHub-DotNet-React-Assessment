import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Box, Eye, LogOut, PackageCheck, Pencil, Plus, RefreshCw, Search, Trash2, X } from 'lucide-react';
import './styles.css';

const API_URL = (import.meta.env.VITE_API_URL || '/api/v1').replace(/\/$/, '');
const emptyForm = { productName: '', description: '', totalQuantity: 0 };

const storage = {
  getSession: () => ({
    accessToken: localStorage.getItem('accessToken') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    role: localStorage.getItem('role') || '',
    userName: localStorage.getItem('userName') || '',
  }),
  saveSession(session) {
    Object.entries(session).forEach(([key, value]) => localStorage.setItem(key, value));
  },
  clear() {
    ['accessToken', 'refreshToken', 'role', 'userName'].forEach((key) => localStorage.removeItem(key));
  },
};

async function safeFetch(url, options) {
  try { return await fetch(url, options); }
  catch { throw new Error('Cannot connect to the backend API. Make sure the API is running.'); }
}

async function readResponse(response) {
  if (response.status === 204) return null;
  const contentType = response.headers.get('content-type') || '';
  return contentType.includes('json') ? response.json() : response.text();
}

function getErrorMessage(payload, fallback = 'The request could not be completed.') {
  if (!payload) return fallback;
  if (typeof payload === 'string') return payload;
  if (Array.isArray(payload.errors)) return payload.errors.map((x) => x.errorMessage || x.ErrorMessage).filter(Boolean).join(' ');
  return payload.title || payload.message || fallback;
}

function ProductDialog({ open, mode, form, product, busy, onChange, onClose, onSubmit }) {
  if (!open) return null;
  const isView = mode === 'view';
  const title = mode === 'create' ? 'Add product' : mode === 'edit' ? 'Edit product' : 'View product';
  const totalQuantity = product
    ? (product.items || []).reduce((sum, item) => sum + item.quantity, 0)
    : Number(form.totalQuantity || 0);

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <section className="modal" role="dialog" aria-modal="true" aria-labelledby="product-dialog-title">
        <header className="modal-header">
          <div><span className="modal-kicker">PRODUCT DETAILS</span><h2 id="product-dialog-title">{title}</h2></div>
          <button type="button" className="close-button" onClick={onClose} aria-label="Close"><X size={20} /></button>
        </header>
        {isView ? (
          <div className="view-body">
            <div className="detail-grid">
              <div className="detail-field"><span>Product ID</span><strong>#{product.id}</strong></div>
              <div className="detail-field"><span>Total quantity</span><strong>{totalQuantity}</strong></div>
              <div className="detail-field detail-wide"><span>Product name</span><strong>{product.productName}</strong></div>
              <div className="detail-field detail-wide"><span>Description</span><p>{product.description || 'No description provided.'}</p></div>
              <div className="detail-field"><span>Created by</span><strong>{product.createdBy}</strong></div>
              <div className="detail-field"><span>Created on</span><strong>{new Date(product.createdOn).toLocaleString()}</strong></div>
              <div className="detail-field"><span>Modified by</span><strong>{product.modifiedBy || '—'}</strong></div>
              <div className="detail-field"><span>Modified on</span><strong>{product.modifiedOn ? new Date(product.modifiedOn).toLocaleString() : '—'}</strong></div>
            </div>
            <footer className="modal-footer view-footer"><button type="button" className="button secondary" onClick={onClose}>Close</button></footer>
          </div>
        ) : (
          <form onSubmit={onSubmit}>
            <div className="modal-body">
              <label>Product name<input autoFocus value={form.productName} onChange={(e) => onChange('productName', e.target.value)} maxLength={255} required placeholder="Enter product name" /></label>
              <label>Description<textarea value={form.description} onChange={(e) => onChange('description', e.target.value)} maxLength={1000} rows={4} placeholder="Describe the product" /></label>
              <label>Total quantity<input type="number" min="0" value={form.totalQuantity} onChange={(e) => onChange('totalQuantity', e.target.value)} required /></label>
              <p className="form-help">Quantity is stored as the product's related inventory item.</p>
            </div>
            <footer className="modal-footer">
              <button type="button" className="button secondary" onClick={onClose}>Cancel</button>
              <button type="submit" disabled={busy}>{busy && <RefreshCw className="spin" size={17} />} {mode === 'create' ? 'Add product' : 'Save changes'}</button>
            </footer>
          </form>
        )}
      </section>
    </div>
  );
}

function App() {
  const [session, setSession] = useState(storage.getSession);
  const [products, setProducts] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState(null);
  const [dialog, setDialog] = useState({ open: false, mode: 'create', product: null });
  const [form, setForm] = useState(emptyForm);
  const isAdmin = session.role === 'Admin';

  useEffect(() => {
    if (!toast) return undefined;
    const timer = window.setTimeout(() => setToast(null), 3000);
    return () => window.clearTimeout(timer);
  }, [toast]);

  const notify = useCallback((type, text) => setToast({ type, text }), []);
  const logout = useCallback(() => { storage.clear(); setSession(storage.getSession()); setProducts([]); }, []);

  const refreshSession = useCallback(async () => {
    if (!session.refreshToken) return false;
    const response = await safeFetch(`${API_URL}/auth/refresh`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ refreshToken: session.refreshToken }) });
    if (!response.ok) return false;
    const next = await response.json(); storage.saveSession(next); setSession(next); return next.accessToken;
  }, [session.refreshToken]);

  const apiRequest = useCallback(async (path, options = {}, retry = true) => {
    const headers = { ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...(session.accessToken ? { Authorization: `Bearer ${session.accessToken}` } : {}), ...options.headers };
    let response = await safeFetch(`${API_URL}${path}`, { ...options, headers });
    if (response.status === 401 && retry && session.refreshToken) {
      const token = await refreshSession();
      if (token) response = await safeFetch(`${API_URL}${path}`, { ...options, headers: { ...headers, Authorization: `Bearer ${token}` } });
    }
    if (response.status === 401) logout();
    const payload = await readResponse(response);
    if (!response.ok) throw new Error(getErrorMessage(payload, `${response.status} ${response.statusText}`));
    return payload;
  }, [logout, refreshSession, session.accessToken, session.refreshToken]);

  const loadProducts = useCallback(async () => {
    if (!session.accessToken) return;
    setLoading(true);
    try {
      const result = await apiRequest(`/products?pageNumber=1&pageSize=100&search=${encodeURIComponent(search)}`);
      setProducts(result.items || []); setTotalCount(result.totalCount || 0);
    } catch (e) { notify('error', e.message); }
    finally { setLoading(false); }
  }, [apiRequest, notify, search, session.accessToken]);

  useEffect(() => { const timer = window.setTimeout(loadProducts, 250); return () => window.clearTimeout(timer); }, [loadProducts]);

  const login = async (event) => {
    event.preventDefault(); setLoading(true);
    const data = new FormData(event.currentTarget);
    try {
      const response = await safeFetch(`${API_URL}/auth/login`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ userName: data.get('email'), password: data.get('password') }) });
      const payload = await readResponse(response);
      if (!response.ok) throw new Error(getErrorMessage(payload, 'Invalid username or password.'));
      storage.saveSession(payload); setSession(payload); notify('success', `Welcome, ${payload.userName}.`);
    } catch (e) { notify('error', e.message); }
    finally { setLoading(false); }
  };

  const openCreate = () => { setForm(emptyForm); setDialog({ open: true, mode: 'create', product: null }); };
  const openView = (product) => setDialog({ open: true, mode: 'view', product });
  const openEdit = (product) => {
    const totalQuantity = (product.items || []).reduce((sum, item) => sum + item.quantity, 0);
    setForm({ productName: product.productName, description: product.description || '', totalQuantity });
    setDialog({ open: true, mode: 'edit', product });
  };
  const closeDialog = () => !busy && setDialog((d) => ({ ...d, open: false }));

  const saveProduct = async (event) => {
    event.preventDefault(); setBusy(true);
    const quantity = Number(form.totalQuantity);
    try {
      if (dialog.mode === 'create') {
        await apiRequest('/products', { method: 'POST', body: JSON.stringify({ productName: form.productName.trim(), description: form.description.trim() || null, items: [{ quantity }] }) });
        notify('success', 'Product created successfully.');
      } else {
        const product = dialog.product;
        await apiRequest(`/products/${product.id}`, { method: 'PUT', body: JSON.stringify({ productName: form.productName.trim(), description: form.description.trim() || null }) });
        const items = product.items || [];
        if (items.length) {
          await apiRequest(`/products/${product.id}/items/${items[0].id}`, { method: 'PUT', body: JSON.stringify({ quantity }) });
          for (const extra of items.slice(1)) await apiRequest(`/products/${product.id}/items/${extra.id}`, { method: 'DELETE' });
        } else {
          await apiRequest(`/products/${product.id}/items`, { method: 'POST', body: JSON.stringify({ quantity }) });
        }
        notify('success', 'Product updated successfully.');
      }
      setDialog((d) => ({ ...d, open: false })); await loadProducts();
    } catch (e) { notify('error', e.message); }
    finally { setBusy(false); }
  };

  const deleteProduct = async (product) => {
    if (!window.confirm(`Delete “${product.productName}”?`)) return;
    try { await apiRequest(`/products/${product.id}`, { method: 'DELETE' }); notify('success', 'Product deleted successfully.'); await loadProducts(); }
    catch (e) { notify('error', e.message); }
  };

  const filteredCount = useMemo(() => products.length, [products]);

  if (!session.accessToken) return (
    <main className="login-page"><section className="login-card">
      <div className="brand"><Box /><strong>ProductHub</strong></div><p className="eyebrow">CRN TECHNICAL ASSESSMENT</p>
      <h1>Product operations, secured.</h1><p>Sign in to access the .NET 8 Product management application.</p>
      <form onSubmit={login} className="login-form"><label>Email<input name="email" type="email" defaultValue="admin@crn.local" required /></label><label>Password<input name="password" type="password" defaultValue="Admin@123" required /></label><button disabled={loading}>{loading && <RefreshCw className="spin" size={18} />} Sign in</button></form>
      <div className="demo-credentials"><strong>Development credentials</strong><span>Admin: admin@crn.local / Admin@123</span><span>User: user@crn.local / User@123</span></div>
      {toast && <div className={`toast ${toast.type}`}>{toast.text}</div>}
    </section></main>
  );

  return <div className="app-shell">
    <header className="topbar"><div className="brand"><Box /><strong>ProductHub</strong></div><div className="account"><span><strong>{session.userName}</strong><small>{session.role}</small></span><button className="button secondary" onClick={logout}><LogOut size={17} /> Logout</button></div></header>
    <main className="container">
      <section className="hero"><div><p className="eyebrow">PRODUCT OPERATIONS</p><h1>Catalogue dashboard</h1><p>Manage products and inventory through the secured REST API.</p></div><div className="metrics"><div className="metric"><PackageCheck /><span><strong>{totalCount}</strong>Total products</span></div></div></section>
      <section className="toolbar"><div className="search-box"><Search size={18} /><input placeholder="Search product name" value={search} onChange={(e) => setSearch(e.target.value)} /></div>{isAdmin && <button onClick={openCreate}><Plus size={18} /> Add product</button>}</section>
      <section className="table-card"><div className="table-scroll"><table><thead><tr><th>ID</th><th>Product</th><th>Description</th><th>Total quantity</th><th>Created</th><th>Actions</th></tr></thead><tbody>
        {products.map((product) => <tr key={product.id}><td>#{product.id}</td><td><strong>{product.productName}</strong><small>Created by {product.createdBy}</small></td><td className="description-cell"><span>{product.description || '—'}</span></td><td>{(product.items || []).reduce((sum, item) => sum + item.quantity, 0)}</td><td>{new Date(product.createdOn).toLocaleDateString()}</td><td><div className="actions"><button className="icon-button view" title="View product" onClick={() => openView(product)}><Eye size={16} /></button>{isAdmin && <button className="icon-button" title="Edit product" onClick={() => openEdit(product)}><Pencil size={16} /></button>}{isAdmin && <button className="icon-button danger" title="Delete product" onClick={() => deleteProduct(product)}><Trash2 size={16} /></button>}</div></td></tr>)}
      </tbody></table>{loading && <div className="empty"><RefreshCw className="spin" /> Loading products...</div>}{!loading && filteredCount === 0 && <div className="empty">No products found.</div>}</div></section>
    </main>
    {toast && <div className={`toast ${toast.type}`} role="status" aria-live="polite">{toast.text}</div>}
    <ProductDialog open={dialog.open} mode={dialog.mode} form={form} product={dialog.product} busy={busy} onChange={(key, value) => setForm((f) => ({ ...f, [key]: value }))} onClose={closeDialog} onSubmit={saveProduct} />
  </div>;
}

createRoot(document.getElementById('root')).render(<App />);
