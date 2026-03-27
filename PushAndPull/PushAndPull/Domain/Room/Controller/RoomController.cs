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
    private readonly ICreateRoomService _createRoomService;
    private readonly IGetRoomService _getRoomService;
    private readonly IGetAllRoomService _getAllRoomService;
    private readonly IJoinRoomService _joinRoomService;

    public RoomController(
        ICreateRoomService createRoomService,
        IGetRoomService getRoomService,
        IGetAllRoomService getAllRoomService,
        IJoinRoomService joinRoomService
        )
    {
        _createRoomService = createRoomService;
        _getRoomService = getRoomService;
        _getAllRoomService = getAllRoomService;
        _joinRoomService = joinRoomService;
    }

    [SessionAuthorize]
    [HttpPost]
    public async Task<CommonApiResponse<CreateRoomResponse>> CreateRoom(
        [FromBody] CreateRoomRequest request
        )
    {
        var hostSteamId = User.GetSteamId();

        var result = await _createRoomService.ExecuteAsync(new CreateRoomCommand(
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
        var result = await _getRoomService.ExecuteAsync(
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
        var result = await _getAllRoomService.ExecuteAsync(ct);

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
        await _joinRoomService.ExecuteAsync(new JoinRoomCommand(
            roomCode,
            request.Password)
        );
    }
}
