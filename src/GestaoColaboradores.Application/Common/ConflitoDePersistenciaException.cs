namespace GestaoColaboradores.Application.Common;

/// <summary>
/// O banco recusou a gravação por violar o índice único de Nome.
///
/// A checagem prévia de duplicidade no caso de uso resolve o caso comum, mas
/// entre a consulta e o INSERT existe uma janela em que outra requisição pode
/// inserir o mesmo nome. Sob concorrência, só a restrição do banco garante a
/// regra - e esta exceção a converte em HTTP 409 com mensagem de negócio.
/// </summary>
public sealed class ConflitoDePersistenciaException : Exception
{
    public ConflitoDePersistenciaException(string mensagem, Exception innerException)
        : base(mensagem, innerException)
    {
    }
}
