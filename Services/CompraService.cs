using ApiTeste.Data;
using ApiTeste.Domain;
using ApiTeste.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CompraService : ICompraService
{
    private readonly AppDbContext _context;

    public CompraService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Compra> CriarAsync(CriarCompraDto dto)
    {
        bool usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId);
        if (!usuarioExiste)
        {
            throw new Exception("Usuário não encontrado");
        }

        bool produtoExiste = await _context.Produtos.AnyAsync(p => p.Id == dto.ProdutoId);
        if (!produtoExiste)
        {
            throw new Exception("Produto não encontrado");
        }

        var compra = new Compra
        {
            UsuarioId = dto.UsuarioId,
            ProdutoId = dto.ProdutoId
        };

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();

        return compra;
    }

    public async Task<List<Compra>> ListarAsync()
    {
        return await _context.Compras
            .AsNoTracking()
            .Include(c => c.Usuario)
            .Include(c => c.Produto)
            .OrderByDescending(c => c.DataCadastro)
            .ToListAsync();
    }

    public async Task<Compra?> ListarUltimaAsync()
    {
        return await _context.Compras
            .AsNoTracking()
            .Include(c => c.Usuario)
            .Include(c => c.Produto)
            .OrderByDescending(c => c.DataCadastro)
            .FirstOrDefaultAsync();
    }

    public async Task<List<RelatorioCompraDto>> GerarRelatorioAsync()
    {
        return await _context.Compras
            .AsNoTracking()
            .OrderByDescending(c => c.DataCadastro)
            .Select(c => new RelatorioCompraDto
            {
                CompraId = c.Id,
                DataCompra = c.DataCadastro,
                UsuarioId = c.UsuarioId,
                UsuarioNome = c.Usuario.Nome,
                UsuarioEmail = c.Usuario.Email,
                ProdutoId = c.ProdutoId,
                ProdutoNome = c.Produto.Nome,
                ProdutoDescricao = c.Produto.Descricao
            })
            .ToListAsync();
    }
}
