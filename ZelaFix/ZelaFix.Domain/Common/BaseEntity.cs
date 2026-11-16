namespace ZelaFix.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; protected set; }

    /// <summary>Permite definir o Id em cenários de teste unitário com mocks em memória.</summary>
    public void DefinirIdParaTeste(int id) => Id = id;
}