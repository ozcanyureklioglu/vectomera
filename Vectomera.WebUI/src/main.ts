import './style.css';
import { api } from './api';

// --- View Router & State ---
const views = document.querySelectorAll('.view');
const menuItems = document.querySelectorAll('.menu-item');
let activeProductId: string | null = null;
let currentProductsList: any[] = []; // Cache loaded products

async function switchView(viewId: string, param?: string) {
    views.forEach(v => v.classList.remove('active'));
    menuItems.forEach(m => m.classList.remove('active'));
    
    document.getElementById(`${viewId}-view`)?.classList.add('active');
    
    const menuItem = document.querySelector(`[data-view="${viewId}"]`);
    if (menuItem) menuItem.classList.add('active');

    if (viewId === 'product-detail' && param) {
        activeProductId = param;
        await renderProductDetail(param);
    } else {
        await loadViewData(viewId);
    }
}

menuItems.forEach(item => {
    item.addEventListener('click', (e) => {
        e.preventDefault();
        const viewId = (e.currentTarget as HTMLElement).dataset.view;
        if (viewId) switchView(viewId);
    });
});

// --- View Data Loaders ---
const DOM = {
    aiAgent: document.getElementById('ai-agent-view')!,
    products: document.getElementById('products-view')!,
    productDetail: document.getElementById('product-detail-view')!,
    reviews: document.getElementById('reviews-view')!,
    warehouse: document.getElementById('warehouse-view')!
};

async function loadViewData(viewId: string) {
    switch (viewId) {
        case 'ai-agent':
            renderAiAgent();
            break;
        case 'products':
            await renderProducts();
            break;
        case 'reviews':
            await renderReviews();
            break;
        case 'warehouse':
            await renderWarehouse();
            break;
    }
}

// Helper: Basic Markdown to HTML
function parseMd(text: string) {
    return text
        .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
        .replace(/\n/g, '<br/>')
        .replace(/\[(.*?)\]/g, '<strong>[$1]</strong>');
}

// 1. AI Agent View
let isAiAgentInitialized = false;
function renderAiAgent() {
    if (isAiAgentInitialized) return;
    isAiAgentInitialized = true;

    DOM.aiAgent.innerHTML = `
        <h2 class="page-title">AI Ajan</h2>
        <div class="chat-container">
            <div class="chat-messages" id="chat-messages">
                <div class="message">
                    <div class="message-avatar">🤖</div>
                    <div class="message-content md-content">
                        Merhaba! Vectomera ürün ve stok sistemi hakkında bana istediğini sorabilirsin.
                    </div>
                </div>
            </div>
            <div class="chat-input-container">
                <input type="text" id="chat-input" class="chat-input" placeholder="Bir şeyler sorun..." />
                <button id="chat-send" class="send-btn">➤</button>
            </div>
        </div>
    `;

    const input = document.getElementById('chat-input') as HTMLInputElement;
    const sendBtn = document.getElementById('chat-send') as HTMLButtonElement;
    const messages = document.getElementById('chat-messages')!;

    const sendMessage = async () => {
        const query = input.value.trim();
        if (!query) return;

        // User message
        messages.innerHTML += `
            <div class="message user">
                <div class="message-avatar">👤</div>
                <div class="message-content">${query}</div>
            </div>
        `;
        input.value = '';
        input.disabled = true;
        sendBtn.disabled = true;
        messages.scrollTop = messages.scrollHeight;

        // Loader
        const loaderId = 'loader-' + Date.now();
        messages.innerHTML += `
            <div class="message bot" id="${loaderId}">
                <div class="message-avatar">🤖</div>
                <div class="message-content"><div class="loader" style="margin: 0; width: 16px; height:16px;"></div></div>
            </div>
        `;
        messages.scrollTop = messages.scrollHeight;

        try {
            const res = await api.askAi(query);
            const text = res.data?.answer || res.message || 'Cevap alınamadı.';
            document.getElementById(loaderId)!.innerHTML = `
                <div class="message-avatar">🤖</div>
                <div class="message-content md-content">${parseMd(text)}</div>
            `;
        } catch (err: any) {
            document.getElementById(loaderId)!.innerHTML = `
                <div class="message-avatar">⚠️</div>
                <div class="message-content" style="color: #ef4444;">Hata: ${err.message}</div>
            `;
        } finally {
            input.disabled = false;
            sendBtn.disabled = false;
            input.focus();
            messages.scrollTop = messages.scrollHeight;
        }
    };

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });
}

