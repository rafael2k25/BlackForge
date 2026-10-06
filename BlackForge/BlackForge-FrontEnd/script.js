// =========================================================
// CONFIGURAÇÃO DA API
// =========================================================

const API_URL = "https://localhost:7089/api";

// =========================================================
// NOTIFICAÇÕES (substitui alert())
// =========================================================

const TOAST_TIPOS = {
    sucesso: { icone: "checkmark-circle-outline", titulo: "SUCESSO", duracao: 4000 },
    erro:    { icone: "alert-circle-outline",     titulo: "ERRO",    duracao: 7000 },
    aviso:   { icone: "warning-outline",          titulo: "ATENÇÃO", duracao: 5000 },
    info:    { icone: "information-circle-outline", titulo: "INFO",  duracao: 4000 }
};

function detectarTipoToast(mensagem) {
    const texto = mensagem.toLowerCase();

    if (texto.includes("não foi possível") || texto.includes("erro")) return "erro";
    if (texto.includes("sucesso")) return "sucesso";
    if (
        texto.includes("informe") || texto.includes("selecione") ||
        texto.includes("preencha") || texto.includes("não pode") ||
        texto.includes("bloqueou")
    ) return "aviso";

    return "info";
}

function notificar(mensagem, tipo) {
    tipo = TOAST_TIPOS[tipo] ? tipo : detectarTipoToast(String(mensagem));
    const config = TOAST_TIPOS[tipo];

    let container = document.getElementById("toastContainer");
    if (!container) {
        container = document.createElement("div");
        container.id = "toastContainer";
        container.className = "toast-container";
        container.setAttribute("aria-live", "polite");
        document.body.appendChild(container);
    }

    // Primeira linha vira o texto principal; o resto (ex.: erro.message) vira detalhe
    const [principal, ...resto] = String(mensagem).split("\n");
    const detalhe = resto.join("\n").trim();

    const toast = document.createElement("div");
    toast.className = `toast toast-${tipo}`;
    toast.setAttribute("role", tipo === "erro" ? "alert" : "status");
    toast.style.setProperty("--toast-duracao", `${config.duracao}ms`);

    const icone = document.createElement("ion-icon");
    icone.setAttribute("name", config.icone);
    icone.className = "toast-icone";

    const corpo = document.createElement("div");
    corpo.className = "toast-corpo";

    const titulo = document.createElement("span");
    titulo.className = "toast-titulo";
    titulo.textContent = config.titulo;

    const texto = document.createElement("span");
    texto.className = "toast-texto";
    texto.textContent = principal;

    corpo.append(titulo, texto);

    if (detalhe) {
        const det = document.createElement("span");
        det.className = "toast-detalhe";
        det.textContent = detalhe;
        corpo.appendChild(det);
    }

    const fechar = document.createElement("button");
    fechar.type = "button";
    fechar.className = "toast-fechar";
    fechar.setAttribute("aria-label", "Fechar notificação");
    fechar.innerHTML = '<ion-icon name="close-outline"></ion-icon>';

    const barra = document.createElement("div");
    barra.className = "toast-barra";

    toast.append(icone, corpo, fechar, barra);
    container.appendChild(toast);

    let removido = false;
    function remover() {
        if (removido) return;
        removido = true;
        toast.classList.add("toast-saindo");
        toast.addEventListener("animationend", () => toast.remove(), { once: true });
    }

    fechar.addEventListener("click", remover);
    barra.addEventListener("animationend", remover);

    // Máximo de 5 notificações na tela
    while (container.children.length > 5) {
        container.firstElementChild.remove();
    }
}

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
const btnFecharDetalhesOS = document.getElementById("fecharDetalhesOS");
const btnFecharDetalhesOSRodape = document.getElementById("fecharDetalhesOSBotao");
const tituloModalOS = document.getElementById("tituloModalOS");
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
    clienteInfo: "detalhesOSClienteInfo",
    contato: "detalhesOSContato",
    endereco: "detalhesOSEndereco",
    dataAbertura: "detalhesOSDataAbertura",
    dataEntrega: "detalhesOSDataEntrega",
    tipoServico: "detalhesOSTipoServico",
    responsavel: "detalhesOSResponsavel",
    descricao: "detalhesOSDescricao",
    valorMaoObra: "detalhesOSMaoObra",
    valorMateriais: "detalhesOSMateriais",
    desconto: "detalhesOSDesconto",
    valorTotal: "detalhesOSTotal",
    condicaoPagamento: "detalhesOSPagamento",
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
    torneamento: "Torneamento",
    fresagem: "Fresagem",
    corte: "Corte",
    solda: "Solda",
    outro: "Outro"
};

