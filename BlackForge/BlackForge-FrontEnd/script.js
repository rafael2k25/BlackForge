// =========================================================
// CONFIGURAÇÃO DA API
// =========================================================

const API_URL = "https://localhost:7089/api";

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
    category.addEventListener("mouseenter", () => {
        clearTimeout(closeTimer);
        categories.forEach(otherCategory => {
            if (otherCategory !== category) {
                otherCategory.classList.remove("open");
            }
        });
        category.classList.add("open");
    });
    category.addEventListener("mouseleave", () => {
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

// ORDEM DE SERVIÇO

const modalOS = document.getElementById("modalOS");
const novaOS = document.getElementById("novaOS");
const criarOS = document.getElementById("criarOS");
const fecharModalOS = document.getElementById("fecharModalOS");
const cancelarOS = document.getElementById("cancelarOS");

function abrirModalOS() {
    modalOS.classList.add("active");
    document.body.style.overflow = "hidden";
}
function fecharOS() {
    modalOS.classList.remove("active");
    document.body.style.overflow = "";
}
novaOS.addEventListener("click", abrirModalOS);
criarOS.addEventListener("click", abrirModalOS);
fecharModalOS.addEventListener("click", fecharOS);
cancelarOS.addEventListener("click", fecharOS);
modalOS.addEventListener("click", function (event) {
    if (event.target === modalOS) {
        fecharOS();
    }
});
document.addEventListener("keydown", function (event) {
    if (
        event.key === "Escape" &&
        modalOS.classList.contains("active")
    ) {
        fecharOS();
    }
});

// ORDEM DE SERVIÇO

const salvarOS = document.getElementById("salvarOS");
const salvarImprimirOS = document.getElementById("salvarImprimirOS");

async function cadastrarOrdemServico() {
    const ordem = {
        numeroOS:
            document.getElementById("numeroOS").value.trim(),
        cliente:
            document.getElementById("clienteOS").value.trim(),
        contato:
            document.getElementById("contatoOS").value.trim(),
        endereco:
            document.getElementById("enderecoOS").value.trim(),
        dataAbertura:
            document.getElementById("dataOS").value,
        descricaoServico:
            document.getElementById("descricaoOS").value.trim(),
        tipoServico:
            document.getElementById("tipoServico").value,
        dataInicio:
            document.getElementById("dataInicioOS").value,
        dataEntrega:
            document.getElementById("dataEntregaOS").value,
        funcionarioId:
            null,
        valorMaoObra:
            Number(document.getElementById("valorMaoObra").value) || 0,
        desconto:
            Number(document.getElementById("descontoOS").value) || 0,
        condicaoPagamento:
            document.getElementById("condicaoPagamento").value,
        observacoes:
            document.getElementById("observacoesOS").value.trim(),
        materiais: []
    };
    if (!ordem.numeroOS) {
        alert("Informe o número da OS.");
        return;
    }
    if (!ordem.cliente) {
        alert("Informe o cliente.");
        return;
    }
    if (!ordem.descricaoServico) {
        alert("Informe a descrição do serviço.");
        return;
    }
    if (!ordem.tipoServico) {
        alert("Selecione o tipo de serviço.");
        return;
    }
    try {
        const resposta = await fetch(
            `${API_URL}/OrdensServico`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(ordem)
            }
        );
        if (!resposta.ok) {
            const mensagem = await resposta.text();
            throw new Error(
                mensagem ||
                `Erro HTTP: ${resposta.status}`
            );
        }
        const ordemCriada = await resposta.json();
        console.log(
            "Ordem de serviço criada:",
            ordemCriada
        );
        alert(
            `Ordem de serviço ${ordemCriada.numeroOS} criada com sucesso!`
        );
        fecharOS();
        limparFormularioOS();
    }
    catch (erro) {
        console.error(
            "Erro ao cadastrar ordem de serviço:",
            erro
        );
        alert(
            `Não foi possível cadastrar a ordem de serviço.\n\n${erro.message}`
        );
    }
}

// LIMPAR FORMULÁRIO

function limparFormularioOS() {
    document.getElementById("clienteOS").value = "";
    document.getElementById("contatoOS").value = "";
    document.getElementById("enderecoOS").value = "";
    document.getElementById("numeroOS").value = "";
    document.getElementById("dataOS").value = "";
    document.getElementById("descricaoOS").value = "";
    document.getElementById("tipoServico").value = "";
    document.getElementById("dataInicioOS").value = "";
    document.getElementById("dataEntregaOS").value = "";
    document.getElementById("responsavelOS").value = "";
    document.getElementById("valorMaoObra").value = "0.00";
    document.getElementById("descontoOS").value = "0.00";
    document.getElementById("condicaoPagamento").value = "";
    document.getElementById("observacoesOS").value = "";
    document.getElementById("valorMateriais").value = "0.00";
    document.getElementById("valorTotalOS").textContent = "R$ 0,00";
    const listaMateriais =
        document.getElementById("listaMateriais");
    if (listaMateriais) {
        listaMateriais.innerHTML = `
            <tr class="os-table-empty">
                <td colspan="6">Nenhum material adicionado à ordem.</td>
            </tr>
        `;
    }
}
if (salvarOS) {
    salvarOS.addEventListener(
        "click",
        cadastrarOrdemServico
    );
}

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

// NOVA MÁQUINA

const abrirModalMaquina = document.getElementById("abrirModalMaquina");
const modalMaquina = document.getElementById("modalMaquina");
const fecharModalMaquina = document.getElementById("fecharModalMaquina");
const cancelarModalMaquina = document.getElementById("cancelarModalMaquina");
abrirModalMaquina.addEventListener("click", () => {
    modalMaquina.classList.add("active");
});
fecharModalMaquina.addEventListener("click", () => {
    modalMaquina.classList.remove("active");
});
cancelarModalMaquina.addEventListener("click", () => {
    modalMaquina.classList.remove("active");
});
modalMaquina.addEventListener("click", (event) => {
    if (event.target === modalMaquina) {
        modalMaquina.classList.remove("active");
    }
});

// NOVO MATERIAL

const modalNovoMaterial = document.getElementById("modalNovoMaterial");
const abrirModalMaterial = document.getElementById("abrirModalMaterial");
const cadastrarMaterialVazio = document.getElementById("cadastrarMaterialVazio");
const fecharModalMaterial = document.getElementById("fecharModalMaterial");
const cancelarMaterial = document.getElementById("cancelarMaterial");

function abrirModalNovoMaterial() {
    modalNovoMaterial.classList.add("active");
}
function fecharModalNovoMaterial() {
    modalNovoMaterial.classList.remove("active");
}
abrirModalMaterial.addEventListener(
    "click",
    abrirModalNovoMaterial
);
cadastrarMaterialVazio.addEventListener(
    "click",
    abrirModalNovoMaterial
);
fecharModalMaterial.addEventListener(
    "click",
    fecharModalNovoMaterial
);
cancelarMaterial.addEventListener(
    "click",
    fecharModalNovoMaterial
);
modalNovoMaterial.addEventListener("click", (event) => {

    if (event.target === modalNovoMaterial) {

        fecharModalNovoMaterial();

    }

});

// NOVO LOTE

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

// FUNCIONÁRIOS

let funcionarios = [];
let funcionarioSelecionado = null;
let modoEdicaoFuncionario = false;

async function carregarFuncionarios() {
    try {
        const resposta = await fetch(
            `${API_URL}/funcionarios`
        );
        if (!resposta.ok) {
            throw new Error(
                `Erro HTTP: ${resposta.status}`
            );
        }
        funcionarios = await resposta.json();
        console.log(
            "Funcionários carregados:",
            funcionarios
        );
        const funcionariosCount =
            document.getElementById("funcionariosCount");
        if (funcionariosCount) {
            funcionariosCount.textContent =
                `${funcionarios.length} FUNCIONÁRIOS`;
        }
        renderizarFuncionarios();
    } catch (erro) {
        console.error(
            "Erro ao carregar funcionários:",
            erro
        );
    }
}

// CADASTRAR FUNCIONÁRIO

async function cadastrarNovoFuncionario() {
    const funcionario = {
        nome:
            document.getElementById("funcionarioNome").value.trim(),
        matricula:
            document.getElementById("funcionarioMatricula").value.trim(),
        cpf:
            document.getElementById("funcionarioCpf").value.trim(),
        cargo:
            document.getElementById("funcionarioCargo").value.trim(),
        idade:
            Number(document.getElementById("funcionarioIdade").value),
        telefone:
            document.getElementById("funcionarioTelefone").value.trim(),
        setor:
            document.getElementById("funcionarioSetor").value,
        admissao:
            document.getElementById("funcionarioAdmissao").value,
        email:
            document.getElementById("funcionarioEmail").value.trim(),
        observacoes:
            document.getElementById("funcionarioObservacoes").value.trim()
    };

    // VALIDAÇÕES

    if (!funcionario.nome) {
        alert("Informe o nome do funcionário.");
        return;
    }
    if (!funcionario.matricula) {
        alert("Informe a matrícula do funcionário.");
        return;
    }
    if (!funcionario.cpf) {
        alert("Informe o CPF do funcionário.");
        return;
    }
    if (!funcionario.cargo) {
        alert("Informe o cargo do funcionário.");
        return;
    }
    if (!funcionario.idade) {
        alert("Informe a idade do funcionário.");
        return;
    }
    if (!funcionario.setor) {
        alert("Selecione o setor do funcionário.");
        return;
    }
    if (!funcionario.admissao) {
        alert("Informe a data de admissão.");
        return;
    }
    try {
        let resposta;
        if (modoEdicaoFuncionario) {

            funcionario.id = funcionarioSelecionado.id;

            resposta = await fetch(
                `${API_URL}/funcionarios/${funcionario.id}`,
                {
                    method: "PUT",

                    headers: {
                        "Content-Type": "application/json"
                    },

                    body: JSON.stringify(funcionario)
                }
            );
        }
        else {
            resposta = await fetch(
                `${API_URL}/funcionarios`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(funcionario)
                }
            );
        }
        if (!resposta.ok) {
            const mensagem = await resposta.text();
            throw new Error(
                mensagem ||
                `Erro HTTP: ${resposta.status}`
            );
        }
        await carregarFuncionarios();
        fecharCadastroFuncionario();
        limparFormularioFuncionario();
        if (modoEdicaoFuncionario) {
            alert(
                "Funcionário atualizado com sucesso!"
            );
        } else {
            alert(
                "Funcionário cadastrado com sucesso!"
            );
        }
    }
    catch (erro) {
        console.error(
            "Erro ao salvar funcionário:",
            erro
        );
        alert(
            `Não foi possível salvar o funcionário.\n\n${erro.message}`
        );
    }
}
function limparFormularioFuncionario() {
    document.getElementById("funcionarioNome").value = "";
    document.getElementById("funcionarioMatricula").value = "";
    document.getElementById("funcionarioCpf").value = "";
    document.getElementById("funcionarioCargo").value = "";
    document.getElementById("funcionarioIdade").value = "";
    document.getElementById("funcionarioSetor").value = "";
    document.getElementById("funcionarioAdmissao").value = "";
    document.getElementById("funcionarioTelefone").value = "";
    document.getElementById("funcionarioEmail").value = "";
    document.getElementById("funcionarioObservacoes").value = "";
}

