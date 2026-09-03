# NexaLog — Trechos JS extraídos (Estoque, Cadastro Produto, Cadastro Lote)

## Constantes e variáveis globais usadas por essas telas

```javascript
const API_URL = "https://localhost:7071/api";
let produtos = [];
const MAPA_UNIDADE = { Kg: 0, Un: 1, L: 2, G: 3 };
const MAPA_UNIDADE_REVERSO = { 0: "Kg", 1: "Un", 2: "L", 3: "G" };
let produtoEditandoId = null;
let loteEditandoId = null;
```

## Carregamento de produtos (usado por Estoque e por Cadastro de Lote)

```javascript
async function carregarProdutos() {
  const resposta = await fetch(`${API_URL}/Produto`, {
    credentials: "include"
  });

  if (!resposta.ok) {
    showToast("Erro ao carregar produtos.");
    return;
  }

  produtos = await resposta.json();
  console.log(produtos);
  document.getElementById("idProduto").innerHTML = '';
for (let i = 0; i < produtos.length; i++) {
  document.getElementById("idProduto").innerHTML += '<option value=' + produtos[i].idProduto + '>' + produtos[i].nome + '</option>';
}
console.log(produtos[0].unidade);
console.log(MAPA_UNIDADE_REVERSO[produtos[0].unidade]);
  atualizarTudo();
}
```

> Chama `atualizarTudo()`, definida em outro trecho do script.js (não incluída aqui por atualizar também Dashboard e Relatórios).

## Cadastro de Produto

```javascript
async function adicionarProduto() {
  const nome = document.getElementById("nomeProduto").value.trim();
  const unidade = document.getElementById("unidadeProduto").value;
  const codigoProduto = document.getElementById("codProduto").value;
 
  const dataCadastro = new Date().toISOString().split("T")[0];

  let descricao = document.getElementById("descricaoProduto").value.trim();

  if (!descricao) {
    descricao = gerarDescricaoAutomatica(nome);
  }

  if (!nome || !unidade || !codigoProduto) {
    showToast("Preencha todos os campos obrigatórios");
    return;
  }

  try {

    const resposta = await fetch(`${API_URL}/Produto`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        nome,
        dataCadastro,
        unidade: MAPA_UNIDADE[unidade],
        codProduto: Number(codigoProduto),
        descricao
      })
    });

    if (!resposta.ok) {
      const erro = await resposta.text();
      showToast(erro || "Erro ao cadastrar produto.");
      return;
    }
    
  }
   catch (erro) {
    console.error(erro);
    showToast("Erro ao conectar com o servidor.");
  }
}

function gerarDescricaoAutomatica(nome) {
  nome = nome.toLowerCase();
  const alimenticios = ["farinha", "açúcar", "sal", "fermento", "leite", "ovos"];
  if (alimenticios.some(item => nome.includes(item))) {
    return "Produto alimentício utilizado no preparo de refeições. Conservar em local seco e arejado.";
  }
  return `${nome}, produto destinado ao uso comercial. Verifique validade e condições de armazenamento.`;
}
```

## Cadastro de Lote

```javascript
async function adicionarLote() {
    const quantidade = document.getElementById('quantidadeLote').value;
    const validade = document.getElementById('validadeProduto').value;
    const fabricacao = document.getElementById('validadeProduto').value;
    const codigoLote = document.getElementById('codLote').value;
    const respostaLote = await fetch(`${API_URL}/Lote`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json" 
      },
      body: JSON.stringify({
        quantidadeLote: Number(quantidade),
        codLote: codigoLote,
        dataValidade: validade,
        dataFabricacao: fabricacao,
        fkProdutoIdProduto: parseInt(document.getElementById('idProduto').value)
      })
    });

    if (!respostaLote.ok) {
      const erro = await respostaLote.text();
      showToast(erro || "Erro ao cadastrar lote.");
      return;
    }

    // Limpar os campos
    document.getElementById("nomeProduto").value = "";
    document.getElementById("quantidadeLote").value = "";
    document.getElementById("codLote").value = "";
    document.getElementById("validadeProduto").value = "";
    document.getElementById("fabricacaoProduto").value = "";

    // Atualizar a lista de produtos
    await carregarProdutos();

    showToast("Produto cadastrado com sucesso.");

  }
```

## Estoque

