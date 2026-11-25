using System.Security.Claims;
using haobtPC11458_ASM01.DTOs;
using haobtPC11458_ASM01.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace haobtPC11458_ASM01.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize]
public class CharactersController : ControllerBase
{
    private readonly CharacterService _characterService;

    public CharactersController(CharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCharacterRequest request)
    {
        if (!TryGetAccountId(out var accountId))
            return Unauthorized();

        var result = await _characterService.CreateAsync(accountId, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCharacterRequest request)
    {
        if (!TryGetAccountId(out var accountId))
            return Unauthorized();

        var result = await _characterService.UpdateAsync(accountId, id, request);

        if (result is null)
            return NotFound(new { message = "Không tìm thấy nhân vật của bạn" });

        return Ok(result);
    }

    private bool TryGetAccountId(out int accountId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out accountId);
    }
}