// 2. Products View
async function renderProducts() {
    DOM.products.innerHTML = `
        <div class="view-header">
            <h2 class="page-title">Ürünler</h2>
            <button id="add-product-btn" class="btn btn-primary">+ Yeni Ürün</button>
        </div>
        <div class="loader"></div>
    `;

    try {
        const res = await api.getProducts();
        currentProductsList = res.data || [];
        
        if (currentProductsList.length === 0) {
            DOM.products.innerHTML = `
                <div class="view-header">
                    <h2 class="page-title">Ürünler</h2>
                    <button id="add-product-btn" class="btn btn-primary">+ Yeni Ürün</button>
                </div>
                <p style="text-align: center; margin-top: 40px; color: var(--text-secondary);">Henüz ürün eklenmemiş.</p>
            `;
            document.getElementById('add-product-btn')?.addEventListener('click', () => {
                openCreateProductModal();
            });
            return;
        }

        const html = currentProductsList.map((p: any) => `
            <div class="data-card product-card" data-id="${p.id}">
                <div class="card-actions">
                    <button class="btn btn-secondary btn-sm edit-card-btn" data-id="${p.id}">✏️</button>
                    <button class="btn btn-danger btn-sm delete-card-btn" data-id="${p.id}">🗑️</button>
                </div>
                <h3>${p.name}</h3>
                <div class="meta">
                    <span>SKU: ${p.sku}</span>
                    <span class="badge">${p.brandName || p.brand || 'Markasız'}</span>
                </div>
                <p class="desc">${p.description || 'Açıklama yok.'}</p>
            </div>
        `).join('');
        
        DOM.products.innerHTML = `
            <div class="view-header">
                <h2 class="page-title">Ürünler</h2>
                <button id="add-product-btn" class="btn btn-primary">+ Yeni Ürün</button>
            </div>
            <div class="data-grid">${html}</div>
        `;

        // Add Event Listeners
        // 1. Click on card to open details
        document.querySelectorAll('.product-card').forEach(card => {
            card.addEventListener('click', (e) => {
                const target = e.target as HTMLElement;
                if (target.closest('.edit-card-btn') || target.closest('.delete-card-btn')) {
                    return;
                }
                const id = (card as HTMLElement).dataset.id;
                if (id) switchView('product-detail', id);
            });
        });

        // 2. Click on edit button
        document.querySelectorAll('.edit-card-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const id = (btn as HTMLElement).dataset.id;
                if (id) openEditProductModal(id);
            });
        });

        // 3. Click on delete button
        document.querySelectorAll('.delete-card-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.stopPropagation();
                const id = (btn as HTMLElement).dataset.id;
                if (id && confirm('Bu ürünü silmek istediğinize emin misiniz? Tüm yorumlar ve stok bilgileri de silinecektir.')) {
                    try {
                        await api.deleteProduct(id);
                        alert('Ürün silindi.');
                        await renderProducts();
                    } catch (err: any) {
                        alert('Hata: ' + err.message);
                    }
                }
            });
        });

        // Hook create button
        document.getElementById('add-product-btn')?.addEventListener('click', () => {
            openCreateProductModal();
        });

    } catch (err: any) {
        DOM.products.innerHTML = `
            <div class="view-header">
                <h2 class="page-title">Ürünler</h2>
                <button id="add-product-btn" class="btn btn-primary">+ Yeni Ürün</button>
            </div>
            <p>Hata: ${err.message}</p>
        `;
        document.getElementById('add-product-btn')?.addEventListener('click', () => {
            openCreateProductModal();
        });
    }
}