const CONDICOES_PAGAMENTO_OS = {
    avista: "À vista",
    pix: "PIX",
    cartao: "Cartão",
    boleto: "Boleto",
    "30dias": "30 dias",
    parcelado: "Parcelado",
    outro: "Outro"
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

// Lê um campo do objeto ignorando maiúsculas/minúsculas (valorMateriais, ValorMateriais...)
function lerCampoOS(obj, nomes) {
    if (!obj) {
        return undefined;
    }
    const chaves = Object.keys(obj);
    for (const nome of nomes) {
        const alvo = normalizarChaveOS(nome);
        const chave = chaves.find(function (k) {
            return normalizarChaveOS(k) === alvo;
        });
        if (chave !== undefined && obj[chave] !== null && obj[chave] !== undefined) {
            return obj[chave];
        }
    }
    return undefined;
}

function obterValorMateriaisOS(ordem) {
    const direto = lerCampoOS(ordem, ["valorMateriais", "valorMaterial", "totalMateriais"]);
    if (direto !== undefined) {
        return paraNumeroOS(direto);
    }
    const lista = lerCampoOS(ordem, ["materiais"]);
    if (Array.isArray(lista)) {
        return lista.reduce(function (soma, m) {
            const total = lerCampoOS(m, ["valorTotal", "subtotal", "total"]);
            if (total !== undefined) {
                return soma + paraNumeroOS(total);
            }
            const qtd = lerCampoOS(m, ["quantidade"]);
            const unit = lerCampoOS(m, ["valorUnitario", "precoUnitario", "preco", "valor"]);
            return soma + (qtd === undefined ? 1 : paraNumeroOS(qtd)) * paraNumeroOS(unit);
        }, 0);
    }
    return 0;
}

function obterDescontoOS(ordem) {
    return paraNumeroOS(lerCampoOS(ordem, ["desconto", "valorDesconto"]));
}

// Seleciona a <option> comparando sem acento/maiúscula, pelo value ou pelo texto
function selecionarOpcaoOS(idSelect, valor) {
    const select = document.getElementById(idSelect);
    if (!select) {
        return;
    }
    select.value = "";
    if (valor === null || valor === undefined || valor === "") {
        return;
    }
    const alvo = normalizarChaveOS(valor);
    const opcao = Array.from(select.options).find(function (o) {
        return normalizarChaveOS(o.value) === alvo
            || normalizarChaveOS(o.textContent) === alvo;
    });
    if (opcao) {
        select.value = opcao.value;
    } else {
        console.warn(`[OS] Nenhuma opção de "${idSelect}" corresponde a:`, valor);
    }
}

function calcularTotalOS(ordem) {
    if (ordem.valorTotal !== undefined && ordem.valorTotal !== null) {
        return paraNumeroOS(ordem.valorTotal);
    }
    return paraNumeroOS(ordem.valorMaoObra)
        + paraNumeroOS(obterValorMateriaisOS(ordem))
        - paraNumeroOS(obterDescontoOS(ordem));
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
        console.log("Ordens de serviço carregadas:", ordensServico);
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
        const linha = document.createElement("div");
        linha.className = "os-row";
        const responsavel = ordem.funcionario?.nome || "Não definido";

        linha.innerHTML = `
            <div class="os-identificacao">
                <div class="os-icon">
                    <ion-icon name="document-text-outline"></ion-icon>
                </div>
                <div class="os-info">
                    <div class="os-numero">${escaparHtmlOS(ordem.numeroOS)}</div>
                    <div class="os-cliente">${escaparHtmlOS(ordem.cliente)}</div>
                </div>
            </div>
            <div class="os-coluna">
                <span>SERVIÇO</span>
                <strong>${escaparHtmlOS(obterTipoServicoOS(ordem.tipoServico))}</strong>
            </div>
            <div class="os-coluna os-responsavel">
                <span>RESPONSÁVEL</span>
                <strong>${escaparHtmlOS(responsavel)}</strong>
            </div>
            <div class="os-coluna os-valor-coluna">
                <span>VALOR TOTAL</span>
                <strong class="os-valor">${formatarMoedaOS(ordem.valorTotal)}</strong>
            </div>
            <button
                type="button"
                class="btn-detalhes-os"
                data-os-id="${ordem.id}">
                DETALHES →
            </button>
        `;
        lista.appendChild(linha);
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
        salvarOS.textContent = "SALVAR OS";
    }
    if (tituloModalOS) {
        tituloModalOS.textContent = "NOVA ORDEM DE SERVIÇO";
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

function fecharModalDetalhesOS() {
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
if (btnFecharDetalhesOS) btnFecharDetalhesOS.addEventListener("click", fecharModalDetalhesOS);
if (btnFecharDetalhesOSRodape) btnFecharDetalhesOSRodape.addEventListener("click", fecharModalDetalhesOS);

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
            fecharModalDetalhesOS();
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
        fecharModalDetalhesOS();
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
        notificar("Não foi possível carregar os detalhes da ordem de serviço.");
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
    definirTexto(ids.clienteInfo, ordem.cliente || "-");
    definirTexto(ids.contato, ordem.contato || "-");
    definirTexto(ids.endereco, ordem.endereco || "-");
    definirTexto(ids.dataAbertura, formatarDataOS(ordem.dataAbertura));
    definirTexto(ids.dataEntrega, formatarDataOS(ordem.dataEntrega));
    definirTexto(ids.tipoServico, obterTipoServicoOS(ordem.tipoServico));
    definirTexto(ids.responsavel, ordem.funcionario?.nome || "Não definido");
    definirTexto(ids.descricao, ordem.descricaoServico || "-");
    definirTexto(ids.valorMaoObra, formatarMoedaOS(ordem.valorMaoObra));
    definirTexto(ids.valorMateriais, formatarMoedaOS(obterValorMateriaisOS(ordem)));
    definirTexto(ids.desconto, formatarMoedaOS(obterDescontoOS(ordem)));
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
        valorMateriais: Number(document.getElementById("valorMateriais").value) || 0,
        desconto: Number(document.getElementById("descontoOS").value) || 0,
        condicaoPagamento: document.getElementById("condicaoPagamento").value,
        observacoes: document.getElementById("observacoesOS").value.trim(),
        // Na edição, preserva os materiais que já existem na OS
        materiais: modoEdicaoOS && ordemSelecionadaOS?.materiais
            ? ordemSelecionadaOS.materiais.map(function (m) {
                return {
                    materialId: m.materialId,
                    quantidade: m.quantidade,
                    valorUnitario: m.valorUnitario
                };
            })
            : []
    };
}

function validarOS(ordem) {
    if (!ordem.numeroOS) {
        notificar("Informe o número da OS.");
        return false;
    }
    if (!ordem.cliente) {
        notificar("Informe o cliente.");
        return false;
    }
    if (!ordem.descricaoServico) {
        notificar("Informe a descrição do serviço.");
        return false;
    }
    if (!ordem.tipoServico) {
        notificar("Selecione o tipo de serviço.");
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
            notificar(
                editando
                    ? `Ordem de serviço ${ordemSalva.numeroOS || ordem.numeroOS} atualizada com sucesso!`
                    : `Ordem de serviço ${ordemSalva.numeroOS || ordem.numeroOS} criada com sucesso!`
            );
        }
    } catch (erro) {
        console.error("Erro ao salvar ordem de serviço:", erro);
        notificar(`Não foi possível salvar a ordem de serviço.\n\n${erro.message}`);
    } finally {
        if (salvarOS) salvarOS.disabled = false;
        if (salvarImprimirOS) salvarImprimirOS.disabled = false;
    }
}

