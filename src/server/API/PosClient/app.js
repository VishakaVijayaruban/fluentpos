// FluentPOS offline-first till client.
// Reference implementation of the store-node sync protocol:
//  - catalog pulled incrementally from /catalog/sync and cached in IndexedDB
//  - basket lives on the device; checkout posts a complete sale document with a
//    device-generated UUID, so replays after connectivity loss are idempotent
//  - failed submissions land in a durable outbox and auto-replay when back online

const API = '/api/v1';
const DB_NAME = 'fluentpos-pos';

let db;
let token = localStorage.getItem('pos.token');
let products = new Map();   // productId -> product (central data)
let overlays = new Map();   // productId -> store overlay (price override / ranging)
let cart = new Map();       // productId -> qty

// ---------- IndexedDB ----------
function openDb() {
  return new Promise((resolve, reject) => {
    const req = indexedDB.open(DB_NAME, 1);
    req.onupgradeneeded = () => {
      req.result.createObjectStore('products', { keyPath: 'id' });
      req.result.createObjectStore('overlays', { keyPath: 'productId' });
      req.result.createObjectStore('outbox', { keyPath: 'clientSaleId' });
      req.result.createObjectStore('meta', { keyPath: 'k' });
    };
    req.onsuccess = () => resolve(req.result);
    req.onerror = () => reject(req.error);
  });
}

function tx(store, mode, fn) {
  return new Promise((resolve, reject) => {
    const t = db.transaction(store, mode);
    const result = fn(t.objectStore(store));
    t.oncomplete = () => resolve(result && 'result' in result ? result.result : undefined);
    t.onerror = () => reject(t.error);
  });
}

const idb = {
  put: (store, value) => tx(store, 'readwrite', s => s.put(value)),
  del: (store, key) => tx(store, 'readwrite', s => s.delete(key)),
  getAll: store => tx(store, 'readonly', s => s.getAll()),
  get: (store, key) => tx(store, 'readonly', s => s.get(key)),
};

// ---------- helpers ----------
const $ = id => document.getElementById(id);
const gbp = n => '£' + n.toFixed(2);

function toast(msg) {
  const el = $('toast');
  el.textContent = msg;
  el.classList.add('show');
  clearTimeout(el._t);
  el._t = setTimeout(() => el.classList.remove('show'), 3500);
}

function authHeaders() {
  return { 'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json' };
}

function decodeJwt(t) {
  try { return JSON.parse(atob(t.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))); }
  catch { return {}; }
}

// ---------- catalog sync ----------
async function syncCatalog(showToast) {
  const cursorRow = await idb.get('meta', 'cursor');
  const since = cursorRow ? '?since=' + encodeURIComponent(cursorRow.v) : '';
  const res = await fetch(`${API}/catalog/sync${since}`, { headers: authHeaders() });
  if (!res.ok) throw new Error('sync failed: ' + res.status);
  const body = await res.json();
  const data = body.data;
  for (const p of data.products) await idb.put('products', p);
  for (const sp of data.storeProducts) await idb.put('overlays', { productId: sp.productId, ...sp });
  await idb.put('meta', { k: 'cursor', v: data.serverTime });
  await loadCatalogFromCache();
  if (showToast) toast(`Catalog synced: ${data.products.length} products, ${data.storeProducts.length} overrides`);
}

async function loadCatalogFromCache() {
  products = new Map((await idb.getAll('products')).map(p => [p.id, p]));
  overlays = new Map((await idb.getAll('overlays')).map(o => [o.productId, o]));
  renderProducts();
}

function effectivePrice(p) {
  const o = overlays.get(p.id);
  return o && o.price != null ? o.price : p.price;
}

function isRanged(p) {
  const o = overlays.get(p.id);
  return !o || o.isRanged !== false;
}

// ---------- rendering ----------
function renderProducts() {
  const host = $('products');
  host.innerHTML = '';
  [...products.values()]
    .filter(isRanged)
    .sort((a, b) => a.name.localeCompare(b.name))
    .forEach(p => {
      const b = document.createElement('button');
      b.className = 'tile';
      b.innerHTML = `<div class="name"></div><div class="price">${gbp(effectivePrice(p))}</div>` +
        (p.isAgeRestricted ? `<span class="age">${p.minimumAge}+</span>` : '');
      b.querySelector('.name').textContent = p.name;
      b.onclick = () => { cart.set(p.id, (cart.get(p.id) || 0) + 1); renderCart(); };
      host.appendChild(b);
    });
}

function cartTotals() {
  let sub = 0, vat = 0;
  for (const [pid, qty] of cart) {
    const p = products.get(pid);
    if (!p) continue;
    const line = effectivePrice(p) * qty;
    sub += line;
    vat += line * (p.tax / 100);
  }
  return { sub, vat, total: sub + vat };
}

function renderCart() {
  const host = $('cartItems');
  host.innerHTML = '';
  for (const [pid, qty] of cart) {
    const p = products.get(pid);
    if (!p) continue;
    const row = document.createElement('div');
    row.className = 'cartRow';
    row.innerHTML = `<span class="n"></span><button class="minus">−</button><span>${qty}</span><button class="plus">+</button><span>${gbp(effectivePrice(p) * qty)}</span>`;
    row.querySelector('.n').textContent = p.name;
    row.querySelector('.minus').onclick = () => { qty > 1 ? cart.set(pid, qty - 1) : cart.delete(pid); renderCart(); };
    row.querySelector('.plus').onclick = () => { cart.set(pid, qty + 1); renderCart(); };
    host.appendChild(row);
  }
  const t = cartTotals();
  $('subTotal').textContent = gbp(t.sub);
  $('vatTotal').textContent = gbp(t.vat);
  $('grandTotal').textContent = gbp(t.total);
  $('payBtn').disabled = cart.size === 0;
}

