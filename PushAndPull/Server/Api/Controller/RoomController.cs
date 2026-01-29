using Microsoft.AspNetCore.Mvc;
using Server.Api.Dto.Request;
using Server.Api.Dto.Response;
using Server.Application.Port.Input;

namespace Server.Api.Controller
{
    [Route("api/v1/room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly ICreateRoomUseCase _createRoomUseCase;
        private readonly IGetRoomUseCase _getRoomUseCase;

        public RoomController(
            ICreateRoomUseCase createRoomUseCase,
            IGetRoomUseCase getRoomUseCase
            )
        {
            _createRoomUseCase = createRoomUseCase;
            _getRoomUseCase = getRoomUseCase;
        }
        
        [HttpPost]
        public async Task<ActionResult<CreateRoomResponse>> CreateRoom(
            [FromBody] CreateRoomRequest request
            )
        {
            var result = await _createRoomUseCase.ExecuteAsync(new CreateRoomCommand(
                request.LobbyId,
                request.RoomName,
                request.IsPrivate,
                request.Password,
                request.MaxPlayers,
                request.HostSteamId
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
                result.RoomName,
                result.RoomCode,
                result.MaxPlayers,
                result.IsPrivate
            );
            
            return Ok(response);
        }
    }
}
