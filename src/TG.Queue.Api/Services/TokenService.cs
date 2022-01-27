using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TG.Core.App.Extensions;
using TG.Core.App.Services;
using TG.Queue.Api.Config;
using TG.Queue.Api.Config.Options;

namespace TG.Queue.Api.Services;

public class TokenService : ITokenService
{
    private readonly BattleJwtTokenOptions _battleJwtTokenOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRsaParser _rsaParser;

    public TokenService(IOptions<BattleJwtTokenOptions> battleJwtTokenOptions, IDateTimeProvider dateTimeProvider, IRsaParser rsaParser)
    {
        _dateTimeProvider = dateTimeProvider;
        _rsaParser = rsaParser;
        _battleJwtTokenOptions = battleJwtTokenOptions.Value;
    }

    public string GenerateBattleAccessToken(Guid userId, Guid battleId)
    {
        var accessTokenPayload = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = ServiceConst.ServiceName,
            [JwtRegisteredClaimNames.Aud] = battleId.ToString(),
            [JwtRegisteredClaimNames.Sub] = userId.ToString(),
            [JwtRegisteredClaimNames.Exp] = _dateTimeProvider.UtcNow.AddSeconds(_battleJwtTokenOptions.LifetimeSec).ToUnixTime(),
        };

        var rsaPrivateKey = _rsaParser.ParseRsaPrivateKey(_battleJwtTokenOptions.PrivateKey);
        var credentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha512);
        var header = new JwtHeader(credentials);
        var jwtToken = new JwtSecurityToken(header, accessTokenPayload);
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(jwtToken);
    }
}