// ELEMENTOS

const cadastrarFuncionario =
    document.getElementById("cadastrarFuncionario");
const modalNovoFuncionario =
    document.getElementById("modalNovoFuncionario");
const modalDetalhesFuncionario =
    document.getElementById("modalDetalhesFuncionario");
const abrirModalFuncionario =
    document.getElementById("abrirModalFuncionario");
const cadastrarPrimeiroFuncionario =
    document.getElementById("cadastrarPrimeiroFuncionario");
const fecharModalFuncionario =
    document.getElementById("fecharModalFuncionario");
const cancelarFuncionario =
    document.getElementById("cancelarFuncionario");
const fecharDetalhesFuncionario =
    document.getElementById("fecharDetalhesFuncionario");
const fecharDetalhesFuncionarioBotao =
    document.getElementById("fecharDetalhesFuncionarioBotao");
const editarFuncionario =
    document.getElementById("editarFuncionario");
const removerFuncionario =
    document.getElementById("removerFuncionario");
if (cadastrarFuncionario) {
    cadastrarFuncionario.addEventListener(
        "click",
        cadastrarNovoFuncionario
    );
}
if (editarFuncionario) {

    editarFuncionario.addEventListener(
        "click",
        editarFuncionarioSelecionado
    );
}
if (removerFuncionario) {
    removerFuncionario.addEventListener(
        "click",
        removerFuncionarioSelecionado
    );
}
function abrirCadastroFuncionario() {
    if (!modalNovoFuncionario) {
        return;
    }
    modoEdicaoFuncionario = false;
    funcionarioSelecionado = null;
    document.getElementById("cadastrarFuncionario").textContent = "CADASTRAR FUNCIONÁRIO";
    modalNovoFuncionario.classList.add("active");
}
function fecharCadastroFuncionario() {

    if (!modalNovoFuncionario) {
        return;
    }
    modalNovoFuncionario.classList.remove("active");
}
if (abrirModalFuncionario) {
    abrirModalFuncionario.addEventListener(
        "click",
        abrirCadastroFuncionario
    );
}
if (cadastrarPrimeiroFuncionario) {
    cadastrarPrimeiroFuncionario.addEventListener(
        "click",
        abrirCadastroFuncionario
    );
}
if (fecharModalFuncionario) {
    fecharModalFuncionario.addEventListener(
        "click",
        fecharCadastroFuncionario
    );
}
if (cancelarFuncionario) {
    cancelarFuncionario.addEventListener(
        "click",
        fecharCadastroFuncionario
    );
}
if (modalNovoFuncionario) {
    modalNovoFuncionario.addEventListener(
        "click",
        function (event) {
            if (
                event.target === modalNovoFuncionario
            ) {
                fecharCadastroFuncionario();
            }
        }
    );
}
function fecharModalDetalhesFuncionario() {
    if (!modalDetalhesFuncionario) {
        return;
    }
    modalDetalhesFuncionario.classList.remove("active");
}
if (fecharDetalhesFuncionario) {
    fecharDetalhesFuncionario.addEventListener(
        "click",
        fecharModalDetalhesFuncionario
    );
}
if (fecharDetalhesFuncionarioBotao) {
    fecharDetalhesFuncionarioBotao.addEventListener(
        "click",
        fecharModalDetalhesFuncionario
    );
}
if (modalDetalhesFuncionario) {
    modalDetalhesFuncionario.addEventListener(
        "click",
        function (event) {
            if (
                event.target === modalDetalhesFuncionario
            ) {
                fecharModalDetalhesFuncionario();
            }
        }
    );
}
// ESC

