using System;

namespace TG.Queue.Api.Services;

public interface ITokenService
{
    string GenerateBattleAccessToken(Guid userId, Guid battleId);
}