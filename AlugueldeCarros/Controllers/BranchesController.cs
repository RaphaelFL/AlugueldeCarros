using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/branches")]
public class BranchesController : ControllerBase
{
    private readonly BranchService _branchService;

    public BranchesController(BranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _branchService.GetAllAsync();
        return Ok(branches);
    }
}