```javascript
let produtoRemovendoId = null;

async function confirmarRemocao() {
  if (!produtoRemovendoId) return;

  const produto = produtos.find(p => p.idProduto === produtoRemovendoId);
  if (!produto) {
    showToast("Produto não encontrado.");
    return;
  }

  const checks = document.querySelectorAll("#removerLotesContainer .lote-check:checked");

  if (checks.length === 0) {
    showToast("Selecione ao menos um lote.");
    return;
  }

  const baixas = [];

  for (const chk of checks) {
    const loteId = Number(chk.dataset.loteId);
    const lote = produto.lotes.find(l => l.idLote === loteId);
    const input = document.querySelector(
      `#removerLotesContainer .lote-qtd-input[data-lote-id="${loteId}"]`
    );
    const qtd = Number(input.value);

    if (!qtd || qtd <= 0) {
      showToast(`Digite uma quantidade válida para o lote ${lote.codLote}.`);
      return;
    }

    if (qtd > lote.quantidadeLote) {
      showToast(`Quantidade maior que o disponível no lote ${lote.codLote}.`);
      return;
    }

    baixas.push({ lote, novaQuantidade: lote.quantidadeLote - qtd });
  }

  try {
    for (const { lote, novaQuantidade } of baixas) {
      const respostaLote = await fetch(`${API_URL}/Lote/${lote.idLote}`, {
        method: "PUT",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          codLote: lote.codLote,
          quantidadeLote: novaQuantidade,
          dataValidade: lote.dataValidade,
          dataFabricacao: lote.dataFabricacao,
          fkProdutoIdProduto: produto.idProduto
        })
      });

      if (!respostaLote.ok) {
        showToast("Erro ao atualizar um dos lotes.");
        await carregarProdutos();
        return;
      }
    }

    showToast("Quantidade removida com sucesso");
    fecharModalRemover();
    await carregarProdutos();

  } catch (erro) {
    console.error(erro);
    showToast("Erro ao conectar com o servidor.");
  }
}

function atualizarEstoque(lista = produtos) {
  const container = document.getElementById("estoqueLista");
  if (!container) return;
  container.innerHTML = lista.map(p => {
    const lote = p.lotes?.[0];
    const validade = lote?.dataValidade ?? "-";
    const quantidade = p.quantidadeTotal;
    const unidade = MAPA_UNIDADE_REVERSO[p.unidade];
    const vencendo = lote && diasRestantes(validade) <= 7;
    return `
      <div class="product-card ${vencendo ? 'warning' : ''}">
        <h3>${p.nome} <ion-icon name="trash-outline" class="icone-lixeira" onclick="confirmarExclusaoProduto(${p.idProduto}, '${p.nome.replace(/'/g, "\\'")}')"></ion-icon></h3> 
        <p><strong>Quantidade em estoque: </strong>${quantidade} ${unidade}</p>
        <p><strong>Validade: </strong>${validade}</p>
        
      <div class="acoes-container">        
          <button class="btn-atualizar" onclick="abrirModalEdicao(${p.idProduto})">
            Atualizar
          </button>

          <button class="btn-danger" onclick="abrirModalRemover(${p.idProduto})">
            Retirar do Lote
          </button>
      </div>
    </div>
    `;
  }).join("");

  if (lista.length === 0) {
    container.innerHTML = "<p class='sem-resultado'>Nenhum produto encontrado.</p>";
  }
}

function filtrarEstoque() {
  const termo = normalizar(document.getElementById("buscaEstoque").value.trim());

  if (!termo) {
    atualizarEstoque();
    return;
  }

  const filtrados = produtos.filter(p =>
    normalizar(p.nome).includes(termo) ||
    p.codProduto?.toString().includes(termo)
  );

  atualizarEstoque(filtrados);
}

async function confirmarExclusaoProduto(idProduto, nome) {
  const confirmado = confirm(`Tem certeza que deseja excluir o produto "${nome}"? Essa ação não pode ser desfeita.`);
  if (!confirmado) return;

  try {
    const resposta = await fetch(`${API_URL}/Produto/${idProduto}`, {
      method: "DELETE",
      credentials: "include"
    });

    if (!resposta.ok) {
      const erro = await resposta.text();
      showToast(erro || "Erro ao excluir produto.");
      return;
    }

    showToast("Produto excluído com sucesso.");
    await carregarProdutos();

  } catch (erro) {
    console.error(erro);
    showToast("Erro ao conectar com o servidor.");
  }
}

function abrirModalEdicao(idProduto) {
  const produto = produtos.find(p => p.idProduto === idProduto);
  if (!produto) {
    showToast("Produto não encontrado.");
    return;
  }

  const lote = produto.lotes?.[0] ?? null;

  produtoEditandoId = produto.idProduto;
  loteEditandoId = lote?.idLote ?? null;

  document.getElementById("editNome").value = produto.nome;
  document.getElementById("editCodProduto").value = produto.codProduto;
  document.getElementById("editUnidade").value = MAPA_UNIDADE_REVERSO[produto.unidade] ?? "Kg";
  document.getElementById("editDescricao").value = produto.descricao ?? "";
  document.getElementById("modalEditar").classList.add("show");
}

