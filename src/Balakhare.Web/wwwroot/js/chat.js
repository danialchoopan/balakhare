let connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

let currentUser = null;
let currentChatId = null;
let activeTab = 'all';
let replyToId = null;

// DOM Elements
const loginOverlay = document.getElementById('login-overlay');
const loginBtn = document.getElementById('login-btn');
const usernameInput = document.getElementById('username-input');
const chatList = document.getElementById('chat-list');
const messagesContainer = document.getElementById('messages-container');
const messageInput = document.getElementById('message-input');
const sendBtn = document.getElementById('send-btn');
const chatHeader = document.getElementById('chat-header');
const inputArea = document.getElementById('input-area');
const themeToggle = document.getElementById('theme-toggle');
const sunIcon = document.getElementById('sun-icon');
const moonIcon = document.getElementById('moon-icon');
const fileBtn = document.getElementById('file-btn');
const fileInput = document.getElementById('file-input');
const pinnedBar = document.getElementById('pinned-message-bar');
const pinnedText = document.getElementById('pinned-message-text');
const replyPreview = document.getElementById('reply-preview');
const searchBar = document.getElementById('in-chat-search-bar');
const searchInput = document.getElementById('in-chat-search-input');

// Initialize Connection
async function startConnection() {
    try {
        await connection.start();
        if (currentUser) {
            await connection.invoke("UpdateUserStatus", currentUser.id, true);
        }
    } catch (err) {
        console.error(err);
        setTimeout(startConnection, 5000);
    }
}

// Login
loginBtn.addEventListener('click', async () => {
    const username = usernameInput.value.trim();
    if (username) {
        const response = await fetch('/api/account/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(username)
        });
        currentUser = await response.json();
        loginOverlay.classList.add('hidden');
        localStorage.setItem('balakhare_user', JSON.stringify(currentUser));
        loadChats();
        startConnection();
    }
});

// Load Chats
async function loadChats() {
    const response = await fetch(`/api/chat/list?userId=${currentUser.id}`);
    let chats = await response.json();
    if (activeTab === 'groups') chats = chats.filter(c => c.type === 1);
    else if (activeTab === 'channels') chats = chats.filter(c => c.type === 2);
    renderChatList(chats);
}

function renderChatList(chats) {
    chatList.innerHTML = '';
    chats.forEach(chat => {
        const div = document.createElement('div');
        div.className = 'p-4 border-b border-gray-100 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center transition-colors';
        div.innerHTML = `
            <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold ml-3">${chat.title ? chat.title[0] : 'CH'}</div>
            <div class="flex-1 overflow-hidden">
                <h3 class="font-bold text-sm dark:text-white truncate">${chat.title || 'PV'}</h3>
                <p class="text-xs text-gray-500 dark:text-gray-400 truncate">${chat.lastMessage || ''}</p>
            </div>
        `;
        div.onclick = () => selectChat(chat);
        chatList.appendChild(div);
    });
}

function switchTab(tab) {
    activeTab = tab;
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.toggle('text-blue-500', btn.dataset.tab === tab);
        btn.classList.toggle('border-b-2', btn.dataset.tab === tab);
        btn.classList.toggle('border-blue-500', btn.dataset.tab === tab);
    });
    if (tab === 'users') loadUsers(); else loadChats();
}
window.switchTab = switchTab;

async function loadUsers() {
    const response = await fetch('/api/account/users');
    const users = await response.json();
    chatList.innerHTML = '';
    users.filter(u => u.id !== currentUser.id).forEach(user => {
        const div = document.createElement('div');
        div.className = 'p-4 border-b border-gray-100 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center transition-colors';
        div.innerHTML = `
            <div class="w-12 h-12 rounded-full bg-green-500 flex items-center justify-center text-white font-bold ml-3">${user.fullName[0]}</div>
            <div class="flex-1 overflow-hidden">
                <h3 class="font-bold text-sm dark:text-white truncate">${user.fullName}</h3>
                <p class="text-xs text-gray-500 dark:text-gray-400 truncate">@${user.username}</p>
            </div>
        `;
        div.onclick = () => startPV(user);
        chatList.appendChild(div);
    });
}

