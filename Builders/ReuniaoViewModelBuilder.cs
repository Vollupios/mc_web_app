using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class ReuniaoViewModelBuilder
    {
        public static ReuniaoViewModel Build(Reuniao reuniao)
        {
            return new ReuniaoViewModel
            {
                Id = reuniao.Id,
                Titulo = reuniao.Titulo,
                Data = reuniao.Data,
                HoraInicio = reuniao.HoraInicio,
                HoraFim = reuniao.HoraFim,
                TipoReuniao = reuniao.TipoReuniao,
                Sala = reuniao.Sala,
                Veiculo = reuniao.Veiculo,
                Participantes = reuniao.Participantes?
                    .Select(p => new ParticipanteViewModel
                    {
                        Id = p.Id,
                        Nome = p.Nome
                        // Adicione outros campos necessários do ParticipanteViewModel aqui
                    }).ToList(),
                Observacoes = reuniao.Observacoes
            };
        }
    }
}
