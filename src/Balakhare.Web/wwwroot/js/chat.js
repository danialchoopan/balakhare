let connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
let currentUser = null, currentChatId = null, activeTab = 'all', replyToId = null, typingTimeout = null, allChats = [];

// DOM Elements
const loginOverlay = document.getElementById('login-overlay'), loginBtn = document.getElementById('login-btn'), usernameInput = document.getElementById('username-input');
const chatList = document.getElementById('chat-list'), messagesContainer = document.getElementById('messages-container'), messageInput = document.getElementById('message-input'), sendBtn = document.getElementById('send-btn');
const chatHeader = document.getElementById('chat-header'), inputArea = document.getElementById('input-area'), themeToggle = document.getElementById('theme-toggle');
const pinnedBar = document.getElementById('pinned-message-bar'), pinnedText = document.getElementById('pinned-message-text'), replyPreview = document.getElementById('reply-preview'), searchBar = document.getElementById('in-chat-search-bar'), searchInput = document.getElementById('in-chat-search-input');
const profileModal = document.getElementById('profile-modal'), forwardModal = document.getElementById('forward-modal'), forwardList = document.getElementById('forward-list');
const typingIndicator = document.getElementById('typing-indicator'), sidebarSearch = document.querySelector('input[placeholder="جستجو..."]');

// Connection
async function startConnection() {
    try { await connection.start(); if (currentUser) await connection.invoke("UpdateUserStatus", currentUser.id, true); }
    catch (err) { setTimeout(startConnection, 5000); }
}

// Login
loginBtn.addEventListener('click', async () => {
    const username = usernameInput.value.trim();
    if (username) {
        const res = await fetch('/api/account/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(username) });
        currentUser = await res.json(); loginOverlay.classList.add('hidden'); localStorage.setItem('balakhare_user', JSON.stringify(currentUser));
        loadChats(); startConnection();
    }
});

// Chats
async function loadChats() {
    const res = await fetch(`/api/chat/list?userId=${currentUser.id}`); allChats = await res.json();
    filterAndRenderChats();
}

function filterAndRenderChats() {
    let chats = allChats;
    const query = sidebarSearch.value.trim().toLowerCase();
    if (query) chats = chats.filter(c => c.title?.toLowerCase().includes(query) || c.lastMessage?.toLowerCase().includes(query));
    if (activeTab === 'groups') chats = chats.filter(c => c.type === 1); else if (activeTab === 'channels') chats = chats.filter(c => c.type === 2);
    renderChatList(chats);
}

function renderChatList(chats) {
    chatList.innerHTML = '';
    chats.forEach(chat => {
        const div = document.createElement('div');
        div.className = 'p-4 border-b border-gray-100 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center transition-all';
        div.innerHTML = `
            <div class="relative ml-3">
                <div class="w-12 h-12 rounded-full bg-gradient-to-tr from-blue-500 to-blue-300 flex items-center justify-center text-white font-bold shadow-sm">${chat.title ? chat.title[0] : 'CH'}</div>
                ${chat.unreadCount > 0 ? `<div class="absolute -top-1 -right-1 bg-red-500 text-white text-[8px] font-bold w-4 h-4 rounded-full flex items-center justify-center border-2 border-white dark:border-gray-800">${chat.unreadCount}</div>` : ''}
            </div>
            <div class="flex-1 overflow-hidden">
                <div class="flex justify-between items-baseline"><h3 class="font-bold text-sm dark:text-white truncate">${chat.title || 'PV'}</h3></div>
                <p class="text-xs text-gray-400 truncate">${chat.lastMessage || ''}</p>
            </div>
        `;
        div.onclick = () => selectChat(chat); chatList.appendChild(div);
    });
}

function switchTab(tab) {
    activeTab = tab; document.querySelectorAll('.tab-btn').forEach(btn => {
        const active = btn.dataset.tab === tab; btn.classList.toggle('text-blue-500', active); btn.classList.toggle('border-b-2', active); btn.classList.toggle('border-blue-500', active);
    });
    if (tab === 'users') loadUsers(); else loadChats();
}
window.switchTab = switchTab;

async function loadUsers() {
    const res = await fetch('/api/account/users'); let users = await res.json();
    const query = sidebarSearch.value.trim().toLowerCase();
    if (query) users = users.filter(u => u.fullName.toLowerCase().includes(query) || u.username.toLowerCase().includes(query));
    chatList.innerHTML = '';
    users.filter(u => u.id !== currentUser.id).forEach(user => {
        const div = document.createElement('div'); div.className = 'p-4 border-b border-gray-100 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center';
        div.innerHTML = `
            <div class="relative ml-3">
                <div class="w-12 h-12 rounded-full bg-green-500 flex items-center justify-center text-white font-bold">${user.fullName[0]}</div>
                ${user.isOnline ? `<div class="absolute bottom-0 right-0 w-3 h-3 bg-green-400 border-2 border-white dark:border-gray-800 rounded-full"></div>` : ''}
            </div>
            <div class="flex-1 overflow-hidden">
                <h3 class="font-bold text-sm dark:text-white truncate">${user.fullName}</h3>
                <p class="text-xs text-gray-400 truncate">@${user.username}</p>
            </div>`;
        div.onclick = () => startPV(user); chatList.appendChild(div);
    });
}

async function startPV(user) {
    const res = await fetch('/api/chat/create', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ title: user.fullName, type: 0, userIds: [currentUser.id, user.id] }) });
    const chat = await res.json(); switchTab('all'); selectChat(chat);
}

