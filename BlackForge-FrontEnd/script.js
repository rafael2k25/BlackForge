// =========================================================
// MENUS
// =========================================================

const menuItems = document.querySelectorAll(".menu-item");
const categoryButtons = document.querySelectorAll(".menu-category-button");
const sections = document.querySelectorAll(".page-section");
const pageTitle = document.getElementById("page-title");

// =========================================================
// NAVEGAÇÃO ENTRE SEÇÕES
// =========================================================

menuItems.forEach(menuItem => {
    menuItem.addEventListener("click", () => {
        // Pega o ID da seção através do data-section
        const sectionId = menuItem.dataset.section;
        // Pega o título através do data-title
        const title = menuItem.dataset.title;
        // Remove a seção ativa de todas as sections
        sections.forEach(section => {
            section.classList.remove("active-section");
        });
        // Procura a section correspondente
        const targetSection = document.getElementById(sectionId);
        // Mostra a section encontrada
        if (targetSection) {
            targetSection.classList.add("active-section");
        }
        // Atualiza o título da Topbar
        if (pageTitle && title) {
            pageTitle.textContent = title;
        }
        // Remove o active de todos os itens do menu
        menuItems.forEach(item => {
            item.classList.remove("active");
        });

        // Ativa o item clicado
        menuItem.classList.add("active");
    });
});

// =========================================================
// CATEGORIAS
// =========================================================

categoryButtons.forEach(categoryButton => {
    categoryButton.addEventListener("click", () => {
        const category = categoryButton.closest(".menu-category");
        if (!category) return;
        // Fecha as outras categorias
        document.querySelectorAll(".menu-category").forEach(otherCategory => {
            if (otherCategory !== category) {
                otherCategory.classList.remove("open");
            }
        });
        // Abre ou fecha a categoria selecionada
        category.classList.toggle("open");
    });
});

// =========================================================
// CATEGORIAS DO MENU
// =========================================================

const categories = document.querySelectorAll(".menu-category");
categories.forEach(category => {
    let closeTimer;

    // =====================================================
    // MOUSE ENTROU NA CATEGORIA
    // =====================================================

    category.addEventListener("mouseenter", () => {
        // Cancela um fechamento anterior
        clearTimeout(closeTimer);
        // Fecha as outras categorias
        categories.forEach(otherCategory => {
            if (otherCategory !== category) {
                otherCategory.classList.remove("open");
            }
        });
        // Abre a categoria atual
        category.classList.add("open");
    });

    // =====================================================
    // MOUSE SAIU DA CATEGORIA
    // =====================================================

    category.addEventListener("mouseleave", () => {
        // Aguarda 12 segundos para fechar
        closeTimer = setTimeout(() => {
            category.classList.remove("open");
        }, 1000);
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
    ) 
    {
        fecharOS();
    }
});

// =========================================================
// GRÁFICOS DASHBOARD
// =========================================================

const ctx = document.getElementById("productionChart");
const productionChart = new Chart(ctx, {
    type: "bar",
    data: {
        labels: [
            "CNC 01",
            "CNC 02",
            "CNC 03"
        ],
        datasets: [
            {
                label: "Produção",
                data: [
                    85,
                    62,
                    74
                ],
                backgroundColor: "#f5c400",
                borderWidth: 0,
                borderRadius: 2,
                barThickness: 8
            }
        ]
    },
    options: {
        indexAxis: "y",
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false
            }
        },
        scales: {
            x: {
                beginAtZero: true,
                max: 100,
                grid: {
                    color: "rgba(255, 255, 255, 0.05)"
                },
                ticks: {
                    color: "#9aa4aa",
                    font: {
                        family: "'Share Tech Mono', monospace",
                        size: 9
                    }
                }
            },
            y: {
                grid: {
                    display: false
                },
                ticks: {
                    color: "#9aa4aa",
                    font: {
                        family: "'Share Tech Mono', monospace",
                        size: 10
                    }
                }
            }
        }
    }
});

// =========================================================
// MODAL NOVA MÁQUINA
// =========================================================

const abrirModalMaquina = document.getElementById("abrirModalMaquina");
const modalMaquina = document.getElementById("modalMaquina");
const fecharModalMaquina = document.getElementById("fecharModalMaquina");
const cancelarModalMaquina = document.getElementById("cancelarModalMaquina");

// ABRIR MODAL

abrirModalMaquina.addEventListener("click", () => {
    modalMaquina.classList.add("active");
});

// FECHAR PELO X

fecharModalMaquina.addEventListener("click", () => {
    modalMaquina.classList.remove("active");
});

// FECHAR PELO CANCELAR

cancelarModalMaquina.addEventListener("click", () => {
    modalMaquina.classList.remove("active");
});

// FECHAR CLICANDO FORA DO MODAL

modalMaquina.addEventListener("click", (event) => {
    if (event.target === modalMaquina) {
        modalMaquina.classList.remove("active");
    }
});

// =========================================================
// MODAL - NOVO MATERIAL
// =========================================================

const modalNovoMaterial = document.getElementById("modalNovoMaterial");

const abrirModalMaterial = document.getElementById("abrirModalMaterial");

const cadastrarMaterialVazio = document.getElementById("cadastrarMaterialVazio");

const fecharModalMaterial = document.getElementById("fecharModalMaterial");

const cancelarMaterial = document.getElementById("cancelarMaterial");

// =========================================================
// ABRIR MODAL
// =========================================================

function abrirModalNovoMaterial() {

    modalNovoMaterial.classList.add("active");

}

// =========================================================
// FECHAR MODAL
// =========================================================

function fecharModalNovoMaterial() {

    modalNovoMaterial.classList.remove("active");

}

// =========================================================
// BOTÕES DE ABERTURA
// =========================================================

abrirModalMaterial.addEventListener(
    "click",
    abrirModalNovoMaterial
);

cadastrarMaterialVazio.addEventListener(
    "click",
    abrirModalNovoMaterial
);

// =========================================================
// BOTÕES DE FECHAMENTO
// =========================================================

fecharModalMaterial.addEventListener(
    "click",
    fecharModalNovoMaterial
);

cancelarMaterial.addEventListener(
    "click",
    fecharModalNovoMaterial
);

// =========================================================
// FECHAR CLICANDO FORA
// =========================================================

modalNovoMaterial.addEventListener("click", (event) => {

    if (event.target === modalNovoMaterial) {

        fecharModalNovoMaterial();

    }

});

// =========================================================
// MODAL NOVO LOTE
// =========================================================

const modalNovoLote = document.getElementById("modalNovoLote");
const abrirModalLote = document.getElementById("abrirModalLote");
const cadastrarLoteVazio = document.getElementById("cadastrarLoteVazio");
const fecharModalLote = document.getElementById("fecharModalLote");
const cancelarLote = document.getElementById("cancelarLote");

function abrirModalNovoLote() {
    modalNovoLote.classList.add("active");
}

function fecharModalNovoLote() {
    modalNovoLote.classList.remove("active");
}

if (abrirModalLote) {
    abrirModalLote.addEventListener("click", abrirModalNovoLote);
}

if (cadastrarLoteVazio) {
    cadastrarLoteVazio.addEventListener("click", abrirModalNovoLote);
}

if (fecharModalLote) {
    fecharModalLote.addEventListener("click", fecharModalNovoLote);
}

if (cancelarLote) {
    cancelarLote.addEventListener("click", fecharModalNovoLote);
}

if (modalNovoLote) {
    modalNovoLote.addEventListener("click", (event) => {
        if (event.target === modalNovoLote) {
            fecharModalNovoLote();
        }
    });
}