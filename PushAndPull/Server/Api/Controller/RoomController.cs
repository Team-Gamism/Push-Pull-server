using Microsoft.AspNetCore.Mvc;
using Server.Api.Attribute;
using Server.Api.Dto.Request;
using Server.Api.Dto.Response;
using Server.Api.Extension;
using Server.Application.Port.Input;

namespace Server.Api.Controller
{
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
        public async Task<ActionResult<CreateRoomResponse>> CreateRoom(
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

            var response = new CreateRoomResponse(
                result.RoomCode
            );
            
            return CreatedAtAction(
                nameof(GetRoom),
                new { roomCode = result.RoomCode },
                response
            );
        }

        [HttpGet("{roomCode}")]
        public async Task<ActionResult<GetRoomResponse>> GetRoom(
            [FromRoute] string roomCode
        )
        {
            var result = await _getRoomUseCase.ExecuteAsync(
                new GetRoomCommand(roomCode)
            );
            
            var response = new GetRoomResponse(
                result.RoomCode,
                result.RoomName,
                result.CurrentPlayers,
                result.IsPrivate
            );
            
            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<ActionResult<GetAllRoomResponse>> GetAllRoom(CancellationToken ct)
        {
            var result = await _getAllRoomUseCase.ExecuteAsync(ct);

            var response = new GetAllRoomResponse(
                result.Rooms.Select(r => new GetRoomResponse(
                    r.RoomCode,
                    r.RoomName,
                    r.CurrentPlayers,
                    r.IsPrivate
                )).ToList()   
            );
            
            return Ok(response);
        }

        [SessionAuthorize]
        [HttpPost("{roomCode}/join")]
        public async Task<IActionResult> JoinRoom(
            [FromRoute] string roomCode,
            [FromBody] JoinRoomRequest request
            )
        {
            await _joinRoomUseCase.ExecuteAsync(new JoinRoomCommand(
                roomCode,
                request.Password)
            );
            
            return NoContent();
        }
    }
}
