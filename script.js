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
        if (sectionId === "maquinas") {
            carregarMaquinas();
        }
    });
});
function fecharDetalhesMaquina() {
    pararSimulacaoProgresso();
    const modal =
        document.getElementById(
            "modalDetalhesMaquina"
        );
    if (!modal) {
        return;
    }
    modal.classList.remove("active");
}
const fecharDetalhesMaquinaX =
    document.getElementById("fecharDetalhesMaquina");
const fecharDetalhesMaquinaBotao =
    document.getElementById("fecharDetalhesMaquinaBotao");
if (fecharDetalhesMaquinaX) {
    fecharDetalhesMaquinaX.addEventListener(
        "click",
        fecharDetalhesMaquina
    );
}
if (fecharDetalhesMaquinaBotao) {

    fecharDetalhesMaquinaBotao.addEventListener(
        "click",
        fecharDetalhesMaquina
    );
}
const modalDetalhesMaquina =
    document.getElementById("modalDetalhesMaquina");
if (modalDetalhesMaquina) {
    modalDetalhesMaquina.addEventListener(
        "click",
        function (event) {

            if (event.target === modalDetalhesMaquina) {
                fecharDetalhesMaquina();
            }
        }
    );
}
document.addEventListener(
    "keydown",
    function (event) {
        if (event.key !== "Escape") {
            return;
        }
        fecharDetalhesMaquina();
    }
);

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

// =========================================================
// ORDEM DE SERVIÇO
// =========================================================

const modalOS = document.getElementById("modalOS");
const novaOS = document.getElementById("novaOS");
const criarOS = document.getElementById("criarOS");
const fecharModalOS = document.getElementById("fecharModalOS");
const cancelarOS = document.getElementById("cancelarOS");
const salvarOS = document.getElementById("salvarOS");
const salvarImprimirOS = document.getElementById("salvarImprimirOS");

// Modal de detalhes
const modalDetalhesOS = document.getElementById("modalDetalhesOS");
const fecharModalDetalhesOS = document.getElementById("fecharModalDetalhesOS");
const editarOS = document.getElementById("editarOS");
const removerOS = document.getElementById("removerOS");

let ordensServico = [];
let ordemSelecionadaOS = null;
let modoEdicaoOS = false;

// IDs usados no modal de detalhes (se algum id do seu HTML for
// diferente, ajuste APENAS aqui).
const IDS_DETALHES_OS = {
    numero: "detalhesOSNumero",
    cliente: "detalhesOSCliente",
    contato: "detalhesOSContato",
    endereco: "detalhesOSEndereco",
    dataAbertura: "detalhesOSDataAbertura",
    dataEntrega: "detalhesOSDataEntrega",
    tipoServico: "detalhesOSTipoServico",
    responsavel: "detalhesOSResponsavel",
    descricao: "detalhesOSDescricao",
    valorMaoObra: "detalhesOSValorMaoObra",
    valorMateriais: "detalhesOSValorMateriais",
    desconto: "detalhesOSDesconto",
    valorTotal: "detalhesOSValorTotal",
    condicaoPagamento: "detalhesOSCondicaoPagamento",
    observacoes: "detalhesOSObservacoes"
};

// =========================================================
// FORMATADORES
// =========================================================

function paraNumeroOS(valor) {
    if (typeof valor === "number") {
        return Number.isFinite(valor) ? valor : 0;
    }
    if (typeof valor === "string") {
        let texto = valor.replace(/[R$\s]/g, "");
        if (texto.includes(",")) {
            texto = texto.replace(/\./g, "").replace(",", ".");
        }
        const numero = Number(texto);
        return Number.isFinite(numero) ? numero : 0;
    }
    return 0;
}

function formatarMoedaOS(valor) {
    return new Intl.NumberFormat("pt-BR", {
        style: "currency",
        currency: "BRL"
    }).format(paraNumeroOS(valor));
}

function formatarDataOS(valor) {
    if (!valor) {
        return "-";
    }
    const texto = String(valor);
    // yyyy-MM-dd (ou ISO): não passa por new Date() para não voltar um dia por causa do fuso
    const iso = texto.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (iso) {
        return `${iso[3]}/${iso[2]}/${iso[1]}`;
    }
    const data = new Date(texto);
    if (Number.isNaN(data.getTime())) {
        return "-";
    }
    return data.toLocaleDateString("pt-BR");
}

