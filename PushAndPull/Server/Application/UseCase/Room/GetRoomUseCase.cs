using Server.Application.Port.Input;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Exception.Room;

namespace Server.Application.UseCase.Room;

public class GetRoomUseCase : IGetRoomUseCase
{
    private readonly IRoomRepository _roomRepository;
    
    public GetRoomUseCase(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }
    
    public async Task<GetRoomResult> ExecuteAsync(GetRoomCommand request)
    {
        if (string.IsNullOrEmpty(request.RoomCode))
            throw new ArgumentException("REQUIRED_ROOMCODE");

        var room = await _roomRepository.GetAsync(request.RoomCode)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        return new GetRoomResult(
            room.Name,
            room.RoomCode,
            room.MaxPlayers,
            room.IsPrivate
        );
    }
}