async function send() {
    const msg = document.getElementById("msg").value;
    if (!msg) return;

    const box = document.getElementById("chatBox");
    box.innerHTML += `<div><b>Bạn:</b> ${msg}</div>`;

    const res = await fetch("/Chat/Send", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: msg })
    });

    const data = await res.json();

    box.innerHTML += `<div class="mt-2"><b>AI:</b> ${data.reply}</div>`;

    if (data.products.length > 0) {
        data.products.forEach(p => {
            box.innerHTML += `<div>👉 ${p.title} - ${p.price.toLocaleString()}đ</div>`;
        });
    }

    document.getElementById("msg").value = "";
    box.scrollTop = box.scrollHeight;
}
