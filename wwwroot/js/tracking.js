// ========================
// Leaflet Map Configuration
// ========================
let map = null;
let markers = {};
let shipmentsData = [];

// Ініціалізація карти
function initMap() {
    map = L.map('map').setView([50.4501, 30.5234], 6); // Україна

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 18
    }).addTo(map);

    loadShipments();
    
    // Оновлення кожні 5 секунд
    setInterval(loadShipments, 5000);
}

// Завантажити посилки із backend
async function loadShipments() {
    const regionFilter = document.getElementById('regionFilter').value;
    const url = regionFilter 
        ? `/api/shipments/region/${regionFilter}`
        : '/api/gateway/sharding-info';

    try {
        // Для демо - будемо генерувати фіксовані дані
        shipmentsData = generateMockShipments();
        displayShipments();
        updateStatistics();
    } catch (error) {
        console.error('Помилка завантаження даних:', error);
    }
}

// Mock-дані для демонстрації (поки API не готовий)
function generateMockShipments() {
    const cities = [
        { name: 'Київ', lat: 50.4501, lon: 30.5234, region: 1 },
        { name: 'Харків', lat: 50.0038, lon: 36.2305, region: 1 },
        { name: 'Львів', lat: 49.8397, lon: 24.0297, region: 2 },
        { name: 'Одеса', lat: 46.4856, lon: 30.7326, region: 2 },
        { name: 'Запоріжжя', lat: 47.8388, lon: 35.1392, region: 3 }
    ];

    const statuses = ['Stored', 'InTransit', 'Delivered'];
    const shipments = [];

    for (let i = 0; i < 15; i++) {
        const city = cities[Math.floor(Math.random() * cities.length)];
        shipments.push({
            id: 'UA-' + Math.random().toString(36).substr(2, 8),
            trackNumber: 'TRK' + i,
            regionId: city.region,
            status: statuses[Math.floor(Math.random() * statuses.length)],
            latitude: city.lat + (Math.random() - 0.5) * 0.5,
            longitude: city.lon + (Math.random() - 0.5) * 0.5,
            createdAt: new Date()
        });
    }

    return shipments;
}

// Відобразити посилки на карті
function displayShipments() {
    // Очистити старі маркери
    Object.keys(markers).forEach(key => {
        map.removeLayer(markers[key]);
    });
    markers = {};

    // Фільтри
    const statusFilters = {
        'Stored': document.getElementById('statusStored')?.checked,
        'InTransit': document.getElementById('statusInTransit')?.checked,
        'Delivered': document.getElementById('statusDelivered')?.checked
    };

    shipmentsData.forEach(shipment => {
        // Застосувати фільтр статусу
        if (!statusFilters[shipment.status]) return;

        const color = getStatusColor(shipment.status);
        const icon = getStatusIcon(shipment.status);

        const marker = L.circleMarker(
            [shipment.latitude, shipment.longitude],
            {
                radius: 8,
                fillColor: color,
                color: '#333',
                weight: 2,
                opacity: 1,
                fillOpacity: 0.8
            }
        ).addTo(map);

        // Popup з інформацією
        const popupContent = `
            <div>
                <strong>${icon} ${shipment.trackNumber}</strong><br>
                <small>Статус: ${translateStatus(shipment.status)}</small><br>
                <small>Регіон: ${shipment.regionId}</small><br>
                <small>Час: ${new Date(shipment.createdAt).toLocaleString('uk-UA')}</small>
            </div>
        `;

        marker.bindPopup(popupContent);
        markers[shipment.id] = marker;
    });

    // Автоматично змінювати масштаб на всі маркери
    if (Object.keys(markers).length > 0) {
        const group = new L.featureGroup(Object.values(markers));
        map.fitBounds(group.getBounds().pad(0.1));
    }
}

// Отримати колір за статусом
function getStatusColor(status) {
    switch(status) {
        case 'Stored': return '#FFD700';    // Золотий - на складі
        case 'InTransit': return '#FF6347'; // Помаранчевий - у дорозі
        case 'Delivered': return '#00AA00'; // Зелений - доставлено
        default: return '#808080';
    }
}

// Отримати емодзі за статусом
function getStatusIcon(status) {
    switch(status) {
        case 'Stored': return '📦';
        case 'InTransit': return '🚚';
        case 'Delivered': return '✅';
        default: return '❓';
    }
}

// Перекладакномі статуси на українську
function translateStatus(status) {
    switch(status) {
        case 'Stored': return 'На складі';
        case 'InTransit': return 'У дорозі';
        case 'Delivered': return 'Доставлено';
        default: return status;
    }
}

// Оновити статистику
function updateStatistics() {
    const total = shipmentsData.length;
    const inTransit = shipmentsData.filter(s => s.status === 'InTransit').length;
    const delivered = shipmentsData.filter(s => s.status === 'Delivered').length;

    document.getElementById('totalShipments').textContent = total;
    document.getElementById('inTransitCount').textContent = inTransit;
    document.getElementById('deliveredCount').textContent = delivered;
}

// Event Listeners
document.addEventListener('DOMContentLoaded', () => {
    initMap();

    document.getElementById('regionFilter').addEventListener('change', loadShipments);
    document.getElementById('statusStored').addEventListener('change', displayShipments);
    document.getElementById('statusInTransit').addEventListener('change', displayShipments);
    document.getElementById('statusDelivered').addEventListener('change', displayShipments);
    document.getElementById('refreshBtn').addEventListener('click', loadShipments);
});
