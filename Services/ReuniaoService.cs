using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services.Validation;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço de reuniões implementando lógica de negócio
    /// Segue os princípios SRP e DIP
    /// Utiliza Strategy Pattern para validações
    /// </summary>
    public class ReuniaoService : IReuniaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReuniaoService> _logger;
        private readonly IReuniaoValidatorFactory _validatorFactory;

        public ReuniaoService(
            ApplicationDbContext context, 
            ILogger<ReuniaoService> logger,
            IReuniaoValidatorFactory validatorFactory)
        {
            _context = context;
            _logger = logger;
            _validatorFactory = validatorFactory;
        }

        public async Task<List<Reuniao>> GetReunioesPorPeriodoAsync(DateTime inicio, DateTime fim, CalendarioFiltros? filtros = null)
        {
            try
            {
                var query = _context.Reunioes
                    .Include(r => r.ResponsavelUser!)
                    .ThenInclude(u => u.Department)
                    .Include(r => r.Participantes)
                    .ThenInclude(p => p.Ramal)
                    .Include(r => r.Participantes)
                    .ThenInclude(p => p.Departamento)
                    .Where(r => r.Data >= inicio && r.Data <= fim);                if (filtros != null)
                {
                    if (filtros.TipoReuniao.HasValue)
                        query = query.Where(r => r.TipoReuniao == filtros.TipoReuniao.Value);

                    if (filtros.Status.HasValue)
                        query = query.Where(r => r.Status == filtros.Status.Value);

                    if (filtros.Sala.HasValue)
                        query = query.Where(r => r.Sala == filtros.Sala.Value);

                    if (!string.IsNullOrEmpty(filtros.Empresa))
                        query = query.Where(r => r.Empresa != null && r.Empresa.Contains(filtros.Empresa));
                }                var reunioes = await query
                    .OrderBy(r => r.Data)
                    .ToListAsync();

                // Ordenar por horário no lado do cliente (SQLite não suporta OrderBy com TimeSpan)
                reunioes = reunioes.OrderBy(r => r.Data).ThenBy(r => r.HoraInicio).ToList();

                await CarregarDepartamentosAsync(reunioes);
                return reunioes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar reuniões por período: {Inicio} - {Fim}", inicio, fim);
                throw;
            }
        }

        public async Task<Reuniao?> GetReuniaoDetalhadaAsync(int id)
        {
            try
            {
                return await _context.Reunioes
                    .Include(r => r.ResponsavelUser!)
                    .ThenInclude(u => u.Department)
                    .Include(r => r.Participantes)
                    .ThenInclude(p => p.Ramal)
                    .Include(r => r.Participantes)
                    .ThenInclude(p => p.Departamento)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar reunião por ID: {Id}", id);
                throw;
            }
        }

        public async Task<Reuniao> CriarReuniaoAsync(ReuniaoViewModel viewModel, string userId)
        {
            try
            {
                var reuniao = MapearViewModelParaEntity(viewModel, userId);
                
                _context.Reunioes.Add(reuniao);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reunião criada: {Id} - {Titulo}", reuniao.Id, reuniao.Titulo);
                return reuniao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar reunião: {Titulo}", viewModel.Titulo);
                throw;
            }
        }

        public async Task<bool> AtualizarReuniaoAsync(int id, ReuniaoViewModel viewModel, string userId)
        {
            try
            {
                var reuniao = await _context.Reunioes
                    .Include(r => r.Participantes)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reuniao == null) return false;

                // Verificar permissões
                if (!await PodeEditarReuniaoAsync(id, userId, false))
                    throw new UnauthorizedAccessException("Usuário não tem permissão para editar esta reunião");

                // Atualizar dados
                reuniao.Titulo = viewModel.Titulo;
                reuniao.TipoReuniao = viewModel.TipoReuniao;
                reuniao.Data = viewModel.Data;
                reuniao.HoraInicio = viewModel.HoraInicio;
                reuniao.HoraFim = viewModel.HoraFim;
                reuniao.Sala = viewModel.Sala;
                reuniao.Veiculo = viewModel.Veiculo;
                reuniao.LinkOnline = viewModel.LinkOnline;
                reuniao.Empresa = viewModel.Empresa;
                reuniao.Observacoes = viewModel.Observacoes;                // Atualizar participantes
                _context.ReuniaoParticipantes.RemoveRange(reuniao.Participantes);
                if (viewModel.Participantes?.Any() == true)
                {
                    reuniao.Participantes = viewModel.Participantes.Select(p => new ReuniaoParticipante
                    {
                        Nome = p.Nome,
                        RamalId = p.RamalId,
                        DepartamentoId = p.DepartamentoId,
                        ReuniaoId = reuniao.Id
                    }).ToList();
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Reunião atualizada: {Id} - {Titulo}", reuniao.Id, reuniao.Titulo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar reunião: {Id}", id);
                throw;
            }
        }

        public async Task<bool> RemoverReuniaoAsync(int id, string userId)
        {
            try
            {
                var reuniao = await _context.Reunioes
                    .Include(r => r.Participantes)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reuniao == null) return false;

                // Verificar permissões
                if (!await PodeEditarReuniaoAsync(id, userId, false))
                    throw new UnauthorizedAccessException("Usuário não tem permissão para remover esta reunião");                _context.ReuniaoParticipantes.RemoveRange(reuniao.Participantes);
                _context.Reunioes.Remove(reuniao);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reunião removida: {Id} - {Titulo}", reuniao.Id, reuniao.Titulo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover reunião: {Id}", id);
                throw;
            }
        }

        public async Task<ValidationResult> ValidarReuniaoAsync(ReuniaoViewModel viewModel, int? reuniaoIdExcluir = null)
        {
            try
            {
                var validator = _validatorFactory.GetValidator(viewModel.TipoReuniao);
                return await validator.ValidateAsync(viewModel, reuniaoIdExcluir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar reunião: {Titulo}", viewModel.Titulo);
                
                var result = new ValidationResult();
                result.AddError("Erro interno na validação. Tente novamente.");
                return result;
            }
        }

        public async Task<bool> PodeEditarReuniaoAsync(int reuniaoId, string userId, bool isAdmin)
        {
            if (isAdmin) return true;
            
            var reuniao = await _context.Reunioes.FindAsync(reuniaoId);
            return reuniao != null && reuniao.ResponsavelUserId == userId;
        }

        public async Task PopularDadosViewModelAsync(ReuniaoViewModel viewModel)
        {
            viewModel.RamaisDisponiveis = await _context.Ramais
                .Where(r => r.Ativo)
                .OrderBy(r => r.Numero)
                .ToListAsync();

            viewModel.DepartamentosDisponiveis = await _context.Departments
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<bool> MarcarReuniaoRealizadaAsync(int reuniaoId, string userId, bool isAdmin)
        {
            try
            {
                var reuniao = await _context.Reunioes.FindAsync(reuniaoId);
                if (reuniao == null) return false;

                if (!await PodeEditarReuniaoAsync(reuniaoId, userId, isAdmin))
                    throw new UnauthorizedAccessException("Usuário não tem permissão para marcar esta reunião como realizada");

                if (reuniao.Status == StatusReuniao.Agendada)
                {
                    reuniao.Status = StatusReuniao.Realizada;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Reunião marcada como realizada: {Id} - {Titulo}", reuniao.Id, reuniao.Titulo);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar reunião como realizada: {Id}", reuniaoId);
                throw;
            }
        }

        public async Task<bool> CancelarReuniaoAsync(int reuniaoId, string userId, bool isAdmin)
        {
            try
            {
                var reuniao = await _context.Reunioes.FindAsync(reuniaoId);
                if (reuniao == null) return false;

                if (!await PodeEditarReuniaoAsync(reuniaoId, userId, isAdmin))
                    throw new UnauthorizedAccessException("Usuário não tem permissão para cancelar esta reunião");

                if (reuniao.Status == StatusReuniao.Agendada)
                {
                    reuniao.Status = StatusReuniao.Cancelada;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Reunião cancelada: {Id} - {Titulo}", reuniao.Id, reuniao.Titulo);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar reunião: {Id}", reuniaoId);
                throw;
            }
        }

        public async Task<bool> ReuniaoExisteAsync(int reuniaoId)
        {
            return await _context.Reunioes.AnyAsync(r => r.Id == reuniaoId);
        }

        public async Task<Reuniao?> GetReuniaoAsync(int id)
        {
            return await _context.Reunioes.FindAsync(id);
        }

        public async Task<ReuniaoViewModel> MapearParaViewModelAsync(Reuniao reuniao)
        {
            var viewModel = new ReuniaoViewModel
            {
                Id = reuniao.Id,
                Titulo = reuniao.Titulo,
                TipoReuniao = reuniao.TipoReuniao,
                Data = reuniao.Data,
                HoraInicio = reuniao.HoraInicio,
                HoraFim = reuniao.HoraFim,
                Sala = reuniao.Sala,
                Veiculo = reuniao.Veiculo,
                LinkOnline = reuniao.LinkOnline,
                Empresa = reuniao.Empresa,
                Observacoes = reuniao.Observacoes,
                Status = reuniao.Status,
                ResponsavelUserId = reuniao.ResponsavelUserId,
                Participantes = reuniao.Participantes?.Select(p => new ParticipanteViewModel
                {
                    Nome = p.Nome,
                    RamalId = p.RamalId,
                    DepartamentoId = p.DepartamentoId
                }).ToList() ?? new List<ParticipanteViewModel>()
            };

            await PopularDadosViewModelAsync(viewModel);
            return viewModel;
        }        private async Task CarregarDepartamentosAsync(List<Reuniao> reunioes)
        {
            var departmentIds = reunioes
                .Where(r => r.ResponsavelUser != null)
                .Select(r => r.ResponsavelUser!.DepartmentId)
                .Distinct()
                .ToList();

            if (departmentIds.Any())
            {
                var departments = await _context.Departments
                    .Where(d => departmentIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => d);

                foreach (var reuniao in reunioes)
                {
                    if (reuniao.ResponsavelUser != null &&
                        departments.TryGetValue(reuniao.ResponsavelUser.DepartmentId, out var dept))
                    {
                        reuniao.ResponsavelUser.Department = dept;
                    }
                }
            }
        }        private static Reuniao MapearViewModelParaEntity(ReuniaoViewModel viewModel, string userId)
        {
            return new Reuniao
            {
                Titulo = viewModel.Titulo,
                TipoReuniao = viewModel.TipoReuniao,
                Data = viewModel.Data,
                HoraInicio = viewModel.HoraInicio,
                HoraFim = viewModel.HoraFim,
                Sala = viewModel.Sala,
                Veiculo = viewModel.Veiculo,
                LinkOnline = viewModel.LinkOnline,
                Empresa = viewModel.Empresa,
                Observacoes = viewModel.Observacoes,
                Status = StatusReuniao.Agendada,
                ResponsavelUserId = userId,
                Participantes = viewModel.Participantes?.Select(p => new ReuniaoParticipante
                {
                    Nome = p.Nome,
                    RamalId = p.RamalId,
                    DepartamentoId = p.DepartamentoId
                }).ToList() ?? new List<ReuniaoParticipante>()
            };
        }
    }
}