// 3. Reviews View
async function renderReviews() {
    DOM.reviews.innerHTML = `<h2 class="page-title">Ürün Yorumları</h2><div class="loader"></div>`;
    try {
        const res = await api.getReviews();
        const reviews = res.data || [];
        
        const html = reviews.map((r: any) => `
            <div class="data-card">
                <h3>${r.title}</h3>
                <div class="meta">
                    <span class="badge rating">⭐ ${r.point} / 5</span>
                </div>
                <p class="desc">${r.description}</p>
            </div>
        `).join('');
        
        DOM.reviews.innerHTML = `<h2 class="page-title">Ürün Yorumları</h2><div class="data-grid">${html}</div>`;
    } catch (err: any) {
        DOM.reviews.innerHTML = `<h2 class="page-title">Ürün Yorumları</h2><p>Hata: ${err.message}</p>`;
    }
}

// 4. Warehouse View
async function renderWarehouse() {
    DOM.warehouse.innerHTML = `<h2 class="page-title">Depo ve Ürünler</h2><div class="loader"></div>`;
    try {
        const res = await api.getWarehouseInventories();
        const inv = res.data || [];
        
        const html = inv.map((i: any) => `
            <div class="data-card">
                <h3>${i.warehouseName}</h3>
                <div class="meta">
                    <span class="badge stock">Stok: ${i.availableStock}</span>
                    <span class="badge price">₺${i.price}</span>
                </div>
                <p class="desc"><strong>${i.productName}</strong><br/>${i.description}</p>
            </div>
        `).join('');
        
        DOM.warehouse.innerHTML = `<h2 class="page-title">Depo ve Ürünler</h2><div class="data-grid">${html}</div>`;
    } catch (err: any) {
        DOM.warehouse.innerHTML = `<h2 class="page-title">Depo ve Ürünler</h2><p>Hata: ${err.message}</p>`;
    }
}

// Init
switchView('ai-agent');


// --- CRUD Modal Handlers & Forms ---
const createModal = document.getElementById('create-product-modal')!;
const editModal = document.getElementById('edit-product-modal')!;

// Close handlers
document.getElementById('close-create-modal')?.addEventListener('click', () => createModal.classList.remove('active'));
document.getElementById('close-edit-modal')?.addEventListener('click', () => editModal.classList.remove('active'));

async function openCreateProductModal() {
    createModal.classList.add('active');
    
    const brandSelect = document.getElementById('create-brand') as HTMLSelectElement;
    const catSelect = document.getElementById('create-category') as HTMLSelectElement;
    
    brandSelect.innerHTML = '<option value="">Yükleniyor...</option>';
    catSelect.innerHTML = '<option value="">Yükleniyor...</option>';

    try {
        const [brandsRes, catsRes] = await Promise.all([api.getBrands(), api.getCategories()]);
        
        const brands = brandsRes.data || [];
        brandSelect.innerHTML = '<option value="">Seçiniz...</option>' + 
            brands.map((b: any) => `<option value="${b.id}">${b.name}</option>`).join('');

        const cats = catsRes.data || [];
        catSelect.innerHTML = '<option value="">Seçiniz...</option>' + 
            cats.map((c: any) => `<option value="${c.id}">${c.name}</option>`).join('');
    } catch (err: any) {
        brandSelect.innerHTML = '<option value="">Hata oluştu</option>';
        catSelect.innerHTML = '<option value="">Hata oluştu</option>';
    }
}

function openEditProductModal(id: string) {
    const p = currentProductsList.find((x: any) => x.id === id);
    if (!p) return;

    editModal.classList.add('active');
    
    (document.getElementById('edit-id') as HTMLInputElement).value = p.id;
    (document.getElementById('edit-name') as HTMLInputElement).value = p.name;
    (document.getElementById('edit-sku') as HTMLInputElement).value = p.sku;
    (document.getElementById('edit-description') as HTMLTextAreaElement).value = p.description || '';
    // Optional SearchText field
    (document.getElementById('edit-searchtext') as HTMLInputElement).value = '';
}

