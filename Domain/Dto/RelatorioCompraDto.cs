public class RelatorioCompraDto
{
    public int CompraId { get; set; }
    public DateTime DataCompra { get; set; }

    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; }
    public string UsuarioEmail { get; set; }

    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; }
    public string ProdutoDescricao { get; set; }
}
