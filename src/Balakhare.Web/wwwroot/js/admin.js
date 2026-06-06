async function loadAdminData() {
    try {
        const statsRes = await fetch('/api/admin/stats');
        const stats = await statsRes.json();

        const statsHtml = `
            <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-100 flex items-center">
                <div class="w-12 h-12 bg-blue-100 rounded-xl flex items-center justify-center text-blue-600 ml-4">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path></svg>
                </div>
                <div>
                    <p class="text-xs text-slate-400 font-bold mb-1">کل کاربران</p>
                    <p class="text-2xl font-bold">${stats.userCount}</p>
                </div>
            </div>
            <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-100 flex items-center">
                <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center text-green-600 ml-4">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path></svg>
                </div>
                <div>
                    <p class="text-xs text-slate-400 font-bold mb-1">گفتگوها</p>
                    <p class="text-2xl font-bold">${stats.chatCount}</p>
                </div>
            </div>
            <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-100 flex items-center">
                <div class="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center text-purple-600 ml-4">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"></path></svg>
                </div>
                <div>
                    <p class="text-xs text-slate-400 font-bold mb-1">پیام‌های ارسال شده</p>
                    <p class="text-2xl font-bold">${stats.messageCount}</p>
                </div>
            </div>
            <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-100 flex items-center">
                <div class="w-12 h-12 bg-rose-100 rounded-xl flex items-center justify-center text-rose-600 ml-4">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"></path></svg>
                </div>
                <div>
                    <p class="text-xs text-slate-400 font-bold mb-1">تعاملات (Reactions)</p>
                    <p class="text-2xl font-bold">${stats.reactionCount}</p>
                </div>
            </div>
        `;
        document.getElementById('stats-container').innerHTML = statsHtml;

        const usersRes = await fetch('/api/admin/users');
        const users = await usersRes.json();
        const table = document.getElementById('users-table');
        table.innerHTML = '';
        users.forEach(u => {
            const date = new Date(u.createdAt).toLocaleDateString('fa-IR');
            const tr = document.createElement('tr');
            tr.className = 'border-b border-slate-50 hover:bg-slate-50 transition-colors';
            tr.innerHTML = `
                <td class="p-4 flex items-center mr-2">
                    <div class="w-10 h-10 rounded-full bg-slate-200 ml-3 flex items-center justify-center text-slate-500 font-bold text-xs">${u.fullName[0]}</div>
                    <div class="text-right">
                        <p class="font-bold">${u.fullName}</p>
                        <p class="text-[10px] text-slate-400">${u.isAdmin ? 'مدیر سیستم' : 'کاربر عادی'}</p>
                    </div>
                </td>
                <td class="p-4 text-slate-500 font-mono text-xs">@${u.username}</td>
                <td class="p-4 text-slate-500 text-xs">${date}</td>
                <td class="p-4 text-center">
                    <span class="px-3 py-1 rounded-full text-[10px] font-bold ${u.isBlocked ? 'bg-rose-100 text-rose-600' : 'bg-green-100 text-green-600'}">
                        ${u.isBlocked ? 'مسدود شده' : 'فعال'}
                    </span>
                </td>
                <td class="p-4 text-center">
                    <button onclick="toggleBlock(${u.id})" class="text-blue-500 hover:text-blue-700 font-bold text-xs transition-colors">
                        ${u.isBlocked ? 'رفع محدودیت' : 'مسدودسازی'}
                    </button>
                </td>
            `;
            table.appendChild(tr);
        });

        document.getElementById('last-update').innerText = `آخرین بروزرسانی: ${new Date().toLocaleTimeString('fa-IR')}`;
    } catch (e) {
        console.error("Failed to load admin data", e);
    }
}

async function toggleBlock(id) {
    if (confirm('آیا از تغییر وضعیت این کاربر اطمینان دارید؟')) {
        await fetch(`/api/admin/users/${id}/block`, { method: 'POST' });
        loadAdminData();
    }
}

loadAdminData();
setInterval(loadAdminData, 60000);
