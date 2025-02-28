document.addEventListener("mousemove", (event) => {
    const { clientX, clientY } = event;
    const centerX = window.innerWidth / 2;
    const centerY = window.innerHeight / 2;
    const moveX = Math.max(-30, Math.min((clientX - centerX) * 0.02, 30));
    const moveY = Math.max(-30, Math.min((clientY - centerY) * 0.02, 30));

    const parallaxBackground = document.querySelector(".parallax-background");

    // Apply the transformation to the parallax background
    parallaxBackground.style.transform = `translate(${moveX}px, ${moveY}px) scale(1.2)`;
});