async function salvarEdicaoProduto() {
  if (!produtoEditandoId) return;

  const produto = produtos.find(p => p.idProduto === produtoEditandoId);
  if (!produto) {
    showToast("Produto não encontrado.");
    return;
  }

  const nome = document.getElementById("editNome").value.trim();
  const codProduto = Number(document.getElementById("editCodProduto").value);
  const unidade = document.getElementById("editUnidade").value;
  const descricao = document.getElementById("editDescricao").value.trim();

  if (!nome || !codProduto) {
    showToast("Preencha todos os campos obrigatórios.");
    return;
  }

  try {
    const respostaProduto = await fetch(`${API_URL}/Produto/${produtoEditandoId}`, {
      method: "PUT",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        nome,
        dataCadastro: produto.dataCadastro,
        unidade: MAPA_UNIDADE[unidade],
        codProduto,
        descricao
      })
    });

    if (!respostaProduto.ok) {
      showToast("Erro ao atualizar produto.");
      return;
    }

   /* if (loteEditandoId) {
      const lote = produto.lotes.find(l => l.idLote === loteEditandoId);
      const respostaLote = await fetch(`${API_URL}/Lote/${loteEditandoId}`, {
        method: "PUT",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          codLote,
          quantidadeLote,
          dataValidade: lote.dataValidade,
          dataFabricacao: lote.dataFabricacao,
          fkProdutoIdProduto: produtoEditandoId
        })
      });

      if (!respostaLote.ok) {
        showToast("Erro ao atualizar lote.");
        return;
      }
    }*/

    showToast("Produto atualizado com sucesso.");
    fecharModalEdicao();
    await carregarProdutos();

  } catch (erro) {
    console.error(erro);
    showToast("Erro ao conectar com o servidor.");
  }
}

function fecharModalEdicao() {
  document.getElementById("modalEditar").classList.remove("show");
  produtoEditandoId = null;
  loteEditandoId = null;
}

function abrirModalRemover(idProduto) {
  const produto = produtos.find(p => p.idProduto === idProduto);
  if (!produto) {
    showToast("Produto não encontrado.");
    return;
  }

  produtoRemovendoId = produto.idProduto;

  document.getElementById("removerNomeProduto").textContent = produto.nome;
  document.getElementById("removerEstoqueAtual").textContent =
    `${produto.quantidadeTotal} ${MAPA_UNIDADE_REVERSO[produto.unidade]}`;

  const unidade = MAPA_UNIDADE_REVERSO[produto.unidade];
  const container = document.getElementById("removerLotesContainer");

  if (!produto.lotes || produto.lotes.length === 0) {
    container.innerHTML = "<p>Nenhum lote disponível.</p>";
  } else {
    container.innerHTML = produto.lotes.map(l => `
      <div class="lote-remover-item">
        <label>
          <input type="checkbox" class="lote-check" data-lote-id="${l.idLote}">
          Lote ${l.codLote} <br> Disponível: ${l.quantidadeLote}  ${unidade}</br>
        </label>
        <input type="number" class="lote-qtd-input" data-lote-id="${l.idLote}"
               min="0" max="${l.quantidadeLote}" placeholder="Qtd" disabled>
      </div>
    `).join("");

    container.querySelectorAll(".lote-check").forEach(chk => {
      chk.addEventListener("change", () => {
        const input = container.querySelector(
          `.lote-qtd-input[data-lote-id="${chk.dataset.loteId}"]`
        );
        input.disabled = !chk.checked;
        if (!chk.checked) input.value = "";
      });
    });
  }

  document.getElementById("modalRemover").classList.add("show");
}

function fecharModalRemover() {
  document.getElementById("modalRemover").classList.remove("show");
  document.getElementById("removerLotesContainer").innerHTML = "";
  produtoRemovendoId = null;
}

function diasRestantes(data) {
  if (!data) return Infinity;
  const hoje = new Date();
  hoje.setHours(0,0,0,0);
  const v = new Date(data);
  v.setHours(0,0,0,0);
  return Math.ceil((v - hoje) / (1000 * 60 * 60 * 24));
}
```

## Utilitários compartilhados (usados por Estoque, entre outras telas)

```javascript
function normalizar(texto) {

    return texto
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/[^\w\s]/g, "");

}

function showToast(msg) {
  const toast = document.getElementById("toast");
  toast.textContent = msg;
  toast.classList.add("show");
  setTimeout(() => toast.classList.remove("show"), 3000);
}
```

---

**Duas coisas que notei ao extrair, caso ainda não tenham sido tratadas:**
- Em `adicionarLote()`, a variável `fabricacao` lê o valor de `validadeProduto` em vez de `fabricacaoProduto` — a data de fabricação enviada ao backend é sempre igual à data de validade.
- `adicionarProduto()` não chama `carregarProdutos()` nem `showToast()` de sucesso ao final (diferente de `adicionarLote()`), então a lista e o select de produtos só atualizam depois de outra ação que recarregue os dados.