document.addEventListener(
    "keydown",
    function (event) {
        if (event.key !== "Escape") {
            return;
        }
        fecharCadastroFuncionario();
        fecharModalDetalhesFuncionario();
    }
);

// RENDERIZAR FUNCIONÁRIOS

function renderizarFuncionarios() {
    const grid =
        document.getElementById("funcionariosGrid");
    const empty =
        document.getElementById("funcionariosEmpty");
    if (!grid || !empty) {
        return;
    }
    grid.innerHTML = "";
    if (funcionarios.length === 0) {
        empty.style.display = "flex";
        grid.style.display = "none";
        return;
    }
    empty.style.display = "none";
    grid.style.display = "grid";
    funcionarios.forEach(
        function (funcionario, index) {
            const card =
                document.createElement("div");
            card.className =
                "funcionario-card";
            const iniciais =
                funcionario.nome
                    .split(" ")
                    .map(
                        nome => nome.charAt(0)
                    )
                    .slice(0, 2)
                    .join("")
                    .toUpperCase();
            card.innerHTML = `
                <div class="funcionario-avatar">
                    ${iniciais}
                </div>
                <h3>
                    ${funcionario.nome}
                </h3>
                <span class="funcionario-card-cargo">
                    ${funcionario.cargo}
                </span>
                <div class="funcionario-card-info">
                    <div>
                        <span>MATRÍCULA:</span>
                        <strong>${funcionario.matricula}</strong>
                    </div>
                    <div>
                        <span>SETOR:</span>
                        <strong>${funcionario.setor}</strong>
                    </div>
                </div>
                <button
                    class="funcionario-card-button"
                    type="button"
                    data-funcionario-index="${index}"
                >
                    VER DETALHES →
                </button>
            `;
            grid.appendChild(card);
        }
    );
    adicionarEventosDetalhes();
}
async function removerFuncionarioSelecionado() {
    if (!funcionarioSelecionado) {
        return;
    }
    const confirmar = confirm(
        `Deseja realmente remover o funcionário "${funcionarioSelecionado.nome}"?`
    );
    if (!confirmar) {
        return;
    }
    try {
        const resposta = await fetch(
            `${API_URL}/funcionarios/${funcionarioSelecionado.id}`,
            {
                method: "DELETE"
            }
        );
        if (!resposta.ok) {
            const mensagem = await resposta.text();
            throw new Error(
                mensagem || `Erro HTTP: ${resposta.status}`
            );
        }

        fecharModalDetalhesFuncionario();

        funcionarioSelecionado = null;
        modoEdicaoFuncionario = false;

        await carregarFuncionarios();

        alert("Funcionário removido com sucesso!");
    } catch (erro) {
        console.error(
            "Erro ao remover funcionário:",
            erro
        );

        alert(
            `Não foi possível remover o funcionário.\n\n${erro.message}`
        );
    }
}

