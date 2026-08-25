// MENUS
const menuItems = document.querySelectorAll(".menu-item");
const sections = document.querySelectorAll(".page-section");
const pageTitle = document.getElementById("page-title");

menuItems.forEach(item => {

    item.addEventListener("click", () => {

        const sectionId = item.dataset.section;

        menuItems.forEach(menu => {
            menu.classList.remove("active");
        });

        item.classList.add("active");

        sections.forEach(section => {
            section.classList.remove("active-section");
        });

        const section = document.getElementById(sectionId);

        if (section) {
            section.classList.add("active-section");
        }

        pageTitle.textContent = item.dataset.title;
    });

});

// DATA E HORA
function atualizarDataHora() {
    const agora = new Date();

    const data = agora.toLocaleDateString('pt-BR');
    const hora = agora.toLocaleTimeString('pt-BR', {
    hour: '2-digit',
    minute: '2-digit'
    });

    document.getElementById('dataHora').textContent =
        ` ${data} - ${hora}`;
}

atualizarDataHora();
setInterval(atualizarDataHora, 1000);