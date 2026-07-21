import axios from 'axios';

const API_BASE_URL = 'http://localhost:5140/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});

export const api = {
    // Products
    getProducts: async (searchText: string = '') => {
        const response = await apiClient.get(`/products${searchText ? `?searchText=${searchText}` : ''}`);
        return response.data;
    },
    createProduct: async (product: any) => {
        const response = await apiClient.post('/products', product);
        return response.data;
    },
    updateProduct: async (id: string, product: any) => {
        const response = await apiClient.put(`/products/${id}`, product);
        return response.data;
    },
    deleteProduct: async (id: string) => {
        const response = await apiClient.delete(`/products/${id}`);
        return response.data;
    },

    // Warehouse Inventory
    getWarehouseInventories: async (productId?: string) => {
        const url = productId ? `/warehouse-inventories?productId=${productId}` : '/warehouse-inventories';
        const response = await apiClient.get(url);
        return response.data;
    },

    // Reviews
    getReviews: async (productId?: string) => {
        const url = productId ? `/product-reviews?productId=${productId}` : '/product-reviews';
        const response = await apiClient.get(url);
        return response.data;
    },
    createReview: async (review: any) => {
        // Bulk API endpoint expects a list
        const response = await apiClient.post('/product-reviews', [review]);
        return response.data;
    },

    // Brands & Categories
    getBrands: async () => {
        const response = await apiClient.get('/brands');
        return response.data;
    },
    getCategories: async () => {
        const response = await apiClient.get('/categories');
        return response.data;
    },

    // AI Advice
    askAi: async (query: string) => {
        const response = await apiClient.post('/ai/advice', { query });
        return response.data;
    }
};
