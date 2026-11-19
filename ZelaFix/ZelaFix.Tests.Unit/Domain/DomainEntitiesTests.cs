using ZelaFix.Domain.Common;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;
using ZelaFix.Domain.Helpers;

namespace ZelaFix.Tests.Unit.Domain;

public class DomainEntitiesTests
{
    [Fact]
    public void Chamado_ComDadosValidos_DeveCriarEApoiarSemDuplicidade()
    {
        // Arrange & Act
        var chamado = new Chamado("Buraco na via", "Buraco fundo próximo ao meio-fio", [0xFF, 0xD8, 0xFF, 0xE0], 1, 1, 2, 2, 1);

        // Assert
        Assert.Equal("Buraco na via", chamado.Titulo);
        Assert.Null(chamado.StatusAtual);

        chamado.AtualizarPrioridade(3);
        Assert.Equal(3, chamado.PrioridadeId);

        Assert.Throws<DomainException>(() => chamado.Apoiar(99));
        typeof(BaseEntity).GetProperty("Id")!.SetValue(chamado, 10);

        chamado.Apoiar(99);
        Assert.True(chamado.JaApoiadoPor(99));
        Assert.Throws<DomainException>(() => chamado.Apoiar(99));
    }

    [Fact]
    public void Chamado_ComFotoVaziaOuTextoInvalido_DeveLancarDomainException()
    {
        // Arrange, Act & Assert
        Assert.Throws<DomainException>(() => new Chamado("Título", "Descrição longa", [], 1, 1, 1, 1, 1));
        Assert.Throws<DomainException>(() => new Chamado("", "Descrição longa", [1, 2], 1, 1, 1, 1, 1));
        Assert.Throws<DomainException>(() => new Chamado("Título", new string('a', 1001), [1, 2], 1, 1, 1, 1, 1));

        var chamado = new Chamado("Título", "Descrição longa", [1, 2], 1, 1, 1, 1, 1);
        Assert.Throws<DomainException>(() => chamado.AtualizarPrioridade(0));
    }

    [Fact]
    public void Chamado_IdsInvalidos_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() => new Chamado("Título", "Descrição longa", [1, 2], 0, 1, 1, 1, 1));
        Assert.Throws<DomainException>(() => new Chamado("Título", "Descrição longa", [1, 2], 1, 0, 1, 1, 1));
        Assert.Throws<DomainException>(() => new Chamado("Título", "Descrição longa", [1, 2], 1, 1, 0, 1, 1));
        Assert.Throws<DomainException>(() => new Chamado("Título", "Descrição longa", [1, 2], 1, 1, 1, 0, 1));
        Assert.Throws<DomainException>(() => new Chamado("Título", "Descrição longa", [1, 2], 1, 1, 1, 1, 0));
    }

    [Fact]
    public void PrioridadeEOrgaoGoverno_ValoresInvalidos_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() => new Prioridade("CRITICA"));
        Assert.Throws<DomainException>(() => new OrgaoGoverno("ORGAO_INEXISTENTE"));

        var p = new Prioridade("alta");
        var o = new OrgaoGoverno("defesa_civil");
        Assert.Equal("ALTA", p.Nome);
        Assert.Equal("DEFESA_CIVIL", o.Nome);
    }

    [Fact]
    public void TelefoneEHashHelper_CasosBorda_DeveValidarECriptografar()
    {
        Assert.Throws<DomainException>(() => new Telefone("1", "999999999", 1));
        Assert.Throws<DomainException>(() => new Telefone("11", "123", 1));
        Assert.Throws<DomainException>(() => new Telefone("11", "999999999", 0));

        var tel = new Telefone("11", "999999999", 1);
        Assert.Equal("(11) 999999999", tel.Completo);

        var hash = HashHelper.Hash("SenhaForte123");
        Assert.True(HashHelper.Verificar("SenhaForte123", hash));
        Assert.False(HashHelper.Verificar("Errada", hash));
        Assert.False(HashHelper.Verificar("", hash));
        Assert.False(HashHelper.Verificar("SenhaForte123", ""));
        Assert.False(HashHelper.Verificar("SenhaForte123", "hash-malformado-nao-bcrypt"));
    }

    [Fact]
    public void Usuario_ComCargoAdmin_DeveTerPerfilAdminEPermissaoGestor()
    {
        var admin = new Usuario("Admin Geral", "admin@zelafix.com.br", "Admin#2026", ZelaFix.Domain.Enums.Cargo.Admin, 1);

        Assert.True(admin.EhGestor);
        Assert.Equal("ADMIN", ZelaFix.Domain.Enums.CargoExtensions.ParaPerfil(admin.Cargo));
    }
}
