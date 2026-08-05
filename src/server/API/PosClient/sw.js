// FluentPOS till service worker: keeps the app shell available offline.
// API calls are never cached - live data comes from the network, offline data
// comes from IndexedDB managed by the app itself.
const SHELL_CACHE = 'fluentpos-pos-shell-v1';
const SHELL = ['./', 'index.html', 'app.js', 'manifest.json'];

self.addEventListener('install', event => {
  event.waitUntil(caches.open(SHELL_CACHE).then(c => c.addAll(SHELL)).then(() => self.skipWaiting()));
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys()
      .then(keys => Promise.all(keys.filter(k => k !== SHELL_CACHE).map(k => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);
  if (event.request.method !== 'GET' || url.pathname.startsWith('/api/')) {
    return; // pass through - the app handles API failures via its outbox
  }

  // Shell: network-first with cache fallback so updates roll out but offline still works.
  event.respondWith(
    fetch(event.request)
      .then(res => {
        const copy = res.clone();
        caches.open(SHELL_CACHE).then(c => c.put(event.request, copy));
        return res;
      })
      .catch(() => caches.match(event.request, { ignoreSearch: true })
        .then(hit => hit || caches.match('index.html')))
  );
});
