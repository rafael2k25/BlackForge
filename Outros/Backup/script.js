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

// MODAL ORDEM DE SERVIÇO

// =========================================================
// MODAL - ORDEM DE SERVIÇO
// =========================================================

const modalOS = document.getElementById("modalOS");

const novaOS = document.getElementById("novaOS");
const criarOS = document.getElementById("criarOS");

const fecharModalOS = document.getElementById("fecharModalOS");
const cancelarOS = document.getElementById("cancelarOS");


// =========================================================
// ABRIR MODAL
// =========================================================

function abrirModalOS() {

    modalOS.classList.add("active");

    document.body.style.overflow = "hidden";

}


// =========================================================
// FECHAR MODAL
// =========================================================

function fecharOS() {

    modalOS.classList.remove("active");

    document.body.style.overflow = "";

}


// =========================================================
// BOTÕES
// =========================================================

novaOS.addEventListener("click", abrirModalOS);

criarOS.addEventListener("click", abrirModalOS);

fecharModalOS.addEventListener("click", fecharOS);

cancelarOS.addEventListener("click", fecharOS);


// =========================================================
// FECHAR CLICANDO FORA DO MODAL
// =========================================================

modalOS.addEventListener("click", function (event) {

    if (event.target === modalOS) {

        fecharOS();

    }

});


// =========================================================
// FECHAR COM ESC
// =========================================================

document.addEventListener("keydown", function (event) {

    if (
        event.key === "Escape" &&
        modalOS.classList.contains("active")
    ) {

        fecharOS();

    }

});