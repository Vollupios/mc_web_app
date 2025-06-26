const CACHE_NAME = 'marcos-docs-v1.0.0';
const STATIC_CACHE = 'static-v1.0.0';
const DYNAMIC_CACHE = 'dynamic-v1.0.0';

// Arquivos essenciais para cache estático
const STATIC_FILES = [
  '/',
  '/css/site.css',
  '/lib/bootstrap/dist/css/bootstrap.min.css',
  '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
  '/lib/jquery/dist/jquery.min.js',
  '/js/site.js',
  '/manifest.json',
  '/images/logo.png',
  'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css'
];

// Páginas importantes para cache
const IMPORTANT_PAGES = [
  '/',
  '/Documents',
  '/Documents/Upload',
  '/Documents/AdvancedSearch',
  '/Ramais',
  '/Reunioes'
];

// Instalar service worker
self.addEventListener('install', event => {
  console.log('Service Worker: Instalando...');
  event.waitUntil(
    caches.open(STATIC_CACHE)
      .then(cache => {
        console.log('Service Worker: Cache estático criado');
        return cache.addAll(STATIC_FILES);
      })
      .catch(err => console.log('Erro ao criar cache estático:', err))
  );
});

// Ativar service worker
self.addEventListener('activate', event => {
  console.log('Service Worker: Ativando...');
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cache => {
          if (cache !== STATIC_CACHE && cache !== DYNAMIC_CACHE) {
            console.log('Service Worker: Removendo cache antigo:', cache);
            return caches.delete(cache);
          }
        })
      );
    })
  );
  return self.clients.claim();
});

// Interceptar requisições (Estratégia Cache First para recursos estáticos)
self.addEventListener('fetch', event => {
  // Apenas requisições GET
  if (event.request.method !== 'GET') return;

  // Recursos estáticos (CSS, JS, imagens)
  if (isStaticResource(event.request.url)) {
    event.respondWith(cacheFirst(event.request));
    return;
  }

  // Páginas importantes (Network First)
  if (isImportantPage(event.request.url)) {
    event.respondWith(networkFirst(event.request));
    return;
  }

  // Outras requisições (Network First com fallback)
  event.respondWith(networkWithFallback(event.request));
});

// Verificar se é recurso estático
function isStaticResource(url) {
  return url.includes('.css') || 
         url.includes('.js') || 
         url.includes('.png') || 
         url.includes('.jpg') || 
         url.includes('.jpeg') || 
         url.includes('.gif') || 
         url.includes('.svg') ||
         url.includes('bootstrap-icons');
}

// Verificar se é página importante
function isImportantPage(url) {
  return IMPORTANT_PAGES.some(page => url.endsWith(page) || url.includes(page));
}

// Estratégia Cache First
async function cacheFirst(request) {
  try {
    const cacheResponse = await caches.match(request);
    if (cacheResponse) {
      return cacheResponse;
    }

    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(STATIC_CACHE);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    console.log('Cache First falhou:', error);
    return new Response('Recurso não disponível offline', { status: 503 });
  }
}

// Estratégia Network First
async function networkFirst(request) {
  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(DYNAMIC_CACHE);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    console.log('Network First - Tentando cache:', error);
    const cacheResponse = await caches.match(request);
    if (cacheResponse) {
      return cacheResponse;
    }
    
    // Fallback para página offline
    if (request.headers.get('Accept').includes('text/html')) {
      return caches.match('/offline.html') || 
             new Response('Página não disponível offline', { 
               status: 503,
               headers: { 'Content-Type': 'text/html' }
             });
    }
    
    return new Response('Conteúdo não disponível offline', { status: 503 });
  }
}

// Network com fallback
async function networkWithFallback(request) {
  try {
    const networkResponse = await fetch(request);
    return networkResponse;
  } catch (error) {
    const cacheResponse = await caches.match(request);
    if (cacheResponse) {
      return cacheResponse;
    }
    return new Response('Conteúdo não disponível', { status: 503 });
  }
}

// Sincronização em background (quando voltar online)
self.addEventListener('sync', event => {
  if (event.tag === 'background-sync') {
    console.log('Service Worker: Sincronização em background');
    event.waitUntil(syncData());
  }
});

// Função para sincronizar dados quando voltar online
async function syncData() {
  try {
    // Aqui você pode implementar lógica para sincronizar dados offline
    console.log('Sincronizando dados...');
    
    // Exemplo: reenviar formulários salvos offline
    const pendingForms = await getStoredForms();
    for (const form of pendingForms) {
      try {
        await fetch(form.url, {
          method: form.method,
          body: form.data,
          headers: form.headers
        });
        // Remover formulário sincronizado
        await removeStoredForm(form.id);
      } catch (error) {
        console.log('Erro ao sincronizar formulário:', error);
      }
    }
  } catch (error) {
    console.log('Erro na sincronização:', error);
  }
}

// Funções auxiliares para IndexedDB (implementar conforme necessário)
async function getStoredForms() {
  // Implementar busca no IndexedDB
  return [];
}

async function removeStoredForm(id) {
  // Implementar remoção no IndexedDB
  return true;
}