// ---------- checkout & outbox ----------
async function checkout() {
  const restricted = [...cart.keys()].some(pid => products.get(pid)?.isAgeRestricted);
  let ageVerified = false;
  if (restricted) {
    ageVerified = confirm('Challenge 25: basket contains age-restricted items.\n\nHas the customer proven they are old enough?');
    if (!ageVerified) { toast('Sale cancelled: age not verified.'); return; }
  }

  const t = cartTotals();
  const sale = {
    clientSaleId: crypto.randomUUID(),
    items: [...cart.entries()].map(([productId, quantity]) => ({ productId, quantity })),
    paymentType: 0,
    tenderedAmount: Math.ceil(t.total),
    ageVerified,
    occurredAt: new Date().toISOString(),
  };

  cart = new Map();
  renderCart();
  await submitSale(sale, true);
}

async function submitSale(sale, fresh) {
  try {
    const res = await fetch(`${API}/sales/orders/pos`, { method: 'POST', headers: authHeaders(), body: JSON.stringify(sale) });
    if (res.ok) {
      const body = await res.json();
      toast(body.messages?.[0] || 'Sale recorded');
      return true;
    }
    if (res.status === 401) { await idb.put('outbox', sale); await refreshQueueBadge(); toast('Session expired - sale queued. Sign in to submit.'); return false; }
    const body = await res.json().catch(() => ({}));
    toast('Sale rejected: ' + (body.exception || res.status));
    return false; // rejected sales are NOT queued - they would never succeed
  } catch {
    // Network failure: keep the sale safe and replay later.
    await idb.put('outbox', sale);
    await refreshQueueBadge();
    toast(fresh ? 'Offline - sale queued and will sync automatically.' : 'Still offline.');
    return false;
  }
}

let flushing = false;
async function flushOutbox() {
  if (flushing || !token) return;
  flushing = true;
  try {
    const queued = await idb.getAll('outbox');
    for (const sale of queued) {
      try {
        const res = await fetch(`${API}/sales/orders/pos`, { method: 'POST', headers: authHeaders(), body: JSON.stringify(sale) });
        if (res.ok) {
          await idb.del('outbox', sale.clientSaleId);
          toast('Queued sale submitted.');
        } else if (res.status === 401) {
          break; // needs a fresh sign-in; keep everything queued
        } else {
          // Permanently rejected (e.g. product removed): drop it so the queue can drain,
          // the attempt is still auditable server-side via logs.
          const body = await res.json().catch(() => ({}));
          console.warn('Dropping rejected queued sale', sale.clientSaleId, body);
          await idb.del('outbox', sale.clientSaleId);
          toast('A queued sale was rejected by the server and removed.');
        }
      } catch {
        break; // still offline
      }
    }
  } finally {
    flushing = false;
    await refreshQueueBadge();
  }
}

async function refreshQueueBadge() {
  const n = (await idb.getAll('outbox')).length;
  const badge = $('queueBadge');
  badge.textContent = `${n} queued`;
  badge.classList.toggle('hidden', n === 0);
}

// ---------- auth & shell ----------
async function login() {
  $('loginError').textContent = '';
  try {
    const res = await fetch(`${API}/identity/tokens`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: $('email').value, password: $('password').value }),
    });
    if (!res.ok) throw new Error('Invalid credentials');
    const body = await res.json();
    token = body.data.token;
    localStorage.setItem('pos.token', token);
    await enterPos();
  } catch (e) {
    $('loginError').textContent = e.message === 'Failed to fetch' ? 'Offline - cannot sign in.' : 'Sign-in failed.';
  }
}

function logout() {
  token = null;
  localStorage.removeItem('pos.token');
  $('pos').classList.add('hidden');
  $('syncBtn').classList.add('hidden');
  $('logoutBtn').classList.add('hidden');
  $('userLabel').textContent = '';
  $('login').classList.remove('hidden');
}

async function enterPos() {
  $('login').classList.add('hidden');
  $('pos').classList.remove('hidden');
  $('syncBtn').classList.remove('hidden');
  $('logoutBtn').classList.remove('hidden');
  const claims = decodeJwt(token);
  $('userLabel').textContent = claims.fullName || '';
  await loadCatalogFromCache();
  try { await syncCatalog(false); } catch { toast('Using cached catalog (offline).'); }
  await refreshQueueBadge();
  await flushOutbox();
}

function updateConnLabel() {
  const online = navigator.onLine;
  $('statusbar').classList.toggle('offline', !online);
  $('connLabel').textContent = online ? 'Online' : 'Offline';
}

// ---------- boot ----------
(async function boot() {
  db = await openDb();
  if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('sw.js').catch(() => { /* http or unsupported */ });
  }
  $('loginBtn').onclick = login;
  $('password').addEventListener('keydown', e => { if (e.key === 'Enter') login(); });
  $('logoutBtn').onclick = logout;
  $('syncBtn').onclick = () => syncCatalog(true).catch(() => toast('Sync failed (offline?).'));
  $('payBtn').onclick = checkout;
  window.addEventListener('online', () => { updateConnLabel(); flushOutbox(); });
  window.addEventListener('offline', updateConnLabel);
  setInterval(flushOutbox, 15000);
  updateConnLabel();
  await refreshQueueBadge();
  if (token) await enterPos();
})();