function imprimirOS(ordem) {
    const janela = window.open("", "_blank");
    if (!janela) {
        notificar("O navegador bloqueou a janela de impressão. Libere os pop-ups para este site.");
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
    ${linha("Materiais", formatarMoedaOS(obterValorMateriaisOS(ordem)))}
    ${linha("Desconto", formatarMoedaOS(obterDescontoOS(ordem)))}
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
    console.log("[OS] Editando:", o);
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
    selecionarOpcaoOS("tipoServico", o.tipoServico);
    document.getElementById("responsavelOS").value = o.funcionarioId ?? o.funcionario?.id ?? "";
    document.getElementById("valorMaoObra").value = o.valorMaoObra ?? "0.00";
    document.getElementById("descontoOS").value = obterDescontoOS(o).toFixed(2);
    selecionarOpcaoOS("condicaoPagamento", o.condicaoPagamento);
    document.getElementById("observacoesOS").value = o.observacoes || "";

    if (salvarOS) {
        salvarOS.textContent = "SALVAR ALTERAÇÕES";
    }
    if (tituloModalOS) {
        tituloModalOS.textContent = "EDITAR ORDEM DE SERVIÇO";
    }
    document.getElementById("valorMateriais").value = obterValorMateriaisOS(o).toFixed(2);
    atualizarTotalOS();
    fecharModalDetalhesOS();
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
        fecharModalDetalhesOS();
        ordemSelecionadaOS = null;
        await carregarOrdensServico();
        notificar("Ordem de serviço removida com sucesso!");
    } catch (erro) {
        console.error("Erro ao remover ordem de serviço:", erro);
        notificar(`Não foi possível remover a ordem de serviço.\n\n${erro.message}`);
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
// TOTAL EM TEMPO REAL (materiais + mão de obra - desconto)
// =========================================================

function atualizarTotalOS() {
    const total =
        (Number(document.getElementById("valorMateriais").value) || 0) +
        (Number(document.getElementById("valorMaoObra").value) || 0) -
        (Number(document.getElementById("descontoOS").value) || 0);
    definirTexto("valorTotalOS", formatarMoedaOS(total));
}

["valorMateriais", "valorMaoObra", "descontoOS"].forEach(function (id) {
    const campo = document.getElementById(id);
    if (campo) {
        campo.addEventListener("input", atualizarTotalOS);
    }
});

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
        "modalDetalhesOS", "fecharDetalhesOS", "fecharDetalhesOSBotao", "editarOS", "removerOS",
        "tituloModalOS",
        "ordensServicoLista", "ordensServicoEmpty",
        "numeroOS", "clienteOS", "contatoOS", "enderecoOS", "dataOS",
        "dataEntregaOS", "descricaoOS", "tipoServico", "responsavelOS",
        "valorMaoObra", "valorMateriais", "descontoOS", "condicaoPagamento",
        "observacoesOS", "valorTotalOS"
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
                        size: 13
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
                        size: 13
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
let intervaloProgressoLista = null;

const VELOCIDADE_SIMULACAO_LISTA = 0.15;

// CARREGAR MÁQUINAS

async function carregarMaquinas() {
    const container = document.getElementById("maquinasGrid");
    try {
        const resposta = await fetch(`${API_URL}/Maquinas`);
        if (!resposta.ok) {
            throw new Error(`Erro HTTP: ${resposta.status}`);
        }
        maquinas = await resposta.json();
        await carregarProcessosAtivos();
        renderizarMaquinas();
        iniciarProgressoListaMaquinas();
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
                        <span
                        data-producao-maquina="${maquina.id}">
                        ${Math.round(Number(processo.quantidadeProduzida) || 0)}
                        /
                        ${Math.round(Number(processo.quantidadePlanejada) || 0)}
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
function atualizarContadoresListaMaquinas() {
    processosAtivos.forEach(processo => {
        if (processo.status !== "EM_EXECUCAO") {
            return;
        }
        const maquinaId = processo.maquinaId;
        const produzida =
            Number(processo.quantidadeProduzida) || 0;
        const planejada =
            Number(processo.quantidadePlanejada) || 0;
        const producaoPorMinuto =
            Number(processo.producaoPorMinuto) || 0;
        if (
            planejada <= 0 ||
            producaoPorMinuto <= 0 ||
            produzida >= planejada
        ) {
            return;
        }
        processo.quantidadeProduzida = Math.min(
            produzida +
            producaoPorMinuto * VELOCIDADE_SIMULACAO_LISTA,
            planejada
        );
        const contador = document.querySelector(
            `[data-producao-maquina="${maquinaId}"]`
        );
        if (contador) {
            contador.textContent =
                `${Math.round(processo.quantidadeProduzida)} / ${Math.round(planejada)}`;
        }
    });
}
function iniciarProgressoListaMaquinas() {
    if (intervaloProgressoLista) {
        clearInterval(intervaloProgressoLista);
    }
    intervaloProgressoLista = setInterval(() => {
        atualizarContadoresListaMaquinas();
    }, 1000);
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
        notificar("Não foi possível carregar os detalhes da máquina.");
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
const cadastrarMaterial = document.getElementById("cadastrarMaterial");

let materialEmEdicaoId = null;

const tituloModalMaterial = document.getElementById("tituloModalMaterial");
const subtituloModalMaterial = document.getElementById("subtituloModalMaterial");

function limparFormularioMaterial() {
    document
        .querySelectorAll("#modalNovoMaterial option[data-temporaria]")
        .forEach(o => o.remove());
    document.getElementById("materialCodigo").value = "";
    document.getElementById("materialNome").value = "";
    document.getElementById("materialCategoria").value = "";
    document.getElementById("materialUnidade").value = "";
    document.getElementById("materialEstoqueMinimo").value = "";
    document.getElementById("materialDescricao").value = "";
}

function normalizarTexto(texto) {
    return (texto ?? "")
        .toString()
        .trim()
        .normalize("NFKD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLowerCase();
}

function selecionarOpcao(select, valor) {
    const alvo = normalizarTexto(valor);
    const opcao = Array.from(select.options).find(
        o => o.value && normalizarTexto(o.value) === alvo
    );
    if (opcao) {
        select.value = opcao.value;
        return;
    }
    if (valor) {
        const extra = new Option(valor, valor);
        extra.dataset.temporaria = "true";
        select.add(extra);
        select.value = valor;
    } else {
        select.value = "";
    }
}

function abrirModalNovoMaterial() {
    materialEmEdicaoId = null;
    limparFormularioMaterial();
    tituloModalMaterial.textContent = "NOVO MATERIAL";
    subtituloModalMaterial.textContent = "Cadastro de material industrial";
    cadastrarMaterial.textContent = "CADASTRAR";
    modalNovoMaterial.classList.add("active");
}

function fecharModalNovoMaterial() {
    modalNovoMaterial.classList.remove("active");
    materialEmEdicaoId = null;
}

function editarMaterial(id) {
    const material = materiais.find(item => item.id === id);
    if (!material) {
        notificar("Não foi possível localizar o material.");
        return;
    }
    materialEmEdicaoId = material.id;
    limparFormularioMaterial();
    tituloModalMaterial.textContent = "EDITAR MATERIAL";
    subtituloModalMaterial.textContent = "Atualização de material industrial";
    cadastrarMaterial.textContent = "SALVAR ALTERAÇÕES";

    document.getElementById("materialCodigo").value = material.codigo || "";
    document.getElementById("materialNome").value = material.nome || "";
    selecionarOpcao(document.getElementById("materialCategoria"), material.categoria);
    selecionarOpcao(document.getElementById("materialUnidade"), material.unidade);
    document.getElementById("materialEstoqueMinimo").value = material.estoqueMinimo ?? "";
    document.getElementById("materialDescricao").value = material.descricao || "";

    modalNovoMaterial.classList.add("active");
}

async function salvarNovoMaterial() {
    const codigo = document.getElementById("materialCodigo").value.trim();
    const nome = document.getElementById("materialNome").value.trim();
    const categoria = document.getElementById("materialCategoria").value;
    const unidade = document.getElementById("materialUnidade").value;
    const descricao = document.getElementById("materialDescricao").value.trim();
    const estoqueMinimoTexto = document.getElementById("materialEstoqueMinimo").value;
    const estoqueMinimo = estoqueMinimoTexto === "" ? 0 : Number(estoqueMinimoTexto);

    if (!Number.isFinite(estoqueMinimo) || estoqueMinimo < 0) {
        notificar("Informe um estoque mínimo válido (zero ou maior).");
        document.getElementById("materialEstoqueMinimo").focus();
        return;
    }
    if (!codigo || !nome || !categoria || !unidade) {
        notificar("Preencha todos os campos obrigatórios.");
        return;
    }

    const editando = materialEmEdicaoId !== null;
    const materialAtual = editando
        ? materiais.find(item => item.id === materialEmEdicaoId)
        : null;

    const mesmoValor = (novo, antigo) =>
        normalizarTexto(novo) === normalizarTexto(antigo);

    const material = {
        codigo,
        nome,
        categoria: materialAtual && mesmoValor(categoria, materialAtual.categoria)
            ? materialAtual.categoria
            : categoria,
        unidade: materialAtual && mesmoValor(unidade, materialAtual.unidade)
            ? materialAtual.unidade
            : unidade,
        descricao: descricao || null,
        estoqueMinimo: estoqueMinimo
    };

    const url = editando
        ? `${API_URL}/materiais/${materialEmEdicaoId}`
        : `${API_URL}/materiais`;

    try {
        cadastrarMaterial.disabled = true;
        cadastrarMaterial.textContent = editando ? "SALVANDO..." : "CADASTRANDO...";
        const resposta = await fetch(url, {
            method: editando ? "PUT" : "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(material)
        });
        if (!resposta.ok) {
            const erro = await resposta.text();
            throw new Error(erro || `Erro HTTP: ${resposta.status}`);
        }
        fecharModalNovoMaterial();
        await carregarMateriais();
        notificar(editando
            ? "Material atualizado com sucesso!"
            : "Material cadastrado com sucesso!");
    } catch (erro) {
        console.error("Erro ao salvar material:", erro);
        notificar(`Não foi possível salvar o material.\n\n${erro.message}`);
    } finally {
        cadastrarMaterial.disabled = false;
        cadastrarMaterial.textContent =
            materialEmEdicaoId !== null ? "SALVAR ALTERAÇÕES" : "CADASTRAR";
    }
}
if (abrirModalMaterial) {
    abrirModalMaterial.addEventListener(
        "click",
        abrirModalNovoMaterial
    );
}
if (fecharModalMaterial) {
    fecharModalMaterial.addEventListener(
        "click",
        fecharModalNovoMaterial
    );
}
if (cadastrarMaterial) {
    cadastrarMaterial.addEventListener("click", salvarNovoMaterial);
}
if (cancelarMaterial) {
    cancelarMaterial.addEventListener(
        "click",
        fecharModalNovoMaterial
    );
}
modalNovoMaterial.addEventListener("click", (event) => {
    if (event.target === modalNovoMaterial) {
        fecharModalNovoMaterial();
    }
});

let materiais = [];

async function carregarMateriais() {
    const lista = document.getElementById("materiaisLista");
    if (!lista) {
        console.warn("[MATERIAIS] Elemento #materiaisLista não encontrado.");
        return;
    }
    try {
        lista.innerHTML = `
            <div class="materiais-loading">
                <span>CARREGANDO MATERIAIS...</span>
            </div>
        `;
        const response = await fetch(`${API_URL}/materiais`);
        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }
        materiais = await response.json();
        console.log("[MATERIAIS] Dados recebidos:", materiais);
        renderizarMateriais();
    } catch (error) {
        console.error("[MATERIAIS] Erro ao carregar:", error);
        lista.innerHTML = `
            <div class="materiais-empty">
                <h3>Não foi possível carregar os materiais.</h3>
                <p>Verifique se a API do BlackForge está funcionando.</p>
            </div>
        `;
    }
}

// RENDERIZAR MATERIAIS

function renderizarMateriais() {
    const lista = document.getElementById("materiaisLista");
    if (!lista) {
        return;
    }
    if (!materiais || materiais.length === 0) {
        lista.innerHTML = `
            <div class="materiais-empty">
                <h3>
                    O estoque ainda não possui materiais registrados.
                </h3>
                <p>
                    Cadastre um material para começar o controle do estoque.
                </p>
                <button
                    type="button"
                    class="secondary-button"
                    id="cadastrarMaterialVazio">
                    CADASTRAR MATERIAL
                </button>
            </div>
        `;
        const botao = document.getElementById("cadastrarMaterialVazio");
        if (botao) {
            botao.addEventListener("click", abrirModalNovoMaterial);
        }
        return;
    }
    lista.innerHTML = materiais.map(function (material) {
        const estoqueStatus = material.abaixoDoMinimo
            ? "ABAIXO DO MÍNIMO"
            : "ESTOQUE NORMAL";
        return `
            <div class="material-card">
                <div class="material-card-header">
                    <div>
                        <span class="material-card-codigo">
                            ${material.codigo}
                        </span>
                        <h3>
                            ${material.nome}
                        </h3>
                    </div>
                    <span class="material-card-status ${material.abaixoDoMinimo ? "status-alerta" : "status-ok"}">
                        ${estoqueStatus}
                    </span>
                </div>
                <div class="material-card-info">
                    <div>
                        <span>CATEGORIA</span>
                        <strong>
                            ${material.categoria || "-"}
                        </strong>
                    </div>
                    <div>
                        <span>UNIDADE</span>
                        <strong>
                            ${material.unidade || "-"}
                        </strong>
                    </div>
                    <div>
                        <span>ESTOQUE</span>
                        <strong>
                            ${material.quantidadeTotal} ${material.unidade || ""}
                        </strong>
                    </div>
                    <div>
                        <span>ESTOQUE MÍNIMO</span>
                        <strong>
                            ${material.estoqueMinimo ?? 0} ${material.unidade || ""}
                        </strong>
                    </div>
                    <div>
                        <span>CUSTO MÉDIO</span>
                        <strong>
                            ${formatarMoedaMaterial(material.custoMedio)}
                        </strong>
                    </div>
                </div>
                ${material.descricao
                ? `
                        <div class="material-card-descricao">
                            ${material.descricao}
                        </div>
                    `
                : ""
            }
                <div class="material-card-acoes">
                <button
                type="button"
                class="btn-editar-material"
                data-material-id="${material.id}">
                <ion-icon name="create-outline"></ion-icon>
                EDITAR
                </button>
                    <button
                        type="button"
                        class="btn-excluir-material"
                        data-material-id="${material.id}"
                        data-material-nome="${material.nome}">
                        <ion-icon name="trash-outline"></ion-icon>
                        EXCLUIR
                    </button>
                </div>
            </div>
        `;
    }).join("");
    document.querySelectorAll(".btn-excluir-material")
        .forEach(function (botao) {
            botao.addEventListener("click", function () {
                const materialId = Number(botao.dataset.materialId);
                const materialNome = botao.dataset.materialNome;
                excluirMaterial(materialId, materialNome, botao);
            });
        });
    document.querySelectorAll(".btn-editar-material")
        .forEach(function (botao) {
            botao.addEventListener("click", function () {
                editarMaterial(Number(botao.dataset.materialId));
            });
        });
}

async function excluirMaterial(id, nome, botao) {
    const confirmado = await confirmarExclusaoMaterial(nome);
    if (!confirmado) {
        return;
    }
    try {
        botao.disabled = true;
        botao.textContent = "EXCLUINDO...";
        const resposta = await fetch(
            `${API_URL}/materiais/${id}`,
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
        console.log(`[MATERIAIS] Material ${id} excluído.`);
        await carregarMateriais();
        notificar(`Material "${nome}" excluído com sucesso!`);
    } catch (erro) {
        console.error("[MATERIAIS] Erro ao excluir:", erro);
        notificar(
            `Não foi possível excluir o material.\n\n${erro.message}`
        );
        botao.disabled = false;
        botao.innerHTML = `
            <ion-icon name="trash-outline"></ion-icon>
            EXCLUIR
        `;
    }
}

// FORMATAR MOEDA

function formatarMoedaMaterial(valor) {

    return Number(valor || 0).toLocaleString("pt-BR", {
        style: "currency",
        currency: "BRL"
    });

}

// NOVO LOTE

let lotes = [];
let loteEmEdicaoId = null;

const tituloModalLote = document.getElementById("tituloModalLote");
const modalNovoLote = document.getElementById("modalNovoLote");
const abrirModalLote = document.getElementById("abrirModalLote");
const cadastrarLoteVazio = document.getElementById("cadastrarLoteVazio");
const fecharModalLote = document.getElementById("fecharModalLote");
const cancelarLote = document.getElementById("cancelarLote");
const cadastrarLote = document.getElementById("cadastrarLote");

const buscarLote = document.getElementById("buscarLote");
const filtroLote = document.getElementById("filtroLote");

const loteCodigo = document.getElementById("loteCodigo");
const loteMaterial = document.getElementById("loteMaterial");
const loteQuantidade = document.getElementById("loteQuantidade");
const loteCustoUnitario = document.getElementById("loteCustoUnitario");
const loteDataFabricacao = document.getElementById("loteDataFabricacao");
const loteDataValidade = document.getElementById("loteDataValidade");
const loteObservacoes = document.getElementById("loteObservacoes");

// ABRIR MODAL

function abrirModalNovoLote() {
    if (!modalNovoLote) return;
    loteEmEdicaoId = null;
    modalNovoLote.classList.add("active");
    carregarMateriaisSelectLote();
    tituloModalLote.textContent = "NOVO LOTE";
    cadastrarLote.textContent = "CADASTRAR";
    loteMaterial.disabled = false;
    loteQuantidade.disabled = false;
    loteCodigo.value = "";
    loteMaterial.value = "";
    loteQuantidade.value = "";
    loteCustoUnitario.value = "";
    loteDataFabricacao.value = "";
    loteDataValidade.value = "";
    loteObservacoes.value = "";
}

// FECHAR MODAL

function fecharModalNovoLote() {
    if (!modalNovoLote) return;

    modalNovoLote.classList.remove("active");

    loteEmEdicaoId = null;

    loteMaterial.disabled = false;
    loteQuantidade.disabled = false;

    cadastrarLote.textContent = "CADASTRAR";
    tituloModalLote.textContent = "NOVO LOTE";
}

// CARREGAR MATERIAIS NO SELECT

function carregarMateriaisSelectLote() {
    if (!loteMaterial) return;

    loteMaterial.innerHTML = `
        <option value="">Selecione um material</option>
    `;

    materiais.forEach(material => {
        const option = document.createElement("option");

        option.value = material.id;
        option.textContent =
            `${material.codigo} - ${material.nome}`;

        loteMaterial.appendChild(option);
    });
}

// CARREGAR LOTES DA API

async function carregarLotes() {
    const lista = document.getElementById("listaLotes");

    if (!lista) return;

    try {
        lista.innerHTML = `
            <tr>
                <td colspan="5" style="text-align:center;">
                    CARREGANDO LOTES...
                </td>
            </tr>
        `;

        const resposta = await fetch(`${API_URL}/lotes`);

        if (!resposta.ok) {
            throw new Error(`Erro HTTP: ${resposta.status}`);
        }

        lotes = await resposta.json();

        console.log("[LOTES] Dados recebidos:", lotes);

        renderizarLotes();

    } catch (erro) {
        console.error("[LOTES] Erro ao carregar:", erro);

        lista.innerHTML = `
            <tr>
                <td colspan="5" style="text-align:center;">
                    Não foi possível carregar os lotes.
                </td>
            </tr>
        `;
    }
}

// FORMATAR DATA

function formatarDataLote(data) {
    if (!data) return "-";

    const dataLocal = new Date(data);

    if (Number.isNaN(dataLocal.getTime())) {
        return "-";
    }

    return dataLocal.toLocaleDateString("pt-BR", {
        timeZone: "UTC"
    });
}

// RENDERIZAR TABELA

function renderizarLotes() {
    const lista = document.getElementById("listaLotes");
    if (!lista) return;
    const termo = (buscarLote?.value || "")
        .trim()
        .toLowerCase();
    const filtro = filtroLote?.value || "todos";

    // INDICADORES GERAIS

    document.getElementById("totalLotes").textContent =
        lotes.length;
    document.getElementById("lotesProximos").textContent =
        lotes.filter(lote => lote.status === "proximo").length;
    document.getElementById("lotesVencidos").textContent =
        lotes.filter(lote => lote.status === "vencido").length;

    // PESQUISA E FILTRO

    const lotesFiltrados = lotes.filter(lote => {
        const correspondeBusca =
            (lote.codigo || "").toLowerCase().includes(termo) ||
            (lote.materialNome || "").toLowerCase().includes(termo) ||
            (lote.materialCodigo || "").toLowerCase().includes(termo);
        let correspondeFiltro = true;
        if (filtro === "normal") {
            correspondeFiltro =
                lote.status === "ok" ||
                lote.status === "sem-validade";
        }
        if (filtro === "proximo") {
            correspondeFiltro = lote.status === "proximo";
        }
        if (filtro === "vencido") {
            correspondeFiltro = lote.status === "vencido";
        }
        return correspondeBusca && correspondeFiltro;
    });

    // EVENTOS DO MODAL DE LOTE

    if (abrirModalLote) {
        abrirModalLote.addEventListener("click", abrirModalNovoLote);
    }
    if (fecharModalLote) {
        fecharModalLote.addEventListener("click", fecharModalNovoLote);
    }
    if (cancelarLote) {
        cancelarLote.addEventListener("click", fecharModalNovoLote);
    }
    if (cadastrarLote) {
        cadastrarLote.addEventListener("click", salvarNovoLote);
    }

    if (lotesFiltrados.length === 0) {
        lista.innerHTML = `
            <tr class="lotes-empty">
                <td colspan="6">
                    <div class="empty-state">
                        <ion-icon name="cube-outline"></ion-icon>
                        <h3>Nenhum lote encontrado</h3>
                    </div>
                </td>
            </tr>
        `;
        return;
    }
    lista.innerHTML = lotesFiltrados.map(lote => {
        let statusTexto = "DENTRO DA VALIDADE";
        if (lote.status === "proximo") {
            statusTexto = "PRÓXIMO DO VENCIMENTO";
        } else if (lote.status === "vencido") {
            statusTexto = "VENCIDO";
        } else if (lote.status === "sem-validade") {
            statusTexto = "SEM VALIDADE";
        }
        return `
            <tr>
                <td>
                    <strong>${lote.codigo}</strong>
                </td>
                <td>
                    <div>
                        <strong>${lote.materialNome}</strong>
                        <small>${lote.materialCodigo}</small>
                    </div>
                </td>
                <td>
                    ${Number(lote.quantidade).toLocaleString("pt-BR", {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        })} ${lote.unidade || ""}
                </td>
                <td>
                    ${formatarDataLote(lote.dataFabricacao)}
                </td>
                <td>
                    <div>
                        ${formatarDataLote(lote.dataValidade)}
                        <small>${statusTexto}</small>
                    </div>
                </td>
                <td class="lote-acoes">
                    <button
                        type="button"
                        class="btn-editar-lote"
                        data-lote-id="${lote.id}"
                        title="Editar lote"
                        aria-label="Editar lote">
                        <ion-icon name="create-outline"></ion-icon>
                    </button>
                    <button
                        type="button"
                        class="btn-excluir-lote"
                        data-lote-id="${lote.id}"
                        title="Excluir lote"
                        aria-label="Excluir lote">
                        <ion-icon name="trash-outline"></ion-icon>
                    </button>
                </td>
            </tr>
        `;
    }).join("");
}


// AÇÕES DA TABELA DE LOTES

const listaLotes = document.getElementById("listaLotes");
if (listaLotes) {
    listaLotes.addEventListener("click", function (event) {
        const botao = event.target.closest("button[data-lote-id]");
        if (!botao) return;
        const loteId = Number(botao.dataset.loteId);
        if (botao.classList.contains("btn-editar-lote")) {
            editarLote(loteId);
            return;
        }
        if (botao.classList.contains("btn-excluir-lote")) {
            excluirLote(loteId, botao);
        }
        if (event.target.closest("#cadastrarLoteVazio")) {
            abrirModalNovoLote();
            return;
        }
    });
}

// EDITAR LOTE

function editarLote(id) {
    const lote = lotes.find(item => item.id === id);
    if (!lote) {
        notificar("Não foi possível localizar o lote.");
        return;
    }
    loteEmEdicaoId = lote.id;
    carregarMateriaisSelectLote();
    tituloModalLote.textContent = "EDITAR LOTE";
    cadastrarLote.textContent = "SALVAR ALTERAÇÕES";
    loteCodigo.value = lote.codigo || "";
    loteMaterial.value = lote.materialId;
    loteQuantidade.value = lote.quantidade ?? "";
    loteCustoUnitario.value = lote.custoUnitario ?? "";
    loteDataFabricacao.value =
        lote.dataFabricacao
            ? lote.dataFabricacao.slice(0, 10)
            : "";
    loteDataValidade.value =
        lote.dataValidade
            ? lote.dataValidade.slice(0, 10)
            : "";
    loteObservacoes.value = lote.observacoes || "";
    loteMaterial.disabled = true;
    loteQuantidade.disabled = true;
    modalNovoLote.classList.add("active");
}

// EXCLUIR LOTE

async function excluirLote(id, botao) {
    const lote = lotes.find(item => item.id === id);
    if (!lote) {
        notificar("Não foi possível localizar o lote.");
        return;
    }
    const confirmado = await confirmarExclusaoLote(lote);
    if (!confirmado) {
        return;
    }
    try {
        botao.disabled = true;
        const resposta = await fetch(
            `${API_URL}/lotes/${id}`,
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
        console.log(`[LOTES] Lote ${id} excluído com sucesso.`);
        await carregarLotes();
        await carregarMateriais();
        notificar(`Lote "${lote.codigo}" excluído com sucesso!`);
    } catch (erro) {
        console.error("[LOTES] Erro ao excluir:", erro);
        notificar(
            `Não foi possível excluir o lote.\n\n${erro.message}`
        );
        botao.disabled = false;
    }
}

// CADASTRAR LOTE

async function salvarNovoLote() {
    const materialId = Number(loteMaterial.value);
    const quantidade = Number(loteQuantidade.value);
    const custoUnitario = Number(loteCustoUnitario.value);
    if (!loteCodigo.value.trim()) {
        notificar("Informe o código do lote.");
        loteCodigo.focus();
        return;
    }
    if (!loteEmEdicaoId && !materialId) {
        notificar("Selecione um material.");
        loteMaterial.focus();
        return;
    }
    if (
        !loteEmEdicaoId &&
        (!Number.isFinite(quantidade) || quantidade <= 0)
    ) {
        notificar("Informe uma quantidade maior que zero.");
        loteQuantidade.focus();
        return;
    }
    if (!Number.isFinite(custoUnitario) || custoUnitario < 0) {
        notificar("Informe um custo unitário válido.");
        loteCustoUnitario.focus();
        return;
    }
    const dataFabricacao = loteDataFabricacao.value || null;
    const dataValidade = loteDataValidade.value || null;
    if (
        dataFabricacao &&
        dataValidade &&
        dataValidade < dataFabricacao
    ) {
        notificar("A validade não pode ser anterior à fabricação.");
        return;
    }
    const editando = loteEmEdicaoId !== null;
    const observacoes = loteObservacoes.value.trim();
    const dadosLote = editando
        ? {
            codigo: loteCodigo.value.trim(),
            custoUnitario: custoUnitario,
            dataFabricacao: dataFabricacao,
            dataValidade: dataValidade,
            observacoes: observacoes
        }
        : {
            materialId: materialId,
            codigo: loteCodigo.value.trim(),
            quantidade: quantidade,
            custoUnitario: custoUnitario,
            dataFabricacao: dataFabricacao,
            dataValidade: dataValidade,
            observacoes: observacoes
        };
    const url = editando
        ? `${API_URL}/lotes/${loteEmEdicaoId}`
        : `${API_URL}/lotes`;
    try {
        cadastrarLote.disabled = true;
        cadastrarLote.textContent = editando
            ? "SALVANDO..."
            : "CADASTRANDO...";
        const resposta = await fetch(url, {
            method: editando ? "PUT" : "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(dadosLote)
        });
        if (!resposta.ok) {
            const mensagem = await resposta.text();
            throw new Error(
                mensagem || `Erro HTTP: ${resposta.status}`
            );
        }
        console.log(
            editando
                ? "[LOTES] Lote atualizado:"
                : "[LOTES] Lote cadastrado:",
            dadosLote
        );
        fecharModalNovoLote();
        await carregarLotes();
        await carregarMateriais();
        notificar(
            editando
                ? "Lote atualizado com sucesso!"
                : "Lote cadastrado com sucesso!"
        );
    } catch (erro) {
        console.error("[LOTES] Erro ao salvar:", erro);
        notificar(
            `Não foi possível salvar o lote.\n\n${erro.message}`
        );
    } finally {
        cadastrarLote.disabled = false;
        cadastrarLote.textContent = "CADASTRAR";
    }
}

// PESQUISA E FILTRO

if (buscarLote) {
    buscarLote.addEventListener("input", renderizarLotes);
}
if (filtroLote) {
    filtroLote.addEventListener("change", renderizarLotes);
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
        notificar("Informe o nome do funcionário.");
        return;
    }
    if (!funcionario.matricula) {
        notificar("Informe a matrícula do funcionário.");
        return;
    }
    if (!funcionario.cpf) {
        notificar("Informe o CPF do funcionário.");
        return;
    }
    if (!funcionario.cargo) {
        notificar("Informe o cargo do funcionário.");
        return;
    }
    if (!funcionario.idade) {
        notificar("Informe a idade do funcionário.");
        return;
    }
    if (!funcionario.setor) {
        notificar("Selecione o setor do funcionário.");
        return;
    }
    if (!funcionario.admissao) {
        notificar("Informe a data de admissão.");
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
            notificar(
                "Funcionário atualizado com sucesso!"
            );
        } else {
            notificar(
                "Funcionário cadastrado com sucesso!"
            );
        }
    }
    catch (erro) {
        console.error(
            "Erro ao salvar funcionário:",
            erro
        );
        notificar(
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

        notificar("Funcionário removido com sucesso!");
    } catch (erro) {
        console.error(
            "Erro ao remover funcionário:",
            erro
        );

        notificar(
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

// INICIALIZAÇÃO

carregarFuncionarios();
carregarMaquinas();
carregarOrdensServico();
carregarMateriais();
carregarLotes();

// RELATÓRIO DE SERVIÇOS

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


// VARIÁVEIS DO RELATÓRIO

let graficoServicosStatus = null;
let graficoServicosTipo = null;


// ELEMENTOS DO RELATÓRIO

const secaoRelatorioServicos =
    document.getElementById("relatorio-servicos");

const indicadoresServicos =
    secaoRelatorioServicos
        ? secaoRelatorioServicos.querySelectorAll(".relatorio-indicador strong")
        : [];

const graficosServicos =
    secaoRelatorioServicos
        ? secaoRelatorioServicos.querySelectorAll(".relatorio-grafico")
        : [];

const tabelaRelatorioServicos =
    document.getElementById("relatorio-servicosTabela");

const registrosRelatorioServicos =
    document.getElementById("relatorio-servicosRegistros");


// FORMATADORES DO RELATÓRIO

function formatarMoedaRelatorioServicos(valor) {
    return new Intl.NumberFormat("pt-BR", {
        style: "currency",
        currency: "BRL"
    }).format(Number(valor) || 0);
}


function formatarDataRelatorioServicos(valor) {
    if (!valor) {
        return "-";
    }

    const texto = String(valor);

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


function obterNomeStatusRelatorioServicos(status) {
    const nomes = {
        pendente: "Pendente",
        em_execucao: "Em execução",
        concluida: "Concluída"
    };

    return nomes[status] || status || "-";
}


function obterNomeTipoRelatorioServicos(tipo) {
    if (!tipo) {
        return "-";
    }

    const nomes = {
        usinagem: "Usinagem",
        torneamento: "Torneamento",
        fresagem: "Fresagem",
        corte: "Corte",
        solda: "Solda",
        outro: "Outro"
    };

    return nomes[String(tipo).toLowerCase()] || formatarTextoLivreOS(tipo);
}


function obterClasseStatusRelatorioServicos(status) {
    const classes = {
        pendente: "pendente",
        em_execucao: "em-execucao",
        concluida: "concluida"
    };

    return classes[status] || "pendente";
}


// LIMPAR GRÁFICOS

function limparGraficosRelatorioServicos() {

    if (graficoServicosStatus) {
        graficoServicosStatus.destroy();
        graficoServicosStatus = null;
    }

    if (graficoServicosTipo) {
        graficoServicosTipo.destroy();
        graficoServicosTipo = null;
    }

    graficosServicos.forEach(function (container) {

        container.innerHTML = `
            <div class="relatorio-sem-dados">
                <ion-icon name="bar-chart-outline"></ion-icon>
                <span>SEM DADOS PARA EXIBIR</span>
                <small>
                    Gere o relatório para visualizar os dados.
                </small>
            </div>
        `;

    });
}


// LIMPAR FILTROS

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

    if (indicadoresServicos.length >= 4) {

        indicadoresServicos[0].textContent = "0";
        indicadoresServicos[1].textContent = "0";
        indicadoresServicos[2].textContent = "0";
        indicadoresServicos[3].textContent = "R$ 0,00";

    }

    if (registrosRelatorioServicos) {
        registrosRelatorioServicos.textContent = "0 REGISTROS";
    }

    if (tabelaRelatorioServicos) {

        tabelaRelatorioServicos.innerHTML = `
            <tr>
                <td class="relatorio-tabela-vazia" colspan="7">
                    <ion-icon name="document-text-outline"></ion-icon>
                    <span>NENHUM REGISTRO ENCONTRADO</span>
                    <small>
                        Defina os filtros e clique em GERAR RELATÓRIO.
                    </small>
                </td>
            </tr>
        `;

    }

    limparGraficosRelatorioServicos();
}


// RENDERIZAR INDICADORES

function renderizarIndicadoresRelatorioServicos(dados) {

    if (indicadoresServicos.length < 4) {
        console.warn(
            "[RELATÓRIO SERVIÇOS] Indicadores não encontrados corretamente."
        );
        return;
    }

    indicadoresServicos[0].textContent =
        dados.totalOS ?? 0;

    indicadoresServicos[1].textContent =
        dados.emExecucao ?? 0;

    indicadoresServicos[2].textContent =
        dados.concluidas ?? 0;

    indicadoresServicos[3].textContent =
        formatarMoedaRelatorioServicos(dados.valorTotal);

}


// RENDERIZAR TABELA

function renderizarTabelaRelatorioServicos(registros) {

    if (!tabelaRelatorioServicos) {
        return;
    }

    if (!Array.isArray(registros) || registros.length === 0) {

        tabelaRelatorioServicos.innerHTML = `
            <tr>
                <td class="relatorio-tabela-vazia" colspan="7">
                    <ion-icon name="document-text-outline"></ion-icon>
                    <span>NENHUM REGISTRO ENCONTRADO</span>
                    <small>
                        Nenhuma ordem de serviço corresponde aos filtros.
                    </small>
                </td>
            </tr>
        `;

        return;
    }

    tabelaRelatorioServicos.innerHTML = "";

    registros.forEach(function (registro) {

        const linha = document.createElement("tr");

        const statusTexto =
            obterNomeStatusRelatorioServicos(registro.status);

        const statusClasse =
            obterClasseStatusRelatorioServicos(registro.status);

        linha.innerHTML = `
            <td>
                <strong>
                    ${escaparHtmlOS(registro.numeroOS || "-")}
                </strong>
            </td>

            <td>
                ${escaparHtmlOS(registro.cliente || "-")}
            </td>

            <td>
                ${escaparHtmlOS(
                    obterNomeTipoRelatorioServicos(registro.tipoServico)
                )}
            </td>

            <td>
                ${escaparHtmlOS(registro.responsavel || "Não definido")}
            </td>

            <td>
                ${formatarDataRelatorioServicos(registro.dataEntrega)}
            </td>

            <td>
                <span class="relatorio-status ${statusClasse}">
                    ${escaparHtmlOS(statusTexto)}
                </span>
            </td>

            <td>
                <strong>
                    ${formatarMoedaRelatorioServicos(registro.valorTotal)}
                </strong>
            </td>
        `;

        tabelaRelatorioServicos.appendChild(linha);

    });

}


// CRIAR GRÁFICO DE STATUS

function renderizarGraficoStatusRelatorioServicos(dados) {

    if (!graficosServicos[0]) {
        return;
    }

    if (graficoServicosStatus) {
        graficoServicosStatus.destroy();
        graficoServicosStatus = null;
    }

    const canvas = document.createElement("canvas");

    graficosServicos[0].innerHTML = "";

    graficosServicos[0].appendChild(canvas);

    const statusDados =
        Array.isArray(dados.porStatus)
            ? dados.porStatus
            : [];

    const labels =
        statusDados.map(item => item.status);

    const valores =
        statusDados.map(item => Number(item.quantidade) || 0);

    graficoServicosStatus = new Chart(canvas, {

        type: "doughnut",

        data: {
            labels: labels,

            datasets: [
                {
                    data: valores,

                    backgroundColor: [
                        "#f5c400",
                        "#1688ff",
                        "#00a651"
                    ],

                    borderWidth: 0
                }
            ]
        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            cutout: "68%",

            plugins: {

                legend: {
                    position: "bottom",

                    labels: {
                        color: "#9aa4aa",

                        font: {
                            family: "'Share Tech Mono', monospace",
                            size: 13
                        },

                        padding: 18
                    }
                }

            }

        }

    });

}

// CRIAR GRÁFICO DE TIPOS DE SERVIÇO

function renderizarGraficoTipoRelatorioServicos(dados) {

    if (!graficosServicos[1]) {
        return;
    }

    if (graficoServicosTipo) {
        graficoServicosTipo.destroy();
        graficoServicosTipo = null;
    }

    const canvas = document.createElement("canvas");

    graficosServicos[1].innerHTML = "";

    graficosServicos[1].appendChild(canvas);

    const tipoDados =
        Array.isArray(dados.porTipo)
            ? dados.porTipo
            : [];

    const labels =
        tipoDados.map(item =>
            obterNomeTipoRelatorioServicos(item.tipoServico)
        );

    const valores =
        tipoDados.map(item =>
            Number(item.quantidade) || 0
        );

    graficoServicosTipo = new Chart(canvas, {

        type: "bar",

        data: {
            labels: labels,

            datasets: [
                {
                    label: "Ordens de serviço",

                    data: valores,

                    backgroundColor: "#f5c400",

                    borderWidth: 0,

                    borderRadius: 3
                }
            ]
        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            plugins: {

                legend: {
                    display: false
                }

            },

            scales: {

                x: {
                    grid: {
                        display: false
                    },

                    ticks: {
                        color: "#9aa4aa",

                        font: {
                            family: "'Share Tech Mono', monospace",
                            size: 13
                        }
                    }
                },

                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0,

                        color: "#9aa4aa",

                        font: {
                            family: "'Share Tech Mono', monospace",
                            size: 13
                        }
                    },

                    grid: {
                        color: "rgba(255, 255, 255, 0.05)"
                    }
                }
            }
        }
    });
}

// GERAR RELATÓRIO

async function gerarRelatorioDeServicos() {

    const dataInicio =
        servicosDataInicio?.value || "";

    const dataFim =
        servicosDataFim?.value || "";

    const status =
        servicosStatus?.value || "";

    if (dataInicio && dataFim && dataInicio > dataFim) {

        notificar(
            "A data inicial não pode ser maior que a data final.",
            "aviso"
        );

        return;
    }

    const parametros =
        new URLSearchParams();

    if (dataInicio) {
        parametros.append("dataInicio", dataInicio);
    }

    if (dataFim) {
        parametros.append("dataFim", dataFim);
    }

    if (status) {
        parametros.append("status", status);
    }


    const url =
        `${API_URL}/relatorios/servicos` +
        (parametros.toString()
            ? `?${parametros.toString()}`
            : "");

    if (gerarRelatorioServicos) {

        gerarRelatorioServicos.disabled = true;

        gerarRelatorioServicos.textContent =
            "CARREGANDO...";
    }


    try {

        console.log(
            "[RELATÓRIO SERVIÇOS] Buscando:",
            url
        );


        const resposta =
            await fetch(url);


        if (!resposta.ok) {

            const mensagem =
                await resposta.text();

            throw new Error(
                mensagem ||
                `Erro HTTP: ${resposta.status}`
            );
        }


        const dados =
            await resposta.json();


        console.log(
            "[RELATÓRIO SERVIÇOS] Dados recebidos:",
            dados
        );

        renderizarIndicadoresRelatorioServicos(dados);

        renderizarTabelaRelatorioServicos(
            dados.registros
        );

        renderizarGraficoStatusRelatorioServicos(
            dados
        );

        renderizarGraficoTipoRelatorioServicos(
            dados
        );


        if (registrosRelatorioServicos) {

            const quantidade =
                Array.isArray(dados.registros)
                    ? dados.registros.length
                    : 0;

            registrosRelatorioServicos.textContent =
                `${quantidade} REGISTRO${quantidade === 1 ? "" : "S"}`;
        }


        notificar(
            "Relatório de serviços gerado com sucesso!",
            "sucesso"
        );


    } catch (erro) {

        console.error(
            "Erro ao gerar relatório de serviços:",
            erro
        );


        notificar(
            `Não foi possível gerar o relatório de serviços.\n\n${erro.message}`,
            "erro"
        );

    } finally {

        if (gerarRelatorioServicos) {

            gerarRelatorioServicos.disabled = false;

            gerarRelatorioServicos.textContent =
                "GERAR RELATÓRIO";
        }

    }

}

// EVENTOS

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

// EVENTOS

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

// CONTROLE DO MODAL DE CONFIRMAÇÃO

const modalConfirmacaoExclusao = document.getElementById("modalConfirmacaoExclusao");
const nomeMaterialExclusao = document.getElementById("nomeMaterialExclusao");
const btnConfirmarExclusao = document.getElementById("confirmarExclusaoMaterial");
const tipoExclusao = document.getElementById("tipoExclusao");
const avisoExclusao = document.getElementById("avisoExclusao");
const textoBotaoExclusao = document.getElementById("textoBotaoExclusao");
const btnCancelarExclusao = document.getElementById("cancelarConfirmacaoExclusao");
const btnFecharConfirmacao = document.getElementById("fecharConfirmacaoExclusao");

let resolverConfirmacaoExclusao = null;

function confirmarExclusao({ tipo, nome, aviso, textoBotao }) {
    return new Promise((resolve) => {
        resolverConfirmacaoExclusao = resolve;
        tipoExclusao.textContent = tipo;
        nomeMaterialExclusao.textContent = nome;
        avisoExclusao.textContent = aviso;
        textoBotaoExclusao.textContent = textoBotao;
        modalConfirmacaoExclusao.classList.add("ativo");
    });
}

function confirmarExclusaoMaterial(nome) {
    return confirmarExclusao({
        tipo: "o material",
        nome,
        aviso: "Os lotes e as movimentações vinculados a este material também serão excluídos. Esta ação não poderá ser desfeita.",
        textoBotao: "EXCLUIR MATERIAL"
    });
}

function confirmarExclusaoLote(lote) {
    return confirmarExclusao({
        tipo: "o lote",
        nome: lote.codigo,
        aviso: `Material: ${lote.materialNome} • Quantidade: ${lote.quantidade} ${lote.unidade || ""}. Esta ação não poderá ser desfeita.`,
        textoBotao: "EXCLUIR LOTE"
    });
}

function fecharConfirmacaoExclusao(resultado) {
    modalConfirmacaoExclusao.classList.remove("ativo");
    if (resolverConfirmacaoExclusao) {
        resolverConfirmacaoExclusao(resultado);
        resolverConfirmacaoExclusao = null;
    }
}

btnConfirmarExclusao.addEventListener("click", () => {
    fecharConfirmacaoExclusao(true);
});

btnCancelarExclusao.addEventListener("click", () => {
    fecharConfirmacaoExclusao(false);
});

btnFecharConfirmacao.addEventListener("click", () => {
    fecharConfirmacaoExclusao(false);
});

modalConfirmacaoExclusao.addEventListener("click", (event) => {
    if (event.target === modalConfirmacaoExclusao) {
        fecharConfirmacaoExclusao(false);
    }
});

document.addEventListener("keydown", (event) => {
    if (
        event.key === "Escape" &&
        modalConfirmacaoExclusao.classList.contains("ativo")
    ) {
        fecharConfirmacaoExclusao(false);
    }
});