async function startPV(user) {
    const response = await fetch('/api/chat/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: user.fullName, type: 0, userIds: [currentUser.id, user.id] })
    });
    const chat = await response.json();
    switchTab('all');
    selectChat(chat);
}

// Chat Selection
async function selectChat(chat) {
    if (currentChatId) await connection.invoke("LeaveChat", currentChatId);
    currentChatId = chat.id;
    chatHeader.classList.remove('hidden');
    inputArea.classList.remove('hidden');
    pinnedBar.classList.add('hidden');
    document.getElementById('header-title').innerText = chat.title || 'PV';
    await connection.invoke("JoinChat", currentChatId, currentUser.id);
    loadMessages();
}

async function loadMessages(query = null) {
    let url = `/api/chat/${currentChatId}/messages`;
    if (query) url += `?query=${encodeURIComponent(query)}`;
    const response = await fetch(url);
    const messages = await response.json();
    messagesContainer.innerHTML = '';
    messages.forEach(msg => appendMessage(msg));
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

function appendMessage(msg) {
    const isMine = msg.senderId === currentUser.id;
    const div = document.createElement('div');
    div.id = `msg-${msg.id}`;
    div.className = `message-bubble ${isMine ? 'message-sent' : 'message-received shadow-sm'} group flex flex-col relative`;

    let html = '';

    if (msg.forwardedFromUserName) {
        html += `<span class="text-[10px] text-gray-500 italic mb-1">Fwd from ${msg.forwardedFromUserName}</span>`;
    }

    if (msg.parentMessageId) {
        html += `<div class="bg-black bg-opacity-5 border-r-2 border-blue-500 p-1 mb-2 rounded text-xs cursor-pointer" onclick="scrollToMsg(${msg.parentMessageId})">
            <span class="block font-bold text-blue-500">Reply</span>
            <span class="truncate block">${msg.parentContent || 'Deleted'}</span>
        </div>`;
    }

    if (msg.filePath) {
        if (msg.filePath.match(/\.(jpg|jpeg|png|gif|webp)$/i)) {
            html += `<img src="${msg.filePath}" class="rounded-lg mb-1 max-w-full h-auto">`;
        } else {
            html += `<a href="${msg.filePath}" target="_blank" class="flex items-center p-2 bg-black bg-opacity-5 rounded text-blue-600 mb-1 text-xs">File: ${msg.fileName}</a>`;
        }
    }

    if (msg.linkPreview && msg.linkPreview.url) {
        html += `<a href="${msg.linkPreview.url}" target="_blank" class="block border-r-2 border-gray-300 pr-2 my-2 bg-gray-50 dark:bg-gray-800 p-2 rounded text-xs">
            ${msg.linkPreview.imageUrl ? `<img src="${msg.linkPreview.imageUrl}" class="w-full h-24 object-cover rounded mb-1">` : ''}
            <span class="font-bold block text-blue-500">${msg.linkPreview.title || 'Link'}</span>
            <span class="text-gray-500 dark:text-gray-400 block truncate">${msg.linkPreview.description || ''}</span>
        </a>`;
    }

    html += `<span>${msg.content || ''}</span>`;
    html += `<span class="text-[10px] opacity-60 self-end mt-1">${new Date(msg.sentAt).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })}</span>`;

    if (msg.reactions && msg.reactions.length > 0) {
        html += `<div class="flex space-x-1 space-x-reverse mt-1">`;
        msg.reactions.forEach(r => {
            html += `<span class="bg-gray-100 dark:bg-gray-700 px-1 rounded text-[10px] cursor-pointer" onclick="react(${msg.id}, '${r.key}')">${r.key} ${r.count}</span>`;
        });
        html += `</div>`;
    }

    html += `<div class="absolute top-0 -left-12 hidden group-hover:flex bg-white dark:bg-gray-800 shadow rounded border dark:border-gray-700 p-1 space-x-1 space-x-reverse z-10 text-[10px]">
        <button onclick="setReply(${msg.id}, '${msg.senderName}', '${msg.content}')" title="Reply">RP</button>
        <button onclick="react(${msg.id}, 'Like')" title="Like">LK</button>
        <button onclick="pin(${msg.id})" title="Pin">PN</button>
    </div>`;

    div.innerHTML = html;
    messagesContainer.appendChild(div);
}

function setReply(id, name, content) {
    replyToId = id;
    replyPreview.classList.remove('hidden');
    document.getElementById('reply-to-name').innerText = `Reply to ${name}`;
    document.getElementById('reply-to-content').innerText = content;
    messageInput.focus();
}

function cancelReply() {
    replyToId = null;
    replyPreview.classList.add('hidden');
}

async function react(msgId, type) {
    await connection.invoke("AddReaction", msgId, currentUser.id, type);
}

async function pin(msgId) {
    await connection.invoke("PinMessage", currentChatId, msgId);
}

function scrollToMsg(id) {
    const el = document.getElementById(`msg-${id}`);
    if (el) el.scrollIntoView({ behavior: 'smooth' });
}

// Search
document.getElementById('search-btn').onclick = () => {
    searchBar.classList.remove('hidden');
    searchInput.focus();
};
function closeSearch() {
    searchBar.classList.add('hidden');
    searchInput.value = '';
    loadMessages();
}
searchInput.onkeypress = (e) => { if (e.key === 'Enter') loadMessages(searchInput.value); };

// Send Message
async function sendMessage() {
    const content = messageInput.value.trim();
    if (content || pendingFile) {
        let filePath = pendingFile?.filePath;
        let fileName = pendingFile?.fileName;
        await connection.invoke("SendMessage", currentChatId, currentUser.id, content, filePath, fileName, replyToId);
        messageInput.value = '';
        cancelReply();
        pendingFile = null;
        fileBtn.classList.remove('text-blue-500');
    }
}
sendBtn.onclick = sendMessage;
messageInput.onkeypress = (e) => { if (e.key === 'Enter') sendMessage(); };

// SignalR Events
connection.on("ReceiveMessage", (msg) => { if (msg.chatId === currentChatId) appendMessage(msg); messagesContainer.scrollTop = messagesContainer.scrollHeight; });
connection.on("MessagePinned", (p) => { pinnedBar.classList.remove('hidden'); pinnedText.innerText = p.Content; });
connection.on("UpdateReactions", (r) => { loadMessages(); });

// Theme
themeToggle.onclick = () => {
    document.documentElement.classList.toggle('dark');
    const isDark = document.documentElement.classList.contains('dark');
    sunIcon.classList.toggle('hidden', !isDark);
    moonIcon.classList.toggle('hidden', isDark);
    localStorage.setItem('balakhare_theme', isDark ? 'dark' : 'light');
};

// File
let pendingFile = null;
fileBtn.onclick = () => fileInput.click();
fileInput.onchange = async () => {
    if (fileInput.files.length > 0) {
        const formData = new FormData(); formData.append('file', fileInput.files[0]);
        const res = await fetch('/api/chat/upload', { method: 'POST', body: formData });
        if (res.ok) { pendingFile = await res.json(); fileBtn.classList.add('text-blue-500'); }
    }
};

// Init
(function init() {
    if (localStorage.getItem('balakhare_theme') === 'dark') document.documentElement.classList.add('dark');
    const saved = localStorage.getItem('balakhare_user');
    if (saved) { currentUser = JSON.parse(saved); loginOverlay.classList.add('hidden'); loadChats(); startConnection(); }
})();