// Create Form Submit
document.getElementById('create-product-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const name = (document.getElementById('create-name') as HTMLInputElement).value;
    const sku = (document.getElementById('create-sku') as HTMLInputElement).value;
    const brandId = (document.getElementById('create-brand') as HTMLSelectElement).value;
    const categoryId = (document.getElementById('create-category') as HTMLSelectElement).value;
    const description = (document.getElementById('create-description') as HTMLTextAreaElement).value;
    const searchTextStr = (document.getElementById('create-searchtext') as HTMLInputElement).value;
    
    const searchText = searchTextStr ? searchTextStr.split(',').map(s => s.trim()).join(' ') : name;

    try {
        await api.createProduct({
            name,
            sku,
            brandId,
            categoryId,
            description,
            searchText,
            propertyValueIds: [] // Simple default properties
        });
        
        createModal.classList.remove('active');
        (e.target as HTMLFormElement).reset();
        alert('Ürün başarıyla oluşturuldu.');
        await renderProducts();
    } catch (err: any) {
        alert('Hata: ' + (err.response?.data?.message || err.message));
    }
});

// Edit Form Submit
document.getElementById('edit-product-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = (document.getElementById('edit-id') as HTMLInputElement).value;
    const name = (document.getElementById('edit-name') as HTMLInputElement).value;
    const sku = (document.getElementById('edit-sku') as HTMLInputElement).value;
    const description = (document.getElementById('edit-description') as HTMLTextAreaElement).value;
    const searchTextStr = (document.getElementById('edit-searchtext') as HTMLInputElement).value;
    
    const searchText = searchTextStr ? searchTextStr.split(',').map(s => s.trim()).join(' ') : name;

    try {
        await api.updateProduct(id, {
            name,
            sku,
            description,
            searchText
        });
        
        editModal.classList.remove('active');
        (e.target as HTMLFormElement).reset();
        alert('Ürün başarıyla güncellendi.');
        await renderProducts();
    } catch (err: any) {
        alert('Hata: ' + (err.response?.data?.message || err.message));
    }
});

