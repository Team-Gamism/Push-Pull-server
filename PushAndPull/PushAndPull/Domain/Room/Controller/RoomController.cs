using Gamism.SDK.Core.Network;
using Microsoft.AspNetCore.Mvc;
using PushAndPull.Domain.Room.Dto.Request;
using PushAndPull.Domain.Room.Dto.Response;
using PushAndPull.Domain.Room.Service.Interface;
using PushAndPull.Global.Security;

namespace PushAndPull.Domain.Room.Controller;

[Route("api/v1/room")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly ICreateRoomUseCase _createRoomUseCase;
    private readonly IGetRoomUseCase _getRoomUseCase;
    private readonly IGetAllRoomUseCase _getAllRoomUseCase;
    private readonly IJoinRoomUseCase _joinRoomUseCase;

    public RoomController(
        ICreateRoomUseCase createRoomUseCase,
        IGetRoomUseCase getRoomUseCase,
        IGetAllRoomUseCase getAllRoomUseCase,
        IJoinRoomUseCase joinRoomUseCase
        )
    {
        _createRoomUseCase = createRoomUseCase;
        _getRoomUseCase = getRoomUseCase;
        _getAllRoomUseCase = getAllRoomUseCase;
        _joinRoomUseCase = joinRoomUseCase;
    }

    [SessionAuthorize]
    [HttpPost]
    public async Task<CommonApiResponse<CreateRoomResponse>> CreateRoom(
        [FromBody] CreateRoomRequest request
        )
    {
        var hostSteamId = User.GetSteamId();

        var result = await _createRoomUseCase.ExecuteAsync(new CreateRoomCommand(
            request.LobbyId,
            request.RoomName,
            request.IsPrivate,
            request.Password,
            hostSteamId
            )
        );

        return CommonApiResponse.Created("방이 생성되었습니다.", new CreateRoomResponse(result.RoomCode));
    }

    [HttpGet("{roomCode}")]
    public async Task<GetRoomResponse> GetRoom(
        [FromRoute] string roomCode
    )
    {
        var result = await _getRoomUseCase.ExecuteAsync(
            new GetRoomCommand(roomCode)
        );

        return new GetRoomResponse(
            result.RoomCode,
            result.RoomName,
            result.CurrentPlayers,
            result.IsPrivate
        );
    }

    [HttpGet("all")]
    public async Task<GetAllRoomResponse> GetAllRoom(CancellationToken ct)
    {
        var result = await _getAllRoomUseCase.ExecuteAsync(ct);

        return new GetAllRoomResponse(
            result.Rooms.Select(r => new GetRoomResponse(
                r.RoomCode,
                r.RoomName,
                r.CurrentPlayers,
                r.IsPrivate
            )).ToList()
        );
    }

    [SessionAuthorize]
    [HttpPost("{roomCode}/join")]
    public async Task JoinRoom(
        [FromRoute] string roomCode,
        [FromBody] JoinRoomRequest request
        )
    {
        await _joinRoomUseCase.ExecuteAsync(new JoinRoomCommand(
            roomCode,
            request.Password)
        );
    }
}