function normalizarChaveOS(valor) {
    return String(valor ?? "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLowerCase()
        .replace(/[^a-z0-9]/g, "");
}

function formatarTextoLivreOS(valor) {
    const texto = String(valor ?? "").replace(/[_-]+/g, " ").trim();
    if (!texto) {
        return "-";
    }
    return texto.charAt(0).toUpperCase() + texto.slice(1).toLowerCase();
}

// Ajuste as chaves conforme os "value" das opções do seu <select>
const TIPOS_SERVICO_OS = {
    usinagem: "Usinagem",
    manutencao: "Manutenção",
    soldagem: "Soldagem",
    corte: "Corte",
    dobra: "Dobra",
    fabricacao: "Fabricação",
    reparo: "Reparo",
    instalacao: "Instalação",
    outro: "Outro",
    outros: "Outros"
};

const CONDICOES_PAGAMENTO_OS = {
    avista: "À vista",
    pix: "PIX",
    dinheiro: "Dinheiro",
    boleto: "Boleto",
    cartao: "Cartão",
    cartaocredito: "Cartão de crédito",
    cartaodebito: "Cartão de débito",
    "30dias": "30 dias",
    "60dias": "60 dias",
    "3060dias": "30/60 dias",
    parcelado: "Parcelado"
};

function obterTipoServicoOS(tipo) {
    if (tipo === null || tipo === undefined || tipo === "") {
        return "-";
    }
    return TIPOS_SERVICO_OS[normalizarChaveOS(tipo)] || formatarTextoLivreOS(tipo);
}

function obterCondicaoPagamentoOS(condicao) {
    if (condicao === null || condicao === undefined || condicao === "") {
        return "-";
    }
    return CONDICOES_PAGAMENTO_OS[normalizarChaveOS(condicao)] || formatarTextoLivreOS(condicao);
}

function escaparHtmlOS(valor) {
    return String(valor ?? "").replace(/[&<>"']/g, function (c) {
        return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c];
    });
}

function calcularTotalOS(ordem) {
    if (ordem.valorTotal !== undefined && ordem.valorTotal !== null) {
        return paraNumeroOS(ordem.valorTotal);
    }
    return paraNumeroOS(ordem.valorMaoObra)
        + paraNumeroOS(ordem.valorMateriais)
        - paraNumeroOS(ordem.desconto);
}

// =========================================================
// LISTAGEM
// =========================================================

async function carregarOrdensServico() {
    try {
        const resposta = await fetch(`${API_URL}/OrdensServico`);
        if (!resposta.ok) {
            throw new Error(`Erro HTTP: ${resposta.status}`);
        }
        ordensServico = await resposta.json();
        renderizarOrdensServico();
    } catch (erro) {
        console.error("Erro ao carregar ordens de serviço:", erro);
    }
}

function renderizarOrdensServico() {
    const lista = document.getElementById("ordensServicoLista");
    const empty = document.getElementById("ordensServicoEmpty");
    if (!lista || !empty) {
        return;
    }
    lista.innerHTML = "";

    if (ordensServico.length === 0) {
        empty.style.display = "flex";
        lista.style.display = "none";
        return;
    }
    empty.style.display = "none";
    lista.style.display = "flex";

    ordensServico.forEach(function (ordem) {
        const card = document.createElement("div");
        card.className = "ordem-servico-card";
        const responsavel = ordem.funcionario?.nome || "Não definido";

        card.innerHTML = `
            <div class="ordem-servico-info">
                <div class="ordem-servico-numero">
                    ${escaparHtmlOS(ordem.numeroOS)}
                </div>
                <div>
                    <span>CLIENTE</span>
                    <strong>${escaparHtmlOS(ordem.cliente)}</strong>
                </div>
                <div>
                    <span>SERVIÇO</span>
                    <strong>${escaparHtmlOS(obterTipoServicoOS(ordem.tipoServico))}</strong>
                </div>
                <div>
                    <span>RESPONSÁVEL</span>
                    <strong>${escaparHtmlOS(responsavel)}</strong>
                </div>
                <div>
                    <span>VALOR TOTAL</span>
                    <strong>${formatarMoedaOS(ordem.valorTotal)}</strong>
                </div>
            </div>
            <button
                type="button"
                class="btn-detalhes-os"
                data-os-id="${ordem.id}">
                DETALHES →
            </button>
        `;
        lista.appendChild(card);
    });
    adicionarEventosDetalhesOS();
}

function adicionarEventosDetalhesOS() {
    document.querySelectorAll(".btn-detalhes-os").forEach(function (botao) {
        botao.addEventListener("click", function () {
            carregarDetalhesOS(Number(botao.dataset.osId));
        });
    });
}

// =========================================================
// ABRIR / FECHAR MODAIS
// =========================================================

function abrirModalOS() {
    if (!modalOS) {
        return;
    }
    modalOS.classList.add("active");
    document.body.style.overflow = "hidden";
}

function fecharOS() {
    if (!modalOS) {
        return;
    }
    modalOS.classList.remove("active");
    document.body.style.overflow = "";
}

function abrirNovaOS() {
    modoEdicaoOS = false;
    ordemSelecionadaOS = null;
    limparFormularioOS();
    if (salvarOS) {
        salvarOS.textContent = "SALVAR";
    }
    abrirModalOS();
}

function abrirModalDetalhesOS() {
    if (!modalDetalhesOS) {
        return;
    }
    modalDetalhesOS.classList.add("active");
    document.body.style.overflow = "hidden";
}

function fecharDetalhesOS() {
    if (!modalDetalhesOS) {
        return;
    }
    modalDetalhesOS.classList.remove("active");
    document.body.style.overflow = "";
}

if (novaOS) novaOS.addEventListener("click", abrirNovaOS);
if (criarOS) criarOS.addEventListener("click", abrirNovaOS);
if (fecharModalOS) fecharModalOS.addEventListener("click", fecharOS);
if (cancelarOS) cancelarOS.addEventListener("click", fecharOS);
if (fecharModalDetalhesOS) fecharModalDetalhesOS.addEventListener("click", fecharDetalhesOS);