// =========================================================
// EVENTOS DOS BOTÕES DE DETALHES
// =========================================================

function adicionarEventosDetalhes() {

    const botoes =
        document.querySelectorAll(
            ".funcionario-card-button"
        );


    botoes.forEach(
        function (botao) {

            botao.addEventListener(
                "click",
                function () {

                    const index =
                        Number(
                            botao.dataset.funcionarioIndex
                        );


                    abrirDetalhesFuncionario(
                        funcionarios[index]
                    );

                }
            );

        }
    );

}


// =========================================================
// ABRIR DETALHES
// =========================================================

function abrirDetalhesFuncionario(funcionario) {

    if (
        !modalDetalhesFuncionario ||
        !funcionario
    ) {
        return;
    }
    funcionarioSelecionado = funcionario;

    document.getElementById(
        "detalhesFuncionarioNome"
    ).textContent =
        funcionario.nome;


    document.getElementById(
        "detalhesFuncionarioCargoInfo"
    ).textContent =
        funcionario.cargo;


    document.getElementById(
        "detalhesFuncionarioMatricula"
    ).textContent =
        funcionario.matricula;


    document.getElementById(
        "detalhesFuncionarioCpf"
    ).textContent =
        funcionario.cpf;


    document.getElementById(
        "detalhesFuncionarioIdade"
    ).textContent =
        funcionario.idade;

    document.getElementById(
        "detalhesFuncionarioSetor"
    ).textContent =
        funcionario.setor;


    document.getElementById("detalhesFuncionarioAdmissao").textContent =
        funcionario.admissao
            ? funcionario.admissao.split("T")[0]
            : "-";

    document.getElementById(
        "detalhesFuncionarioTelefone"
    ).textContent =
        funcionario.telefone;


    document.getElementById(
        "detalhesFuncionarioEmail"
    ).textContent =
        funcionario.email;


    document.getElementById(
        "detalhesFuncionarioObservacoes"
    ).textContent =
        funcionario.observacoes ||
        "Nenhuma observação registrada.";


    fecharCadastroFuncionario();

    modalDetalhesFuncionario.classList.add(
        "active"
    );

}

