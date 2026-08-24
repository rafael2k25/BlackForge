const menuItems = document.querySelectorAll(".menu-item");
const sections = document.querySelectorAll(".page-section");
const pageTitle = document.getElementById("page-title");

menuItems.forEach(item => {

    item.addEventListener("click", () => {

        const sectionId = item.dataset.section;

        // Remove ativo dos menus
        menuItems.forEach(menu => {
            menu.classList.remove("active");
        });

        // Ativa o menu clicado
        item.classList.add("active");

        // Esconde todas as sections
        sections.forEach(section => {
            section.classList.remove("active-section");
        });

        // Mostra a section escolhida
        const section = document.getElementById(sectionId);

        if (section) {
            section.classList.add("active-section");
        }

        // Atualiza título
        pageTitle.textContent = item.textContent.trim();

    });

});