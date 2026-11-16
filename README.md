### OBJETIVOS ###

1º Criar lógica para salvar uma compra
2º Listar todas as compras
3º Listar última compra.
4º Melhorar performance da listagem de produtos e usuários.
5º Ajustar migration em caso de alteração dos objetos.
6º Adicionar no readme, a explicação das decisões tomadas para a solução desenvolvida.
7º Montar um endpoint para exportar um relatório relacionando as compras, produtos e usuários.

## EXPLICAÇÃO:

Já tinha no projeto a entidade Compra, o DbSet no contexto, o CriarCompraDto e até o CompraController com as rotas. O que faltava mesmo era a regra de negócio no CompraService (estava retornando null) e o restante dos itens do enunciado.

- Fui primeiro pro CompraService, no mesmo padrão do UsuarioService e ProdutoService:

Método CriarAsync:
Antes de salvar, confiro se o usuário e o produto existem com AnyAsync. Se não existir, lanço uma Exception com mensagem clara. Assim evito cair num erro feio de foreign key do SQLite. Depois crio o objeto Compra com os ids do dto, adiciono no contexto e salvo. O Id é gerado pelo banco.

Método ListarAsync:
Busco todas as compras com Include do Usuario e do Produto, pra já devolver a compra relacionada. Ordenei por DataCadastro descendente pra ficar as mais recentes primeiro. Se der erro, o controller captura e devolve ErrorResponse.

Método ListarUltimaAsync:
Mesma ideia da listagem, só que uso OrderByDescending na DataCadastro e FirstOrDefaultAsync. Se não tiver nenhuma compra, volta null e o controller responde 404 com "Nenhuma compra encontrada".

Na ICompraService: adicionei ListarUltimaAsync e também GerarRelatorioAsync, porque o controller da última compra já chamava um método que não existia na interface (isso quebrava o build).

Nas Controllers: o CompraController já tinha criar, listar e última. Eu só completei o fluxo no service e acrescentei a rota do relatório.

- Performance (item 4):

Nas listagens de UsuarioService e ProdutoService coloquei AsNoTracking. Nessas rotas a gente só lê, então não precisa do EF ficar rastreando as entidades pra possível update. Isso alivia um pouco memória e custo. Usei a mesma ideia nas consultas de leitura de compra e no relatório.

- Migration (item 5):

A migration AdicionandoTabelaCompra estava com Up e Down vazios. A tabela Compras já tinha sido criada no InitialCreate, então não fazia sentido criar a tabela de novo. O que eu fiz foi ajustar o modelo: declarei um índice em DataCadastro no AppDbContext (ajuda na busca da última compra, que ordena por essa coluna), preenchi a migration com CreateIndex/DropIndex e atualizei o snapshot. No Program.cs liguei de novo o Database.Migrate que já existia comentado, pra aplicar as migrations ao subir a API.

- Ciclo no JSON:

Quando listei as compras com Include de Usuario e Produto, a API começou a dar 500. O problema era loop infinito no JSON: Compra > Usuario > Compras > Compra... Configurei ReferenceHandler.IgnoreCycles no AddControllers do Program.cs pra o serializer ignorar essas referências cíclicas.

- Relatório (item 7):

Criei o RelatorioCompraDto com os campos planos (ids, nomes, email, descrição, data). No service, o GerarRelatorioAsync usa Select pra projetar direto compra + usuário + produto nesse DTO. Assim não carrego entidade inteira, evito o ciclo de serialização e fica um retorno limpo pra exportar/consultar.

Ficaram duas formas de saída:
- GET /api/compras/relatorio → JSON (bom pra testar no Swagger)
- GET /api/compras/relatorio/csv → download do arquivo CSV (export de verdade)

No front do simulador coloquei os botões "Relatório JSON" e "Exportar CSV". O CSV usa ; como separador e BOM UTF-8 pra abrir direito no Excel.

Resumindo o caminho: service de compra de verdade, última compra na interface, AsNoTracking nas listagens, migration do índice que estava vazia, IgnoreCycles pra não travar o JSON, e o endpoint de relatório juntando as três entidades.

Pra algumas partes eu fui na documentação oficial da Microsoft, principalmente quando travei no ciclo do JSON e na hora de decidir o AsNoTracking / Include / migrations:

## DOCS de apoio usadas

- Tracking / AsNoTracking (performance nas listagens):
  https://learn.microsoft.com/ef/core/querying/tracking

- Include e dados relacionados (listar compra com usuário/produto):
  https://learn.microsoft.com/ef/core/querying/related-data

- Projeções com Select (usei no relatório):
  https://learn.microsoft.com/ef/core/querying/complex-query-operators

- ReferenceHandler.IgnoreCycles (loop Compra > Usuario > Compra):
  https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/preserve-references

- Migrations e ApplyMigrations / Database.Migrate:
  https://learn.microsoft.com/ef/core/managing-schemas/migrations
  https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying

- Retorno de arquivo no ASP.NET Core (usei File(...) no export CSV):
  https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file