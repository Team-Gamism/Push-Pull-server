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
        [FromBody] CreateRoomRequest request,
        CancellationToken ct
        )
    {
        var hostSteamId = User.GetSteamId();

        var result = await _createRoomService.ExecuteAsync(new CreateRoomCommand(
            request.LobbyId,
            request.RoomName,
            request.IsPrivate,
            request.Password,
            hostSteamId
        ), ct);

        return CommonApiResponse.Created("방이 생성되었습니다.", new CreateRoomResponse(result.RoomCode));
    }

    [HttpGet("{roomCode}")]
    public async Task<CommonApiResponse<GetRoomResponse>> GetRoom(
        [FromRoute] string roomCode,
        CancellationToken ct
    )
    {
        var result = await _getRoomService.ExecuteAsync(new GetRoomCommand(roomCode), ct);

        return CommonApiResponse.Success("방 조회 성공.", new GetRoomResponse(
            result.RoomCode,
            result.RoomName,
            result.CurrentPlayers,
            result.IsPrivate
        ));
    }

    [HttpGet("all")]
    public async Task<CommonApiResponse<GetAllRoomResponse>> GetAllRoom(CancellationToken ct)
    {
        var result = await _getAllRoomService.ExecuteAsync(ct);

        return CommonApiResponse.Success("방 목록 조회 성공.", new GetAllRoomResponse(result.Rooms));
    }

    [SessionAuthorize]
    [HttpPost("{roomCode}/join")]
    public async Task<CommonApiResponse> JoinRoom(
        [FromRoute] string roomCode,
        [FromBody] JoinRoomRequest request,
        CancellationToken ct
        )
    {
        await _joinRoomService.ExecuteAsync(new JoinRoomCommand(roomCode, request.Password), ct);

        return CommonApiResponse.Success("방에 참여했습니다.");
    }
}
