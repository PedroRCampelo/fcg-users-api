using FCG.Users.Application.Dtos;
using FCG.Users.Application.Exceptions;
using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;

namespace FCG.Users.Application.Services;

public class UsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarios.ListarAsync(cancellationToken);
        return usuarios.Select(UsuarioMapper.ParaResponse).ToList();
    }

    public async Task PromoverAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await ObterOuFalharAsync(usuarioId, cancellationToken);
        usuario.Promover();
        await _unitOfWork.SalvarAsync(cancellationToken);
    }

    public async Task DesativarAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await ObterOuFalharAsync(usuarioId, cancellationToken);
        usuario.Desativar();
        await _unitOfWork.SalvarAsync(cancellationToken);
    }

    private async Task<Usuario> ObterOuFalharAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        await _usuarios.ObterPorIdAsync(usuarioId, cancellationToken)
            ?? throw new NaoEncontradoException("Usuário não encontrado.");
}
