using Vox.Domain.Models;

namespace Vox.Application.Handlers
{
    public class HorarioHandler
    {
        public Task<bool> HorarioConflita(HorarioModel horario, IEnumerable<HorarioModel> horariosOcupados)
        {
            if (horariosOcupados == null)
                return Task.FromResult(false);

            var novoInicio= Combine(horario.Data, horario.HoraInicio);
            var novoFim = Combine(horario.Data, horario.HoraFim);

            if (novoFim <= novoInicio)
                throw new ArgumentException("HoraFim deve ser maior que HoraInicio.");

            bool conflito = horariosOcupados.Any(h =>
            {
                if (h.Data.Date != horario.Data.Date)
                    return false;

                var inicio = Combine(h.Data, h.HoraInicio);
                var fim = Combine(h.Data, h.HoraFim);

                return inicio < novoFim && fim > novoInicio;
            });

            return Task.FromResult(conflito);
        }

        private static DateTime Combine(DateTime data, TimeSpan hora) => data.Date + hora;
    }
}