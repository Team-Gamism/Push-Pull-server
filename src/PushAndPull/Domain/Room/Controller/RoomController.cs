using System.Net;
using Gamism.SDK.Core.Network;
using Gamism.SDK.Extensions.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PushAndPull.Domain.Room.Dto.Request;
using PushAndPull.Domain.Room.Dto.Response;
using PushAndPull.Domain.Room.Exception;
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
    private readonly IReconnectRoomService _reconnectRoomService;

    public RoomController(
        ICreateRoomService createRoomService,
        IGetRoomService getRoomService,
        IGetAllRoomService getAllRoomService,
        IJoinRoomService joinRoomService,
        ILeaveRoomService leaveRoomService,
        ICloseRoomService closeRoomService,
        IHeartbeatRoomService heartbeatRoomService,
        IReconnectRoomService reconnectRoomService
        )
    {
        _createRoomService = createRoomService;
        _getRoomService = getRoomService;
        _getAllRoomService = getAllRoomService;
        _joinRoomService = joinRoomService;
        _leaveRoomService = leaveRoomService;
        _closeRoomService = closeRoomService;
        _heartbeatRoomService = heartbeatRoomService;
        _reconnectRoomService = reconnectRoomService;
    }

    [ApiDoc("방 생성", "스팀 로비를 기반으로 새 방을 생성한다.")]
    [ApiError(typeof(InvalidRoomNameException), "방 이름이 올바르지 않습니다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
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

    [ApiDoc("방 조회", "roomCode로 단일 방 정보를 조회한다.")]
    [ApiError(typeof(RoomNotFoundException), "방을 찾을 수 없습니다.")]
    [HttpGet("{roomCode}")]
    public async Task<CommonApiResponse<GetRoomResponse>> GetRoom(
        [FromRoute] string roomCode,
        CancellationToken ct
    )
    {
        var result = await _getRoomService.ExecuteAsync(new GetRoomCommand(roomCode), ct);

        return CommonApiResponse.Success("방 조회 성공.", ToGetRoomResponse(result));
    }

    [ApiDoc("방 목록 조회", "페이지네이션으로 공개된 방 목록을 조회한다.")]
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

    [ApiDoc("방 참여", "roomCode와 비밀번호로 방에 참여한다.")]
    [ApiError(typeof(RoomNotFoundException), "방을 찾을 수 없습니다.")]
    [ApiError(typeof(RoomNotActiveException), "참여할 수 없는 방 상태입니다.")]
    [ApiError(typeof(RoomFullException), "방이 가득 찼습니다.")]
    [ApiError(typeof(AlreadyJoinedRoomException), "이미 참여한 방입니다.")]
    [ApiError(typeof(PasswordRequiredException), "비밀번호가 필요합니다.")]
    [ApiError(typeof(InvalidPasswordException), "비밀번호가 올바르지 않습니다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
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

    [ApiDoc("방 나가기", "참여 중인 방에서 나간다.")]
    [ApiError(typeof(RoomNotFoundException), "방을 찾을 수 없습니다.")]
    [ApiError(typeof(RoomNotActiveException), "이용할 수 없는 방 상태입니다.")]
    [ApiError(typeof(RoomNotParticipantException), "방 참여자가 아닙니다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
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

    [ApiDoc("방 닫기", "방장이 방을 닫는다.")]
    [ApiError(typeof(RoomNotFoundException), "방을 찾을 수 없습니다.")]
    [ApiError(typeof(NotRoomHostException), "방장만 방을 닫을 수 있습니다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
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

    [ApiDoc("방 하트비트", "참여자의 활성 상태를 갱신해 방 세션을 유지한다.")]
    [ApiError(typeof(RoomNotFoundException), "방을 찾을 수 없습니다.")]
    [ApiError(typeof(RoomNotActiveException), "이용할 수 없는 방 상태입니다.")]
    [ApiError(typeof(RoomNotParticipantException), "방 참여자가 아닙니다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
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

    [ApiDoc("방 재접속", "끊긴 세션으로 참여 중이던 방에 재접속한다.")]
    [ApiError(typeof(RoomNotFoundException), "방을 찾을 수 없습니다.")]
    [ApiError(typeof(RoomNotActiveException), "이용할 수 없는 방 상태입니다.")]
    [ApiError(typeof(RoomNotParticipantException), "방 참여자가 아닙니다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
    [SessionAuthorize]
    [HttpPost("{roomCode}/reconnect")]
    [EnableRateLimiting("join_room")]
    public async Task<CommonApiResponse<ReconnectRoomResponse>> Reconnect(
        [FromRoute] string roomCode,
        CancellationToken ct
        )
    {
        var result = await _reconnectRoomService.ExecuteAsync(new ReconnectRoomCommand(roomCode, User.GetSteamId()), ct);

        return CommonApiResponse.Success("재접속했습니다.", new ReconnectRoomResponse(result.SteamLobbyId, result.Role));
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
