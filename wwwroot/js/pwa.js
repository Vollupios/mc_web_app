// PWA Funcionalidades
class PWAManager {
    constructor() {
        this.deferredPrompt = null;
        this.isInstalled = false;
        this.isOnline = navigator.onLine;
        this.init();
    }

    init() {
        this.setupInstallPrompt();
        this.setupNetworkEvents();
        this.setupPushNotifications();
        this.checkInstallStatus();
    }

    // Gerenciar prompt de instalação
    setupInstallPrompt() {
        window.addEventListener('beforeinstallprompt', (e) => {
            e.preventDefault();
            this.deferredPrompt = e;
            this.showInstallBanner();
        });

        window.addEventListener('appinstalled', () => {
            this.isInstalled = true;
            this.hideInstallBanner();
            this.showToast('App instalado com sucesso!', 'success');
        });
    }

    // Mostrar banner de instalação
    showInstallBanner() {
        if (this.isInstalled) return;

        const banner = document.createElement('div');
        banner.id = 'pwa-install-banner';
        banner.className = 'pwa-install-banner';
        banner.innerHTML = `
            <div class="container">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <i class="bi bi-phone"></i>
                        <strong>Instalar App</strong>
                        <br><small>Acesse rapidamente pelo seu dispositivo</small>
                    </div>
                    <div>
                        <button class="btn btn-light btn-sm me-2" onclick="pwaManager.installApp()">
                            <i class="bi bi-download"></i> Instalar
                        </button>
                        <button class="btn btn-outline-light btn-sm" onclick="pwaManager.hideInstallBanner()">
                            <i class="bi bi-x"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(banner);
        
        // Mostrar com animação
        setTimeout(() => {
            banner.classList.add('show');
        }, 1000);
    }

    // Instalar PWA
    async installApp() {
        if (!this.deferredPrompt) return;

        this.deferredPrompt.prompt();
        const { outcome } = await this.deferredPrompt.userChoice;
        
        if (outcome === 'accepted') {
            this.showToast('Instalando app...', 'info');
        }
        
        this.deferredPrompt = null;
        this.hideInstallBanner();
    }

    // Esconder banner de instalação
    hideInstallBanner() {
        const banner = document.getElementById('pwa-install-banner');
        if (banner) {
            banner.classList.remove('show');
            setTimeout(() => banner.remove(), 300);
        }
    }

    // Verificar se já está instalado
    checkInstallStatus() {
        // Verificar se está rodando como PWA
        if (window.matchMedia('(display-mode: standalone)').matches || 
            window.navigator.standalone === true) {
            this.isInstalled = true;
        }
    }

    // Gerenciar eventos de rede
    setupNetworkEvents() {
        window.addEventListener('online', () => {
            this.isOnline = true;
            this.hideOfflineIndicator();
            this.showToast('Conectado novamente!', 'success');
            this.syncOfflineData();
        });

        window.addEventListener('offline', () => {
            this.isOnline = false;
            this.showOfflineIndicator();
            this.showToast('Modo offline ativado', 'warning');
        });

        // Verificar status inicial
        if (!this.isOnline) {
            this.showOfflineIndicator();
        }
    }

    // Mostrar indicador offline
    showOfflineIndicator() {
        let indicator = document.getElementById('offline-indicator');
        if (!indicator) {
            indicator = document.createElement('div');
            indicator.id = 'offline-indicator';
            indicator.className = 'offline-indicator';
            indicator.innerHTML = `
                <i class="bi bi-wifi-off"></i>
                <strong>Modo Offline</strong> - Algumas funcionalidades podem estar limitadas
            `;
            document.body.appendChild(indicator);
        }
        indicator.classList.add('show');
    }

    // Esconder indicador offline
    hideOfflineIndicator() {
        const indicator = document.getElementById('offline-indicator');
        if (indicator) {
            indicator.classList.remove('show');
        }
    }

    // Configurar notificações push
    setupPushNotifications() {
        if ('Notification' in window && 'serviceWorker' in navigator) {
            // Solicitar permissão se necessário
            if (Notification.permission === 'default') {
                this.requestNotificationPermission();
            }
        }
    }

    // Solicitar permissão para notificações
    async requestNotificationPermission() {
        try {
            const permission = await Notification.requestPermission();
            if (permission === 'granted') {
                this.showToast('Notificações ativadas!', 'success');
            }
        } catch (error) {
            console.log('Erro ao solicitar permissão de notificação:', error);
        }
    }

    // Enviar notificação local
    showNotification(title, options = {}) {
        if ('serviceWorker' in navigator && Notification.permission === 'granted') {
            navigator.serviceWorker.ready.then(registration => {
                registration.showNotification(title, {
                    icon: '/images/icons/icon-192x192.png',
                    badge: '/images/icons/icon-72x72.png',
                    vibrate: [200, 100, 200],
                    ...options
                });
            });
        }
    }

    // Sincronizar dados offline
    async syncOfflineData() {
        try {
            // Verificar se há dados para sincronizar
            const pendingData = this.getOfflineData();
            
            if (pendingData.length > 0) {
                this.showToast('Sincronizando dados...', 'info');
                
                for (const data of pendingData) {
                    await this.uploadOfflineData(data);
                }
                
                this.clearOfflineData();
                this.showToast('Dados sincronizados!', 'success');
            }
        } catch (error) {
            console.log('Erro na sincronização:', error);
            this.showToast('Erro ao sincronizar dados', 'danger');
        }
    }

    // Salvar dados para sincronização offline
    saveOfflineData(data) {
        const offlineData = this.getOfflineData();
        offlineData.push({
            id: Date.now(),
            timestamp: new Date().toISOString(),
            ...data
        });
        localStorage.setItem('offlineData', JSON.stringify(offlineData));
    }

    // Obter dados offline
    getOfflineData() {
        try {
            return JSON.parse(localStorage.getItem('offlineData') || '[]');
        } catch {
            return [];
        }
    }

    // Limpar dados offline
    clearOfflineData() {
        localStorage.removeItem('offlineData');
    }

    // Upload de dados offline
    async uploadOfflineData(data) {
        // Implementar conforme necessário para cada tipo de dados
        console.log('Enviando dados offline:', data);
        
        // Exemplo para formulários
        if (data.type === 'form') {
            const response = await fetch(data.url, {
                method: data.method || 'POST',
                headers: data.headers || { 'Content-Type': 'application/json' },
                body: JSON.stringify(data.formData)
            });
            
            if (!response.ok) {
                throw new Error(`Erro HTTP: ${response.status}`);
            }
        }
    }

    // Mostrar toast notification
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 350px; min-width: 250px;';
        toast.innerHTML = `
            ${this.getToastIcon(type)} ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(toast);
        
        // Auto-remover após 5 segundos
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 5000);
    }

    // Ícones para toast
    getToastIcon(type) {
        const icons = {
            success: '<i class="bi bi-check-circle"></i>',
            danger: '<i class="bi bi-exclamation-triangle"></i>',
            warning: '<i class="bi bi-exclamation-circle"></i>',
            info: '<i class="bi bi-info-circle"></i>'
        };
        return icons[type] || icons.info;
    }

    // Adicionar loading state a elementos
    addLoading(element) {
        if (element) {
            element.classList.add('loading');
            element.disabled = true;
        }
    }

    // Remover loading state
    removeLoading(element) {
        if (element) {
            element.classList.remove('loading');
            element.disabled = false;
        }
    }

    // Share API
    async shareContent(data) {
        if (navigator.share) {
            try {
                await navigator.share(data);
                this.showToast('Compartilhado com sucesso!', 'success');
            } catch (error) {
                if (error.name !== 'AbortError') {
                    console.log('Erro ao compartilhar:', error);
                }
            }
        } else {
            // Fallback para navegadores sem Share API
            this.copyToClipboard(data.url || data.text || '');
        }
    }

    // Copiar para clipboard
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            this.showToast('Copiado para área de transferência!', 'success');
        } catch (error) {
            console.log('Erro ao copiar:', error);
            this.showToast('Erro ao copiar', 'danger');
        }
    }
}

// Inicializar PWA Manager
let pwaManager;
document.addEventListener('DOMContentLoaded', () => {
    pwaManager = new PWAManager();
});

// Interceptar formulários para funcionamento offline
document.addEventListener('submit', (e) => {
    if (!navigator.onLine && e.target.tagName === 'FORM') {
        e.preventDefault();
        
        const formData = new FormData(e.target);
        const data = {
            type: 'form',
            url: e.target.action,
            method: e.target.method,
            formData: Object.fromEntries(formData)
        };
        
        pwaManager.saveOfflineData(data);
        pwaManager.showToast('Dados salvos para sincronização', 'info');
    }
});
