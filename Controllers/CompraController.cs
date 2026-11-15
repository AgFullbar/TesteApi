using System.Text;
using ApiTeste.Domain;
using ApiTeste.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/compras")]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _service;

    public ComprasController(ICompraService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarCompraDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse(
                "Dados inválidos"
            ));
        }

        try
        {
            Compra compra = await _service.CriarAsync(dto);
            return StatusCode(201, compra);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(
                "Erro ao criar compra",
                ex.Message
            ));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try
        {
            List<Compra> compras = await _service.ListarAsync();
            return Ok(compras);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(
                "Erro ao listar compras",
                ex.Message
            ));
        }
    }

    [HttpGet("ultima")]
    public async Task<IActionResult> ListarUltima()
    {
        try
        {
            Compra? compra = await _service.ListarUltimaAsync();

            if (compra == null)
            {
                return NotFound(new ErrorResponse(
                    "Nenhuma compra encontrada"
                ));
            }

            return Ok(compra);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(
                "Erro ao buscar última compra",
                ex.Message
            ));
        }
    }

    [HttpGet("relatorio")]
    public async Task<IActionResult> Relatorio()
    {
        try
        {
            List<RelatorioCompraDto> relatorio = await _service.GerarRelatorioAsync();
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(
                "Erro ao gerar relatório",
                ex.Message
            ));
        }
    }

    [HttpGet("relatorio/csv")]
    public async Task<IActionResult> RelatorioCsv()
    {
        try
        {
            List<RelatorioCompraDto> relatorio = await _service.GerarRelatorioAsync();

            var sb = new StringBuilder();
            sb.AppendLine("CompraId;DataCompra;UsuarioId;UsuarioNome;UsuarioEmail;ProdutoId;ProdutoNome;ProdutoDescricao");

            foreach (RelatorioCompraDto item in relatorio)
            {
                sb.AppendLine(string.Join(";",
                    item.CompraId,
                    item.DataCompra.ToString("yyyy-MM-dd HH:mm:ss"),
                    item.UsuarioId,
                    EscaparCsv(item.UsuarioNome),
                    EscaparCsv(item.UsuarioEmail),
                    item.ProdutoId,
                    EscaparCsv(item.ProdutoNome),
                    EscaparCsv(item.ProdutoDescricao)
                ));
            }

            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] conteudo = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] arquivo = new byte[bom.Length + conteudo.Length];
            Buffer.BlockCopy(bom, 0, arquivo, 0, bom.Length);
            Buffer.BlockCopy(conteudo, 0, arquivo, bom.Length, conteudo.Length);

            return File(arquivo, "text/csv", "relatorio-compras.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(
                "Erro ao exportar relatório CSV",
                ex.Message
            ));
        }
    }

    private static string EscaparCsv(string? valor)
    {
        valor ??= string.Empty;
        if (valor.Contains(';') || valor.Contains('"') || valor.Contains('\n'))
        {
            return "\"" + valor.Replace("\"", "\"\"") + "\"";
        }

        return valor;
    }
}
