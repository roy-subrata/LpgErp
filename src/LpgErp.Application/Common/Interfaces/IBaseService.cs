using LpgErp.Application.Common.Models;

namespace LpgErp.Application.Common.Interfaces;

public interface IService<TCreateDto, TUpdateDto, TResponseDto>
    where TCreateDto : class
    where TUpdateDto : class
    where TResponseDto : class
{
    Task<Result<PagedResult<TResponseDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TResponseDto>>> GetAllListAsync(CancellationToken cancellationToken = default);
    Task<Result<TResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TResponseDto>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
    Task<Result<TResponseDto>> UpdateAsync(Guid id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
