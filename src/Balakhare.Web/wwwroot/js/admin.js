async function loadAdminData() {
    const statsRes = await fetch('/api/admin/stats');
    const stats = await statsRes.json();

    document.getElementById('stats-container').innerHTML = `
        <div class="bg-white p-4 rounded shadow text-center">
            <div class="text-2xl font-bold">${stats.userCount}</div>
            <div class="text-gray-500">کاربران</div>
        </div>
        <div class="bg-white p-4 rounded shadow text-center">
            <div class="text-2xl font-bold">${stats.chatCount}</div>
            <div class="text-gray-500">گفتگوها</div>
        </div>
        <div class="bg-white p-4 rounded shadow text-center">
            <div class="text-2xl font-bold">${stats.messageCount}</div>
            <div class="text-gray-500">پیام‌ها</div>
        </div>
        <div class="bg-white p-4 rounded shadow text-center">
            <div class="text-2xl font-bold">${stats.reactionCount}</div>
            <div class="text-gray-500">واکنش‌ها</div>
        </div>
    `;

    const usersRes = await fetch('/api/admin/users');
    const users = await usersRes.json();
    const table = document.getElementById('users-table');
    table.innerHTML = '';
    users.forEach(u => {
        const tr = document.createElement('tr');
        tr.className = 'border-b';
        tr.innerHTML = `
            <td class="p-2">${u.id}</td>
            <td class="p-2">${u.fullName}</td>
            <td class="p-2">@${u.username}</td>
            <td class="p-2">${u.isBlocked ? '<span class="text-red-500">مسدود</span>' : '<span class="text-green-500">فعال</span>'}</td>
            <td class="p-2">
                <button onclick="toggleBlock(${u.id})" class="bg-yellow-500 text-white px-2 py-1 rounded text-xs">${u.isBlocked ? 'رفع مسدودیت' : 'مسدود کردن'}</button>
            </td>
        `;
        table.appendChild(tr);
    });
}

async function toggleBlock(id) {
    await fetch(`/api/admin/users/${id}/block`, { method: 'POST' });
    loadAdminData();
}

// Initial Load
loadAdminData();
