document.addEventListener('DOMContentLoaded', function () {
    // 1. Validación del formulario de contacto
    document.querySelector('.contact-form form').addEventListener('submit', function (event) {
        const name = document.querySelector('input[type="text"]').value;
        const email = document.querySelector('input[type="email"]').value;
        const message = document.querySelector('textarea').value;

        if (!name || !email || !message) {
            alert("Por favor, completa todos los campos antes de enviar.");
            event.preventDefault(); // Prevenir el envío del formulario si los campos no están completos
        } else {
            alert("¡Gracias por tu mensaje! Nos pondremos en contacto contigo pronto.");
        }
    });

    // 2. Mostrar mensaje de éxito al enviar el formulario
    document.querySelector('.contact-form form').addEventListener('submit', function (event) {
        event.preventDefault(); // Prevenir el envío real para demostración

        // Aquí podrías enviar el formulario a un servidor

        // Mostrar mensaje de éxito
        alert("¡Tu mensaje ha sido enviado con éxito!");
        document.querySelector('.contact-form').reset(); // Limpiar el formulario
    });

    // 3. Animación al hacer clic en los enlaces de navegación
    document.querySelectorAll('a').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault(); // Prevenir comportamiento por defecto del enlace

            const targetId = this.getAttribute('href').substring(1); // Obtener el id del destino
            const targetElement = document.getElementById(targetId);

            window.scrollTo({
                top: targetElement.offsetTop,
                behavior: 'smooth' // Desplazamiento suave
            });
        });
    });

    // 4. Cambiar color del botón al hacer hover
    const button = document.querySelector('.contact-form button');

    button.addEventListener('mouseover', function () {
        button.style.backgroundColor = "#005bb5"; // Cambiar color al pasar el mouse
    });

    button.addEventListener('mouseout', function () {
        button.style.backgroundColor = "#003366"; // Restaurar el color original
    });

    // 5. Mostrar u ocultar detalles de contacto al hacer clic
    const toggleButton = document.createElement('button');
    toggleButton.textContent = 'Mostrar/Ocultar Detalles de Contacto';
    document.querySelector('.contact-info').appendChild(toggleButton);

    toggleButton.addEventListener('click', function () {
        const contactInfo = document.querySelector('.contact-info ul');
        contactInfo.style.display = contactInfo.style.display === 'none' ? 'block' : 'none';
    });
});
