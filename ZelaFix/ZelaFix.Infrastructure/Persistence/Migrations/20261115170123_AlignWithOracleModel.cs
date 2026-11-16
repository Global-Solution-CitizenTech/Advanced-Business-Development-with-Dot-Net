using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZelaFix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AlignWithOracleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE
                    v_chamados NUMBER;
                    v_categorias_invalidas NUMBER;
                BEGIN
                    SELECT COUNT(*) INTO v_chamados FROM CHAMADO;
                    IF v_chamados > 0 THEN
                        RAISE_APPLICATION_ERROR(
                            -20999,
                            'Migration requer chamados existentes classificados manualmente antes da inclusão de prioridade e órgão.'
                        );
                    END IF;

                    SELECT COUNT(*) INTO v_categorias_invalidas
                    FROM CATEGORIA
                    WHERE NM_CATEGORIA NOT IN (
                        'ILUMINACAO_PUBLICA', 'VIAS_E_PAVIMENTACAO', 'DRENAGEM_E_ESGOTO',
                        'LIMPEZA_URBANA', 'ARVORES_E_PARQUES', 'SINALIZACAO_E_TRANSITO',
                        'Iluminação Pública', 'Vias e Pavimentação', 'Drenagem e Esgoto',
                        'Limpeza Urbana', 'Árvores e Parques', 'Sinalização e Trânsito'
                    );
                    IF v_categorias_invalidas > 0 THEN
                        RAISE_APPLICATION_ERROR(
                            -20998,
                            'Migration encontrou categorias fora do catálogo oficial; ajuste-as antes de migrar.'
                        );
                    END IF;

                    UPDATE CATEGORIA
                    SET NM_CATEGORIA = CASE NM_CATEGORIA
                        WHEN N'Iluminação Pública' THEN N'ILUMINACAO_PUBLICA'
                        WHEN N'Vias e Pavimentação' THEN N'VIAS_E_PAVIMENTACAO'
                        WHEN N'Drenagem e Esgoto' THEN N'DRENAGEM_E_ESGOTO'
                        WHEN N'Limpeza Urbana' THEN N'LIMPEZA_URBANA'
                        WHEN N'Árvores e Parques' THEN N'ARVORES_E_PARQUES'
                        WHEN N'Sinalização e Trânsito' THEN N'SINALIZACAO_E_TRANSITO'
                        ELSE NM_CATEGORIA
                    END
                    WHERE NM_CATEGORIA IN (
                        N'Iluminação Pública', N'Vias e Pavimentação', N'Drenagem e Esgoto',
                        N'Limpeza Urbana', N'Árvores e Parques', N'Sinalização e Trânsito'
                    );

                    UPDATE STATUS SET NM_STATUS = N'PENDENTE' WHERE NM_STATUS = N'ABERTO';
                END;
                """);

            migrationBuilder.DropIndex(
                name: "IX_VOTO_CHAMADO_CHAMADO_ID_CHAMADO",
                table: "VOTO_CHAMADO");

            migrationBuilder.AlterColumn<string>(
                name: "SENHA",
                table: "USUARIO",
                type: "VARCHAR2(60)",
                unicode: false,
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "NM_USUARIO",
                table: "USUARIO",
                type: "VARCHAR2(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EMAIL",
                table: "USUARIO",
                type: "VARCHAR2(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CARGO",
                table: "USUARIO",
                type: "VARCHAR2(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "NUM_TEL",
                table: "TELEFONE",
                type: "VARCHAR2(9)",
                unicode: false,
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "NUM_DDD",
                table: "TELEFONE",
                type: "VARCHAR2(2)",
                unicode: false,
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "NM_STATUS",
                table: "STATUS",
                type: "VARCHAR2(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "NM_ESTADO",
                table: "ESTADO",
                type: "VARCHAR2(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "RUA",
                table: "ENDERECO",
                type: "VARCHAR2(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "REFERENCIA",
                table: "ENDERECO",
                type: "VARCHAR2(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NUMERO",
                table: "ENDERECO",
                type: "VARCHAR2(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CEP",
                table: "ENDERECO",
                type: "VARCHAR2(8)",
                unicode: false,
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "NM_CIDADE",
                table: "CIDADE",
                type: "VARCHAR2(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TITULO",
                table: "CHAMADO",
                type: "VARCHAR2(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DESCRICAO",
                table: "CHAMADO",
                type: "VARCHAR2(1000)",
                unicode: false,
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<short>(
                name: "ORGAO_GOVERNO_ID_ORGAO",
                table: "CHAMADO",
                type: "NUMBER(5)",
                nullable: false);

            migrationBuilder.AddColumn<short>(
                name: "PRIORIDADE_ID_PRIORIDADE",
                table: "CHAMADO",
                type: "NUMBER(5)",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "NM_CATEGORIA",
                table: "CATEGORIA",
                type: "VARCHAR2(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NM_BAIRRO",
                table: "BAIRRO",
                type: "VARCHAR2(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "ORGAO_GOVERNO",
                columns: table => new
                {
                    ID_ORGAO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ORGAO_GOVERNO = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORGAO_GOVERNO", x => x.ID_ORGAO);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ORGAO_GOVERNO_NOME",
                table: "ORGAO_GOVERNO",
                sql: "ORGAO_GOVERNO IN ('SECRETARIA_OBRAS', 'CET_TRANSITO', 'ILUMINACAO_PUBLICA', 'DEFESA_CIVIL', 'SUBPREFEITURA', 'SANEAMENTO_BASICO')");

            migrationBuilder.CreateTable(
                name: "PRIORIDADE",
                columns: table => new
                {
                    ID_PRIORIDADE = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_PRIORIDADE = table.Column<string>(type: "VARCHAR2(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRIORIDADE", x => x.ID_PRIORIDADE);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PRIORIDADE_NOME",
                table: "PRIORIDADE",
                sql: "NM_PRIORIDADE IN ('BAIXA', 'MEDIA', 'ALTA', 'URGENTE')");

            migrationBuilder.InsertData(
                table: "ORGAO_GOVERNO",
                columns: new[] { "ID_ORGAO", "ORGAO_GOVERNO" },
                values: new object[,]
                {
                    { (short)1, "SECRETARIA_OBRAS" },
                    { (short)2, "CET_TRANSITO" },
                    { (short)3, "ILUMINACAO_PUBLICA" },
                    { (short)4, "DEFESA_CIVIL" },
                    { (short)5, "SUBPREFEITURA" },
                    { (short)6, "SANEAMENTO_BASICO" }
                });

            migrationBuilder.InsertData(
                table: "PRIORIDADE",
                columns: new[] { "ID_PRIORIDADE", "NM_PRIORIDADE" },
                values: new object[,]
                {
                    { (short)1, "BAIXA" },
                    { (short)2, "MEDIA" },
                    { (short)3, "ALTA" },
                    { (short)4, "URGENTE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_VOTO_CHAMADO_CHAMADO_ID_CHAMADO_USUARIO_ID_USUARIO",
                table: "VOTO_CHAMADO",
                columns: new[] { "CHAMADO_ID_CHAMADO", "USUARIO_ID_USUARIO" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_STATUS_NOME",
                table: "STATUS",
                sql: "NM_STATUS IN ('PENDENTE', 'EM_ANALISE', 'EM_EXECUCAO', 'CONCLUIDO', 'REJEITADO')");

            migrationBuilder.CreateIndex(
                name: "IX_CHAMADO_ORGAO_GOVERNO_ID_ORGAO",
                table: "CHAMADO",
                column: "ORGAO_GOVERNO_ID_ORGAO");

            migrationBuilder.CreateIndex(
                name: "IX_CHAMADO_PRIORIDADE_ID_PRIORIDADE",
                table: "CHAMADO",
                column: "PRIORIDADE_ID_PRIORIDADE");

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIA_NM_CATEGORIA",
                table: "CATEGORIA",
                column: "NM_CATEGORIA",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CATEGORIA_NOME",
                table: "CATEGORIA",
                sql: "NM_CATEGORIA IN ('ILUMINACAO_PUBLICA', 'VIAS_E_PAVIMENTACAO', 'DRENAGEM_E_ESGOTO', 'LIMPEZA_URBANA', 'ARVORES_E_PARQUES', 'SINALIZACAO_E_TRANSITO')");

            migrationBuilder.CreateIndex(
                name: "IX_ORGAO_GOVERNO_ORGAO_GOVERNO",
                table: "ORGAO_GOVERNO",
                column: "ORGAO_GOVERNO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRIORIDADE_NM_PRIORIDADE",
                table: "PRIORIDADE",
                column: "NM_PRIORIDADE",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CHAMADO_ORGAO_GOVERNO_ORGAO_GOVERNO_ID_ORGAO",
                table: "CHAMADO",
                column: "ORGAO_GOVERNO_ID_ORGAO",
                principalTable: "ORGAO_GOVERNO",
                principalColumn: "ID_ORGAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CHAMADO_PRIORIDADE_PRIORIDADE_ID_PRIORIDADE",
                table: "CHAMADO",
                column: "PRIORIDADE_ID_PRIORIDADE",
                principalTable: "PRIORIDADE",
                principalColumn: "ID_PRIORIDADE",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CHAMADO_ORGAO_GOVERNO_ORGAO_GOVERNO_ID_ORGAO",
                table: "CHAMADO");

            migrationBuilder.DropForeignKey(
                name: "FK_CHAMADO_PRIORIDADE_PRIORIDADE_ID_PRIORIDADE",
                table: "CHAMADO");

            migrationBuilder.DropTable(
                name: "ORGAO_GOVERNO");

            migrationBuilder.DropTable(
                name: "PRIORIDADE");

            migrationBuilder.DropIndex(
                name: "IX_VOTO_CHAMADO_CHAMADO_ID_CHAMADO_USUARIO_ID_USUARIO",
                table: "VOTO_CHAMADO");

            migrationBuilder.DropCheckConstraint(
                name: "CK_STATUS_NOME",
                table: "STATUS");

            migrationBuilder.DropIndex(
                name: "IX_CHAMADO_ORGAO_GOVERNO_ID_ORGAO",
                table: "CHAMADO");

            migrationBuilder.DropIndex(
                name: "IX_CHAMADO_PRIORIDADE_ID_PRIORIDADE",
                table: "CHAMADO");

            migrationBuilder.DropIndex(
                name: "IX_CATEGORIA_NM_CATEGORIA",
                table: "CATEGORIA");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CATEGORIA_NOME",
                table: "CATEGORIA");

            migrationBuilder.DropColumn(
                name: "ORGAO_GOVERNO_ID_ORGAO",
                table: "CHAMADO");

            migrationBuilder.DropColumn(
                name: "PRIORIDADE_ID_PRIORIDADE",
                table: "CHAMADO");

            migrationBuilder.AlterColumn<string>(
                name: "SENHA",
                table: "USUARIO",
                type: "NVARCHAR2(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(60)",
                oldUnicode: false,
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "NM_USUARIO",
                table: "USUARIO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EMAIL",
                table: "USUARIO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CARGO",
                table: "USUARIO",
                type: "NVARCHAR2(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "NUM_TEL",
                table: "TELEFONE",
                type: "NVARCHAR2(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(9)",
                oldUnicode: false,
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "NUM_DDD",
                table: "TELEFONE",
                type: "NVARCHAR2(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(2)",
                oldUnicode: false,
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "NM_STATUS",
                table: "STATUS",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "NM_ESTADO",
                table: "ESTADO",
                type: "NVARCHAR2(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(30)",
                oldUnicode: false,
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "RUA",
                table: "ENDERECO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "REFERENCIA",
                table: "ENDERECO",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NUMERO",
                table: "ENDERECO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CEP",
                table: "ENDERECO",
                type: "NVARCHAR2(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(8)",
                oldUnicode: false,
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "NM_CIDADE",
                table: "CIDADE",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TITULO",
                table: "CHAMADO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DESCRICAO",
                table: "CHAMADO",
                type: "NVARCHAR2(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(1000)",
                oldUnicode: false,
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "NM_CATEGORIA",
                table: "CATEGORIA",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NM_BAIRRO",
                table: "BAIRRO",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR2(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_VOTO_CHAMADO_CHAMADO_ID_CHAMADO",
                table: "VOTO_CHAMADO",
                column: "CHAMADO_ID_CHAMADO");
        }
    }
}