function editarFuncionarioSelecionado() {

    if (!funcionarioSelecionado) {
        return;
    }

    modoEdicaoFuncionario = true;

    document.getElementById("funcionarioNome").value = funcionarioSelecionado.nome || "";
    document.getElementById("funcionarioMatricula").value = funcionarioSelecionado.matricula || "";
    document.getElementById("funcionarioCpf").value = funcionarioSelecionado.cpf || "";
    document.getElementById("funcionarioCargo").value = funcionarioSelecionado.cargo || "";
    document.getElementById("funcionarioIdade").value = funcionarioSelecionado.idade || "";

    const setor = funcionarioSelecionado.setor || "";

    const setores = {
        producao: "Produção",
        usinagem: "Usinagem",
        manutencao: "Manutenção"
    };

    document.getElementById("funcionarioSetor").value = setores[setor.toLowerCase()] || setor;
    document.getElementById("funcionarioAdmissao").value = funcionarioSelecionado.admissao
        ? funcionarioSelecionado.admissao.split("T")[0] : "";
    document.getElementById("funcionarioTelefone").value = funcionarioSelecionado.telefone || "";
    document.getElementById("funcionarioEmail").value = funcionarioSelecionado.email || "";
    document.getElementById("funcionarioObservacoes").value = funcionarioSelecionado.observacoes || "";

    fecharModalDetalhesFuncionario();
    modalNovoFuncionario.classList.add("active");
    document.getElementById("cadastrarFuncionario").textContent = "SALVAR ALTERAÇÕES";
}

