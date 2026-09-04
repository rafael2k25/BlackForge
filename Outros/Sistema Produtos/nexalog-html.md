# NexaLog — Trechos extraídos (Estoque, Cadastro Produto, Cadastro Lote)

## Itens de navegação (sidebar)

```html
<button onclick="navigate('estoque',this)" data-page="estoque" class="nav-item">Estoque</button>
<button onclick="navigate('cadastro',this)" data-page="cadastro" class="nav-item">Cadastro Produto</button>
<button onclick="navigate('lote',this); carregarProdutos()" data-page="lote" class="nav-item">Cadastro Lote</button>
```

## Estoque

```html
<section id="estoque" class="page">
  <h1>Estoque</h1>

  <div class="busca-container">
    <input type="text" id="buscaEstoque"  placeholder="Buscar produtos..." oninput="filtrarEstoque()">
  </div>

  <div id="estoqueLista"></div>
</section>
```

## Cadastro de Produto

```html
<section id="cadastro" class="page">
  <h1>Cadastro de Produto</h1>
  <div class="card form-card">
    <input type="text" id="nomeProduto" placeholder="Nome do Produto">

    <div class="linha-dupla">
      <select id="unidadeProduto">
        <option value="Kg">Kg</option>
        <option value="Un">Un</option>
        <option value="L">L</option>
        <option value="G">G</option>
      </select>
    </div>

    <input type="number" id="codProduto" placeholder="Código do produto">
          
    <textarea id="descricaoProduto" placeholder="Descrição do Produto"></textarea>
    <button class="btn-primary" onclick="adicionarProduto()">Cadastrar</button>
  </div>
</section>
```

## Cadastro de Lote

```html
<section id="lote" class="page">
  <h1>Cadastro de Lote</h1>
  <div class="card form-card">
     <select id="idProduto">
     
      </select>

    <div class="linha-dupla">
      <input type="number" id="quantidadeLote" placeholder="Quantidade">          
    </div>
  
    <input type="text" id="codLote" placeholder="Código do Lote">
    <label for="fabricacaoProduto" style="font-weight: 600; margin-top: 5px;">Data de Fabricação</label>
    <input type="date" id="fabricacaoProduto">
    <label for="validadeProduto" style="font-weight: 600; margin-top: 5px;">Data de Validade</label>
    <input type="date" id="validadeProduto">
   
    <button class="btn-primary" onclick="adicionarLote()">Cadastrar</button>
  </div>
</section>
```

## Modais associados à tela de Estoque

Estes dois modais são usados pelas ações da lista de Estoque (editar produto e retirar quantidade), então incluí aqui também — remova esta seção se não fizer parte do que você precisa.

```html
<div id="modalEditar" class="modal-overlay">
  <div class="modal-card">
    <h2>Editar Produto</h2>
    <input type="text" id="editNome" placeholder="Nome do Produto">
    <input type="number" id="editCodProduto" placeholder="Código do produto">
    <select id="editUnidade">
      <option value="Kg">Kg</option>
      <option value="Un">Un</option>
      <option value="L">L</option>
      <option value="G">G</option>
    </select>
    <textarea id="editDescricao" placeholder="Descrição do Produto"></textarea>
    <div class="modal-botoes">
      <button class="btn-primary" onclick="salvarEdicaoProduto()">Salvar</button>
      <button class="btn-secondary" onclick="fecharModalEdicao()">Cancelar</button>
    </div>
  </div>
</div>

<div id="modalRemover" class="modal-overlay">
  <div class="modal-card">
    <h2>Retirar Quantidade</h2>
    <p id="removerNomeProduto" style="font-weight:600;"></p>
    <p>Estoque total: <strong id="removerEstoqueAtual"></strong></p>
    <div id="removerLotesContainer"></div>
    <div class="modal-botoes">
      <button class="btn-primary" onclick="confirmarRemocao()">Confirmar</button>
      <button class="btn-secondary" onclick="fecharModalRemover()">Cancelar</button>
    </div>
  </div>
</div>
```