if (modalOS) {
    modalOS.addEventListener("click", function (event) {
        if (event.target === modalOS) {
            fecharOS();
        }
    });
}
if (modalDetalhesOS) {
    modalDetalhesOS.addEventListener("click", function (event) {
        if (event.target === modalDetalhesOS) {
            fecharDetalhesOS();
        }
    });
}
document.addEventListener("keydown", function (event) {
    if (event.key !== "Escape") {
        return;
    }
    if (modalOS && modalOS.classList.contains("active")) {
        fecharOS();
    } else if (modalDetalhesOS && modalDetalhesOS.classList.contains("active")) {
        fecharDetalhesOS();
    }
});

// =========================================================
// DETALHES
// =========================================================

async function carregarDetalhesOS(id) {
    let ordem = null;
    try {
        const resposta = await fetch(`${API_URL}/OrdensServico/${id}`);
        if (!resposta.ok) {
            throw new Error(`Erro HTTP: ${resposta.status}`);
        }
        ordem = await resposta.json();
    } catch (erro) {
        console.warn("Não foi possível buscar a OS na API, usando a lista local:", erro);
        ordem = ordensServico.find(function (o) {
            return o.id === id;
        }) || null;
    }

    if (!ordem) {
        alert("Não foi possível carregar os detalhes da ordem de serviço.");
        return;
    }
    preencherDetalhesOS(ordem);
    abrirModalDetalhesOS();
}

function preencherDetalhesOS(ordem) {
    if (!ordem) {
        return;
    }
    ordemSelecionadaOS = ordem;
    const ids = IDS_DETALHES_OS;

    definirTexto(ids.numero, ordem.numeroOS || "-");
    definirTexto(ids.cliente, ordem.cliente || "-");
    definirTexto(ids.contato, ordem.contato || "-");
    definirTexto(ids.endereco, ordem.endereco || "-");
    definirTexto(ids.dataAbertura, formatarDataOS(ordem.dataAbertura));
    definirTexto(ids.dataEntrega, formatarDataOS(ordem.dataEntrega));
    definirTexto(ids.tipoServico, obterTipoServicoOS(ordem.tipoServico));
    definirTexto(ids.responsavel, ordem.funcionario?.nome || "Não definido");
    definirTexto(ids.descricao, ordem.descricaoServico || "-");
    definirTexto(ids.valorMaoObra, formatarMoedaOS(ordem.valorMaoObra));
    definirTexto(ids.valorMateriais, formatarMoedaOS(ordem.valorMateriais));
    definirTexto(ids.desconto, formatarMoedaOS(ordem.desconto));
    definirTexto(ids.valorTotal, formatarMoedaOS(calcularTotalOS(ordem)));
    definirTexto(ids.condicaoPagamento, obterCondicaoPagamentoOS(ordem.condicaoPagamento));
    definirTexto(ids.observacoes, ordem.observacoes || "Nenhuma observação registrada.");
}

// =========================================================
// SALVAR (NOVA / EDIÇÃO) E IMPRIMIR
// =========================================================

function lerFormularioOS() {
    return {
        numeroOS: document.getElementById("numeroOS").value.trim(),
        cliente: document.getElementById("clienteOS").value.trim(),
        contato: document.getElementById("contatoOS").value.trim(),
        endereco: document.getElementById("enderecoOS").value.trim(),
        dataAbertura: document.getElementById("dataOS").value,
        descricaoServico: document.getElementById("descricaoOS").value.trim(),
        tipoServico: document.getElementById("tipoServico").value,
        dataEntrega: document.getElementById("dataEntregaOS").value,
        funcionarioId: Number(document.getElementById("responsavelOS").value) || null,
        valorMaoObra: Number(document.getElementById("valorMaoObra").value) || 0,
        desconto: Number(document.getElementById("descontoOS").value) || 0,
        condicaoPagamento: document.getElementById("condicaoPagamento").value,
        observacoes: document.getElementById("observacoesOS").value.trim(),
        // Na edição, preserva os materiais que já existem na OS
        materiais: modoEdicaoOS && ordemSelecionadaOS?.materiais
            ? ordemSelecionadaOS.materiais
            : []
    };
}

function validarOS(ordem) {
    if (!ordem.numeroOS) {
        alert("Informe o número da OS.");
        return false;
    }
    if (!ordem.cliente) {
        alert("Informe o cliente.");
        return false;
    }
    if (!ordem.descricaoServico) {
        alert("Informe a descrição do serviço.");
        return false;
    }
    if (!ordem.tipoServico) {
        alert("Selecione o tipo de serviço.");
        return false;
    }
    return true;
}

