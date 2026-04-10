using Implementador.Infrastructure;
using Implementador.Application.Validation.Common;

namespace Implementador.Application.Validation.Core;

public abstract class RowValidatorBase
{
    /// <summary>
    /// Retornar esta lista desde el lambda de validación rechaza la fila sin emitir ningún log.
    /// Útil cuando el motivo de rechazo ya fue reportado en una validación anterior.
    /// </summary>
    protected static readonly List<string> SilentReject = [string.Empty];

    protected List<Dictionary<string, string>> FilterValidRows(
        string scope,
        List<Dictionary<string, string>> sourceRows,
        IAppLogger log,
        Func<Dictionary<string, string>, int, List<string>> validateRow,
        out int rejected)
    {
        var accepted = new List<Dictionary<string, string>>();
        rejected = 0;

        for (int i = 0; i < sourceRows.Count; i++)
        {
            var row = sourceRows[i];
            var rowNumber = i + 2;
            var errors = validateRow(row, rowNumber);

            if (errors.Count == 0)
            {
                accepted.Add(row);
                continue;
            }

            rejected++;
            var loggable = errors.Where(e => !string.IsNullOrEmpty(e)).ToList();
            if (loggable.Count > 0)
                log.Warn(ValidationLog.FilaError(scope, rowNumber, string.Join(" | ", loggable)));
        }

        return accepted;
    }
}


