using System.ComponentModel.DataAnnotations;
using GestaoColaboradores.Domain.Entities;

namespace GestaoColaboradores.Application.DTOs;

/// <summary>
/// Contrato de entrada para criação e edição, discriminado por Id (0 = novo).
///
/// Os DataAnnotations rendem validação nos dois lados: o Razor emite os
/// atributos data-val-* que o jquery.validate.unobtrusive consome no
/// navegador, e o ModelState revalida no servidor. Uma declaração, duas
/// barreiras, sem risco de divergirem.
/// </summary>
public sealed class SalvarUsuarioRequest
{
    /// <summary>Zero para novo registro; maior que zero para edição.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(
        UsuarioLimites.NomeTamanhoMaximo,
        MinimumLength = UsuarioLimites.NomeTamanhoMinimo,
        ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor/hora.")]
    // ParseLimitsInInvariantCulture evita que os limites abaixo sejam lidos
    // com a vírgula decimal do pt-BR e quebrem a validação.
    [Range(typeof(decimal), "0.01", "9999999.99",
        ErrorMessage = "O valor/hora deve estar entre R$ 0,01 e R$ 9.999.999,99.",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = false)]
    [Display(Name = "Valor/hora")]
    public decimal ValorHora { get; set; }

    [Required(ErrorMessage = "Informe a data de cadastro.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de cadastro")]
    public DateTime DataCadastro { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    public bool EhNovo => Id <= 0;
}