// =========================================================
// INICIALIZAÇÃO
// =========================================================

carregarFuncionarios();

// =========================================================
// RELATÓRIOS DE SERVIÇOS
// =========================================================

const limparFiltrosServicos =
    document.getElementById("limparFiltrosServicos");

const gerarRelatorioServicos =
    document.getElementById("gerarRelatorioServicos");

const servicosDataInicio =
    document.getElementById("servicosDataInicio");

const servicosDataFim =
    document.getElementById("servicosDataFim");

const servicosStatus =
    document.getElementById("servicosStatus");

function limparFiltrosRelatorioServicos() {

    if (servicosDataInicio) {
        servicosDataInicio.value = "";
    }

    if (servicosDataFim) {
        servicosDataFim.value = "";
    }

    if (servicosStatus) {
        servicosStatus.value = "";
    }

}


function gerarRelatorioDeServicos() {

    /*
     * FUTURO BACKEND
     *
     * Aqui posteriormente vamos enviar os filtros
     * para a API e receber os dados das ordens de serviço.
     *
     * Exemplo futuro:
     *
     * GET /api/relatorios/servicos
     *
     * ?dataInicio=
     * &dataFim=
     * &status=
     */

    console.log("Gerando relatório de serviços...", {
        dataInicio: servicosDataInicio?.value || null,
        dataFim: servicosDataFim?.value || null,
        status: servicosStatus?.value || null,
    });

}


if (limparFiltrosServicos) {

    limparFiltrosServicos.addEventListener(
        "click",
        limparFiltrosRelatorioServicos
    );

}


if (gerarRelatorioServicos) {

    gerarRelatorioServicos.addEventListener(
        "click",
        gerarRelatorioDeServicos
    );

}

// =========================================================
// RELATÓRIO DE ESTOQUE
// =========================================================