async function salvarOrdemServico(imprimir = false) {
    const ordem = lerFormularioOS();
    if (!validarOS(ordem)) {
        return;
    }

    const editando = modoEdicaoOS && ordemSelecionadaOS;
    const url = editando
        ? `${API_URL}/OrdensServico/${ordemSelecionadaOS.id}`
        : `${API_URL}/OrdensServico`;

    if (editando) {
        ordem.id = ordemSelecionadaOS.id;
    }

    if (salvarOS) salvarOS.disabled = true;
    if (salvarImprimirOS) salvarImprimirOS.disabled = true;

    try {
        const resposta = await fetch(url, {
            method: editando ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(ordem)
        });
        if (!resposta.ok) {
            const mensagem = await resposta.text();
            throw new Error(mensagem || `Erro HTTP: ${resposta.status}`);
        }

        // PUT normalmente devolve 204 (sem corpo)
        const texto = await resposta.text();
        const ordemSalva = texto ? JSON.parse(texto) : { ...ordem };

        fecharOS();
        modoEdicaoOS = false;
        ordemSelecionadaOS = null;
        limparFormularioOS();
        await carregarOrdensServico();

        if (imprimir) {
            imprimirOS(ordemSalva);
        } else {
            alert(
                editando
                    ? `Ordem de serviço ${ordemSalva.numeroOS || ordem.numeroOS} atualizada com sucesso!`
                    : `Ordem de serviço ${ordemSalva.numeroOS || ordem.numeroOS} criada com sucesso!`
            );
        }
    } catch (erro) {
        console.error("Erro ao salvar ordem de serviço:", erro);
        alert(`Não foi possível salvar a ordem de serviço.\n\n${erro.message}`);
    } finally {
        if (salvarOS) salvarOS.disabled = false;
        if (salvarImprimirOS) salvarImprimirOS.disabled = false;
    }
}

function imprimirOS(ordem) {
    const janela = window.open("", "_blank");
    if (!janela) {
        alert("O navegador bloqueou a janela de impressão. Libere os pop-ups para este site.");
        return;
    }

    let responsavel = ordem.funcionario?.nome;
    if (!responsavel && ordem.funcionarioId && Array.isArray(funcionarios)) {
        responsavel = funcionarios.find(function (f) {
            return f.id === ordem.funcionarioId;
        })?.nome;
    }
    responsavel = responsavel || "Não definido";

    const linha = function (rotulo, valor) {
        return `<tr><th>${rotulo}</th><td>${escaparHtmlOS(valor)}</td></tr>`;
    };

    janela.document.write(`<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="UTF-8">
<title>OS ${escaparHtmlOS(ordem.numeroOS)}</title>
<style>
    body { font-family: Arial, sans-serif; margin: 32px; color: #111; }
    h1 { font-size: 20px; border-bottom: 2px solid #111; padding-bottom: 8px; }
    table { width: 100%; border-collapse: collapse; margin-top: 16px; }
    th, td { text-align: left; padding: 8px; border: 1px solid #999; font-size: 13px; vertical-align: top; }
    th { width: 30%; background: #eee; }
</style>
</head>
<body>
<h1>ORDEM DE SERVIÇO Nº ${escaparHtmlOS(ordem.numeroOS)}</h1>
<table>
    ${linha("Cliente", ordem.cliente || "-")}
    ${linha("Contato", ordem.contato || "-")}
    ${linha("Endereço", ordem.endereco || "-")}
    ${linha("Data de abertura", formatarDataOS(ordem.dataAbertura))}
    ${linha("Data de entrega", formatarDataOS(ordem.dataEntrega))}
    ${linha("Tipo de serviço", obterTipoServicoOS(ordem.tipoServico))}
    ${linha("Responsável", responsavel)}
    ${linha("Descrição", ordem.descricaoServico || "-")}
    ${linha("Mão de obra", formatarMoedaOS(ordem.valorMaoObra))}
    ${linha("Materiais", formatarMoedaOS(ordem.valorMateriais))}
    ${linha("Desconto", formatarMoedaOS(ordem.desconto))}
    ${linha("Valor total", formatarMoedaOS(calcularTotalOS(ordem)))}
    ${linha("Condição de pagamento", obterCondicaoPagamentoOS(ordem.condicaoPagamento))}
    ${linha("Observações", ordem.observacoes || "-")}
</table>
</body>
</html>`);
    janela.document.close();
    janela.focus();
    janela.onload = function () {
        janela.print();
    };
    // Caso o onload já tenha disparado
    setTimeout(function () {
        janela.print();
    }, 500);
}

// =========================================================
// EDITAR / REMOVER
// =========================================================

function editarOSSelecionada() {
    if (!ordemSelecionadaOS) {
        return;
    }
    const o = ordemSelecionadaOS;
    modoEdicaoOS = true;

    const paraInputData = function (valor) {
        return valor ? String(valor).split("T")[0] : "";
    };

    document.getElementById("numeroOS").value = o.numeroOS || "";
    document.getElementById("clienteOS").value = o.cliente || "";
    document.getElementById("contatoOS").value = o.contato || "";
    document.getElementById("enderecoOS").value = o.endereco || "";
    document.getElementById("dataOS").value = paraInputData(o.dataAbertura);
    document.getElementById("dataEntregaOS").value = paraInputData(o.dataEntrega);
    document.getElementById("descricaoOS").value = o.descricaoServico || "";
    document.getElementById("tipoServico").value = o.tipoServico ?? "";
    document.getElementById("responsavelOS").value = o.funcionarioId ?? o.funcionario?.id ?? "";
    document.getElementById("valorMaoObra").value = o.valorMaoObra ?? "0.00";
    document.getElementById("descontoOS").value = o.desconto ?? "0.00";
    document.getElementById("condicaoPagamento").value = o.condicaoPagamento ?? "";
    document.getElementById("observacoesOS").value = o.observacoes || "";

    if (salvarOS) {
        salvarOS.textContent = "SALVAR ALTERAÇÕES";
    }
    fecharDetalhesOS();
    abrirModalOS();
}

