using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZelaFix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORIA",
                columns: table => new
                {
                    ID_CATEGORIA = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_CATEGORIA = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIA", x => x.ID_CATEGORIA);
                });

            migrationBuilder.CreateTable(
                name: "ESTADO",
                columns: table => new
                {
                    ID_ESTADO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_ESTADO = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESTADO", x => x.ID_ESTADO);
                });

            migrationBuilder.CreateTable(
                name: "STATUS",
                columns: table => new
                {
                    ID_STATUS = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STATUS", x => x.ID_STATUS);
                });

            migrationBuilder.CreateTable(
                name: "CIDADE",
                columns: table => new
                {
                    ID_CIDADE = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_CIDADE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ESTADO_ID_ESTADO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIDADE", x => x.ID_CIDADE);
                    table.ForeignKey(
                        name: "FK_CIDADE_ESTADO_ESTADO_ID_ESTADO",
                        column: x => x.ESTADO_ID_ESTADO,
                        principalTable: "ESTADO",
                        principalColumn: "ID_ESTADO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BAIRRO",
                columns: table => new
                {
                    ID_BAIRRO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_BAIRRO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    CIDADE_ID_CIDADE = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAIRRO", x => x.ID_BAIRRO);
                    table.ForeignKey(
                        name: "FK_BAIRRO_CIDADE_CIDADE_ID_CIDADE",
                        column: x => x.CIDADE_ID_CIDADE,
                        principalTable: "CIDADE",
                        principalColumn: "ID_CIDADE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ENDERECO",
                columns: table => new
                {
                    ID_ENDERECO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CEP = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    RUA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NUMERO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    REFERENCIA = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    BAIRRO_ID_BAIRRO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENDERECO", x => x.ID_ENDERECO);
                    table.ForeignKey(
                        name: "FK_ENDERECO_BAIRRO_BAIRRO_ID_BAIRRO",
                        column: x => x.BAIRRO_ID_BAIRRO,
                        principalTable: "BAIRRO",
                        principalColumn: "ID_BAIRRO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_USUARIO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SENHA = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false),
                    CARGO = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    ENDERECO_ID_ENDERECO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.ID_USUARIO);
                    table.ForeignKey(
                        name: "FK_USUARIO_ENDERECO_ENDERECO_ID_ENDERECO",
                        column: x => x.ENDERECO_ID_ENDERECO,
                        principalTable: "ENDERECO",
                        principalColumn: "ID_ENDERECO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CHAMADO",
                columns: table => new
                {
                    ID_CHAMADO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TITULO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    FOTO = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DT_ABERTURA = table.Column<DateTime>(type: "DATE", nullable: false),
                    USUARIO_ID_USUARIO = table.Column<short>(type: "NUMBER(5)", nullable: false),
                    ENDERECO_ID_ENDERECO = table.Column<short>(type: "NUMBER(5)", nullable: false),
                    CATEGORIA_ID_CATEGORIA = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAMADO", x => x.ID_CHAMADO);
                    table.ForeignKey(
                        name: "FK_CHAMADO_CATEGORIA_CATEGORIA_ID_CATEGORIA",
                        column: x => x.CATEGORIA_ID_CATEGORIA,
                        principalTable: "CATEGORIA",
                        principalColumn: "ID_CATEGORIA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHAMADO_ENDERECO_ENDERECO_ID_ENDERECO",
                        column: x => x.ENDERECO_ID_ENDERECO,
                        principalTable: "ENDERECO",
                        principalColumn: "ID_ENDERECO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CHAMADO_USUARIO_USUARIO_ID_USUARIO",
                        column: x => x.USUARIO_ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TELEFONE",
                columns: table => new
                {
                    ID_TELEFONE = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NUM_DDD = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    NUM_TEL = table.Column<string>(type: "NVARCHAR2(9)", maxLength: 9, nullable: false),
                    USUARIO_ID_USUARIO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TELEFONE", x => x.ID_TELEFONE);
                    table.ForeignKey(
                        name: "FK_TELEFONE_USUARIO_USUARIO_ID_USUARIO",
                        column: x => x.USUARIO_ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HIST_CHAMADO",
                columns: table => new
                {
                    ID_HIST = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DT_MUDANCA = table.Column<DateTime>(type: "DATE", nullable: false),
                    OBSERVACOES = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    CHAMADO_ID_CHAMADO = table.Column<short>(type: "NUMBER(5)", nullable: false),
                    USUARIO_ID_USUARIO = table.Column<short>(type: "NUMBER(5)", nullable: false),
                    STATUS_ID_STATUS = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HIST_CHAMADO", x => x.ID_HIST);
                    table.ForeignKey(
                        name: "FK_HIST_CHAMADO_CHAMADO_CHAMADO_ID_CHAMADO",
                        column: x => x.CHAMADO_ID_CHAMADO,
                        principalTable: "CHAMADO",
                        principalColumn: "ID_CHAMADO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HIST_CHAMADO_STATUS_STATUS_ID_STATUS",
                        column: x => x.STATUS_ID_STATUS,
                        principalTable: "STATUS",
                        principalColumn: "ID_STATUS",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HIST_CHAMADO_USUARIO_USUARIO_ID_USUARIO",
                        column: x => x.USUARIO_ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VOTO_CHAMADO",
                columns: table => new
                {
                    ID_VOTO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DT_VOTO = table.Column<DateTime>(type: "DATE", nullable: false),
                    CHAMADO_ID_CHAMADO = table.Column<short>(type: "NUMBER(5)", nullable: false),
                    USUARIO_ID_USUARIO = table.Column<short>(type: "NUMBER(5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VOTO_CHAMADO", x => x.ID_VOTO);
                    table.ForeignKey(
                        name: "FK_VOTO_CHAMADO_CHAMADO_CHAMADO_ID_CHAMADO",
                        column: x => x.CHAMADO_ID_CHAMADO,
                        principalTable: "CHAMADO",
                        principalColumn: "ID_CHAMADO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VOTO_CHAMADO_USUARIO_USUARIO_ID_USUARIO",
                        column: x => x.USUARIO_ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BAIRRO_CIDADE_ID_CIDADE",
                table: "BAIRRO",
                column: "CIDADE_ID_CIDADE");

            migrationBuilder.CreateIndex(
                name: "IX_CHAMADO_CATEGORIA_ID_CATEGORIA",
                table: "CHAMADO",
                column: "CATEGORIA_ID_CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_CHAMADO_ENDERECO_ID_ENDERECO",
                table: "CHAMADO",
                column: "ENDERECO_ID_ENDERECO");

            migrationBuilder.CreateIndex(
                name: "IX_CHAMADO_USUARIO_ID_USUARIO",
                table: "CHAMADO",
                column: "USUARIO_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_CIDADE_ESTADO_ID_ESTADO",
                table: "CIDADE",
                column: "ESTADO_ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "IX_ENDERECO_BAIRRO_ID_BAIRRO",
                table: "ENDERECO",
                column: "BAIRRO_ID_BAIRRO");

            migrationBuilder.CreateIndex(
                name: "IX_HIST_CHAMADO_CHAMADO_ID_CHAMADO",
                table: "HIST_CHAMADO",
                column: "CHAMADO_ID_CHAMADO");

            migrationBuilder.CreateIndex(
                name: "IX_HIST_CHAMADO_STATUS_ID_STATUS",
                table: "HIST_CHAMADO",
                column: "STATUS_ID_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_HIST_CHAMADO_USUARIO_ID_USUARIO",
                table: "HIST_CHAMADO",
                column: "USUARIO_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TELEFONE_USUARIO_ID_USUARIO",
                table: "TELEFONE",
                column: "USUARIO_ID_USUARIO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_EMAIL",
                table: "USUARIO",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ENDERECO_ID_ENDERECO",
                table: "USUARIO",
                column: "ENDERECO_ID_ENDERECO");

            migrationBuilder.CreateIndex(
                name: "IX_VOTO_CHAMADO_CHAMADO_ID_CHAMADO",
                table: "VOTO_CHAMADO",
                column: "CHAMADO_ID_CHAMADO");

            migrationBuilder.CreateIndex(
                name: "IX_VOTO_CHAMADO_USUARIO_ID_USUARIO",
                table: "VOTO_CHAMADO",
                column: "USUARIO_ID_USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HIST_CHAMADO");

            migrationBuilder.DropTable(
                name: "TELEFONE");

            migrationBuilder.DropTable(
                name: "VOTO_CHAMADO");

            migrationBuilder.DropTable(
                name: "STATUS");

            migrationBuilder.DropTable(
                name: "CHAMADO");

            migrationBuilder.DropTable(
                name: "CATEGORIA");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "ENDERECO");

            migrationBuilder.DropTable(
                name: "BAIRRO");

            migrationBuilder.DropTable(
                name: "CIDADE");

            migrationBuilder.DropTable(
                name: "ESTADO");
        }
    }
}