const limparFiltrosEstoque =
    document.getElementById("limparFiltrosEstoque");

const gerarRelatorioEstoque =
    document.getElementById("gerarRelatorioEstoque");

const estoqueDataInicio =
    document.getElementById("estoqueDataInicio");

const estoqueDataFim =
    document.getElementById("estoqueDataFim");

const estoqueTipoMovimentacao =
    document.getElementById("estoqueTipoMovimentacao");

const estoqueMaterial =
    document.getElementById("estoqueMaterial");


// =========================================================
// LIMPAR FILTROS
// =========================================================

function limparFiltrosRelatorioEstoque() {

    if (estoqueDataInicio) {
        estoqueDataInicio.value = "";
    }

    if (estoqueDataFim) {
        estoqueDataFim.value = "";
    }

    if (estoqueTipoMovimentacao) {
        estoqueTipoMovimentacao.value = "";
    }

    if (estoqueMaterial) {
        estoqueMaterial.value = "";
    }

}


// =========================================================
// GERAR RELATÓRIO
// =========================================================

function gerarRelatorioDeEstoque() {

    console.log(
        "Gerando relatório de estoque...",
        {
            dataInicio:
                estoqueDataInicio?.value || null,

            dataFim:
                estoqueDataFim?.value || null,

            tipoMovimentacao:
                estoqueTipoMovimentacao?.value || null,

            material:
                estoqueMaterial?.value || null
        }
    );

}


// =========================================================
// EVENTOS
// =========================================================

if (limparFiltrosEstoque) {

    limparFiltrosEstoque.addEventListener(
        "click",
        limparFiltrosRelatorioEstoque
    );

}


if (gerarRelatorioEstoque) {

    gerarRelatorioEstoque.addEventListener(
        "click",
        gerarRelatorioDeEstoque
    );

}

// =========================================================
// RELATÓRIO FINANCEIRO
// =========================================================

const limparFiltrosFinanceiro =
    document.getElementById("limparFiltrosFinanceiro");

const gerarRelatorioFinanceiro =
    document.getElementById("gerarRelatorioFinanceiro");

const financeiroDataInicio =
    document.getElementById("financeiroDataInicio");

const financeiroDataFim =
    document.getElementById("financeiroDataFim");

const financeiroTipo =
    document.getElementById("financeiroTipo");

const financeiroStatus =
    document.getElementById("financeiroStatus");

const financeiroPagamento =
    document.getElementById("financeiroPagamento");


// =========================================================
// LIMPAR FILTROS
// =========================================================

function limparFiltrosRelatorioFinanceiro() {

    if (financeiroDataInicio) {
        financeiroDataInicio.value = "";
    }

    if (financeiroDataFim) {
        financeiroDataFim.value = "";
    }

    if (financeiroTipo) {
        financeiroTipo.value = "";
    }

    if (financeiroStatus) {
        financeiroStatus.value = "";
    }

    if (financeiroPagamento) {
        financeiroPagamento.value = "";
    }

}


// =========================================================
// GERAR RELATÓRIO
// =========================================================

function gerarRelatorioDeFinanceiro() {

    console.log(
        "Gerando relatório financeiro...",
        {
            dataInicio:
                financeiroDataInicio?.value || null,

            dataFim:
                financeiroDataFim?.value || null,

            tipo:
                financeiroTipo?.value || null,

            status:
                financeiroStatus?.value || null,

            pagamento:
                financeiroPagamento?.value || null
        }
    );

}


// =========================================================
// EVENTOS
// =========================================================

if (limparFiltrosFinanceiro) {

    limparFiltrosFinanceiro.addEventListener(
        "click",
        limparFiltrosRelatorioFinanceiro
    );

}


if (gerarRelatorioFinanceiro) {

    gerarRelatorioFinanceiro.addEventListener(
        "click",
        gerarRelatorioDeFinanceiro
    );

}