// --- 5. Product Detail View ---
async function renderProductDetail(productId: string) {
    DOM.productDetail.innerHTML = '<div class="loader"></div>';

    try {
        // Fetch specific product by checking cached list, or refetching products
        let p = currentProductsList.find((x: any) => x.id === productId);
        if (!p) {
            const res = await api.getProducts();
            currentProductsList = res.data || [];
            p = currentProductsList.find((x: any) => x.id === productId);
        }

        if (!p) {
            DOM.productDetail.innerHTML = `
                <button class="back-btn" id="detail-back-btn">← Geri Dön</button>
                <p>Ürün bulunamadı.</p>
            `;
            document.getElementById('detail-back-btn')?.addEventListener('click', () => switchView('products'));
            return;
        }

        // Fetch inventories & reviews for this product
        const [reviewsRes, inventoryRes] = await Promise.all([
            api.getReviews(productId),
            api.getWarehouseInventories(productId)
        ]);

        const reviews = reviewsRes.data || [];
        const inventories = inventoryRes.data || [];

        // Build reviews html
        let reviewsHtml = '';
        if (reviews.length === 0) {
            reviewsHtml = '<p style="color: var(--text-secondary);">Bu ürün için henüz yorum yapılmamış.</p>';
        } else {
            reviewsHtml = reviews.map((r: any) => `
                <div class="review-item">
                    <div class="review-header">
                        <span class="review-title">${r.title}</span>
                        <span class="badge rating">⭐ ${r.point} / 5</span>
                    </div>
                    <p class="review-text">${r.description}</p>
                </div>
            `).join('');
        }

        // Build stock levels html
        let stockHtml = '';
        if (inventories.length === 0) {
            stockHtml = '<p style="color: var(--text-secondary); font-size: 0.9rem;">Stok kaydı bulunamadı.</p>';
        } else {
            stockHtml = `
                <div class="stock-list">
                    ${inventories.map((i: any) => `
                        <div class="stock-item">
                            <span class="stock-warehouse">${i.warehouseName}</span>
                            <div style="text-align: right;">
                                <span class="badge stock" style="display:block; margin-bottom:4px;">Stok: ${i.availableStock}</span>
                                <span class="badge price">₺${i.price}</span>
                            </div>
                        </div>
                    `).join('')}
                </div>
            `;
        }

        DOM.productDetail.innerHTML = `
            <button class="back-btn" id="detail-back-btn">← Ürünlere Geri Dön</button>
            <div class="detail-container">
                <div class="detail-header-section">
                    <div class="detail-title">
                        <h1>${p.name}</h1>
                        <div class="detail-meta">
                            <span><strong>SKU:</strong> ${p.sku}</span>
                            <span>|</span>
                            <span><strong>Marka:</strong> ${p.brandName || p.brand || 'Markasız'}</span>
                            <span>|</span>
                            <span><strong>Kategori:</strong> ${p.categoryName || p.category || 'Kategorisiz'}</span>
                        </div>
                    </div>
                    <div class="detail-actions">
                        <button class="btn btn-secondary" id="detail-edit-btn">✏️ Düzenle</button>
                        <button class="btn btn-danger" id="detail-delete-btn">🗑️ Sil</button>
                    </div>
                </div>

                <div class="detail-grid-layout">
                    <div class="detail-main">
                        <div>
                            <h3 class="detail-section-title">Ürün Açıklaması</h3>
                            <div class="product-description-card">
                                ${p.description || 'Bu ürün için açıklama bulunmuyor.'}
                            </div>
                        </div>

                        <div>
                            <h3 class="detail-section-title">Kullanıcı Yorumları</h3>
                            <div class="reviews-container">
                                ${reviewsHtml}
                            </div>
                        </div>

                        <!-- Add Review Form -->
                        <div class="card" style="margin-top: 24px;">
                            <h3 style="margin-bottom: 20px; color:#fff;">Yorum Yaz</h3>
                            <form id="add-review-form">
                                <div class="form-group">
                                    <label for="review-title">Başlık *</label>
                                    <input type="text" id="review-title" required placeholder="Örn: Çok başarılı" />
                                </div>
                                <div class="form-group">
                                    <label for="review-point">Puan *</label>
                                    <select id="review-point" required>
                                        <option value="5">5 - Mükemmel</option>
                                        <option value="4">4 - Çok İyi</option>
                                        <option value="3">3 - Ortalama</option>
                                        <option value="2">2 - Kötü</option>
                                        <option value="1">1 - Çok Kötü</option>
                                    </select>
                                </div>
                                <div class="form-group">
                                    <label for="review-desc">Yorumunuz *</label>
                                    <textarea id="review-desc" rows="4" required placeholder="Ürün hakkındaki değerlendirmeniz..."></textarea>
                                </div>
                                <button type="submit" class="btn btn-primary">Yorumu Gönder</button>
                            </form>
                        </div>
                    </div>

                    <div class="detail-sidebar">
                        <div class="sidebar-card">
                            <h3>Depo Stok Durumları</h3>
                            ${stockHtml}
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Event listeners
        document.getElementById('detail-back-btn')?.addEventListener('click', () => switchView('products'));
        
        document.getElementById('detail-edit-btn')?.addEventListener('click', () => {
            openEditProductModal(productId);
        });

        document.getElementById('detail-delete-btn')?.addEventListener('click', async () => {
            if (confirm('Bu ürünü silmek istediğinize emin misiniz? Tüm yorumlar ve stok bilgileri de silinecektir.')) {
                try {
                    await api.deleteProduct(productId);
                    alert('Ürün silindi.');
                    switchView('products');
                } catch (err: any) {
                    alert('Hata: ' + err.message);
                }
            }
        });

        // Add Review Submit
        document.getElementById('add-review-form')?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const title = (document.getElementById('review-title') as HTMLInputElement).value;
            const point = parseInt((document.getElementById('review-point') as HTMLSelectElement).value);
            const description = (document.getElementById('review-desc') as HTMLTextAreaElement).value;

            try {
                await api.createReview({
                    productId,
                    title,
                    point,
                    description
                });
                
                alert('Yorumunuz başarıyla eklendi.');
                (e.target as HTMLFormElement).reset();
                await renderProductDetail(productId); // Reload detail view to show new review
            } catch (err: any) {
                alert('Hata: ' + (err.response?.data?.message || err.message));
            }
        });

    } catch (err: any) {
        DOM.productDetail.innerHTML = `
            <button class="back-btn" id="detail-back-btn">← Geri Dön</button>
            <p>Yüklenirken hata oluştu: ${err.message}</p>
        `;
        document.getElementById('detail-back-btn')?.addEventListener('click', () => switchView('products'));
    }
}