async function removerOSSelecionada() {
    if (!ordemSelecionadaOS) {
        return;
    }
    const confirmar = confirm(
        `Deseja realmente remover a OS "${ordemSelecionadaOS.numeroOS}"?`
    );
    if (!confirmar) {
        return;
    }
    try {
        const resposta = await fetch(
            `${API_URL}/OrdensServico/${ordemSelecionadaOS.id}`,
            { method: "DELETE" }
        );
        if (!resposta.ok) {
            const mensagem = await resposta.text();
            throw new Error(mensagem || `Erro HTTP: ${resposta.status}`);
        }
        fecharDetalhesOS();
        ordemSelecionadaOS = null;
        await carregarOrdensServico();
        alert("Ordem de serviço removida com sucesso!");
    } catch (erro) {
        console.error("Erro ao remover ordem de serviço:", erro);
        alert(`Não foi possível remover a ordem de serviço.\n\n${erro.message}`);
    }
}

// =========================================================
// LIMPAR FORMULÁRIO
// =========================================================

function limparFormularioOS() {
    document.getElementById("clienteOS").value = "";
    document.getElementById("contatoOS").value = "";
    document.getElementById("enderecoOS").value = "";
    document.getElementById("numeroOS").value = "";
    document.getElementById("dataOS").value = "";
    document.getElementById("descricaoOS").value = "";
    document.getElementById("tipoServico").value = "";
    document.getElementById("dataEntregaOS").value = "";
    document.getElementById("responsavelOS").value = "";
    document.getElementById("valorMaoObra").value = "0.00";
    document.getElementById("descontoOS").value = "0.00";
    document.getElementById("condicaoPagamento").value = "";
    document.getElementById("observacoesOS").value = "";
    document.getElementById("valorMateriais").value = "0.00";
    document.getElementById("valorTotalOS").textContent = "R$ 0,00";
    const listaMateriais = document.getElementById("listaMateriais");
    if (listaMateriais) {
        listaMateriais.innerHTML = `
            <tr class="os-table-empty">
                <td colspan="6">
                    Nenhum material adicionado à ordem.
                </td>
            </tr>
        `;
    }
}

// =========================================================
// LIGAÇÃO DOS BOTÕES
// =========================================================

if (salvarOS) {
    salvarOS.addEventListener("click", function () {
        salvarOrdemServico(false);
    });
}
if (salvarImprimirOS) {
    salvarImprimirOS.addEventListener("click", function () {
        salvarOrdemServico(true);
    });
}
if (editarOS) editarOS.addEventListener("click", editarOSSelecionada);
if (removerOS) removerOS.addEventListener("click", removerOSSelecionada);

// =========================================================
// CONFERÊNCIA DE IDs DO HTML (aparece no console do navegador)
// =========================================================

function conferirIdsOS() {
    const idsFixos = [
        "modalOS", "novaOS", "criarOS", "fecharModalOS", "cancelarOS",
        "salvarOS", "salvarImprimirOS",
        "modalDetalhesOS", "fecharModalDetalhesOS", "editarOS", "removerOS",
        "ordensServicoLista", "ordensServicoEmpty",
        "numeroOS", "clienteOS", "contatoOS", "enderecoOS", "dataOS",
        "dataEntregaOS", "descricaoOS", "tipoServico", "responsavelOS",
        "valorMaoObra", "valorMateriais", "descontoOS", "condicaoPagamento",
        "observacoesOS", "valorTotalOS", "listaMateriais"
    ];
    const todos = idsFixos.concat(Object.values(IDS_DETALHES_OS));
    const faltando = todos.filter(function (id) {
        return !document.getElementById(id);
    });
    if (faltando.length > 0) {
        console.warn("[OS] IDs não encontrados no HTML:", faltando);
    } else {
        console.log("[OS] Todos os IDs foram encontrados.");
    }
}
conferirIdsOS();

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

const abrirModalMaquina =
    document.getElementById("abrirModalMaquina");
const modalMaquina =
    document.getElementById("modalMaquina");
const fecharModalMaquina =
    document.getElementById("fecharModalMaquina");
const cancelarModalMaquina =
    document.getElementById("cancelarModalMaquina");

// MODAL NOVA MÁQUINA

if (abrirModalMaquina) {
    abrirModalMaquina.addEventListener("click", () => {
        modalMaquina.classList.add("active");
    });
}
if (fecharModalMaquina) {
    fecharModalMaquina.addEventListener("click", () => {
        modalMaquina.classList.remove("active");
    });
}
if (cancelarModalMaquina) {
    cancelarModalMaquina.addEventListener("click", () => {
        modalMaquina.classList.remove("active");
    });
}
if (modalMaquina) {
    modalMaquina.addEventListener("click", (event) => {
        if (event.target === modalMaquina) {
            modalMaquina.classList.remove("active");
        }
    });
}

