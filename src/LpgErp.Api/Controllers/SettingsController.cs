using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Settings;
using LpgErp.Application.Features.Settings.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

/// <summary>
/// The distributor's own business profile — shop name, address, contact details. Read is open to
/// anyone (even the login page, before signing in, shows the configured name); only an Admin can
/// change it.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ICompanySettingsService _service;

    public SettingsController(ICompanySettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(cancellationToken);
        return Ok(ApiResponse<CompanySettingsDto>.Ok(result.Data!));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateCompanySettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CompanySettingsDto>.Fail(result.Error!));

        return Ok(ApiResponse<CompanySettingsDto>.Ok(result.Data!));
    }
}
