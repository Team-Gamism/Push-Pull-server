using Gamism.SDK.Core.Network;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    private readonly ILeaveRoomService _leaveRoomService;
    private readonly ICloseRoomService _closeRoomService;
    private readonly IHeartbeatRoomService _heartbeatRoomService;

    public RoomController(
        ICreateRoomService createRoomService,
        IGetRoomService getRoomService,
        IGetAllRoomService getAllRoomService,
        IJoinRoomService joinRoomService,
        ILeaveRoomService leaveRoomService,
        ICloseRoomService closeRoomService,
        IHeartbeatRoomService heartbeatRoomService
        )
    {
        _createRoomService = createRoomService;
        _getRoomService = getRoomService;
        _getAllRoomService = getAllRoomService;
        _joinRoomService = joinRoomService;
        _leaveRoomService = leaveRoomService;
        _closeRoomService = closeRoomService;
        _heartbeatRoomService = heartbeatRoomService;
    }

    [SessionAuthorize]
    [HttpPost]
    [EnableRateLimiting("create_room")]
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

        return CommonApiResponse.Success("방 조회 성공.", ToGetRoomResponse(result));
    }

    [HttpGet("all")]
    public async Task<CommonApiResponse<GetAllRoomResponse>> GetAllRoom(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default
        )
    {
        var result = await _getAllRoomService.ExecuteAsync(new GetAllRoomQuery(page, size), ct);

        var rooms = result.Rooms
            .Select(ToGetRoomResponse)
            .ToList();

        return CommonApiResponse.Success("방 목록 조회 성공.",
            new GetAllRoomResponse(rooms, result.Page, result.Size, result.HasNext));
    }

    [SessionAuthorize]
    [HttpPost("{roomCode}/join")]
    [EnableRateLimiting("join_room")]
    public async Task<CommonApiResponse<JoinRoomResponse>> JoinRoom(
        [FromRoute] string roomCode,
        [FromBody] JoinRoomRequest request,
        CancellationToken ct
        )
    {
        var result = await _joinRoomService.ExecuteAsync(new JoinRoomCommand(roomCode, request.Password, User.GetSteamId()), ct);

        return CommonApiResponse.Success("방에 참여했습니다.", new JoinRoomResponse(result.SteamLobbyId));
    }

    [SessionAuthorize]
    [HttpPost("{roomCode}/leave")]
    public async Task<CommonApiResponse> LeaveRoom(
        [FromRoute] string roomCode,
        CancellationToken ct
        )
    {
        await _leaveRoomService.ExecuteAsync(new LeaveRoomCommand(roomCode, User.GetSteamId()), ct);

        return CommonApiResponse.Success("방에서 나갔습니다.");
    }

    [SessionAuthorize]
    [HttpDelete("{roomCode}")]
    public async Task<CommonApiResponse> CloseRoom(
        [FromRoute] string roomCode,
        CancellationToken ct
        )
    {
        await _closeRoomService.ExecuteAsync(new CloseRoomCommand(roomCode, User.GetSteamId()), ct);

        return CommonApiResponse.Success("방을 닫았습니다.");
    }

    [SessionAuthorize]
    [HttpPost("{roomCode}/heartbeat")]
    public async Task<CommonApiResponse> Heartbeat(
        [FromRoute] string roomCode,
        CancellationToken ct
        )
    {
        await _heartbeatRoomService.ExecuteAsync(new HeartbeatRoomCommand(roomCode, User.GetSteamId()), ct);

        return CommonApiResponse.Success("하트비트가 갱신되었습니다.");
    }

    private static GetRoomResponse ToGetRoomResponse(GetRoomResult result) =>
        new(
            result.RoomCode,
            result.RoomName,
            result.CurrentPlayers,
            result.MaxPlayers,
            result.IsPrivate,
            result.HasPassword
        );
}