// VARIÁVEIS DAS MÁQUINAS

let maquinas = [];
let processosAtivos = [];
let configuracoesMaquina = [];

// CARREGAR MÁQUINAS

async function carregarMaquinas() {
    const container = document.getElementById("maquinasGrid");
    try {
        const resposta = await fetch(`${API_URL}/Maquinas`);
        if (!resposta.ok) {
            throw new Error(`Erro HTTP: ${resposta.status}`);
        }
        maquinas = await resposta.json();
        renderizarMaquinas();
        await carregarProcessosAtivos();
        renderizarMaquinas();
    } catch (erro) {
        console.error("Erro ao carregar máquinas:", erro);
        if (container) {
            container.innerHTML = `
                <div class="maquinas-empty">
                    <span>ERRO AO CARREGAR EQUIPAMENTOS</span>
                </div>
            `;
        }
    }
}

// CARREGAR PROCESSOS ATIVOS

async function carregarProcessosAtivos() {
    try {
        const resposta = await fetch(
            `${API_URL}/ProcessosProducao/ativos`
        );
        if (!resposta.ok) {
            throw new Error(
                `Erro HTTP: ${resposta.status}`
            );
        }
        processosAtivos = await resposta.json();
        console.log(
            "Processos ativos:",
            processosAtivos
        );
    } catch (erro) {
        console.error(
            "Erro ao carregar processos ativos:",
            erro
        );
        processosAtivos = [];
    }
}

// ENCONTRAR PROCESSO DA MÁQUINA

function encontrarProcessoDaMaquina(maquinaId) {
    return processosAtivos.find(
        processo =>
            processo.maquinaId === maquinaId
    );
}

// RENDERIZAR MÁQUINAS