// Chat UI
async function selectChat(chat) {
    if (currentChatId) await connection.invoke("LeaveChat", currentChatId);
    currentChatId = chat.id; chatHeader.classList.remove('hidden'); inputArea.classList.remove('hidden'); pinnedBar.classList.add('hidden');
    document.getElementById('header-title').innerText = chat.title || 'PV';
    document.getElementById('header-avatar').innerText = chat.title ? chat.title[0] : 'CH';
    await connection.invoke("JoinChat", currentChatId, currentUser.id); loadMessages();
}

async function loadMessages(query = null) {
    let url = `/api/chat/${currentChatId}/messages?userId=${currentUser.id}`; if (query) url += `&query=${encodeURIComponent(query)}`;
    const res = await fetch(url); const msgs = await res.json(); messagesContainer.innerHTML = ''; msgs.forEach(msg => appendMessage(msg));
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

function appendMessage(msg) {
    const isMine = msg.senderId === currentUser.id; const div = document.createElement('div'); div.id = `msg-${msg.id}`;
    div.className = `message-bubble ${isMine ? 'message-sent' : 'message-received shadow-sm'} group flex flex-col relative`;
    let html = '';
    if (msg.forwardedFromUserName) html += `<span class="text-[9px] text-blue-400 italic mb-1 font-bold">فوروارد شده از ${msg.forwardedFromUserName}</span>`;
    if (msg.parentMessageId) html += `<div class="bg-black/5 border-r-2 border-blue-500 p-1 mb-2 rounded text-[10px] cursor-pointer" onclick="scrollToMsg(${msg.parentMessageId})"><span class="block font-bold text-blue-500">پاسخ</span><span class="truncate block">${msg.parentContent || 'حذف شده'}</span></div>`;
    if (msg.filePath) {
        if (msg.filePath.match(/\.(jpg|jpeg|png|gif|webp)$/i)) html += `<img src="${msg.filePath}" class="rounded-lg mb-1 max-w-full h-auto cursor-pointer shadow-sm" onclick="window.open('${msg.filePath}')">`;
        else html += `<a href="${msg.filePath}" target="_blank" class="flex items-center p-2 bg-black/5 rounded text-blue-600 mb-1 text-[10px] font-bold underline">فایل: ${msg.fileName}</a>`;
    }
    if (msg.linkPreview && msg.linkPreview.url) {
        html += `<a href="${msg.linkPreview.url}" target="_blank" class="block border-r-2 border-gray-300 pr-2 my-2 bg-gray-100 dark:bg-gray-800 p-2 rounded text-[10px]"><span class="font-bold block text-blue-500">${msg.linkPreview.title || 'لینک'}</span><span class="text-gray-400 block truncate">${msg.linkPreview.description || ''}</span></a>`;
    }
    html += `<span class="whitespace-pre-wrap">${msg.content || ''}</span><div class="flex items-center self-end mt-1 space-x-1 space-x-reverse"><span class="text-[9px] opacity-40 font-medium">${new Date(msg.sentAt).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })}</span></div>`;
    if (msg.reactions && msg.reactions.length > 0) {
        html += `<div class="flex space-x-1 space-x-reverse mt-1">`;
        msg.reactions.forEach(r => html += `<span class="bg-black/5 dark:bg-white/5 px-1.5 py-0.5 rounded-full text-[9px] cursor-pointer font-bold transition-colors" onclick="react(${msg.id}, '${r.key}')">${r.key === 'Like' ? 'L' : 'H'} ${r.count}</span>`);
        html += `</div>`;
    }
    html += `<div class="absolute top-0 -left-20 hidden group-hover:flex bg-white dark:bg-gray-800 shadow-xl rounded-full border dark:border-gray-700 p-1 space-x-1 space-x-reverse z-10 text-[9px] font-bold">
        <button onclick="setReply(${msg.id}, '${msg.senderName}', '${msg.content}')" class="p-1 hover:text-blue-500">RP</button>
        <button onclick="openForwardModal(${msg.id}, '${msg.content}')" class="p-1 hover:text-blue-500">FW</button>
        <button onclick="react(${msg.id}, 'Like')" class="p-1 hover:text-blue-500">LK</button>
        <button onclick="pin(${msg.id})" class="p-1 hover:text-blue-500">PN</button>
        ${isMine ? `<button onclick="unsend(${msg.id})" class="p-1 hover:text-red-500 text-red-400">DEL</button>` : ''}
    </div>`;
    div.innerHTML = html; messagesContainer.appendChild(div);
}

// Features
function setReply(id, name, content) { replyToId = id; replyPreview.classList.remove('hidden'); document.getElementById('reply-to-name').innerText = `پاسخ به ${name}`; document.getElementById('reply-to-content').innerText = content; messageInput.focus(); }
function cancelReply() { replyToId = null; replyPreview.classList.add('hidden'); }
async function react(msgId, type) { await connection.invoke("AddReaction", msgId, currentUser.id, type); }
async function pin(msgId) { await connection.invoke("PinMessage", currentChatId, msgId); }
async function unsend(msgId) { if (confirm('Remove this message?')) await connection.invoke("DeleteMessage", msgId, currentUser.id); }
function scrollToMsg(id) { const el = document.getElementById(`msg-${id}`); if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' }); }

function openForwardModal(msgId, content) {
    forwardModal.classList.remove('hidden'); forwardModal.classList.add('flex');
    forwardList.innerHTML = '';
    allChats.forEach(chat => {
        const div = document.createElement('div');
        div.className = 'p-3 hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer flex items-center rounded-xl transition-all mb-1';
        div.innerHTML = `<div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold ml-3">${chat.title ? chat.title[0] : 'PV'}</div><div class="font-bold text-sm dark:text-white">${chat.title}</div>`;
        div.onclick = async () => {
            await connection.invoke("SendMessage", chat.id, currentUser.id, content, null, null, null, currentUser.id);
            closeForwardModal();
        };
        forwardList.appendChild(div);
    });
}
function closeForwardModal() { forwardModal.classList.add('hidden'); forwardModal.classList.remove('flex'); }
window.closeForwardModal = closeForwardModal;

// Profile
function showProfileModal() {
    profileModal.classList.remove('hidden'); profileModal.classList.add('flex');
    document.getElementById('profile-name-display').innerText = currentUser.fullName;
    document.getElementById('profile-username-display').innerText = `@${currentUser.username}`;
    document.getElementById('edit-fullname').value = currentUser.fullName;
    document.getElementById('edit-bio').value = currentUser.bio || '';
}
function closeProfileModal() { profileModal.classList.add('hidden'); profileModal.classList.remove('flex'); }
async function saveProfile() {
    currentUser.fullName = document.getElementById('edit-fullname').value;
    currentUser.bio = document.getElementById('edit-bio').value;
    const res = await fetch('/api/account/profile', { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(currentUser) });
    if (res.ok) { localStorage.setItem('balakhare_user', JSON.stringify(currentUser)); closeProfileModal(); }
}
window.showProfileModal = showProfileModal; window.closeProfileModal = closeProfileModal; window.saveProfile = saveProfile;

// Search & Send
sidebarSearch.oninput = () => { if (activeTab === 'users') loadUsers(); else filterAndRenderChats(); };
document.getElementById('search-btn').onclick = () => { searchBar.classList.remove('hidden'); searchInput.focus(); };
function closeSearch() { searchBar.classList.add('hidden'); searchInput.value = ''; loadMessages(); }
searchInput.onkeypress = (e) => { if (e.key === 'Enter') loadMessages(searchInput.value); };

messageInput.oninput = () => {
    if (currentChatId) {
        clearTimeout(typingTimeout);
        connection.invoke("SendTypingNotification", currentChatId, currentUser.fullName);
    }
};

async function sendMessage() {
    const content = messageInput.value.trim(); if (content || pendingFile) {
        let fPath = pendingFile?.filePath, fName = pendingFile?.fileName;
        await connection.invoke("SendMessage", currentChatId, currentUser.id, content, fPath, fName, replyToId);
        messageInput.value = ''; cancelReply(); pendingFile = null; document.getElementById('file-btn').classList.remove('text-blue-500');
    }
}
sendBtn.onclick = sendMessage; messageInput.onkeypress = (e) => { if (e.key === 'Enter') sendMessage(); };

// Events
connection.on("ReceiveMessage", (msg) => { if (msg.chatId === currentChatId) { appendMessage(msg); messagesContainer.scrollTop = messagesContainer.scrollHeight; } loadChats(); });
connection.on("MessagePinned", (p) => { pinnedBar.classList.remove('hidden'); pinnedText.innerText = p.Content; });
connection.on("UpdateReactions", (r) => { loadMessages(); });
connection.on("MessageDeleted", (id) => { const el = document.getElementById(`msg-${id}`); if (el) el.remove(); });
connection.on("UserStatusChanged", () => { if (activeTab === 'users') loadUsers(); });
connection.on("UserTyping", (data) => {
    if (data.ChatId === currentChatId && data.FullName !== currentUser.fullName) {
        typingIndicator.innerText = `${data.FullName} در حال نوشتن...`;
        typingIndicator.classList.remove('hidden');
        setTimeout(() => typingIndicator.classList.add('hidden'), 3000);
    }
});

// Theme & File
themeToggle.onclick = () => {
    document.documentElement.classList.toggle('dark'); const isDark = document.documentElement.classList.contains('dark');
    document.getElementById('sun-icon').classList.toggle('hidden', !isDark); document.getElementById('moon-icon').classList.toggle('hidden', isDark);
    localStorage.setItem('balakhare_theme', isDark ? 'dark' : 'light');
};
let pendingFile = null; document.getElementById('file-btn').onclick = () => document.getElementById('file-input').click();
document.getElementById('file-input').onchange = async () => {
    if (document.getElementById('file-input').files.length > 0) {
        const fd = new FormData(); fd.append('file', document.getElementById('file-input').files[0]);
        const res = await fetch('/api/chat/upload', { method: 'POST', body: fd });
        if (res.ok) { pendingFile = await res.json(); document.getElementById('file-btn').classList.add('text-blue-500'); }
    }
};

// Init
(function init() {
    if (localStorage.getItem('balakhare_theme') === 'dark') document.documentElement.classList.add('dark');
    const saved = localStorage.getItem('balakhare_user');
    if (saved) { currentUser = JSON.parse(saved); loginOverlay.classList.add('hidden'); loadChats(); startConnection(); }
})();
