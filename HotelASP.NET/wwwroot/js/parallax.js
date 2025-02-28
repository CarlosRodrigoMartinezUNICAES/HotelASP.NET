document.addEventListener("mousemove", (event) => {
    const { clientX, clientY } = event;
    const centerX = window.innerWidth / 2;
    const centerY = window.innerHeight / 2;
    const moveX = Math.max(-20, Math.min((clientX - centerX) * 0.02, 20));
    const moveY = Math.max(-0, Math.min((clientY - centerY) * 0.02, 0));

    document.querySelectorAll(".destination-card img").forEach(img => {
        img.style.transform = `translate(${moveX}px, ${moveY}px) scale(1.1)`;
    });
});