function renderizarMaquinas() {
    const container =
        document.getElementById("maquinasGrid");
    if (!container) {
        console.warn(
            "Elemento #maquinasGrid não encontrado."
        );
        return;
    }
    container.innerHTML = "";

    if (maquinas.length === 0) {
        container.innerHTML = `
            <div class="maquinas-empty">
                <span>NENHUMA MÁQUINA CADASTRADA</span>
            </div>
        `;
        return;
    }
    maquinas.forEach(maquina => {
        const processo =
            encontrarProcessoDaMaquina(maquina.id);
        const produzindo =
            processo &&
            processo.status === "EM_EXECUCAO";
        const statusTexto =
            produzindo
                ? "PRODUZINDO"
                : "OPERACIONAL";
        const statusClasse =
            produzindo
                ? "produzindo"
                : "operacional";
        const maquinaElement =
            document.createElement("div");
        maquinaElement.className =
            "maquina-row";
        maquinaElement.innerHTML = `
            <div class="maquina-identificacao">
                <div class="maquina-icone">
                    ⚙
                </div>
                <div class="maquina-nome">
                    <strong>
                        ${maquina.nome}
                    </strong>
                    <span>
                        ${maquina.codigo}
                    </span>
                </div>
            </div>
            <div class="maquina-status ${statusClasse}">         
                ${statusTexto}
            </div>
            <div class="maquina-processo">
                ${produzindo
                ? `
                        <strong>
                            ${processo.numeroOS}
                        </strong>
                        <span>
                            ${processo.quantidadeProduzida}
                            /
                            ${processo.quantidadePlanejada}
                        </span>
                    `
                : `
                        <span>
                            NENHUM PROCESSO EM EXECUÇÃO
                        </span>
                    `
            }
            </div>
            <button
                type="button"
                class="maquina-detalhes-button"
                data-maquina-id="${maquina.id}"
            >
                DETALHES →
            </button>
        `;
        container.appendChild(
            maquinaElement
        );
    });
    adicionarEventosDetalhesMaquinas();
}
function adicionarEventosDetalhesMaquinas() {
    const botoes =
        document.querySelectorAll(
            ".maquina-detalhes-button"
        );
    botoes.forEach(botao => {
        botao.addEventListener(
            "click",
            () => {
                const maquinaId =
                    Number(
                        botao.dataset.maquinaId
                    );
                abrirDetalhesMaquina(
                    maquinaId
                );
            }
        );
    });
}
function numeroValido(valor) {
    if (valor === null || valor === undefined || valor === "") {
        return null;
    }
    const numero = Number(valor);
    return Number.isFinite(numero) ? numero : null;
}
function normalizarTexto(texto) {
    return String(texto ?? "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .trim()
        .toLowerCase();
}
function encontrarConfiguracaoDoServico(configuracoes, tipoServico) {
    if (!Array.isArray(configuracoes) || !tipoServico) {
        return null;
    }
    const alvo = normalizarTexto(tipoServico);
    return configuracoes.find(
        configuracao => normalizarTexto(configuracao.tipoServico) === alvo
    ) || null;
}
function formatarTempoOperacao(dataInicio) {
    if (!dataInicio) {
        return "-";
    }
    const inicio = new Date(dataInicio);
    if (Number.isNaN(inicio.getTime())) {
        return "-";
    }
    const minutosTotais = Math.max(
        0,
        Math.floor((new Date() - inicio) / 60000)
    );
    const horas = Math.floor(minutosTotais / 60);
    const minutos = minutosTotais % 60;
    return horas > 0 ? `${horas}h ${minutos}min` : `${minutos} min`;
}
function definirTexto(idElemento, texto) {
    const elemento = document.getElementById(idElemento);
    if (elemento) {
        elemento.textContent = texto;
    }
}
const STATUS_EM_EXECUCAO = "EM_EXECUCAO";
const QUANTIDADE_BARRAS_PROGRESSO = 32;
function criarBarrasProgressoMaquina() {

    const container =
        document.getElementById("detalhesMaquinaProgresso");

    if (!container) {
        return;
    }

    container.innerHTML = "";

    for (
        let i = 0;
        i < QUANTIDADE_BARRAS_PROGRESSO;
        i++
    ) {

        const barra = document.createElement("span");

        barra.className = "wake-progress-bar";

        barra.dataset.active = "false";
        barra.dataset.current = "false";
        barra.dataset.wave = "false";

        container.appendChild(barra);
    }
}


function atualizarVisualProgresso(produzida, planejada) {

    const container =
        document.getElementById("detalhesMaquinaProgresso");
    if (!container || planejada <= 0) {
        return;
    }
    const percentual =
        Math.min(
            (produzida / planejada) * 100,
            100
        );
    const barras =
        container.querySelectorAll(
            ".wake-progress-bar"
        );
    const posicaoAtual =
        (percentual / 100) *
        (barras.length - 1);
    barras.forEach((barra, index) => {
        const distancia =
            Math.abs(index - posicaoAtual);
        barra.dataset.active =
            index <= posicaoAtual ? "true" : "false";
        barra.dataset.current = distancia < 0.5 ? "true" : "false";
        barra.dataset.wave = distancia > 0.5 && distancia <= 1
                ? "true"
                : "false";
    });

    definirTexto(
        "detalhesMaquinaProgressoTexto",
        `${Math.round(produzida)} / ${Math.round(planejada)}`
    );

    definirTexto(
        "detalhesMaquinaPercentual",
        `${percentual.toFixed(0)}%`
    );
}


function iniciarSimulacaoProgresso(
    produzidaInicial,
    planejada,
    producaoPorMinuto,
    consumoPorUnidade
) {

    pararSimulacaoProgresso();

    if (
        planejada <= 0 ||
        producaoPorMinuto === null ||
        producaoPorMinuto <= 0
    ) {
        return;
    }

    progressoSimulado =
        Math.min(
            produzidaInicial,
            planejada
        );

    // 1 segundo real = 1 minuto simulado
    const MINUTOS_SIMULADOS_POR_SEGUNDO = 0.15;

    intervaloProgressoMaquina =
        setInterval(() => {

            if (
                progressoSimulado >=
                planejada
            ) {

                progressoSimulado =
                    planejada;

                atualizarVisualProgresso(
                    progressoSimulado,
                    planejada
                );

                atualizarDadosProducaoVisual(
                    progressoSimulado,
                    consumoPorUnidade
                );

                pararSimulacaoProgresso();

                return;
            }

            progressoSimulado +=
                producaoPorMinuto *
                MINUTOS_SIMULADOS_POR_SEGUNDO;

            if (
                progressoSimulado >
                planejada
            ) {
                progressoSimulado =
                    planejada;
            }

            atualizarVisualProgresso(
                progressoSimulado,
                planejada
            );

            atualizarDadosProducaoVisual(
                progressoSimulado,
                consumoPorUnidade
            );

        }, 1000);
}


function pararSimulacaoProgresso() {

    if (intervaloProgressoMaquina) {

        clearInterval(
            intervaloProgressoMaquina
        );

        intervaloProgressoMaquina = null;
    }
}


function atualizarDadosProducaoVisual(
    produzida,
    consumoPorUnidade
) {

    if (
        consumoPorUnidade === null ||
        consumoPorUnidade === undefined
    ) {
        return;
    }

    const materialConsumido =
        produzida *
        consumoPorUnidade;

    definirTexto(
        "detalhesMaquinaMaterial",
        materialConsumido.toFixed(2)
    );
}
let intervaloProgressoMaquina = null;
let progressoSimulado = 0;
async function abrirDetalhesMaquina(id) {
    try {
        const respostaMaquina = await fetch(`${API_URL}/Maquinas/${id}`);
        if (!respostaMaquina.ok) {
            throw new Error(
                `Erro ao buscar máquina: ${respostaMaquina.status}`
            );
        }
        const maquina = await respostaMaquina.json();
        let processo = null;
        const respostaProcesso = await fetch(
            `${API_URL}/ProcessosProducao/maquina/${id}`
        );
        if (respostaProcesso.ok) {
            processo = await respostaProcesso.json();
        } else if (respostaProcesso.status !== 404) {
            throw new Error(
                `Erro ao buscar processo: ${respostaProcesso.status}`
            );
        }
        const configuracoes = await carregarConfiguracoesMaquina(id);
        definirTexto("detalhesMaquinaNome", maquina.nome || "Máquina");
        definirTexto(
            "detalhesMaquinaIdentificacao",
            `${maquina.codigo || "-"} • ` +
            `${maquina.fabricante || "-"} • ` +
            `${maquina.modelo || "-"}`
        );

        const produzindo =
            !!processo && processo.status === STATUS_EM_EXECUCAO;
        if (!produzindo) {
            // ---------- SEM PROCESSO EM EXECUÇÃO ----------
            definirTexto("detalhesMaquinaStatus", "OPERACIONAL");
            definirTexto("detalhesMaquinaOS", "-");
            definirTexto("detalhesMaquinaCliente", "-");
            definirTexto("detalhesMaquinaProcesso", "-");
            definirTexto("detalhesMaquinaProgressoTexto", "0 / 0");
            definirTexto("detalhesMaquinaPercentual", "0%");
            definirTexto("detalhesMaquinaProducaoMinuto", "-");
            definirTexto("detalhesMaquinaTempo", "-");
            definirTexto("detalhesMaquinaMaterial", "-");
            definirTexto(
                "detalhesMaquinaObservacoes",
                maquina.observacoes || "Nenhuma observação registrada."
            );
            pararSimulacaoProgresso();
            criarBarrasProgressoMaquina();
            atualizarVisualProgresso(0, 1);
        } else {
            // ---------- PROCESSO EM EXECUÇÃO ----------
            const configuracao = encontrarConfiguracaoDoServico(
                configuracoes,
                processo.tipoServico
            );
            const produzida = numeroValido(processo.quantidadeProduzida) ?? 0;
            const planejada = numeroValido(processo.quantidadePlanejada) ?? 0;
            const percentualProducao = planejada > 0
                ? Math.min((produzida / planejada) * 100, 100)
                : 0;
            const porMinuto =
                numeroValido(processo.producaoPorMinuto) ??
                numeroValido(configuracao?.producaoPorMinuto);
            const consumoPorUnidade =
                numeroValido(configuracao?.consumoPorUnidade);
            const materialConsumido =
                numeroValido(processo.materialConsumido) ??
                (consumoPorUnidade !== null
                    ? produzida * consumoPorUnidade
                    : null);
            definirTexto("detalhesMaquinaStatus", "PRODUZINDO");
            definirTexto("detalhesMaquinaOS", processo.numeroOS || "-");
            definirTexto("detalhesMaquinaCliente", processo.cliente || "-");
            definirTexto(
                "detalhesMaquinaProcesso",
                processo.tipoServico || "-"
            );
            definirTexto(
                "detalhesMaquinaProgressoTexto",
                `${produzida} / ${planejada}`
            );
            definirTexto(
                "detalhesMaquinaPercentual",
                `${percentualProducao.toFixed(0)}%`
            );
            definirTexto(
                "detalhesMaquinaProducaoMinuto",
                porMinuto !== null ? `${porMinuto.toFixed(2)} un/min` : "-"
            );
            definirTexto(
                "detalhesMaquinaTempo",
                formatarTempoOperacao(processo.dataInicio)
            );
            definirTexto(
                "detalhesMaquinaMaterial",
                materialConsumido !== null
                    ? materialConsumido.toFixed(2)
                    : "-"
            );
            definirTexto(
                "detalhesMaquinaObservacoes",
                processo.observacoes ||
                maquina.observacoes ||
                "Nenhuma observação registrada."
            );
            criarBarrasProgressoMaquina();
            atualizarVisualProgresso(
                produzida,
                planejada
            );
            iniciarSimulacaoProgresso(
                produzida,
                planejada,
                porMinuto,
                consumoPorUnidade
            );
        }
        document
            .getElementById("modalDetalhesMaquina")
            .classList.add("active");
    } catch (erro) {
        console.error("Erro ao abrir detalhes da máquina:", erro);
        alert("Não foi possível carregar os detalhes da máquina.");
    }
}

// CONFIGURAÇÕES MÁQUINA

async function carregarConfiguracoesMaquina(maquinaId) {
    try {
        const resposta = await fetch(
            `${API_URL}/ConfiguracoesMaquina/maquina/${maquinaId}`
        );
        if (!resposta.ok) {
            throw new Error(`Erro HTTP: ${resposta.status}`);
        }
        return await resposta.json();
    } catch (erro) {
        console.error("Erro ao carregar configurações da máquina:",
            erro
        );
        return [];
    }
}

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
        preencherResponsaveisOS();
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

// PREENCHER FUNCIONÁRIO RESPONSÁVEL

function preencherResponsaveisOS() {

    const select = document.getElementById("responsavelOS");

    if (!select) {
        return;
    }

    select.innerHTML = `
        <option value="">
            Selecione o funcionário
        </option>
    `;

    funcionarios.forEach(function (funcionario) {

        const option = document.createElement("option");

        option.value = funcionario.id;
        option.textContent =
            `${funcionario.nome} - ${funcionario.cargo}`;

        select.appendChild(option);

    });
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
carregarMaquinas();
carregarOrdensServico();

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